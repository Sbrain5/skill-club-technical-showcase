using System.Collections;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;
using static InputSerialization;

/// <summary>
/// purrnet bridge that sends inputs to the rollback simulation
/// </summary>
public class RollbackBridge : NetworkBehaviour
{
    #region variables
    private GameSimulation gameSimulation;
    private bool playerOneLocal;
    private bool rollbackbridgeStarted;
    private int remoteInputCount;
    private int lastRemoteInputFrame;
    private FrameInput lastRemoteInputForTool;
    private bool hasLastRemoteInputForTool;
    private RollbackPing pingTool;
    private float nextPingTime;
    private float pingRate = 1f;
    #endregion

    #region methods
    /// <summary>
    /// starts the bridge for one side
    /// </summary>
    /// <param name="isPlayerOneLocal">true if local player is player one</param>
    /// <param name="startFrame">shared start frame</param>
    public void StartRollbackBridge(bool isPlayerOneLocal, int startFrame)
    {
        playerOneLocal = isPlayerOneLocal;
        gameSimulation = new GameSimulation(playerOneLocal, startFrame);
        remoteInputCount = 0;
        lastRemoteInputFrame = -1;
        hasLastRemoteInputForTool = false;
        rollbackbridgeStarted = true;
        pingTool = FindFirstObjectByType<RollbackPing>();
        nextPingTime = Time.realtimeSinceStartup + pingRate;
    }

    /// <summary>
    /// starts a new match
    /// </summary>
    /// <param name="startFrame">shared start frame</param>
    public void StartNewMatch(int startFrame)
    {
        remoteInputCount = 0;
        lastRemoteInputFrame = -1;
        hasLastRemoteInputForTool = false;

        if (rollbackbridgeStarted)
        {
            gameSimulation.StartNewMatch(startFrame); // reset the current simulation for a new match
            return;
        }

        gameSimulation = new GameSimulation(playerOneLocal, startFrame); // create the simulation the first time
        rollbackbridgeStarted = true;
    }

    /// <summary>
    /// sends one local input frame
    /// </summary>
    /// <param name="input">frame input</param>
    public void SendLocalInput(FrameInput input)
    {
        if (!rollbackbridgeStarted)
        {
            return;
        }

        gameSimulation.SaveLocalInput(input); // save the local input for this frame
        SendInputToServer(input, playerOneLocal); // send the same input to the other side
    }

    /// <summary>
    /// runs one simulation frame
    /// </summary>
    public void RunFrame()
    {
        if (!rollbackbridgeStarted)
        {
            return;
        }

        gameSimulation.RunFrame(); // move the rollback simulation forward by one frame

        // sends a ping request every few seconds
        if (Time.realtimeSinceStartup < nextPingTime)
        {
            return;
        }

        nextPingTime = Time.realtimeSinceStartup + pingRate;
        SendPingToServer(Time.realtimeSinceStartup, playerOneLocal);
    }

    /// <summary>
    /// saves remote input into rollback
    /// </summary>
    /// <param name="input">frame input</param>
    private void SaveRemoteInputForRollback(FrameInput input)
    {
        if (!rollbackbridgeStarted)
        {
            return;
        }

        gameSimulation.SaveRemoteInput(input); // save the real remote input and update prediction result

        // checks if the received input is different from the last remote input
        bool inputChanged = !hasLastRemoteInputForTool || lastRemoteInputForTool.direction != input.direction || lastRemoteInputForTool.punch != input.punch || lastRemoteInputForTool.kick != input.kick || lastRemoteInputForTool.heavyAttack != input.heavyAttack;
        bool inputHasValue = input.direction != DirectionInput.Neutral && input.direction != DirectionInput.Unknown;

        if (input.punch != ButtonState.None || input.kick != ButtonState.None || input.heavyAttack != ButtonState.None)
        {
            inputHasValue = true;
        }

        lastRemoteInputForTool = input;
        hasLastRemoteInputForTool = true;

        // only update the input received tool when a useful input actually changed
        if (!inputChanged || !inputHasValue)
        {
            return;
        }

        remoteInputCount++;
        lastRemoteInputFrame = input.frame;
    }

    /// <summary>
    /// saves remote input after a delay
    /// </summary>
    /// <param name="input">frame input</param>
    /// <param name="delay">delay seconds</param>
    /// <returns>coroutine</returns>
    private IEnumerator SaveRemoteInputLater(FrameInput input, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        SaveRemoteInputForRollback(input);
    }

    /// <summary>
    /// returns current game state
    /// </summary>
    /// <returns>game state</returns>
    public GameState GetCurrentState()
    {
        if (rollbackbridgeStarted)
        {
            return gameSimulation.GetCurrentState(); // return the latest simulated state
        }

        return null;
    }

    /// <summary>
    /// returns saved state for one rollback frame
    /// </summary>
    /// <param name="frame">rollback frame</param>
    /// <returns>saved game state or null</returns>
    public GameState GetStateForRollbackFrame(int frame)
    {
        if (rollbackbridgeStarted)
        {
            return gameSimulation.GetStateForRollbackFrame(frame); // return the saved state for this rollback frame
        }

        return null;
    }

