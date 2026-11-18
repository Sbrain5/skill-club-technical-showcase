using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// displays basic player health values on the gameplay HUD
/// </summary>
public sealed class HUDManager : MonoBehaviour
{
    #region variables
    [Header("Health Bar References")]
    [SerializeField] private Image player01HealthFill;
    [SerializeField] private Image player02HealthFill;

    [Header("Runtime Health Sources")]
    [SerializeField] private PlayerHealth player01Health;
    [SerializeField] private PlayerHealth player02Health;

    [Header("Round Result References")]
    [SerializeField] private TextMeshProUGUI roundResultText;

    [Header("Round Score References")]
    [SerializeField] private TextMeshProUGUI roundScoreText;

    [Header("Round Timer References")]
    [SerializeField] private TextMeshProUGUI roundTimerText;

    [Header("Match Over Panel References")]
    [SerializeField] private GameObject matchOverPanel;
    [SerializeField] private TextMeshProUGUI matchOverWinnerText;
    [SerializeField] private TextMeshProUGUI matchOverScoreText;
    [SerializeField] private MenuManager menuManager;

    [Header("Runtime Match Source")]
    [SerializeField] private GameManager gameManager;

    [Header("Combat Box Debug View")]
    [SerializeField] private TextMeshProUGUI combatBoxToggleText;

    private bool combatBoxViewEnabled;
    #endregion

    #region unity methods
    /// <summary>
    /// updates the HUD every frame from the latest mirrored gameplay state
    /// </summary>
    private void Update()
    {
        this.RefreshHealthReferencesIfNeeded();
        this.RefreshGameManagerReferenceIfNeeded();
        this.UpdateHealthBars();
        this.UpdateRoundResultText();
        this.UpdateRoundScoreText();
        this.UpdateRoundTimerText();
        this.UpdateMatchOverPanel();
    }
    #endregion

    #region methods
    /// <summary>
    /// refreshes health references when the cached references are missing or assigned to the wrong slot
    /// </summary>
    private void RefreshHealthReferencesIfNeeded()
    {
        PlayerController[] playerControllers;

        if (this.HasValidHealthReference(this.player01Health, PlayerController.PlayerSlot.Player01) &&
            this.HasValidHealthReference(this.player02Health, PlayerController.PlayerSlot.Player02))
        {
            return;
        }

        this.player01Health = null;
        this.player02Health = null;

        playerControllers = Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (PlayerController playerController in playerControllers)
        {
            if (playerController.AssignedPlayerSlot == PlayerController.PlayerSlot.Player01)
            {
                this.player01Health = playerController.GetComponent<PlayerHealth>();
                continue;
            }

            if (playerController.AssignedPlayerSlot == PlayerController.PlayerSlot.Player02)
            {
                this.player02Health = playerController.GetComponent<PlayerHealth>();
            }
        }
    }

    /// <summary>
    /// checks if a cached health reference still belongs to the expected player slot
    /// </summary>
    /// <param name="playerHealth">cached health reference</param>
    /// <param name="expectedSlot">expected player slot</param>
    /// <returns>true if the health reference belongs to the expected slot</returns>
    private bool HasValidHealthReference(PlayerHealth playerHealth, PlayerController.PlayerSlot expectedSlot)
    {
        PlayerController playerController;

        if (playerHealth == null)
        {
            return false;
        }

        playerController = playerHealth.GetComponent<PlayerController>();
        return playerController.AssignedPlayerSlot == expectedSlot;
    }

    /// <summary>
    /// updates both player health bars
    /// </summary>
    private void UpdateHealthBars()
    {
        this.UpdateHealthBar(this.player01HealthFill, this.player01Health, false);
        this.UpdateHealthBar(this.player02HealthFill, this.player02Health, true);
    }

