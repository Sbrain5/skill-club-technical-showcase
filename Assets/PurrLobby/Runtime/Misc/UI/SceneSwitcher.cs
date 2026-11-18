using PurrNet;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PurrLobby
{
    /// <summary>
    /// Handles scene transitions for menu, character selection, loading, and gameplay scenes.
    /// </summary>
    public class SceneSwitcher : MonoBehaviour
    {
        #region variables
        private NetworkManager networkManager;
        private LobbyManager lobbyManager;
        private bool gameSceneLoadRequested;
        #endregion

        #region unity methods
        /// <summary>
        /// Gets the lobby manager on the persistent lobby object.
        /// </summary>
        private void Awake()
        {
            lobbyManager = GetComponent<LobbyManager>();
        }
        #endregion

        #region methods
        /// <summary>
        /// Switches to the next scene in the current flow.
        /// </summary>
        public void SwitchScene()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;

            // if the current scene is the main menu then start the lobby and load the character selection scene
            if (currentSceneName == "MainMenuScene")
            {
                lobbyManager.SetLobbyStarted();
                SceneManager.LoadSceneAsync("CharacterSelectionScene");
                return;
            }

            // if the current scene is the character selection scene then load the loading scene
            if (currentSceneName == "CharacterSelectionScene")
            {
                SceneManager.LoadSceneAsync("LoadingScene");
                return;
            }
            // if the current scene is the loading scene then load the game scene
            if (currentSceneName == "LoadingScene")
            {
                if (gameSceneLoadRequested)
                {
                    return;
                }

                if (lobbyManager == null)
                {
                    lobbyManager = FindFirstObjectByType<LobbyManager>();
                }

                if (lobbyManager == null)
                {
                    return;
                }

                if (!lobbyManager.CurrentLobby.IsOwner)
                {
                    return;
                }

                if (networkManager == null)
                {
                    networkManager = FindFirstObjectByType<NetworkManager>();
                }

                if (networkManager == null)
                {
                    return;
                }

                gameSceneLoadRequested = true;
                networkManager.sceneModule.LoadSceneAsync("GameScene");
            }
        }
        #endregion
    }
}