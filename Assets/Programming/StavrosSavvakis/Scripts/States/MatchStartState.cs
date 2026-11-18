using System.Collections;
using PurrNet.StateMachine;
using UnityEngine;

/// <summary>
/// waits for players and arbiter session before starting the match
/// </summary>
public class MatchStartState : StateNode
{
    #region variables
    private bool started;
    #endregion

    #region state methods
    /// <summary>
    /// starts this state
    /// </summary>
    /// <param name="asServer">true if server</param>
    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (!asServer || started)
        {
            return;
        }

        started = true;
        StartCoroutine(StartMatch());
    }
    #endregion

    #region match methods
    /// <summary>
    /// waits until the match can start
    /// </summary>
    /// <returns>coroutine</returns>
    private IEnumerator StartMatch()
    {
        ArbiterSessionShare share = null;

        // wait for players and arbiter session
        while (share == null || networkManager == null || networkManager.players == null || networkManager.players.Count < 2 || !share.HasSessionData())
        {
            share = FindFirstObjectByType<ArbiterSessionShare>();
            yield return null;
        }

        share.StartMatchForEveryone();
        machine.Next();
    }
    #endregion
}