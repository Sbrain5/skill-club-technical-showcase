
# Rollback Netcode — Technical Deep Dive

## Overview

I implemented a custom rollback simulation layer for Skill Club, a Unity-based 1v1 online fighting game.

The goal was to preserve responsive controls under network latency by predicting unavailable remote inputs and correcting the simulation when actual inputs arrived.

The rollback layer operates separately from the gameplay simulation and visual presentation systems.

## Core Responsibilities

The system manages:

- Frame-indexed local and remote inputs
- Remote input prediction
- Historical game-state snapshots
- Detection of incorrect predictions
- Rollback and frame resimulation
- Bounded history storage
- Debug information for predicted and corrected inputs

## Remote Input Prediction

When a remote input is unavailable for the current simulation frame, the system generates a predicted input.

The prediction retains the most recently received movement direction, while attack buttons default to `None`.

This avoids repeatedly predicting attack presses.

If the actual input later differs from the prediction, that frame is marked for rollback.

## Rollback and Resimulation

The simulation stores a copy of the game state before advancing each frame.

When an incorrect prediction is detected, the system:

1. Identifies the earliest frame requiring correction.
2. Restores the saved state from that frame.
3. Replays the simulation using the corrected input history.
4. Continues until it reaches the previously simulated frame.

### Implementation excerpt

The following excerpt is from my `GameSimulation.cs` implementation:

```csharp
private void RunRollback()
{
    int rollbackFrame = GetEarliestRollbackFrame();

    if (!stateHistory.ContainsKey(rollbackFrame))
    {
        rollbackFrames.Clear();
        return;
    }

    int targetFrame = currentFrame;

    currentState = new GameState(stateHistory[rollbackFrame]);
    currentFrame = rollbackFrame;
    rollbackFrames.Clear();
    rollbackCount++;

    while (currentFrame < targetFrame)
    {
        stateHistory[currentFrame] = new GameState(currentState);

        currentState = currentState.Tick(
            GetLocalInput(currentFrame),
            GetRemoteInput(currentFrame)
        );

        currentFrame++;
    }

    CleanOldData();
}
```

This implementation reuses the gameplay simulation's `Tick()` method rather than implementing separate logic for replayed frames.

## Network Tick Integration

The `GameManager` uses PurrNet's synchronized network tick as the target simulation frame.

The local client advances its rollback simulation until it catches up with that target.

An input delay of three frames is used in the current configuration to give remote inputs additional time to arrive.

## Gameplay Integration

The rollback system calls `GameState.Tick()` to advance gameplay.

The gameplay state contains movement, health, combat and round information for both fighters.

After simulation, the `GameManager` passes the resulting state to each player's presentation components.

This separates gameplay-state calculations from Unity animations and visual smoothing.

The gameplay simulation and combat implementation were developed collaboratively, with core gameplay and combat work handled by Rafail-Angelos Savvakis.

## Debugging and Testing

I developed a rollback input viewer that displays:

- Inputs for both players
- Predicted remote inputs
- Inputs corrected after prediction
- Frame numbers and rollback counts

I also implemented tools for introducing artificial network delay and observing the resulting simulation behavior.

## Trade-offs and Limitations

The system maintains a limited history window rather than retaining every frame indefinitely.

Rollback correction depends on the required historical state still being available.

The implementation was developed as part of a university game project and should not be presented as a production-proven networking framework.

---

**My contribution:** Rollback architecture, input prediction, state history management, correction and resimulation, network integration, and debugging tools.

**Project:** [Skill Club on Steam](https://store.steampowered.com/app/4379690/Skill_Club/)
