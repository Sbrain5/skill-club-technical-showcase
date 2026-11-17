
# Arbiter Backend — Match Consistency Verification

## Overview

I developed a lightweight match-consistency verification backend for **Skill Club**, using C# and ASP.NET Core Minimal API.

The service runs separately from the Unity clients on a VPS and receives gameplay checkpoint hashes and final match reports from both players.

Its purpose is to identify disagreements between the two clients without implementing a fully authoritative game server.

## Architecture

The implementation consists of three main components:

**Unity Arbiter Client**
- Creates match sessions through HTTP requests.
- Receives and shares session information.
- Submits checkpoint hashes and final match results.

**Rollback Checkpoint Reporter**
- Retrieves historical game states from the rollback system.
- Serializes selected gameplay-state values in a consistent order.
- Generates SHA-256 hashes.
- Sends checkpoints to the backend.

**ASP.NET Core Backend**
- Creates and manages match sessions.
- Validates session tokens and participating player IDs.
- Stores and compares checkpoint reports.
- Compares final match results.
- Returns verification or mismatch responses.

## REST API

The backend exposes four endpoints:

| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `/api/health` | Check service availability and build version |
| POST | `/api/session/create` | Create a match verification session |
| POST | `/api/session/checkpoint` | Submit a gameplay-state checkpoint |
| POST | `/api/session/finish` | Submit a final match report |

These endpoints are registered using ASP.NET Core Minimal API.

### Implementation excerpt

From my `Program.cs`:

```csharp
webApplication.MapGet("/api/health", program.Health);

webApplication.MapPost(
    "/api/session/create", program.CreateSession);

webApplication.MapPost(
    "/api/session/checkpoint", program.Checkpoint);

webApplication.MapPost(
    "/api/session/finish", program.Finish);
```

## Session Lifecycle

When a multiplayer match is prepared:

1. The host requests a new Arbiter session.
2. The backend creates a session ID and token.
3. The session information is shared with both clients through PurrNet.
4. Both clients use the session information when reporting checkpoints and final results.

The backend associates the session with the participating Steam IDs.

Sessions currently expire after two hours and are stored in memory.

## Checkpoint Verification

The `ArbiterRollbackReporter` retrieves historical rollback states and generates SHA-256 hashes from selected gameplay values.

These values include:

- Player positions and movement-related state
- Health and attack state
- Hitstun and hit-reaction information
- Round and match state
- Simulation frame information

The current reporter configuration uses a checkpoint interval of 60 frames and a 240-frame reporting delay.

The reporting delay allows time for delayed inputs to arrive and for rollback corrections to update historical states before checkpoint submission.

When both clients submit a checkpoint for the same frame, the backend compares their hashes.

If the hashes differ, the session is flagged with a mismatch reason and frame number.

## Final Result Verification

At the end of a match, each client submits a report containing:

- Reporting player ID
- Winner ID
- Round scores
- Final rollback frame
- Final state hash

The backend waits until both players have submitted their results.

The current verification decision compares the reported winner, round scores, and final state hashes, while also checking for previously detected checkpoint mismatches.

Depending on the reports, the service returns one of three main outcomes:

- **waiting** — The other player's final report has not arrived.
- **verified** — The checked results agree and no checkpoint mismatch was recorded.
- **flagged** — A disagreement was detected.

The Unity client can resubmit its final report while the backend is waiting for the second player's report.

## Mismatch Testing

I created developer tools to deliberately modify submitted checkpoint or final-result hashes.

This makes it possible to test whether the backend detects inconsistencies without having to produce a real gameplay desynchronization.

The tools include:

- Forced checkpoint mismatch testing
- Forced final-result mismatch testing
- In-game Arbiter response logs
- Checkpoint and verification diagnostics

These features helped validate the expected reporting and mismatch-detection flow during development.

## Technical Decisions

**Separate backend service**

The Arbiter runs independently of the Unity game clients, keeping match reporting separate from gameplay simulation.

**Historical checkpoint reporting**

Checkpoint hashes are generated from saved rollback states rather than relying only on the newest simulated frame.

**Per-session synchronization**

The backend uses locking around session data operations to coordinate updates from multiple HTTP requests.

**Lightweight verification**

The service compares client-submitted information without simulating the full match itself.

## Limitations and Future Improvements

The Arbiter was developed as an academic prototype and has several deliberate limitations:

- It is not a fully authoritative server.
- It does not independently execute the gameplay simulation.
- Session data is stored in memory and is lost when the service restarts.
- Client identity and report integrity require stronger protection for a production environment.
- Validation, transport security, persistence, and final-report checks could be hardened further.

The system demonstrates external match-consistency checking, rather than comprehensive anti-cheat protection.

## Technologies

- C#
- ASP.NET Core Minimal API
- HTTP and JSON
- SHA-256
- UnityWebRequest
- PurrNet
- Steamworks
- VPS deployment

---

**My contribution:** Backend architecture, session management, checkpoint comparison, final-result verification, Unity integration, and developer testing tools.

**Project:** [Skill Club on Steam](https://store.steampowered.com/app/4379690/Skill_Club/)
