using System.Collections;
using PurrLobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PurrNet;
using UnityEngine.SceneManagement;

/// <summary>
/// handles character selection, ready state and timer
/// </summary>
public class CharacterSelectManager : NetworkBehaviour
{
    #region variables
    [SerializeField] private Transform leftPreviewPoint;
    [SerializeField] private Transform rightPreviewPoint;
    [SerializeField] private TMP_Text leftNameText;
    [SerializeField] private TMP_Text leftFightingStyleText;
    [SerializeField] private TMP_Text leftNationalityText;
    [SerializeField] private TMP_Text leftHeightText;
    [SerializeField] private TMP_Text leftWeightText;
    [SerializeField] private TMP_Text rightNameText;
    [SerializeField] private TMP_Text rightFightingStyleText;
    [SerializeField] private TMP_Text rightNationalityText;
    [SerializeField] private TMP_Text rightHeightText;
    [SerializeField] private TMP_Text rightWeightText;
    [SerializeField] private GameObject[] player1Marks;
    [SerializeField] private GameObject[] player2Marks;
    [SerializeField] private CharacterData[] characters;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private AudioSource countdownAudioSource;
    [SerializeField] private AudioClip countdownClip;
    [SerializeField] private Button readyButton;
    private LobbyManager lobbyManager;
    private GameObject leftPreview;
    private GameObject rightPreview;
    private int leftCharacterIndex = -1; // selected character for player one
    private int rightCharacterIndex = -1; // selected character for player two
    private int timeLeft = 60; // shared selection timer
    private Image readyImage;
    private Color normalButtonColor;
    private Color normalTimerColor;
    private bool countdownStarted; // stops the final countdown sound from playing more than once
    private bool localReady; // ready state of this local player
    private bool readyLocked; // locks ready changes during the last seconds
    private bool switchedScene; // stops selection logic after the result is decided
    private bool leftReadyState; // ready state sent by player one
    private bool rightReadyState; // ready state sent by player two
    private float nextSecondTime; // next real time moment where the server reduces the timer
    private float nextEnterSceneSendTime; // controls how often the client tells the server it entered the scene
    private float nextSelectionStartResendTime; // lets the server resend selection start if a client missed it
    private bool finishSent; // stops the server from finishing selection more than once
    private bool leftPlayerInside; // true when player one reached character selection
    private bool rightPlayerInside; // true when player two reached character selection
    private bool selectionStarted; // true when both players can select characters
    private bool networkReady; // true when this network object can send rpc calls
    #endregion

    #region unity methods
    private void Start()
    {
        int i = 0;

        while (i < player1Marks.Length)
        {
            player1Marks[i].SetActive(false);
            i++;
        }

        i = 0;

        while (i < player2Marks.Length)
        {
            player2Marks[i].SetActive(false);
            i++;
        }

        leftNameText.text = "";
        leftFightingStyleText.text = "";
        leftNationalityText.text = "";
        leftHeightText.text = "";
        leftWeightText.text = "";
        rightNameText.text = "";
        rightFightingStyleText.text = "";
        rightNationalityText.text = "";
        rightHeightText.text = "";
        rightWeightText.text = "";
        timerText.text = "WAIT";
        normalTimerColor = timerText.color;
        readyButton.interactable = false;
        lobbyManager = FindFirstObjectByType<LobbyManager>();
        readyImage = readyButton.GetComponent<Image>();
        normalButtonColor = readyImage.color;
        RefreshReadyButton();
    }

    protected override void OnSpawned(bool asServer)
    {
        networkReady = true; // the network object is spawned and can send rpc calls

        if (asServer)
        {
            leftPlayerInside = true;
            TryStartSelectionOnServer(); // the server checks if both players are inside and starts selection if so
            return;
        }

        nextEnterSceneSendTime = 0f;
    }

