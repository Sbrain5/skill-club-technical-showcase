using UnityEngine;

/// <summary>
/// applies authoritative combat state to the player's Unity hitbox
/// </summary>
public sealed class PlayerCombat : MonoBehaviour
{
    #region variables
    [Header("References")]
    [SerializeField] private Hitbox attackHitbox;

    [Header("Runtime State")]
    [SerializeField] private SimAttackState currentAttackState = SimAttackState.None;
    [SerializeField] private bool isAttacking;
    [SerializeField] private bool isAttackHitboxActive;
    [SerializeField] private bool isFacingRight = true;
    [SerializeField] private int attackTick;
    [SerializeField] private int activeHitWindowId = -1;

    private bool hitWindowOpened;
    #endregion

    #region properties
    /// <summary>
    /// gets the current authoritative attack type
    /// </summary>
    public SimAttackState CurrentAttackState
    {
        get
        {
            return this.currentAttackState;
        }
    }

    /// <summary>
    /// gets whether the player is currently inside an attack sequence
    /// </summary>
    public bool IsAttacking
    {
        get
        {
            return this.isAttacking;
        }
    }

    /// <summary>
    /// gets the latest authoritative facing direction
    /// </summary>
    public bool IsFacingRight
    {
        get
        {
            return this.isFacingRight;
        }
    }

    /// <summary>
    /// gets whether the authoritative attack hitbox is currently active
    /// </summary>
    public bool IsAttackHitboxActive
    {
        get
        {
            return this.isAttackHitboxActive;
        }
    }

    /// <summary>
    /// gets the current attack tick
    /// </summary>
    public int AttackTick
    {
        get
        {
            return this.attackTick;
        }
    }

    /// <summary>
    /// gets the current active hit window id
    /// </summary>
    public int ActiveHitWindowId
    {
        get
        {
            return this.activeHitWindowId;
        }
    }
    #endregion

    #region unity methods
    /// <summary>
    /// closes the attack hitbox when the fighter is initialized
    /// </summary>
    private void Awake()
    {
        this.CloseAttackHitbox();
    }
    #endregion

    #region methods
    /// <summary>
    /// applies the authoritative combat state produced by the deterministic simulation
    /// </summary>
    /// <param name="state">authoritative simulated player state</param>
    public void ApplyAuthoritativeCombatState(SimPlayerState state)
    {
        this.currentAttackState = state.AttackState;
        this.isAttacking = state.AttackState != SimAttackState.None;
        this.isAttackHitboxActive = state.IsAttackHitboxActive;
        this.isFacingRight = state.IsFacingRight;
        this.attackTick = state.AttackTick;
        this.activeHitWindowId = state.ActiveHitWindowId;

        if (!this.ShouldOpenHitbox())
        {
            this.CloseAttackHitbox();
            return;
        }

        this.OpenAttackHitbox(state.ActiveHitWindowId);
    }

    /// <summary>
    /// immediately clears the mirrored combat state and closes the hitbox
    /// </summary>
    public void ResetCombat()
    {
        this.currentAttackState = SimAttackState.None;
        this.isAttacking = false;
        this.isAttackHitboxActive = false;
        this.attackTick = 0;
        this.activeHitWindowId = -1;
        this.CloseAttackHitbox();
    }

    /// <summary>
    /// checks if the authoritative state says the hitbox should be open
    /// </summary>
    /// <returns>true if the hitbox should be open</returns>
    private bool ShouldOpenHitbox()
    {
        if (!this.isAttacking)
        {
            return false;
        }

        if (!this.isAttackHitboxActive)
        {
            return false;
        }

        if (this.activeHitWindowId < 0)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// opens the assigned attack hitbox for the given hit window
    /// </summary>
    /// <param name="hitWindowId">authoritative hit window id</param>
    private void OpenAttackHitbox(int hitWindowId)
    {
        if (this.hitWindowOpened &&
            this.attackHitbox.CurrentHitWindowId == hitWindowId)
        {
            return;
        }

        this.attackHitbox.OpenHitWindow(hitWindowId);
        this.hitWindowOpened = true;
    }

    /// <summary>
    /// closes the assigned attack hitbox
    /// </summary>
    private void CloseAttackHitbox()
    {
        this.attackHitbox.CloseHitWindow();
        this.hitWindowOpened = false;
    }
    #endregion
}