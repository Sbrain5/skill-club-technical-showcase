using System.Collections;
using PurrNet.StateMachine;

/// <summary>
/// waits until the match reaches the final state
/// </summary>
public class MatchRunningState : StateNode
{
    #region variables
    private bool started;
    #endregion

    #region state methods
    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (!asServer || started)
        {
            return;
        }

        started = true;
        StartCoroutine(WaitForMatchOver());
    }
    #endregion

    #region match methods
    /// <summary>
    /// waits until the server has a final state
    /// </summary>
    /// <returns>coroutine</returns>
    private IEnumerator WaitForMatchOver()
    {
        GameManager gameManager = null;
        ArbiterSessionShare sessionShare = null;

        // waits until the needed scripts exist
        while (gameManager == null || sessionShare == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
            sessionShare = FindFirstObjectByType<ArbiterSessionShare>();
            yield return null;
        }

        // waits until rollback saves the final match state on the server side
        while (!gameManager.HasFinalState())
        {
            yield return null;
        }

        sessionShare.FinishMatchForEveryone(); // stops checkpoint reporting on everyone before finish state starts
        started = false;
        machine.Next();
    }
    #endregion
}