using System.Collections;
using System.Collections.Generic;
using PurrLobby;
using PurrNet;
using PurrNet.Steam;
using PurrNet.Transports;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// starts the network using steam lobby data
/// </summary>
public sealed class CustomConnectionStarter : MonoBehaviour
{
    #region variables
    private NetworkManager networkManager;
    private SteamTransport steamTransport;
    private LobbyDataHolder lobbyDataHolder;
    private LobbyManager lobbyManager;
    private Coroutine reconnectCoroutine; // keeps the running reconnect attempt so it is not started twice
    private bool returningToMenu; // stops return to menu cleanup from running more than once
    private bool connectionStarted; // stops the network connection from starting more than once
    private bool allowReconnect = true; // controls if the client is allowed to try reconnecting
    private bool wasInOnlineScene; // remembers if the player already reached an online scene
    private bool clientConnectedOnce; // prevents false disconnect handling before the first real connection
    private bool applicationClosing; // stops cleanup logic from running when the game is closing
    #endregion

    #region unity methods
    private void Awake()
    {
        networkManager = GetComponent<NetworkManager>();
        steamTransport = GetComponent<SteamTransport>();
    }

    private void Start()
    {
        NetworkSessionCleanup.ClearCleanupFlag(); // clear the cleanup flag because this is a new network session
        lobbyDataHolder = FindFirstObjectByType<LobbyDataHolder>();
        lobbyManager = FindFirstObjectByType<LobbyManager>();

        if (networkManager != null)
        {
            // listen for client connection changes and player disconnects
            networkManager.onClientConnectionState += OnClientConnectionState;
            networkManager.onPlayerLeft += OnNetworkPlayerLeft;
        }

        if (lobbyManager != null)
        {
            // listen for lobby leave events and player list updates
            lobbyManager.OnRoomLeft.AddListener(OnLobbyLeft);
            lobbyManager.OnPlayerListUpdated.AddListener(OnPlayerListUpdated);
        }

        StartLobbyConnection(); // start the connection using the lobby data
    }

    /// <summary>
    /// checks if the player reached an online scene
    /// </summary>
    private void Update()
    {
        // remember that the player has entered the online match flow
        if (IsOnlineScene())
        {
            wasInOnlineScene = true;
        }

        CheckOnlineSceneConnection(); // keep checking if the connection or lobby became invalid
    }

    private void OnDestroy()
    {
        if (networkManager != null)
        {
            // stop listening to network events when this object is destroyed
            networkManager.onClientConnectionState -= OnClientConnectionState;
            networkManager.onPlayerLeft -= OnNetworkPlayerLeft;
        }

        if (lobbyManager != null)
        {
            // stop listening to lobby events when this object is destroyed
            lobbyManager.OnRoomLeft.RemoveListener(OnLobbyLeft);
            lobbyManager.OnPlayerListUpdated.RemoveListener(OnPlayerListUpdated);
        }
    }

    private void OnApplicationQuit()
    {
        applicationClosing = true;
    }
    #endregion

    #region methods
    /// <summary>
    /// starts networking from steam lobby data
    /// </summary>
    private void StartLobbyConnection()
    {
        if (connectionStarted)
        {
            return;
        }

        if (networkManager == null || steamTransport == null)
        {
            Debug.LogError("custom connection starter is missing network components.");
            return;
        }

        if (lobbyDataHolder == null || !lobbyDataHolder.CurrentLobby.IsValid)
        {
            Debug.LogError("custom connection starter could not find a valid lobby.");
            return;
        }

        string lobbyIdText = lobbyDataHolder.CurrentLobby.LobbyId;
        ulong lobbyId;

        if (!ulong.TryParse(lobbyIdText, out lobbyId))
        {
            Debug.LogError("the custom connection starter could not parse LobbyId.");
            return;
        }

        // use the steam lobby owner as the host address
        CSteamID lobbyOwner = SteamMatchmaking.GetLobbyOwner(new CSteamID(lobbyId));
        steamTransport.address = lobbyOwner.ToString();
        networkManager.transport = steamTransport;
        connectionStarted = true;

        // the lobby owner starts the server before starting the client
        if (lobbyDataHolder.CurrentLobby.IsOwner)
        {
            networkManager.StartServer();
        }

        networkManager.StartClient();
    }

