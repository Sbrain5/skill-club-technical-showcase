using System;
using PurrNet;

/// <summary>
/// shares arbiter session and match start over the network
/// </summary>
public class ArbiterSessionShare : NetworkBehaviour
{
    #region variables
    private event Action<string, string> sessionReceived;
    private event Action matchStarted;
    private event Action matchFinished;
    private bool hasSession;
    private bool hasMatchStarted;
    private bool hasMatchFinished;
    private string sessionId;
    private string sessionToken;
    #endregion

    #region listeners
    /// <summary>
    /// adds session listener
    /// </summary>
    /// <param name="listener">session listener</param>
    public void AddListener(Action<string, string> listener)
    {
        sessionReceived += listener;

        if (hasSession && listener != null)
        {
            listener(sessionId, sessionToken);
        }
    }

    /// <summary>
    /// removes session listener
    /// </summary>
    /// <param name="listener">session listener</param>
    public void RemoveListener(Action<string, string> listener)
    {
        sessionReceived -= listener;
    }

    /// <summary>
    /// adds match start listener
    /// </summary>
    /// <param name="listener">match start listener</param>
    public void AddMatchStartListener(Action listener)
    {
        matchStarted += listener;

        if (hasMatchStarted && listener != null)
        {
            listener();
        }
    }

    /// <summary>
    /// adds match finish listener
    /// </summary>
    /// <param name="listener">match finish listener</param>
    public void AddMatchFinishListener(Action listener)
    {
        matchFinished += listener;

        if (hasMatchFinished && listener != null)
        {
            listener();
        }
    }

    /// <summary>
    /// removes match finish listener
    /// </summary>
    /// <param name="listener">match finish listener</param>
    public void RemoveMatchFinishListener(Action listener)
    {
        matchFinished -= listener;
    }

    /// <summary>
    /// removes match start listener
    /// </summary>
    /// <param name="listener">match start listener</param>
    public void RemoveMatchStartListener(Action listener)
    {
        matchStarted -= listener;
    }
    #endregion

    #region session methods
    /// <summary>
    /// checks if session data exists
    /// </summary>
    /// <returns>true if session exists</returns>
    public bool HasSessionData()
    {
        return hasSession && !string.IsNullOrEmpty(sessionId) && !string.IsNullOrEmpty(sessionToken);
    }

    /// <summary>
    /// sends session to everyone
    /// </summary>
    /// <param name="newSessionId">session id</param>
    /// <param name="newToken">session token</param>
    public void SendSessionToEveryone(string newSessionId, string newToken)
    {
        if (!isServer)
        {
            return;
        }

        ReceiveSession(newSessionId, newToken);
    }

    /// <summary>
    /// receives session
    /// </summary>
    /// <param name="newSessionId">session id</param>
    /// <param name="newToken">session token</param>
    [ObserversRpc(bufferLast: true, runLocally: true)] // bufferLast keeps this rpc for late clients, runLocally also runs it on the host
    private void ReceiveSession(string newSessionId, string newToken)
    {
        hasSession = true;
        sessionId = newSessionId;
        sessionToken = newToken;

        if (sessionReceived != null)
        {
            sessionReceived(sessionId, sessionToken);
        }
    }
    #endregion

    #region match methods
    /// <summary>
    /// tells everyone that the match started
    /// </summary>
    public void StartMatchForEveryone()
    {
        if (!isServer || hasMatchStarted)
        {
            return;
        }

        MatchStarted();
    }

    /// <summary>
    /// receives match start
    /// </summary>
    [ObserversRpc(bufferLast: true, runLocally: true)] // bufferLast keeps this rpc for late clients, runLocally also runs it on the host
    private void MatchStarted()
    {
        hasMatchStarted = true;

        if (matchStarted != null)
        {
            matchStarted();
        }
    }

    /// <summary>
    /// tells everyone that the match finished
    /// </summary>
    public void FinishMatchForEveryone()
    {
        if (!isServer || hasMatchFinished)
        {
            return;
        }

        MatchFinished();
    }

    /// <summary>
    /// receives match finish
    /// </summary>
    [ObserversRpc(bufferLast: true, runLocally: true)]
    private void MatchFinished()
    {
        hasMatchFinished = true;

        if (matchFinished != null)
        {
            matchFinished();
        }
    }
    #endregion
}