    private void Update()
    {
        if (switchedScene)
        {
            return;
        }

        if (!selectionStarted)
        {
            timerText.text = "WAIT";
            readyButton.interactable = false;

            // the client keeps sending the server that they are inside the scene until the server starts selection
            if (networkReady && !isServer && Time.unscaledTime >= nextEnterSceneSendTime)
            {
                nextEnterSceneSendTime = Time.unscaledTime + 0.5f;
                PlayerEnteredSelectionScene();
            }

            return;
        }

        if (isServer && !finishSent)
        {
            // the server keeps resending selection start to clients who might have missed it
            if (Time.unscaledTime >= nextSelectionStartResendTime)
            {
                nextSelectionStartResendTime = Time.unscaledTime + 0.5f;
                StartSelection();
                SyncTimer(timeLeft);
            }

            if (Time.unscaledTime >= nextSecondTime && timeLeft > 0)
            {
                // one real second passed so reduce the shared timer
                nextSecondTime += 1f;
                timeLeft -= 1;

                // when 5 seconds are left auto ready players who already picked
                if (timeLeft == 5)
                {
                    leftReadyState = leftCharacterIndex >= 0;
                    rightReadyState = rightCharacterIndex >= 0;
                }

                SyncTimer(timeLeft); // send the new timer value to everyone

                // when the timer ends decide if we go to the game scene or back to menu
                if (timeLeft <= 0)
                {
                    finishSent = true;
                    bool rightHasCharacter = true;

                    if (lobbyManager.CurrentLobby.IsValid && lobbyManager.CurrentLobby.Members.Count > 1)
                    {
                        rightHasCharacter = rightCharacterIndex >= 0;
                    }

                    FinishSelection(leftCharacterIndex >= 0, rightHasCharacter);
                }
            }
        }

        timerText.text = timeLeft.ToString();

        // last 5 seconds become red and play the countdown sound once
        if (timeLeft <= 5)
        {
            timerText.color = Color.red;

            if (!countdownStarted)
            {
                countdownStarted = true;
                countdownAudioSource.PlayOneShot(countdownClip);
            }
        }
        else
        {
            timerText.color = normalTimerColor;
        }

        bool localHasCharacter; // checks if the local player already picked a character

        if (isHost)
        {
            localHasCharacter = leftCharacterIndex >= 0;
        }
        else
        {
            localHasCharacter = rightCharacterIndex >= 0;
        }

        // in the last 5 seconds the ready button locks automatically
        if (!readyLocked && timeLeft <= 5 && localHasCharacter)
        {
            readyLocked = true;

            // if the player forgot to press ready then do it automatically here
            if (!localReady)
            {
                localReady = true;
                UpdateReady(true);
            }

            RefreshReadyButton();
        }
    }
    #endregion

    #region methods
    /// <summary>
    /// selects one character for the local player
    /// </summary>
    /// <param name="index">character index</param>
    public void SelectCharacter(int index)
    {
        if (!selectionStarted || readyLocked)
        {
            return;
        }

        // changing character removes ready so the player confirms again
        if (localReady)
        {
            localReady = false;
            UpdateReady(false);
            RefreshReadyButton();
        }

        ApplyCharacter(index, isHost); // update the local side

        // host sends left side and client sends right side to the host
        if (isHost)
        {
            SyncCharacter(true, index);
        }
        else
        {
            UpdateCharacter(index);
        }

        // if the player picks during the last 5 seconds lock and auto ready
        if (timeLeft <= 5)
        {
            readyLocked = true;
            readyButton.interactable = false;
            localReady = true;
            UpdateReady(true);
            RefreshReadyButton();
        }

        // plays the selected character voice
        audioSource.Stop();
        audioSource.clip = characters[index].selectVoiceClip;
        audioSource.Play();
    }

    /// <summary>
    /// picks a random character for the local player
    /// </summary>
    public void SelectRandom()
    {
        SelectCharacter(Random.Range(0, characters.Length));
    }

    /// <summary>
    /// toggles ready for the local player
    /// </summary>
    public void PressReady()
    {
        if (!selectionStarted || readyLocked)
        {
            return;
        }

        // host cannot press ready before choosing a left side character
        if (isHost && leftCharacterIndex < 0)
        {
            return;
        }

        // client cannot press ready before choosing a right side character
        if (!isHost && rightCharacterIndex < 0)
        {
            return;
        }

        // toggle the local ready state and send it to the server
        localReady = !localReady;
        UpdateReady(localReady);
        RefreshReadyButton();
    }

