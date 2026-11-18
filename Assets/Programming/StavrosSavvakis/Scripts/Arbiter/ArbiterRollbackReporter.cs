using System.Collections;
using System.Security.Cryptography;
using System.Text;
using Steamworks;
using UnityEngine;

/// <summary>
/// sends rollback state checkpoints to the arbiter
/// </summary>
public class ArbiterRollbackReporter : MonoBehaviour
{
    #region variables
    [SerializeField] private int checkpointStep = 60;
    [SerializeField] private int checkpointDelayFrames = 240;
    [SerializeField] private bool showLogs;
    private RollbackBridge rollbackBridge;
    private ArbiterClient arbiterClient;
    private ArbiterSessionShare arbiterSessionShare;
    private ArbiterForceMismatch forceMismatch;
    private bool matchStarted;
    private int lastCheckpointFrame;
    private int nextCheckpointFrame;
    #endregion

    #region unity methods
    private void Start()
    {
        StartCoroutine(FindScripts());
    }

    private void OnDestroy()
    {
        if (arbiterSessionShare != null)
        {
            arbiterSessionShare.RemoveMatchStartListener(OnMatchStarted);
            arbiterSessionShare.RemoveMatchFinishListener(OnMatchFinished);
        }
    }

    private void Update()
    {
        if (!matchStarted)
        {
            return;
        }

        GameState currentState = rollbackBridge.GetCurrentState(); // get the current rollback simulation state

        // stop checkpoint reporting when the match is over
        if (currentState != null && currentState.MatchState == SimMatchState.MatchOver)
        {
            OnMatchFinished();
            return;
        }

        TrySendCheckpoint(); // try to send the next checkpoint when enough frames have passed
    }
    #endregion

    #region setup methods
    /// <summary>
    /// finds needed scripts
    /// </summary>
    /// <returns>coroutine</returns>
    private IEnumerator FindScripts()
    {
        while (rollbackBridge == null || arbiterClient == null || arbiterSessionShare == null || forceMismatch == null)
        {
            // keeps searching because some network objects may spawn later
            rollbackBridge = FindFirstObjectByType<RollbackBridge>();
            arbiterClient = FindFirstObjectByType<ArbiterClient>();
            arbiterSessionShare = FindFirstObjectByType<ArbiterSessionShare>();
            forceMismatch = FindFirstObjectByType<ArbiterForceMismatch>();
            yield return null;
        }

        arbiterSessionShare.AddMatchStartListener(OnMatchStarted); // listener is added after everything exists so checkpoint reporting can start correctly
        arbiterSessionShare.AddMatchFinishListener(OnMatchFinished); // listener is added after everything exists so checkpoint reporting can stop correctly
        Log("reporter ready");
    }

    /// <summary>
    /// starts checkpoint reporting
    /// </summary>
    private void OnMatchStarted()
    {
        if (forceMismatch == null)
        {
            forceMismatch = FindFirstObjectByType<ArbiterForceMismatch>();
        }

        if (forceMismatch != null)
        {
            forceMismatch.ResetTests(); // stops old local test values from staying on in the next match
        }

        matchStarted = true;
        lastCheckpointFrame = -1;
        nextCheckpointFrame = -1; // first checkpoint starts after the checkpoint step
        Log("checkpoint reporting started");
    }

    /// <summary>
    /// stops checkpoint reporting
    /// </summary>
    private void OnMatchFinished()
    {
        matchStarted = false;
        Log("checkpoint reporting stopped");
    }
    #endregion

    #region checkpoint methods
    /// <summary>
    /// sends checkpoint if the current rollback frame is ready
    /// </summary>
    private void TrySendCheckpoint()
    {
        int currentRollbackFrame = rollbackBridge.GetCurrentFrame();

        if (currentRollbackFrame <= 0)
        {
            return;
        }

        // prepare the first checkpoint based on the real rollback start area
        if (nextCheckpointFrame < 0)
        {
            nextCheckpointFrame = ((currentRollbackFrame / checkpointStep) + 1) * checkpointStep;
        }

        int availableRollbackFrame = currentRollbackFrame - checkpointDelayFrames;
        int checkpointRollbackFrame = nextCheckpointFrame;

        if (availableRollbackFrame <= 0 || availableRollbackFrame < checkpointRollbackFrame || checkpointRollbackFrame == lastCheckpointFrame)
        {
            return;
        }

        GameState checkpointState = rollbackBridge.GetStateForRollbackFrame(checkpointRollbackFrame);

        if (checkpointState == null)
        {
            nextCheckpointFrame += checkpointStep;
            return;
        }

        string stateText = checkpointRollbackFrame + "|" + checkpointState.Frame + "|" + GetMatchText(checkpointState) + "|" + GetPlayerText(checkpointState.Player01) + "|" + GetPlayerText(checkpointState.Player02);
        string stateHash = MakeHash(stateText);

        if (forceMismatch == null)
        {
            forceMismatch = FindFirstObjectByType<ArbiterForceMismatch>();
        }

        if (forceMismatch != null && forceMismatch.IsCheckpointMismatchEnabled())
        {
            stateHash = forceMismatch.ChangeCheckpointHash(stateHash, SteamUser.GetSteamID().m_SteamID);
            Log("the checkpoint hash was forced to change by the mismatch tool");
        }

        lastCheckpointFrame = checkpointRollbackFrame;
        nextCheckpointFrame += checkpointStep;
        arbiterClient.SendCheckpoint(checkpointRollbackFrame, SteamUser.GetSteamID().m_SteamID, stateHash);
        Log("checkpoint sent rollback frame " + checkpointRollbackFrame + " game frame " + checkpointState.Frame + " hash " + stateHash);
    }

