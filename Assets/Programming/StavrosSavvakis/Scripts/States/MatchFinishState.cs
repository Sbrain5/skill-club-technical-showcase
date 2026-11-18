using PurrLobby;
using PurrNet.StateMachine;
using Steamworks;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// sends final match result to the arbiter
/// </summary>
public class MatchFinishState : StateNode
{
    #region variables
    private bool finishSent;
    #endregion

    #region state methods
    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (finishSent)
        {
            return;
        }

        finishSent = true;
        StartCoroutine(SendFinish()); // send the final match result to the arbiter
    }
    #endregion

    #region finish methods
    /// <summary>
    /// sends this client's final result
    /// </summary>
    /// <returns>coroutine</returns>
    private IEnumerator SendFinish()
    {
        GameManager gameManager = null;
        ArbiterClient arbiterClient = null;
        LobbyDataHolder lobbyDataHolder = null;
        ArbiterForceMismatch forceMismatch = null;

        // waits until the needed scene scripts exist
        while (gameManager == null || arbiterClient == null || lobbyDataHolder == null || forceMismatch == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
            arbiterClient = FindFirstObjectByType<ArbiterClient>();
            lobbyDataHolder = FindFirstObjectByType<LobbyDataHolder>();
            forceMismatch = FindFirstObjectByType<ArbiterForceMismatch>();
            yield return null;
        }

        // waits until rollback has saved the first final match state
        while (!gameManager.HasFinalState())
        {
            yield return null;
        }

        GameState finalState = gameManager.GetFinalState();

        if (finalState == null)
        {
            yield break;
        }

        if (lobbyDataHolder.CurrentLobby.Members.Count < 2)
        {
            yield break;
        }

        int finalRollbackFrame = gameManager.GetFinalRollbackFrame();
        ulong winnerId = GetWinnerSteamId(finalState, lobbyDataHolder);

        if (winnerId == 0)
        {
            yield break;
        }

        string finalStateHash = MakeStateHash(finalState, finalRollbackFrame); // creates the normal final hash first

        if (forceMismatch.IsFinalMismatchEnabled())
        {
            finalStateHash = forceMismatch.ChangeFinalHash(finalStateHash, SteamUser.GetSteamID().m_SteamID); // changes the final hash sent to the arbiter
            ArbiterLogView.AddLog("the final hash was forced to change by the mismatch tool");
        }

        ArbiterLogView.AddLog("finish: final frame " + finalRollbackFrame + " hash " + finalStateHash);
        arbiterClient.SendFinish(SteamUser.GetSteamID().m_SteamID, winnerId, finalState.Player01RoundWins, finalState.Player02RoundWins, finalRollbackFrame, finalStateHash);
    }

    /// <summary>
    /// returns winner steam id
    /// </summary>
    /// <param name="state">final game state</param>
    /// <param name="lobbyDataHolder">lobby data holder</param>
    /// <returns>winner steam id</returns>
    private ulong GetWinnerSteamId(GameState state, LobbyDataHolder lobbyDataHolder)
    {
        if (state.MatchWinnerSlot == 1)
        {
            return ulong.Parse(lobbyDataHolder.CurrentLobby.Members[0].Id); // player one is stored as the first lobby member
        }

        if (state.MatchWinnerSlot == 2)
        {
            return ulong.Parse(lobbyDataHolder.CurrentLobby.Members[1].Id); // player two is stored as the second lobby member
        }

        return 0;
    }
    #endregion

    #region hash methods
    /// <summary>
    /// creates final state hash
    /// </summary>
    /// <param name="state">final game state</param>
    /// <param name="rollbackFrame">final rollback frame</param>
    /// <returns>final state hash</returns>
    private string MakeStateHash(GameState state, int rollbackFrame)
    {
        return MakeHash(GetMatchText(state) + "|" + GetPlayerText(state.Player01) + "|" + GetPlayerText(state.Player02)); // final state hash is made from match text and both player texts
    }

    /// <summary>
    /// creates text from player state
    /// </summary>
    /// <param name="player">player state</param>
    /// <returns>player text</returns>
    private string GetPlayerText(SimPlayerState player)
    {
        return player.MaxHealth + "|" + player.CurrentHealth; // final player values
    }

    /// <summary>
    /// creates text from match state
    /// </summary>
    /// <param name="state">game state</param>
    /// <returns>match text</returns>
    private string GetMatchText(GameState state)
    {
        return (int)state.MatchState + "|" + state.RoundWinnerSlot + "|" + state.MatchWinnerSlot + "|" + state.Player01RoundWins + "|" + state.Player02RoundWins; // final match values
    }

    /// <summary>
    /// creates hash from text
    /// </summary>
    /// <param name="text">text value</param>
    /// <returns>hash text</returns>
    private string MakeHash(string text)
    {
        using (SHA256 hashMaker = SHA256.Create())
        {
            return MakeHashText(hashMaker.ComputeHash(Encoding.UTF8.GetBytes(text))); // converts the text into sha256 bytes and then into hex text
        }
    }

    /// <summary>
    /// makes hash text from bytes
    /// </summary>
    /// <param name="bytes">hash bytes</param>
    /// <returns>hash text</returns>
    private string MakeHashText(byte[] bytes)
    {
        return System.BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant(); // removes the dash symbols from the hex text
    }
    #endregion
}