    /// <summary>
    /// applies a character to one side
    /// </summary>
    /// <param name="index">character index</param>
    /// <param name="isLeft">true for left side</param>
    private void ApplyCharacter(int index, bool isLeft)
    {
        if (index < 0 || index >= characters.Length)
        {
            return;
        }

        CharacterData character = characters[index];

        // update the left side preview and info
        if (isLeft)
        {
            if (leftCharacterIndex >= 0)
            {
                player1Marks[leftCharacterIndex].SetActive(false);
            }

            leftCharacterIndex = index;
            Destroy(leftPreview);
            leftPreview = Instantiate(character.previewPrefab, leftPreviewPoint);
            leftPreview.transform.localPosition = character.leftPosition;
            leftPreview.transform.localEulerAngles = character.leftRotation;
            leftPreview.transform.localScale = character.leftScale;
            leftNameText.text = character.characterName;
            leftFightingStyleText.text = character.fightingStyle;
            leftNationalityText.text = character.nationality;
            leftHeightText.text = character.height;
            leftWeightText.text = character.weight;
            player1Marks[leftCharacterIndex].SetActive(true);
        }
        else
        {
            // update the right side preview and info
            if (rightCharacterIndex >= 0)
            {
                player2Marks[rightCharacterIndex].SetActive(false);
            }

            rightCharacterIndex = index;
            Destroy(rightPreview);
            rightPreview = Instantiate(character.previewPrefab, rightPreviewPoint);
            rightPreview.transform.localPosition = character.rightPosition;
            rightPreview.transform.localEulerAngles = character.rightRotation;
            rightPreview.transform.localScale = character.rightScale;
            rightNameText.text = character.characterName;
            rightFightingStyleText.text = character.fightingStyle;
            rightNationalityText.text = character.nationality;
            rightHeightText.text = character.height;
            rightWeightText.text = character.weight;
            player2Marks[rightCharacterIndex].SetActive(true);
        }
    }

    /// <summary>
    /// updates the ready button look
    /// </summary>
    private void RefreshReadyButton()
    {
        readyButton.interactable = selectionStarted && !readyLocked;

        if (localReady || readyLocked)
        {
            readyImage.color = Color.green;
        }
        else
        {
            readyImage.color = normalButtonColor;
        }
    }

    /// <summary>
    /// loads the game scene
    /// </summary>
    private void LoadGameScene()
    {
        LoadingManager.SaveCharacters(leftCharacterIndex, rightCharacterIndex);
        FindFirstObjectByType<SceneSwitcher>().SwitchScene();
    }

    /// <summary>
    /// starts returning to the main menu
    /// </summary>
    private void ReturnToMainMenu()
    {
        StartCoroutine(ReturnToMainMenuRoutine());
    }

    /// <summary>
    /// returns to the main menu after cleanup
    /// </summary>
    /// <returns>coroutine</returns>
    private IEnumerator ReturnToMainMenuRoutine()
    {
        Time.timeScale = 1f;
        NetworkSessionCleanup.CleanupForMainMenu();
        yield return null; // waits one frame so old managers are destroyed before main menu loads
        SceneManager.LoadScene("MainMenuScene");
    }

    /// <summary>
    /// starts selection when both players are inside
    /// </summary>
    private void TryStartSelectionOnServer()
    {
        if (selectionStarted)
        {
            return;
        }

        if (lobbyManager == null)
        {
            lobbyManager = FindFirstObjectByType<LobbyManager>();

            if (lobbyManager == null)
            {
                return;
            }
        }

        // if the lobby is invalid or has only one player start selection anyway
        if (!lobbyManager.CurrentLobby.IsValid || lobbyManager.CurrentLobby.Members.Count < 2)
        {
            StartSelection();
            return;
        }

        if (!leftPlayerInside || !rightPlayerInside)
        {
            return;
        }

        StartSelection();
    }
    #endregion

    #region rpcs
    /// <summary>
    /// client tells the server about their character pick
    /// </summary>
    /// <param name="index">character index</param>
    [ServerRpc]
    private void UpdateCharacter(int index)
    {
        if (!selectionStarted || index < 0 || index >= characters.Length)
        {
            return;
        }

        // if both players are already ready the timer jumps to the last 5 seconds
        if (timeLeft > 5 && leftReadyState && rightReadyState)
        {
            timeLeft = 5;
            nextSecondTime = Time.unscaledTime + 1f;
        }

        // send the right side character pick to everyone
        SyncCharacter(false, index);
    }

