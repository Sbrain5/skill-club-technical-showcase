using PurrLobby;
using PurrNet;
using PurrNet.Steam;
using UnityEngine;

/// <summary>
/// cleans persistent multiplayer objects when returning to the main menu
/// </summary>
public static class NetworkSessionCleanup
{
    #region variables
    private static bool cleaningForMainMenu;
    #endregion

    #region methods
    /// <summary>
    /// says if cleanup is already running
    /// </summary>
    /// <returns>true if cleanup is running</returns>
    public static bool IsCleaningForMainMenu()
    {
        return cleaningForMainMenu;
    }

    /// <summary>
    /// clears the cleanup flag for the next match
    /// </summary>
    public static void ClearCleanupFlag()
    {
        cleaningForMainMenu = false;
    }

    /// <summary>
    /// stops and destroys old network and lobby objects so the next session starts cleanly
    /// </summary>
    public static void CleanupForMainMenu()
    {
        if (cleaningForMainMenu)
        {
            return;
        }

        cleaningForMainMenu = true;

        // clean each multiplayer object
        CleanNetworkManagers();
        ResetLobbyManagers();
        CleanLobbyDataHolders();
    }

    /// <summary>
    /// stops and destroys active network managers
    /// </summary>
    private static void CleanNetworkManagers()
    {
        NetworkManager[] networkManagers = Object.FindObjectsByType<NetworkManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (NetworkManager networkManager in networkManagers)
        {
            if (networkManager == null)
            {
                continue;
            }

            SteamTransport steamTransport = networkManager.GetComponent<SteamTransport>(); // stop the steam transport before destroying the network object

            if (steamTransport != null)
            {
                steamTransport.Disconnect();
                steamTransport.StopListening();
            }

            Object.Destroy(networkManager.gameObject); // destroy the old network manager so the next session starts clean
        }
    }

    /// <summary>
    /// resets persistent lobby managers
    /// </summary>
    private static void ResetLobbyManagers()
    {
        LobbyManager[] lobbyManagers = Object.FindObjectsByType<LobbyManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (LobbyManager lobbyManager in lobbyManagers)
        {
            if (lobbyManager == null)
            {
                continue;
            }

            lobbyManager.ResetForMainMenu(); // the lobby manager clears its lobby data and destroys itself
        }
    }

    /// <summary>
    /// destroys leftover lobby data holders
    /// </summary>
    private static void CleanLobbyDataHolders()
    {
        LobbyDataHolder[] lobbyDataHolders = Object.FindObjectsByType<LobbyDataHolder>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (LobbyDataHolder lobbyDataHolder in lobbyDataHolders)
        {
            if (lobbyDataHolder == null)
            {
                continue;
            }

            Object.Destroy(lobbyDataHolder.gameObject); // remove old lobby data so the main menu can create a fresh session
        }
    }
    #endregion
}