    /// <summary>
    /// checks if the client disconnected
    /// </summary>
    /// <param name="state">client connection state</param>
    private void OnClientConnectionState(ConnectionState state)
    {
        if (state == ConnectionState.Connected)
        {
            clientConnectedOnce = true;
            return;
        }

        if (state != ConnectionState.Disconnected)
        {
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;

        // if the client disconnected in the game scene or in the loading or character selection scene then return to main menu
        if (sceneName == "GameScene" || clientConnectedOnce && (sceneName == "LoadingScene" || sceneName == "CharacterSelectionScene"))
        {
            StartReturnToMenu();
            return;
        }

        // if this was an early disconnect before the first real connection, try reconnecting
        if (!allowReconnect || reconnectCoroutine != null)
        {
            return;
        }

        reconnectCoroutine = StartCoroutine(ReconnectClient());
    }

    /// <summary>
    /// checks if the online scene lost the other player
    /// </summary>
    private void CheckOnlineSceneConnection()
    {
        if (returningToMenu || applicationClosing || NetworkSessionCleanup.IsCleaningForMainMenu())
        {
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;

        // if the current scene is not an online scene then do not check for connection issues
        if (sceneName != "GameScene" && sceneName != "CharacterSelectionScene" && sceneName != "LoadingScene")
        {
            return;
        }

        // if the lobby is invalid or the other player left then return to main menu
        if (lobbyDataHolder == null || !lobbyDataHolder.CurrentLobby.IsValid)
        {
            if (sceneName != "CharacterSelectionScene" || clientConnectedOnce)
            {
                StartReturnToMenu();
            }

            return;
        }

        // if the lobby has less than 2 players then return to main menu
        if (lobbyDataHolder.CurrentLobby.Members.Count < 2)
        {
            if (sceneName != "CharacterSelectionScene" || clientConnectedOnce)
            {
                StartReturnToMenu();
            }

            return;
        }

        // if the client is disconnected then return to main menu
        if (networkManager != null && networkManager.clientState == ConnectionState.Disconnected)
        {
            if (sceneName != "CharacterSelectionScene" || clientConnectedOnce)
            {
                StartReturnToMenu();
            }
        }
    }

    /// <summary>
    /// checks if the lobby was left
    /// </summary>
    private void OnLobbyLeft()
    {
        if (applicationClosing || NetworkSessionCleanup.IsCleaningForMainMenu())
        {
            return;
        }

        if (IsOnlineScene())
        {
            StartReturnToMenu();
        }
    }

    /// <summary>
    /// checks if the other player left the lobby
    /// </summary>
    /// <param name="players">current lobby players</param>
    private void OnPlayerListUpdated(List<LobbyUser> players)
    {
        if (applicationClosing || NetworkSessionCleanup.IsCleaningForMainMenu())
        {
            return;
        }

        if (!IsOnlineScene())
        {
            return;
        }

        if (players == null || players.Count < 2)
        {
            StartReturnToMenu();
        }
    }

    /// <summary>
    /// checks if the current scene is an online scene
    /// </summary>
    /// <returns>true if this is an online scene</returns>
    private bool IsOnlineScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        return sceneName == "CharacterSelectionScene" || sceneName == "LoadingScene" || sceneName == "GameScene";
    }

    /// <summary>
    /// starts returning to main menu
    /// </summary>
    private void StartReturnToMenu()
    {
        if (returningToMenu || applicationClosing || NetworkSessionCleanup.IsCleaningForMainMenu())
        {
            return;
        }

        returningToMenu = true;
        allowReconnect = false;
        SetPlayerLeftMessage(); // set the message to show in the main menu after returning

        if (reconnectCoroutine != null)
        {
            StopCoroutine(reconnectCoroutine);
            reconnectCoroutine = null;
        }

        Time.timeScale = 1f;
        NetworkSessionCleanup.CleanupForMainMenu();
        SceneManager.LoadScene("MainMenuScene");
    }

    /// <summary>
    /// sets the message shown after the other player leaves
    /// </summary>
    private void SetPlayerLeftMessage()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // set the message to show in the main menu after returning from the character selection scene
        if (sceneName == "CharacterSelectionScene")
        {
            TimeoutMessage.pendingMessage = "The other player left during character selection. You have returned to the Main Menu.";
            return;
        }

        // set the message to show in the main menu after returning from the loading scene
        if (sceneName == "LoadingScene")
        {
            TimeoutMessage.pendingMessage = "The other player left during loading. You have returned to the Main Menu.";
            return;
        }

        // set the message to show in the main menu after returning from the game scene
        if (sceneName == "GameScene")
        {
            TimeoutMessage.pendingMessage = "The other player left the match. You have returned to the Main Menu.";
        }
    }

    /// <summary>
    /// checks if a network player left the match
    /// </summary>
    /// <param name="player">player that left</param>
    /// <param name="asServer">true if this was detected by the server</param>
    private void OnNetworkPlayerLeft(PlayerID player, bool asServer)
    {
        if (!asServer)
        {
            return;
        }

        // if the player left before reaching an online scene then do not return to main menu
        if (!wasInOnlineScene && !IsOnlineScene())
        {
            return;
        }

        StartReturnToMenu();
    }

    /// <summary>
    /// tries to reconnect the steam client
    /// </summary>
    /// <returns>coroutine</returns>
    private IEnumerator ReconnectClient()
    {
        while (allowReconnect && networkManager.clientState == ConnectionState.Disconnected)
        {
            // stop reconnecting if the lobby is no longer valid
            if (lobbyDataHolder == null || !lobbyDataHolder.CurrentLobby.IsValid)
            {
                break;
            }

            yield return new WaitForSecondsRealtime(2.0f); // wait a little before trying again

            // try to reconnect only if the client is still disconnected
            if (networkManager.clientState == ConnectionState.Disconnected)
            {
                networkManager.StartClient();
            }

            yield return new WaitForSecondsRealtime(3.0f); // wait before the next reconnect check
        }

        reconnectCoroutine = null; // clear the coroutine reference so another reconnect attempt can start later
    }
    #endregion
}