    /// <summary>
    /// returns current frame
    /// </summary>
    /// <returns>frame number</returns>
    public int GetCurrentFrame()
    {
        if (rollbackbridgeStarted)
        {
            return gameSimulation.GetCurrentFrame(); // return the current rollback frame
        }

        return -1;
    }

    /// <summary>
    /// returns rollback count
    /// </summary>
    /// <returns>rollback count</returns>
    public int GetRollbackCount()
    {
        if (rollbackbridgeStarted)
        {
            return gameSimulation.GetRollbackCount(); // return how many rollback corrections happened
        }

        return 0;
    }

    /// <summary>
    /// returns received remote input count
    /// </summary>
    /// <returns>remote input count</returns>
    public int GetRemoteInputCount()
    {
        if (rollbackbridgeStarted)
        {
            return remoteInputCount; // return how many remote inputs were received since the last match started
        }

        return 0;
    }

    /// <summary>
    /// returns last remote input frame for the tool
    /// </summary>
    /// <returns>last remote input frame</returns>
    public int GetLastRemoteInputFrame()
    {
        if (rollbackbridgeStarted)
        {
            return lastRemoteInputFrame; // return the last remote input frame received since the last match started
        }

        return -1;
    }

    /// <summary>
    /// returns rollback viewer frames
    /// </summary>
    /// <returns>frame list</returns>
    public List<RollbackFrameInfo> GetDebugFrames()
    {
        if (rollbackbridgeStarted)
        {
            return gameSimulation.GetDebugFrames(); // return the rollback frames for the rollback viewer
        }

        return new List<RollbackFrameInfo>();
    }
    #endregion

    #region rpc methods
    /// <summary>
    /// sends one input frame to the server
    /// </summary>
    /// <param name="input">frame input</param>
    /// <param name="sentByPlayerOne">true if input came from player one</param>
    [ServerRpc(requireOwnership: false)]
    private void SendInputToServer(FrameInput input, bool sentByPlayerOne)
    {
        SendInputToClients(input, sentByPlayerOne); // server sends the input to all clients
    }

    /// <summary>
    /// sends one input frame to all clients
    /// </summary>
    /// <param name="input">frame input</param>
    /// <param name="sentByPlayerOne">true if input came from player one</param>
    [ObserversRpc]
    private void SendInputToClients(FrameInput input, bool sentByPlayerOne)
    {
        if (!rollbackbridgeStarted || sentByPlayerOne == playerOneLocal)
        {
            return;
        }

        if (pingTool == null)
        {
            pingTool = FindFirstObjectByType<RollbackPing>();
        }

        if (pingTool == null || pingTool.GetExtraPingMilliseconds() <= 0)
        {
            SaveRemoteInputForRollback(input); // saves the remote input normally
            return;
        }

        StartCoroutine(SaveRemoteInputLater(input, pingTool.GetExtraPingSeconds())); // adds test delay before saving remote input
    }

    /// <summary>
    /// sends ping request to the server
    /// </summary>
    /// <param name="sendTime">time when ping was sent</param>
    /// <param name="sentByPlayerOne">true if player one sent it</param>
    [ServerRpc(requireOwnership: false)]
    private void SendPingToServer(float sendTime, bool sentByPlayerOne)
    {
        SendPingToClients(sendTime, sentByPlayerOne); // server sends the ping request to all clients
    }

    /// <summary>
    /// sends ping request to the other player
    /// </summary>
    /// <param name="sendTime">time when ping was sent</param>
    /// <param name="sentByPlayerOne">true if player one sent it</param>
    [ObserversRpc(runLocally: true)]
    private void SendPingToClients(float sendTime, bool sentByPlayerOne)
    {
        if (!rollbackbridgeStarted || sentByPlayerOne == playerOneLocal)
        {
            return;
        }

        SendPongToServer(sendTime, sentByPlayerOne); // the other player sends the answer back
    }

    /// <summary>
    /// sends ping answer to the server
    /// </summary>
    /// <param name="sendTime">time when ping was sent</param>
    /// <param name="originalPlayerOne">true if player one started the ping</param>
    [ServerRpc(requireOwnership: false)]
    private void SendPongToServer(float sendTime, bool originalPlayerOne)
    {
        SendPongToClients(sendTime, originalPlayerOne); // server sends the answer back to the player that started the ping
    }

    /// <summary>
    /// sends ping answer back to the first player
    /// </summary>
    /// <param name="sendTime">time when ping was sent</param>
    /// <param name="originalPlayerOne">true if player one started the ping</param>
    [ObserversRpc(runLocally: true)]
    private void SendPongToClients(float sendTime, bool originalPlayerOne)
    {
        if (!rollbackbridgeStarted || originalPlayerOne != playerOneLocal)
        {
            return;
        }

        if (pingTool == null)
        {
            pingTool = FindFirstObjectByType<RollbackPing>();
        }

        if (pingTool == null)
        {
            return;
        }

        pingTool.SetPing(Mathf.RoundToInt((Time.realtimeSinceStartup - sendTime) * 1000f)); // calculates the time from ping request until pong answer
    }
    #endregion
}