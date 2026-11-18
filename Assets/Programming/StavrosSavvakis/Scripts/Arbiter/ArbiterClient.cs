using System;
using System.Collections;
using System.Text;
using PurrLobby;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// sends session, checkpoint and finish data to the arbiter
/// </summary>
public class ArbiterClient : MonoBehaviour
{
    #region variables
    [SerializeField] private string arbiterUrl = "http://144.91.100.69:5001";  // 5000 = main and 5001 = staging
    [SerializeField] private bool showLogs;
    [SerializeField] private int finishRetryDelaySeconds = 1;
    [SerializeField] private int maxFinishRetries = 10;
    private LobbyDataHolder lobbyDataHolder;
    private ArbiterSessionShare sessionShare;
    private bool hasSession;
    private string sessionId;
    private string token;
    #endregion

    #region unity methods
    private void Start()
    {
        lobbyDataHolder = FindFirstObjectByType<LobbyDataHolder>();
        StartCoroutine(StartClient());
    }

    private void OnDestroy()
    {
        if (sessionShare != null)
        {
            sessionShare.RemoveListener(OnSessionReceived);
        }
    }
    #endregion

    #region setup methods
    /// <summary>
    /// finds session share and starts the session flow
    /// </summary>
    /// <returns>coroutine</returns>
    private IEnumerator StartClient()
    {
        while (sessionShare == null)
        {
            sessionShare = FindFirstObjectByType<ArbiterSessionShare>();
            yield return null;
        }

        sessionShare.AddListener(OnSessionReceived);
        Log("ready");

        if (lobbyDataHolder.CurrentLobby.IsOwner)
        {
            yield return StartCoroutine(CreateSession());
        }
    }

    /// <summary>
    /// receives session from host
    /// </summary>
    /// <param name="newSessionId">session id</param>
    /// <param name="newToken">session token</param>
    private void OnSessionReceived(string newSessionId, string newToken)
    {
        hasSession = true;
        sessionId = newSessionId;
        token = newToken;
        Log("session received");
    }
    #endregion

