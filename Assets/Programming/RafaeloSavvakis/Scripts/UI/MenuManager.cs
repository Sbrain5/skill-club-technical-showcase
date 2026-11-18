using System.Collections;
using PurrLobby;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


/// <summary>
/// handles safe menu navigation from gameplay UI
/// </summary>
public sealed class MenuManager : MonoBehaviour
{
    #region variables
    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    [Header("Button References")]
    [SerializeField] private Button returnToMenuButton;

    private bool isReturningToMenu;
    #endregion

    #region unity methods
    /// <summary>
    /// connects UI button events when the scene starts
    /// </summary>
    private void Awake()
    {
            this.returnToMenuButton.onClick.AddListener(this.ReturnToMainMenu);
    }

    /// <summary>
    /// removes UI button events when this object is destroyed
    /// </summary>
    private void OnDestroy()
    {
        if (this.returnToMenuButton != null)
        {
            this.returnToMenuButton.onClick.RemoveListener(this.ReturnToMainMenu);
        }
    }
    #endregion

    #region public methods
    /// <summary>
    /// starts the safe return to main menu flow
    /// </summary>
    public void ReturnToMainMenu()
    {
        GameSceneCheck gameSceneCheck;

        if (this.isReturningToMenu)
        {
            return;
        }

        gameSceneCheck = FindFirstObjectByType<GameSceneCheck>();

        if (gameSceneCheck != null)
        {
            this.isReturningToMenu = true;
            gameSceneCheck.ReturnEveryoneToMainMenu();
            return;
        }

        this.StartCoroutine(this.ReturnToMainMenuRoutine());
    }

    /// <summary>
    /// selects the return-to-menu button for keyboard and controller input
    /// </summary>
    public void SelectReturnToMenuButton()
    {
        this.StartCoroutine(this.SelectReturnToMenuButtonRoutine());
    }
    #endregion

    #region methods
    /// <summary>
    /// clears persistent lobby state before loading the main menu scene again
    /// </summary>
    /// <returns>coroutine wait step</returns>
    private IEnumerator ReturnToMainMenuRoutine()
    {
        this.isReturningToMenu = true;
        Time.timeScale = 1f;

        NetworkSessionCleanup.CleanupForMainMenu();

        yield return null; // waits one frame so old managers are destroyed before main menu loads

        SceneManager.LoadScene(this.mainMenuSceneName);
    }

    /// <summary>
    /// selects the return button after the panel finishes becoming active
    /// </summary>
    /// <returns>one-frame UI activation delay</returns>
    private IEnumerator SelectReturnToMenuButtonRoutine()
    {
        yield return null;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(
            this.returnToMenuButton.gameObject);
    }
    #endregion
}