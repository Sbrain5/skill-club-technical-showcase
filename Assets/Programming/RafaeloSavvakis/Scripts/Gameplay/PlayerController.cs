using UnityEngine;

/// <summary>
/// applies authoritative rollback state to the visual player object
/// </summary>
public sealed partial class PlayerController : MonoBehaviour
{
    #region enums
    /// <summary>
    /// identifies which player this controller belongs to
    /// </summary>
    public enum PlayerSlot
    {
        Player01 = 0,
        Player02 = 1
    }
    #endregion

    #region variables
    [Header("Identity")]
    [SerializeField] private PlayerSlot playerSlot = PlayerSlot.Player01;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Visual Settings")]
    [SerializeField] private float crouchVisualScaleY = 0.6f;
    [SerializeField] private bool enableVisualSmoothing = true;
    [SerializeField] private float localVisualSmoothTime = 0.0f;
    [SerializeField] private float remoteVisualSmoothTime = 0.04f;
    [SerializeField] private float visualSnapDistance = 1.0f;

    [Header("Character Visuals")]
    [SerializeField] private GameObject[] characterVisuals;
    private int characterIndex;

    [Header("Animation")]
    [SerializeField] private Animator[] characterAnimators;

    [Header("Audio")]
    [SerializeField] private FighterVoiceController fighterVoice;

    private Vector3 visualRestLocalPosition;
    private Vector3 visualSmoothVelocity;

    private bool isLocalPresentation;
    private bool hasReceivedAuthoritativeState;
    #endregion

    #region properties
    /// <summary>
    /// gets the assigned player slot
    /// </summary>
    public PlayerSlot AssignedPlayerSlot
    {
        get
        {
            return this.playerSlot;
        }
    }
    #endregion

    #region unity methods
    /// <summary>
    /// Keeps inspector values within valid ranges and refreshes editor presentation
    /// </summary>
    private void OnValidate()
    {
        if (this.crouchVisualScaleY < 0.1f)
        {
            this.crouchVisualScaleY = 0.1f;
        }

        if (this.crouchVisualScaleY > 1.0f)
        {
            this.crouchVisualScaleY = 1.0f;
        }

        if (this.localVisualSmoothTime < 0.0f)
        {
            this.localVisualSmoothTime = 0.0f;
        }

        if (this.remoteVisualSmoothTime < 0.0f)
        {
            this.remoteVisualSmoothTime = 0.0f;
        }

        if (this.visualSnapDistance < 0.1f)
        {
            this.visualSnapDistance = 0.1f;
        }

        this.ApplyCharacterVisual();
        this.ApplyPlayerSlotToCombatAnchors();
    }

    /// <summary>
    /// smooths the visual model back toward the authoritative gameplay root
    /// </summary>
    private void LateUpdate()
    {
        this.UpdateVisualPresentationSmoothing();
    }

    /// <summary>
    /// initializes the selected character and visual presentation state
    /// </summary>
    private void Awake()
    {
        this.ApplyCharacterVisual();
        this.CacheVisualRestPosition();
        this.ResetVisualPresentation();
    }
    #endregion

    #region methods
    /// <summary>
    /// sets the player slot for this controller
    /// </summary>
    /// <param name="slot">player slot</param>
    public void SetPlayerSlot(PlayerSlot slot)
    {
        this.playerSlot = slot;
        this.ApplyPlayerSlotToCombatAnchors();
    }

    /// <summary>
    /// sets the selected character visual
    /// </summary>
    /// <param name="index">character index</param>
    public void SetCharacterIndex(int index)
    {
        if (characterVisuals.Length == 0)
        {
            return;
        }

        if (index < 0 || index >= characterVisuals.Length)
        {
            index = 0;
        }

        characterIndex = index;
        this.ApplyCharacterVisual();
        this.ApplyPlayerSlotToCombatAnchors();
        this.fighterVoice.SetCharacterIndex(this.characterIndex);

    }

    /// <summary>
    /// applies one authoritative rollback state to this spawned player object
    /// </summary>
    /// <param name="state">the authoritative simulated player state</param>
    public void ApplyAuthoritativeState(SimPlayerState state)
    {
        Vector3 previousRootPosition;
        Vector3 previousVisualWorldPosition;
        Vector3 updatedPosition;

        previousRootPosition = this.transform.position;
        previousVisualWorldPosition = this.visualRoot.position;

        updatedPosition = this.transform.position;
        updatedPosition.x = state.Position.x.ToFloat;
        updatedPosition.y = state.Position.y.ToFloat;
        updatedPosition.z = state.PositionZ.ToFloat;

        this.transform.position = updatedPosition;
        this.ApplyVisualPresentationCorrection(previousRootPosition, previousVisualWorldPosition, updatedPosition);

        this.ApplyVisualFacing(state.IsFacingRight);
        this.ApplyCrouchVisual(state.MovementState == SimMovementState.Crouch, state.IsGrounded);
        this.ApplyAnimatorState(state);
        this.ApplyAuthoritativeHealthState(state);
        this.fighterVoice.ApplyPlayerState(state);
    }

    /// <summary>
    /// applies authoritative health state to the local health bridge
    /// </summary>
    /// <param name="state">authoritative player state</param>
    private void ApplyAuthoritativeHealthState(SimPlayerState state)
    {
        this.playerHealth.ApplyAuthoritativeHealthState(state);
    }

    /// <summary>
    /// applies one authoritative rollback combat state to this spawned player object
    /// </summary>
    /// <param name="state">the authoritative simulated player state</param>
    public void ApplyAuthoritativeCombatState(SimPlayerState state)
    {
        this.playerCombat.ApplyAuthoritativeCombatState(state);
    }

    /// <summary>
    /// applies authoritative match state to local fighter voice
    /// </summary>
    /// <param name="state">authoritative game state</param>
    public void ApplyAuthoritativeMatchVoiceState(GameState state)
    {
        this.fighterVoice.ApplyMatchState(state, this.playerSlot);
    }
    #endregion
}