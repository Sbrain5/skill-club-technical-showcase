/// <summary>
/// represents the authoritative per-player gameplay state
/// </summary>
public sealed class SimPlayerState
{
    #region variables
    public FixedVector2 Position;
    public FixedNumber PositionZ;
    public FixedNumber VerticalVelocity;

    public int AirHorizontalDirection;

    public bool WasJumpHeldLastFrame;
    public bool MustReleaseHorizontalAfterLanding;
    public bool IsGrounded;
    public bool IsFacingRight;

    public SimMovementState MovementState;

    public int MaxHealth;
    public int CurrentHealth;

    public SimAttackState AttackState;
    public int AttackTick;
    public bool IsAttackHitboxActive;
    public int ActiveHitWindowId;

    public bool IsInHitstun;
    public int HitstunTicksRemaining;
    public int LastReceivedHitWindowId;
    public int LastReceivedAttackStartFrame;

    public int StepTicksRemaining;
    public int StepDirection;
    public int StepRecoveryTicksRemaining;
    public bool RequiresMoveReleaseBeforeNextStep;
    public int StepVisualLockoutTicksRemaining;
    #endregion

    #region constructors
    /// <summary>
    /// creates a clean player state container
    /// </summary>
    public SimPlayerState()
    {
        this.Position = new FixedVector2(FixedNumber.ZERO, FixedNumber.ZERO);
        this.PositionZ = FixedNumber.ZERO;
        this.VerticalVelocity = FixedNumber.ZERO;

        this.AirHorizontalDirection = 0;

        this.WasJumpHeldLastFrame = false;
        this.MustReleaseHorizontalAfterLanding = false;
        this.IsGrounded = true;
        this.IsFacingRight = true;

        this.MovementState = SimMovementState.Idle;

        this.MaxHealth = 0;
        this.CurrentHealth = 0;

        this.AttackState = SimAttackState.None;
        this.AttackTick = 0;
        this.IsAttackHitboxActive = false;
        this.ActiveHitWindowId = -1;

        this.IsInHitstun = false;
        this.HitstunTicksRemaining = 0;
        this.LastReceivedHitWindowId = -1;
        this.LastReceivedAttackStartFrame = -1;

        this.StepTicksRemaining = 0;
        this.StepDirection = 0;
        this.StepRecoveryTicksRemaining = 0;
        this.RequiresMoveReleaseBeforeNextStep = false;
        this.StepVisualLockoutTicksRemaining = 0;
    }

    /// <summary>
    /// creates a deep copy of another player state
    /// </summary>
    /// <param name="other">the source player state to copy</param>
    public SimPlayerState(SimPlayerState other)
    {
        this.Position = other.Position;
        this.PositionZ = other.PositionZ;
        this.VerticalVelocity = other.VerticalVelocity;

        this.AirHorizontalDirection = other.AirHorizontalDirection;

        this.WasJumpHeldLastFrame = other.WasJumpHeldLastFrame;
        this.MustReleaseHorizontalAfterLanding = other.MustReleaseHorizontalAfterLanding;
        this.IsGrounded = other.IsGrounded;
        this.IsFacingRight = other.IsFacingRight;

        this.MovementState = other.MovementState;

        this.MaxHealth = other.MaxHealth;
        this.CurrentHealth = other.CurrentHealth;

        this.AttackState = other.AttackState;
        this.AttackTick = other.AttackTick;
        this.IsAttackHitboxActive = other.IsAttackHitboxActive;
        this.ActiveHitWindowId = other.ActiveHitWindowId;

        this.IsInHitstun = other.IsInHitstun;
        this.HitstunTicksRemaining = other.HitstunTicksRemaining;
        this.LastReceivedHitWindowId = other.LastReceivedHitWindowId;
        this.LastReceivedAttackStartFrame = other.LastReceivedAttackStartFrame;

        this.StepTicksRemaining = other.StepTicksRemaining;
        this.StepDirection = other.StepDirection;
        this.StepRecoveryTicksRemaining = other.StepRecoveryTicksRemaining;
        this.RequiresMoveReleaseBeforeNextStep = other.RequiresMoveReleaseBeforeNextStep;
        this.StepVisualLockoutTicksRemaining = other.StepVisualLockoutTicksRemaining;
    }
    #endregion
}