    #region session methods
    /// <summary>
    /// creates arbiter session
    /// </summary>
    /// <returns>coroutine</returns>
    private IEnumerator CreateSession()
    {
        // get the two steam ids from the lobby
        ulong player1 = ulong.Parse(lobbyDataHolder.CurrentLobby.Members[0].Id);
        ulong player2 = ulong.Parse(lobbyDataHolder.CurrentLobby.Members[1].Id);

        string json = "{\"player1\":" + player1 + ",\"player2\":" + player2 + "}"; // make the json that the arbiter expects

        // unity sends the request body as bytes
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        string url = arbiterUrl + "/api/session/create";
        Log("creating session");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            // attach the json to the request
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;

            yield return request.SendWebRequest(); // send the request and wait for the answer

            if (request.result != UnityWebRequest.Result.Success)
            {
                Log("session creation failed: " + request.error);
                yield break;
            }

            // read the session data from the arbiter response
            CreateSessionResponse response = JsonUtility.FromJson<CreateSessionResponse>(request.downloadHandler.text);

            // save the session locally
            hasSession = true;
            sessionId = response.session;
            token = response.key;
            Log("session created");

            sessionShare.SendSessionToEveryone(sessionId, token); // send the same session to the other player
        }
    }
    #endregion

    #region checkpoint methods
    /// <summary>
    /// sends checkpoint to arbiter
    /// </summary>
    /// <param name="frame">frame number</param>
    /// <param name="playerId">player steam id</param>
    /// <param name="stateHash">state hash</param>
    public void SendCheckpoint(int frame, ulong playerId, string stateHash)
    {
        // checkpoints can only be sent after the arbiter session exists
        if (hasSession)
        {
            StartCoroutine(PostCheckpoint(frame, playerId, stateHash)); // sends the request through a coroutine because it uses a web request
        }
    }

    /// <summary>
    /// posts checkpoint request
    /// </summary>
    /// <param name="frame">frame number</param>
    /// <param name="playerId">player steam id</param>
    /// <param name="stateHash">state hash</param>
    /// <returns>coroutine</returns>
    private IEnumerator PostCheckpoint(int frame, ulong playerId, string stateHash)
    {
        // creates the checkpoint json that the arbiter expects
        string json = "{\"sessionId\":\"" + sessionId + "\",\"token\":\"" + token + "\",\"frame\":" + frame + ",\"player\":" + playerId + ",\"stateHash\":\"" + stateHash + "\"}";
        string url = arbiterUrl + "/api/session/checkpoint";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            // sends the json as the request body
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;
            yield return request.SendWebRequest();

            // stops here if the arbiter did not receive the checkpoint
            if (request.result != UnityWebRequest.Result.Success)
            {
                Log("checkpoint failed: " + request.error);
                yield break;
            }

            Log("checkpoint response: " + request.downloadHandler.text);
        }
    }
    #endregion

    #region finish methods
    /// <summary>
    /// sends finish data to arbiter
    /// </summary>
    /// <param name="playerId">player steam id</param>
    /// <param name="winnerId">winner steam id</param>
    /// <param name="p1Rounds">player one rounds</param>
    /// <param name="p2Rounds">player two rounds</param>
    /// <param name="finalFrame">final frame</param>
    /// <param name="finalStateHash">final state hash</param>
    public void SendFinish(ulong playerId, ulong winnerId, int p1Rounds, int p2Rounds, int finalFrame, string finalStateHash)
    {
        // finish data can only be sent after the arbiter session exists
        if (hasSession)
        {
            StartCoroutine(PostFinish(playerId, winnerId, p1Rounds, p2Rounds, finalFrame, finalStateHash)); // sends the final result through a coroutine because it uses a web request
        }
    }

    /// <summary>
    /// posts finish request
    /// </summary>
    /// <param name="playerId">player steam id</param>
    /// <param name="winnerId">winner steam id</param>
    /// <param name="p1Rounds">player one rounds</param>
    /// <param name="p2Rounds">player two rounds</param>
    /// <param name="finalFrame">final frame</param>
    /// <param name="finalStateHash">final state hash</param>
    /// <returns>coroutine</returns>
    private IEnumerator PostFinish(ulong playerId, ulong winnerId, int p1Rounds, int p2Rounds, int finalFrame, string finalStateHash)
    {
        int retryCount = 0;

        // creates the final result json that the arbiter expects
        string json = "{\"sessionId\":\"" + sessionId + "\",\"token\":\"" + token + "\",\"player\":" + playerId + ",\"winner\":" + winnerId + 
                      ",\"p1Rounds\":" + p1Rounds + ",\"p2Rounds\":" + p2Rounds + ",\"finalFrame\":" + finalFrame + ",\"finalStateHash\":\"" + finalStateHash + "\"}";
        string url = arbiterUrl + "/api/session/finish";

        while (retryCount < maxFinishRetries)
        {
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                // sends the json as the request body
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = 10;

                yield return request.SendWebRequest();

                // stops here if the arbiter did not receive the finish data
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Log("finish failed: " + request.error);
                    yield break;
                }

                string responseText = request.downloadHandler.text;
                Log("finish response: " + responseText);

                if (!responseText.Contains("\"result\":\"waiting\""))
                {
                    yield break;
                }
            }

            retryCount++;
            yield return new WaitForSeconds(finishRetryDelaySeconds);
        }

        Log("finish stopped waiting after max retries");
    }
    #endregion

    #region log methods
    /// <summary>
    /// writes one arbiter client message
    /// </summary>
    /// <param name="message">message text</param>
    private void Log(string message)
    {
        ArbiterLogView.AddLog("client: " + message); // always writes the message in the in-game arbiter log panel

        // also writes to unity console when extra logs are enabled
        if (showLogs)
        {
            Debug.Log("arbiter client: " + message);
        }
    }
    #endregion

    #region data classes
    [Serializable]
    private class CreateSessionResponse
    {
        public string session;
        public string key;
        public long expires;
    }
    #endregion
}