
# Selected Code Highlights

## Overview

This page presents selected C# excerpts from my work on **Skill Club**, focusing on rollback simulation, input prediction, and external match verification.

The excerpts are taken from the actual project implementation and demonstrate specific engineering decisions.

They are intentionally limited to relevant sections rather than exposing the complete collaborative Unity project.

**Note:** These are contextual excerpts, not standalone compilable programs. They depend on other project classes and systems.

---

## 1. Remote Input Prediction

**Source:** `GameSimulation.cs`

When the actual remote input is not available, the rollback simulation predicts it using the last known movement direction.

Attack buttons are not carried forward, avoiding repeated predicted attack presses.

```csharp
FrameInput predictedInput = CreateNeutralInput(frame);

if (hasLastRemoteInput)
{
    predictedInput.direction = lastRemoteInput.direction;
    predictedInput.punch = ButtonState.None;
    predictedInput.kick = ButtonState.None;
    predictedInput.heavyAttack = ButtonState.None;
}

predictedInput.frame = frame;
remoteInputs[frame] = predictedInput;
predictedRemoteFrames.Add(frame);
```

### Engineering Decision

Rather than waiting for the network, the simulation can advance using a temporary input.

The system records which frames were predicted so their values can be checked when the actual inputs arrive.

---

## 2. Detecting Incorrect Predictions

**Source:** `GameSimulation.cs`

When an actual remote input arrives, the simulation checks whether that frame previously used a prediction.

```csharp
bool wasPredicted = predictedRemoteFrames.Contains(input.frame);

bool inputChanged = wasPredicted && remoteInputs.TryGetValue(input.frame, out savedInput) && !savedInput.IsSame(input);

if (inputChanged)
{
    rollbackFrames.Add(input.frame);
}
```

### Engineering Decision

Only predictions that differ from the received input need correction.

Frames with incorrect predictions are registered for rollback.

The simulation restores an earlier state and replays the affected frames through the same gameplay simulation logic.

**Full explanation:** [Rollback Netcode Technical Deep Dive](rollback-netcode.md)

**Video demonstration:** [Rollback Input Viewer](https://youtu.be/H-txEZ7GAmU)

---

## 3. Delayed Checkpoint Reporting

**Source:** `ArbiterRollbackReporter.cs`

The Arbiter reporter does not immediately hash the newest simulation frame.

Instead, it waits until an older frame becomes eligible for reporting.

```csharp
int currentRollbackFrame = rollbackBridge.GetCurrentFrame();

int availableRollbackFrame = currentRollbackFrame - checkpointDelayFrames;

int checkpointRollbackFrame = nextCheckpointFrame;

if (availableRollbackFrame <= 0 || availableRollbackFrame < checkpointRollbackFrame || checkpointRollbackFrame == lastCheckpointFrame)
{
    return;
}

GameState checkpointState = rollbackBridge.GetStateForRollbackFrame(checkpointRollbackFrame);
```

### Engineering Decision

Checkpoint reporting is intentionally delayed to allow rollback corrections to settle before historical gameplay states are hashed.

The current configuration uses:

- Checkpoint interval: 60 frames.
- Reporting delay: 240 frames.

The reporter generates a SHA-256 hash from selected simulation-state values and submits it to the external Arbiter backend.

---

## 4. Backend Checkpoint Comparison

**Source:** `Program.cs` — ASP.NET Core Arbiter

The Arbiter stores checkpoint hashes for each participating player.

When a checkpoint for the same frame has already been submitted by the other player, the backend compares their hashes.

```csharp
if (checkpoints.TryGetValue(otherPlayer, out otherPlayerCheckpoints) && otherPlayerCheckpoints.TryGetValue(frame, out otherPlayerHash) && otherPlayerHash != stateHash)
{
    SetMismatch("state_hash_mismatch", frame, player);
}
```

### Engineering Decision

The backend checks consistency between the clients' submitted gameplay-state hashes without running a separate copy of the game simulation.

When a disagreement is found, the session records the mismatch reason, frame, and associated reporting player.

Checkpoint updates are performed under a per-session lock to coordinate concurrent requests.

**Full explanation:** [Arbiter Backend Technical Deep Dive](arbiter-backend.md)

**Video demonstration:** [Arbiter Mismatch Detection](https://youtu.be/1A-h0easEIE)

---

## Architecture and Collaboration

My work focuses on the rollback infrastructure, multiplayer integration, checkpoint reporting, and Arbiter backend.

The rollback system interacts with `GameState.Tick()`, which belongs to the game's core gameplay simulation.

Core gameplay and combat systems were developed by my collaborator, **Rafail-Angelos Savvakis**, and are not reproduced in this showcase.

This separation allows the portfolio to demonstrate my personal engineering contributions while acknowledging the collaborative architecture.

---

## Further Reading

- [Rollback Netcode](rollback-netcode.md)
- [Arbiter Backend](arbiter-backend.md)
- [Developer Tools & Demonstrations](developer-tools.md)
- [Skill Club on Steam](https://store.steampowered.com/app/4379690/Skill_Club/)
