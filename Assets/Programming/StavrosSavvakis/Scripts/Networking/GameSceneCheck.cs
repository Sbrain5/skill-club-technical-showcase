using System.Collections;
using PurrNet;
using PurrNet.Transports;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// checks if both players reached the game scene and handles return to menu
/// </summary>
public class GameSceneCheck : NetworkBehaviour
{
    #region variables
    private bool hostLoaded;
    private bool clientLoaded;
    private bool bothPlayersLoadedConfirmed;
    private bool returningToMenu;
    private float nextReadySendTime;
    #endregion

    #region unity methods
    /// <summary>
    /// prepares the local repeated ready signal when the object spawns
    /// </summary>
    /// <param name="asServer">true when running on the server side</param>
    protected override void OnSpawned(bool asServer)
    {
        nextReadySendTime = 0.0f;
    }

    /// <summary>
    /// keeps sending local game scene ready until the server confirms both players
    /// </summary>
    private void Update()
    {
        if (bothPlayersLoadedConfirmed || returningToMenu || Time.unscaledTime < nextReadySendTime)
        {
            return;
        }

        nextReadySendTime = Time.unscaledTime + 0.5f;
        SendLoadedToServer(isHost);
    }
    #endregion

    #region methods
    /// <summary>
    /// says if both players are inside the game scene
    /// </summary>
    /// <returns>true when both players reached the game scene</returns>
    public bool BothPlayersLoaded()
    {
        return hostLoaded && clientLoaded;
    }

    /// <summary>
    /// returns everyone to main menu if possible
    /// </summary>
    public void ReturnEveryoneToMainMenu()
    {
        if (returningToMenu)
        {
            return;
        }

        if (isServer)
        {
            ReturnToMenuClients();

            if (networkManager != null)
            {
                networkManager.FlushBatchedRPCs(); // sends the return message before cleanup starts
            }

            return;
        }

        if (networkManager == null || networkManager.clientState != ConnectionState.Connected)
        {
            StartCoroutine(ReturnToMainMenuRoutine()); // host is gone so return locally
            return;
        }

        AskReturnToMenuServer();
    }

    /// <summary>
    /// returns this player to main menu
    /// </summary>
    /// <returns>coroutine</returns>
    private IEnumerator ReturnToMainMenuRoutine()
    {
        returningToMenu = true;
        bothPlayersLoadedConfirmed = true;
        yield return new WaitForSecondsRealtime(0.2f);
        Time.timeScale = 1f;
        NetworkSessionCleanup.CleanupForMainMenu();
        yield return null; // waits one frame so old managers are destroyed before main menu loads
        SceneManager.LoadScene("MainMenuScene");
    }
    #endregion

    #region rpc methods
    /// <summary>
    /// tells the server that one player reached the game scene
    /// </summary>
    /// <param name="senderIsHost">true if the ready player is the host player</param>
    [ServerRpc(requireOwnership: false)]
    private void SendLoadedToServer(bool senderIsHost)
    {
        if (senderIsHost)
        {
            hostLoaded = true;
        }
        else
        {
            clientLoaded = true;
        }

        if (hostLoaded && clientLoaded)
        {
            ConfirmBothPlayersLoaded();
        }
    }

    /// <summary>
    /// confirms to everyone that both players reached the game scene
    /// </summary>
    [ObserversRpc(bufferLast: true, runLocally: true)]
    private void ConfirmBothPlayersLoaded()
    {
        bothPlayersLoadedConfirmed = true;
    }

    /// <summary>
    /// asks the server to return everyone to menu
    /// </summary>
    [ServerRpc(requireOwnership: false)]
    private void AskReturnToMenuServer()
    {
        ReturnToMenuClients();

        if (networkManager != null)
        {
            networkManager.FlushBatchedRPCs();
        }
    }

    /// <summary>
    /// tells everyone to return to menu
    /// </summary>
    [ObserversRpc(runLocally: true)]
    private void ReturnToMenuClients()
    {
        if (returningToMenu)
        {
            return;
        }

        StartCoroutine(ReturnToMainMenuRoutine());
    }
    #endregion
}