    /// <summary>
    /// updates one health bar image from one health source
    /// </summary>
    /// <param name="healthFill">the fill image to update</param>
    /// <param name="playerHealth">the player health source</param>
    /// <param name="drainFromLeft">true when the bar should stay attached to the right side</param>
    private void UpdateHealthBar(Image healthFill, PlayerHealth playerHealth, bool drainFromLeft)
    {
        float healthPercent;
        RectTransform fillRect;
        Vector2 anchorMin;
        Vector2 anchorMax;

        if (playerHealth == null)
        {
            return;
        }

        healthPercent = Mathf.Clamp01(playerHealth.HealthPercent);
        fillRect = healthFill.rectTransform;

        anchorMin = fillRect.anchorMin;
        anchorMax = fillRect.anchorMax;

        if (drainFromLeft)
        {
            anchorMin.x = 1.0f - healthPercent;
            anchorMax.x = 1.0f;
        }
        else
        {
            anchorMin.x = 0.0f;
            anchorMax.x = healthPercent;
        }

        fillRect.anchorMin = anchorMin;
        fillRect.anchorMax = anchorMax;
    }

    /// <summary>
    /// refreshes the GameManager reference when it is missing
    /// </summary>
    private void RefreshGameManagerReferenceIfNeeded()
    {
        if (this.gameManager != null)
        {
            return;
        }

        this.gameManager = Object.FindFirstObjectByType<GameManager>();
    }

    /// <summary>
    /// updates the round result text from the authoritative match state
    /// </summary>
    private void UpdateRoundResultText()
    {
        GameState currentState;

        if (this.gameManager == null)
        {
            this.roundResultText.gameObject.SetActive(false);
            return;
        }

        currentState = this.gameManager.CurrentAuthoritativeState;

        if (currentState == null)
        {
            this.roundResultText.gameObject.SetActive(false);
            return;
        }

        if (currentState.MatchState != SimMatchState.RoundOver)
        {
            this.roundResultText.gameObject.SetActive(false);
            return;
        }

        this.roundResultText.gameObject.SetActive(true);

        if (currentState.RoundEndedByTimeUp)
        {
            this.roundResultText.text = this.GetTimeUpResultMessage(currentState.RoundWinnerSlot);
            return;
        }

        this.roundResultText.text = this.GetRoundResultMessage(currentState.RoundWinnerSlot);
    }

    /// <summary>
    /// returns the round result message for a time-up round end
    /// </summary>
    /// <param name="winnerSlot">the winning player slot, or 0 for no winner</param>
    /// <returns>time-up result message</returns>
    private string GetTimeUpResultMessage(int winnerSlot)
    {
        if (winnerSlot == 1)
        {
            return "TIME UP - PLAYER 1 WINS";
        }

        if (winnerSlot == 2)
        {
            return "TIME UP - PLAYER 2 WINS";
        }

        return "TIME UP - DRAW";
    }

    /// <summary>
    /// returns the correct round result message for the winning slot
    /// </summary>
    /// <param name="winnerSlot">the winning player slot, or 0 for no winner</param>
    /// <returns>round result message</returns>
    private string GetRoundResultMessage(int winnerSlot)
    {
        if (winnerSlot == 1)
        {
            return "PLAYER 1 WINS";
        }

        if (winnerSlot == 2)
        {
            return "PLAYER 2 WINS";
        }

        return "DOUBLE KO";
    }

    /// <summary>
    /// returns the final match result message for the winning slot
    /// </summary>
    /// <param name="winnerSlot">the winning player slot</param>
    /// <returns>match result message</returns>
    private string GetMatchResultMessage(int winnerSlot)
    {
        if (winnerSlot == 1)
        {
            return "PLAYER 1 WINS MATCH";
        }

        if (winnerSlot == 2)
        {
            return "PLAYER 2 WINS MATCH";
        }

        return "MATCH OVER";
    }