    /// <summary>
    /// client sends their ready state to the server
    /// </summary>
    /// <param name="readyValue">true if ready or false if not ready</param>
    /// <param name="info">RPC info to identify the sender</param>
    [ServerRpc]
    private void UpdateReady(bool readyValue, RPCInfo info = default)
    {
        if (!selectionStarted)
        {
            return;
        }

        // checks which player sent the rpc and updates the correct ready state
        if (info.sender == localPlayer)
        {
            leftReadyState = readyValue;
        }
        else
        {
            rightReadyState = readyValue;
        }

        // if the timer is already in the last 5 seconds don't do anything
        if (timeLeft <= 5)
        {
            return;
        }

        // if both players are ready skip directly to the last 5 seconds
        if (leftReadyState && (rightReadyState || !lobbyManager.CurrentLobby.IsValid || lobbyManager.CurrentLobby.Members.Count < 2))
        {
            timeLeft = 5;
            nextSecondTime = Time.unscaledTime + 1f;
            SyncTimer(timeLeft);
        }
    }

    /// <summary>
    /// server tells everyone the current time
    /// </summary>
    /// <param name="time">time left in seconds</param>
    [ObserversRpc(bufferLast: true, runLocally: true)]
    private void SyncTimer(int time)
    {
        timeLeft = time;
    }

    /// <summary>
    /// server tells everyone about a character pick
    /// </summary>
    /// <param name="index">character index</param>
    [ObserversRpc(runLocally: true)]
    private void SyncCharacter(bool isLeft, int index)
    {
        if (isLeft == isHost)
        {
            return;
        }

        ApplyCharacter(index, isLeft);
    }

    /// <summary>
    /// server tells everyone the session is over
    /// </summary>
    /// <param name="leftHasCharacter">true if the left player picked a character</param>
    /// <param name="rightHasCharacter">true if the right player picked a character</param>
    [ObserversRpc(bufferLast: true, runLocally: true)]
    private void FinishSelection(bool leftHasCharacter, bool rightHasCharacter)
    {
        bool localHasCharacter;
        bool otherHasCharacter;
        switchedScene = true; // stops the method from running again after the result was decided

        // host is the left player and client is the right player
        if (isHost)
        {
            localHasCharacter = leftHasCharacter;
            otherHasCharacter = rightHasCharacter;
        }
        else
        {
            localHasCharacter = rightHasCharacter;
            otherHasCharacter = leftHasCharacter;
        }

        // if both players picked a character continue to the next scene
        if (localHasCharacter && otherHasCharacter)
        {
            Invoke(nameof(LoadGameScene), 0.5f);
            return;
        }

        // otherwise show the correct timeout message
        if (!localHasCharacter)
        {
            TimeoutMessage.pendingMessage = "You did not choose a character in time. You have returned to the Main Menu.";
        }
        else
        {
            TimeoutMessage.pendingMessage = "The other player did not choose a character in time. You have returned to the Main Menu.";
        }

        Invoke(nameof(ReturnToMainMenu), 0.5f); // leave the lobby clear the manager and go back to the main menu
    }

    /// <summary>
    /// tells everyone selection can start
    /// </summary>
    [ObserversRpc(bufferLast: true, runLocally: true)]
    private void StartSelection()
    {
        if (selectionStarted)
        {
            return;
        }

        selectionStarted = true;
        timeLeft = 60;
        nextSecondTime = Time.unscaledTime + 1f;
        countdownStarted = false;
        timerText.color = normalTimerColor;
        RefreshReadyButton();
    }

    /// <summary>
    /// tells the server the client reached the scene
    /// </summary>
    /// <summary>
    /// tells the server the client reached the scene
    /// </summary>
    [ServerRpc(requireOwnership: false)]
    private void PlayerEnteredSelectionScene()
    {
        rightPlayerInside = true;

        if (selectionStarted)
        {
            StartSelection();
            SyncTimer(timeLeft);
            return;
        }

        TryStartSelectionOnServer();
    }
    #endregion

    #region nested classes
    [System.Serializable]
    private class CharacterData
    {
        #region character data
        public string characterName;
        public string fightingStyle;
        public string nationality;
        public string height;
        public string weight;
        public GameObject previewPrefab;
        public AudioClip selectVoiceClip;
        public Vector3 leftPosition;
        public Vector3 leftRotation;
        public Vector3 leftScale = Vector3.one;
        public Vector3 rightPosition;
        public Vector3 rightRotation;
        public Vector3 rightScale = Vector3.one;
        #endregion
    }
    #endregion
}