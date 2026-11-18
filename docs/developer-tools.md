
# Developer Tools & Technical Demonstrations

## Overview

During the development of **Skill Club**, a Unity 6 online 1v1 fighting game, I implemented a collection of in-game debugging and testing tools to support the development of my custom rollback netcode and ASP.NET Core match verification backend.

These tools allow me to inspect frame-based player inputs, simulate network latency, monitor rollback corrections, and test the Arbiter verification workflow.

The following five videos demonstrate these tools in action.

---

## 1. Rollback Netcode Input Viewer

**[Watch the Rollback Input Viewer Demo](https://youtu.be/H-txEZ7GAmU)**

The Rollback Input Viewer displays both players' gameplay inputs frame by frame.

It highlights predicted remote inputs and marks frames where a prediction differs from the actual received input.

### Features

- Frame-by-frame local and remote input visualization.
- Predicted input markers.
- Changed input markers indicating incorrect predictions.
- Real-time visualization.
- Timed refresh for easier frame inspection.
- Input-received mode for examining individual frames.

This tool helps visualize how remote input prediction and rollback corrections behave during multiplayer gameplay.

**Related documentation:** [Rollback Netcode Technical Deep Dive](rollback-netcode.md)

---

## 2. Network Latency & Rollback Testing

**[Watch the Network Latency Testing Demo](https://youtu.be/UjyOPIi2Rb4)**

The Network Latency Testing Tool allows me to introduce artificial network delay and observe its effects on rollback behavior.

### Features

- Real connection ping display.
- Additional simulated latency display.
- Combined current ping display.
- Rollback counter.
- Artificial delay adjustments in 50 ms increments.
- Reset function for removing additional delay.

By introducing controlled latency, I can test how delayed remote inputs affect prediction and rollback corrections.

**Related documentation:** [Rollback Netcode Technical Deep Dive](rollback-netcode.md)

---

## 3. Arbiter Mismatch Detection

**[Watch the Arbiter Mismatch Detection Demo](https://youtu.be/1A-h0easEIE)**

The Arbiter Mismatch Detection Tool allows me to deliberately modify the hashes submitted to the external match verification backend.

### Features

- Forced checkpoint hash mismatches.
- Forced final-result hash mismatches.
- Controlled testing of backend mismatch detection.
- Verification of expected error-reporting behavior.

These tests demonstrate how the backend responds when the two clients submit inconsistent verification data.

The Arbiter is a lightweight match-consistency verification service, not a fully authoritative game server or comprehensive anti-cheat solution.

**Related documentation:** [Arbiter Backend Technical Deep Dive](arbiter-backend.md)

---

## 4. Arbiter Backend Log Viewer

**[Watch the Arbiter Backend Log Viewer Demo](https://youtu.be/bNKAj5oAxHw)**

The Arbiter Log Viewer provides an in-game interface for monitoring communication between Unity clients and the ASP.NET Core backend.

### Features

- Session creation and session-sharing logs.
- Checkpoint submission and response logs.
- Final result submission and response logs.
- Timestamped log entries.
- Scrollable log history.
- Clear function.
- Mismatch diagnostics.

The tool displays verification states including:

- `waiting` — The backend is waiting for the other client's final report.
- `verified` — The checked match results agree.
- `flagged` — A verification inconsistency has been detected.

This tool makes backend communication and verification results visible directly inside the game.

**Related documentation:** [Arbiter Backend Technical Deep Dive](arbiter-backend.md)

---

## 5. In-Game Developer Tools Overview

**[Watch the Developer Tools Overview](https://youtu.be/QqpHVvJw9JQ)**

The Tool Manager provides a centralized interface for accessing the custom debugging tools during gameplay.

Pressing the Home key opens the developer tools interface.

From this interface, I can access:

- Rollback Input Viewer.
- Network Latency Testing Tool.
- Arbiter Backend Log Viewer.
- Arbiter Mismatch Detection Tool.

Each tool panel can be toggled independently.

This interface makes testing and monitoring multiplayer systems more accessible during development.

---

## Why I Built These Tools

Rollback netcode and distributed match verification can be difficult to debug because important events are not always visible during normal gameplay.

I developed these tools to make the underlying systems observable and easier to test.

They helped me:

- Inspect frame-based input behavior.
- Identify incorrect remote input predictions.
- Test rollback under artificial latency.
- Verify that intentionally mismatched hashes were detected.
- Monitor communication with the external backend.

Together, these demonstrations showcase my practical experience developing, testing, and debugging multiplayer systems.

---

## Technologies

- Unity 6
- C#
- Unity Input System
- PurrNet
- Steamworks
- Rollback Netcode
- ASP.NET Core Minimal API
- HTTP/JSON
- SHA-256

## Project Credits

**My Role:** Multiplayer Programming, Rollback Netcode, Backend Integration, Match Verification & Developer Tools.

Skill Club is a collaborative game project developed with **Rafail-Angelos Savvakis**.

**[Skill Club on Steam](https://store.steampowered.com/app/4379690/Skill_Club/)**
