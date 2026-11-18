using System.Collections;
using PurrLobby;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Shows the loading portraits and moves locally from the loading screen to the gameplay scene.
/// </summary>
public class LoadingManager : MonoBehaviour
{
    #region variables
    [SerializeField] private Image leftCharacterPanel;
    [SerializeField] private Image rightCharacterPanel;
    [SerializeField] private TMP_Text leftCharacterNameText;
    [SerializeField] private TMP_Text rightCharacterNameText;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private CharacterInfo[] characters;
    [SerializeField] private float minimumLoadingSeconds = 2.0f;
    private float loadingDotTime;
    private int loadingDotCount;
    private bool switchingScene;
    private bool returningToMenu;
    private static int savedLeftCharacterIndex = -1;
    private static int savedRightCharacterIndex = -1;
    #endregion

    #region unity methods
    private void Start()
    {
        loadingText.text = "LOADING";
        ApplyCharacter(leftCharacterPanel, leftCharacterNameText, savedLeftCharacterIndex);
        ApplyCharacter(rightCharacterPanel, rightCharacterNameText, savedRightCharacterIndex);
        RefreshLoadingText();
        StartCoroutine(LoadGameSceneAfterDelay());
    }

    private void Update()
    {
        if (returningToMenu)
        {
            return;
        }

        UpdateLoadingDots();
        CheckLoadingConnection();
    }
    #endregion

    #region methods
    /// <summary>
    /// Saves the selected characters before scene change.
    /// </summary>
    /// <param name="leftCharacterIndex">left character index</param>
    /// <param name="rightCharacterIndex">right character index</param>
    public static void SaveCharacters(int leftCharacterIndex, int rightCharacterIndex)
    {
        savedLeftCharacterIndex = leftCharacterIndex;
        savedRightCharacterIndex = rightCharacterIndex;
    }

    /// <summary>
    /// Returns saved left character index.
    /// </summary>
    /// <returns>left character index</returns>
    public static int GetLeftCharacterIndex()
    {
        return savedLeftCharacterIndex;
    }

    /// <summary>
    /// Returns saved right character index.
    /// </summary>
    /// <returns>right character index</returns>
    public static int GetRightCharacterIndex()
    {
        return savedRightCharacterIndex;
    }

    /// <summary>
    /// Applies one portrait and one character name.
    /// </summary>
    /// <param name="panelImage">portrait image</param>
    /// <param name="nameText">character name text</param>
    /// <param name="characterIndex">character index</param>
    private void ApplyCharacter(Image panelImage, TMP_Text nameText, int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characters.Length)
        {
            return;
        }

        CharacterInfo character = characters[characterIndex];
        panelImage.sprite = character.portrait;
        nameText.text = character.characterName;
    }

    /// <summary>
    /// waits briefly so the loading screen is visible, then loads the gameplay scene locally
    /// </summary>
    /// <returns>coroutine</returns>
    private IEnumerator LoadGameSceneAfterDelay()
    {
        yield return new WaitForSecondsRealtime(minimumLoadingSeconds);

        if (returningToMenu || NetworkSessionCleanup.IsCleaningForMainMenu())
        {
            yield break;
        }

        LoadGameScene();
    }

    /// <summary>
    /// loads the gameplay scene
    /// </summary>
    private void LoadGameScene()
    {
        if (switchingScene || returningToMenu || NetworkSessionCleanup.IsCleaningForMainMenu())
        {
            return;
        }

        if (!CanLoadGameScene())
        {
            ReturnToMainMenu();
            return;
        }

        SceneSwitcher sceneSwitcher = FindFirstObjectByType<SceneSwitcher>();
        switchingScene = true;
        sceneSwitcher.SwitchScene();
    }

    /// <summary>
    /// checks if loading should continue
    /// </summary>
    private void CheckLoadingConnection()
    {
        if (returningToMenu || NetworkSessionCleanup.IsCleaningForMainMenu())
        {
            return;
        }

        LobbyDataHolder lobbyDataHolder = FindFirstObjectByType<LobbyDataHolder>();

        if (lobbyDataHolder == null)
        {
            return;
        }

        // if the lobby is invalid or has less than 2 members then return to main menu
        if (!lobbyDataHolder.CurrentLobby.IsValid || lobbyDataHolder.CurrentLobby.Members.Count < 2)
        {
            ReturnToMainMenu();
        }
    }

    /// <summary>
    /// checks if the game scene can load
    /// </summary>
    /// <returns>true if both players are still in the lobby</returns>
    private bool CanLoadGameScene()
    {
        LobbyDataHolder lobbyDataHolder = FindFirstObjectByType<LobbyDataHolder>();

        if (lobbyDataHolder == null)
        {
            return false;
        }

        return lobbyDataHolder.CurrentLobby.IsValid && lobbyDataHolder.CurrentLobby.Members.Count >= 2;
    }

    /// <summary>
    /// returns to the main menu from loading
    /// </summary>
    private void ReturnToMainMenu()
    {
        if (returningToMenu || NetworkSessionCleanup.IsCleaningForMainMenu())
        {
            return;
        }

        returningToMenu = true;
        TimeoutMessage.pendingMessage = "The other player left during loading. You have returned to the Main Menu.";
        Time.timeScale = 1f; // reset time scale in case it was changed in the previous scene
        NetworkSessionCleanup.CleanupForMainMenu();
        SceneManager.LoadScene("MainMenuScene");
    }

    /// <summary>
    /// Updates the loading dot timer.
    /// </summary>
    private void UpdateLoadingDots()
    {
        loadingDotTime += Time.deltaTime;

        if (loadingDotTime < 0.35f)
        {
            return;
        }

        loadingDotTime = 0.0f;
        loadingDotCount++;

        if (loadingDotCount > 3)
        {
            loadingDotCount = 0;
        }

        RefreshLoadingText();
    }

    /// <summary>
    /// Updates the visible loading text.
    /// </summary>
    private void RefreshLoadingText()
    {
        loadingText.text = "LOADING" + new string('.', loadingDotCount);
    }
    #endregion

    #region nested classes
    [System.Serializable]
    private class CharacterInfo
    {
        #region variables
        public string characterName;
        public Sprite portrait;
        #endregion
    }
    #endregion
}