    /// <summary>
    /// updates the round score text from the authoritative match state
    /// </summary>
    private void UpdateRoundScoreText()
    {
        GameState currentState;

        if (this.gameManager == null)
        {
            this.roundScoreText.text = "P1 0 - 0 P2";
            return;
        }

        currentState = this.gameManager.CurrentAuthoritativeState;

        if (currentState == null)
        {
            this.roundScoreText.text = "P1 0 - 0 P2";
            return;
        }

        this.roundScoreText.text = "P1 " +
                                   currentState.Player01RoundWins +
                                   " - " +
                                   currentState.Player02RoundWins +
                                   " P2";
    }

    /// <summary>
    /// updates the round timer text from the authoritative match state
    /// </summary>
    private void UpdateRoundTimerText()
    {
        GameState currentState;
        int secondsRemaining;

        if (this.gameManager == null)
        {
            this.roundTimerText.text = "TIME 60";
            return;
        }

        currentState = this.gameManager.CurrentAuthoritativeState;

        if (currentState == null)
        {
            this.roundTimerText.text = "TIME 60";
            return;
        }

        secondsRemaining = this.GetTimerSecondsRemaining(currentState.RoundTimerTicksRemaining);
        this.roundTimerText.text = "TIME " + secondsRemaining;
    }

    /// <summary>
    /// converts remaining timer ticks into visible whole seconds
    /// </summary>
    /// <param name="ticksRemaining">timer ticks remaining</param>
    /// <returns>seconds remaining</returns>
    private int GetTimerSecondsRemaining(int ticksRemaining)
    {
        if (ticksRemaining <= 0)
        {
            return 0;
        }

        return (ticksRemaining + 59) / 60;
    }

    /// <summary>
    /// updates the match over panel from the authoritative match state
    /// </summary>
    private void UpdateMatchOverPanel()
    {
        GameState currentState;

        if (this.gameManager == null)
        {
            this.SetMatchOverPanelVisible(false);
            return;
        }

        currentState = this.gameManager.CurrentAuthoritativeState;

        if (currentState == null)
        {
            this.SetMatchOverPanelVisible(false);
            return;
        }

        if (currentState.MatchState != SimMatchState.MatchOver)
        {
            this.SetMatchOverPanelVisible(false);
            return;
        }

        this.SetMatchOverPanelVisible(true);
        this.UpdateMatchOverPanelText(currentState);
    }

    /// <summary>
    /// shows or hides the match over panel
    /// </summary>
    /// <param name="isVisible">true when the panel should be visible</param>
    private void SetMatchOverPanelVisible(bool isVisible)
    {
        if (this.matchOverPanel.activeSelf == isVisible)
        {
            return;
        }

        this.matchOverPanel.SetActive(isVisible);

        if (isVisible)
        {
            this.menuManager.SelectReturnToMenuButton();
        }
    }

    /// <summary>
    /// updates the visible text inside the match over panel
    /// </summary>
    /// <param name="currentState">latest authoritative gameplay state</param>
    private void UpdateMatchOverPanelText(GameState currentState)
    {
        this.matchOverWinnerText.text = this.GetMatchResultMessage(currentState.MatchWinnerSlot);
        this.matchOverScoreText.text ="FINAL SCORE: P1 " +currentState.Player01RoundWins + " - " + currentState.Player02RoundWins +" P2";
    }

    /// <summary>
    /// toggles the animated runtime combat body-region visualization
    /// </summary>
    public void ToggleCombatBoxView()
    {
        this.combatBoxViewEnabled = !this.combatBoxViewEnabled;
        CombatDebugVisualizer.SetDebugVisualsEnabled(this.combatBoxViewEnabled);
        this.UpdateCombatBoxToggleText();
    }

    /// <summary>
    /// updates the combat box toggle button label
    /// </summary>
    private void UpdateCombatBoxToggleText()
    {
        if (this.combatBoxViewEnabled)
        {
            this.combatBoxToggleText.text = "HIDE COMBAT BOXES";
            return;
        }

        this.combatBoxToggleText.text = "SHOW COMBAT BOXES";
    }
    #endregion
}