    /// <summary>
    /// creates text from player state
    /// </summary>
    /// <param name="player">player state</param>
    /// <returns>player text</returns>
    private string GetPlayerText(SimPlayerState player)
    {
               // position and movement values
        return player.Position.x.GetHashCode() + "|" + player.Position.y.GetHashCode() + "|" + player.PositionZ.GetHashCode() + "|" + player.VerticalVelocity.GetHashCode() + "|" + player.AirHorizontalDirection + "|" +
               // movement flags
               GetBoolNumber(player.WasJumpHeldLastFrame) + "|" + GetBoolNumber(player.MustReleaseHorizontalAfterLanding) + "|" + GetBoolNumber(player.IsGrounded) + "|" + GetBoolNumber(player.IsFacingRight) + "|" + (int)player.MovementState + "|" +
               // health and combat values
               player.MaxHealth + "|" + player.CurrentHealth + "|" + (int)player.AttackState + "|" + player.AttackTick + "|" + GetBoolNumber(player.IsAttackHitboxActive) + "|" + player.ActiveHitWindowId + "|" +
               // hit reaction values
               GetBoolNumber(player.IsInHitstun) + "|" + player.HitstunTicksRemaining + "|" + player.LastReceivedHitWindowId + "|" + player.LastReceivedAttackStartFrame + "|" +
               // step movement values
               player.StepTicksRemaining + "|" + player.StepDirection + "|" + player.StepRecoveryTicksRemaining + "|" +
               GetBoolNumber(player.RequiresMoveReleaseBeforeNextStep) + "|" + player.StepVisualLockoutTicksRemaining;
    }

    /// <summary>
    /// creates text from match state
    /// </summary>
    /// <param name="state">game state</param>
    /// <returns>match text</returns>
    private string GetMatchText(GameState state)
    {
        // keep the same value order on both players so the checkpoint hash can match
        return (int)state.MatchState + "|" + state.RoundWinnerSlot + "|" + state.MatchWinnerSlot + "|" + state.RoundEndTicksRemaining + "|" + state.RoundTimerTicksRemaining + "|" + GetBoolNumber(state.RoundEndedByTimeUp) + "|" +
               state.Player01RoundWins + "|" + state.Player02RoundWins;
    }
    #endregion

    #region hash methods
    /// <summary>
    /// creates hash from text
    /// </summary>
    /// <param name="text">text value</param>
    /// <returns>hash text</returns>
    private string MakeHash(string text)
    {
        // converts the checkpoint text into a sha256 hash
        using (SHA256 hashMaker = SHA256.Create())
        {
            return MakeHashText(hashMaker.ComputeHash(Encoding.UTF8.GetBytes(text)));
        }
    }

    /// <summary>
    /// makes hash text from bytes
    /// </summary>
    /// <param name="bytes">hash bytes</param>
    /// <returns>hash text</returns>
    private string MakeHashText(byte[] bytes)
    {
        return System.BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant(); // converts the hash bytes into lowercase hex text
    }

    /// <summary>
    /// gets number from bool
    /// </summary>
    /// <param name="value">bool value</param>
    /// <returns>1 if true, 0 if false</returns>
    private int GetBoolNumber(bool value)
    {
        // converts a bool into a number for easier text comparison
        if (value)
        {
            return 1;
        }

        return 0;
    }
    #endregion

    #region log methods
    /// <summary>
    /// writes one reporter message
    /// </summary>
    /// <param name="message">message text</param>
    private void Log(string message)
    {
        ArbiterLogView.AddLog("reporter: " + message); // always writes the message in the in-game arbiter log panel

        // also writes to unity console when extra logs are enabled
        if (showLogs)
        {
            Debug.Log("arbiter reporter: " + message);
        }
    }
    #endregion
}