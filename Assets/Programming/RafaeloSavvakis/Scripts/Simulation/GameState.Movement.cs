/// <summary>
/// contains deterministic player movement, jump, step, gravity, and landing logic
/// </summary>
public partial class GameState
{
    #region movement methods
    /// <summary>
    /// starts one grounded step only when the player is allowed to step again
    /// </summary>
    /// <param name="player">the player state to update</param>
    /// <param name="horizontalAxis">allowed horizontal input axis</param>
    private void TryStartGroundStep(SimPlayerState player, int horizontalAxis)
    {
        if (horizontalAxis == 0)
        {
            player.RequiresMoveReleaseBeforeNextStep = false;
            return;
        }

        if (!player.IsGrounded)
        {
            return;
        }

        if (player.StepTicksRemaining > 0)
        {
            player.RequiresMoveReleaseBeforeNextStep = true;
            return;
        }

        if (player.StepRecoveryTicksRemaining > 0)
        {
            player.RequiresMoveReleaseBeforeNextStep = true;
            return;
        }

        if (player.StepVisualLockoutTicksRemaining > 0)
        {
            player.RequiresMoveReleaseBeforeNextStep = true;
            return;
        }

        if (player.RequiresMoveReleaseBeforeNextStep)
        {
            return;
        }

        player.StepTicksRemaining = GroundStepDurationTicks;
        player.StepVisualLockoutTicksRemaining = GroundStepVisualLockoutTicks;
        player.StepDirection = horizontalAxis > 0 ? 1 : -1;
        player.RequiresMoveReleaseBeforeNextStep = true;
    }

    /// <summary>
    /// applies one deterministic tick of grounded step movement
    /// </summary>
    /// <param name="player">the player state to update</param>
    private void ApplyGroundStep(SimPlayerState player)
    {
        FixedNumber targetX;

        if (player.StepTicksRemaining <= 0)
        {
            return;
        }

        targetX = player.Position.x + (player.StepDirection * GroundStepSpeed * FixedTickDelta);
        targetX = FixedNumber.Clamp(targetX, MinStageX, MaxStageX);

        player.Position = new FixedVector2(targetX, player.Position.y);

        player.StepTicksRemaining--;

        if (player.StepTicksRemaining <= 0)
        {
            player.StepRecoveryTicksRemaining = GroundStepRecoveryTicks;
        }
    }

    /// <summary>
    /// counts down the grounded step recovery window
    /// </summary>
    /// <param name="player">the player state to update</param>
    private void ApplyGroundStepRecovery(SimPlayerState player)
    {
        if (player.StepRecoveryTicksRemaining <= 0)
        {
            return;
        }

        player.StepRecoveryTicksRemaining--;
    }

    /// <summary>
    /// resets active grounded step state
    /// </summary>
    /// <param name="player">the player state to update</param>
    private void ResetGroundStepState(SimPlayerState player)
    {
        player.StepTicksRemaining = 0;
        player.StepRecoveryTicksRemaining = 0;
        player.StepVisualLockoutTicksRemaining = 0;
        player.StepDirection = 0;
        player.RequiresMoveReleaseBeforeNextStep = false;
    }

    /// <summary>
    /// counts down the visual step lockout window
    /// </summary>
    /// <param name="player">the player state to update</param>
    private void ApplyGroundStepVisualLockout(SimPlayerState player)
    {
        if (player.StepVisualLockoutTicksRemaining <= 0)
        {
            return;
        }

        player.StepVisualLockoutTicksRemaining--;
    }

    /// <summary>
    /// applies committed horizontal movement during a jump based on the direction chosen at takeoff
    /// </summary>
    /// <param name="player">the player state to update</param>
    private void ApplyAirJumpMovement(SimPlayerState player)
    {
        FixedNumber targetX;

        if (player.AirHorizontalDirection == 0)
        {
            return;
        }

        targetX = player.Position.x + (player.AirHorizontalDirection * AirJumpHorizontalSpeed * FixedTickDelta);
        targetX = FixedNumber.Clamp(targetX, MinStageX, MaxStageX);

        player.Position = new FixedVector2(targetX, player.Position.y);
    }

    /// <summary>
    /// applies vertical movement and gravity while the player is airborne
    /// </summary>
    /// <param name="player">the player state to update</param>
    private void ApplyVerticalMovement(SimPlayerState player)
    {
        FixedNumber nextY;

        if (player.IsGrounded)
        {
            player.VerticalVelocity = FixedNumber.ZERO;
            return;
        }

        nextY = player.Position.y + (player.VerticalVelocity * FixedTickDelta);
        player.VerticalVelocity = player.VerticalVelocity - (Gravity * FixedTickDelta);
        player.Position = new FixedVector2(player.Position.x, nextY);
    }

    /// <summary>
    /// applies the simple ground rule at y equals 0
    /// </summary>
    /// <param name="player">the player state to update</param>
    private void ApplyGroundRule(SimPlayerState player)
    {
        if (player.Position.y <= GroundY)
        {
            if (!player.IsGrounded && player.AirHorizontalDirection != 0)
            {
                player.MustReleaseHorizontalAfterLanding = true;
            }

            player.Position = new FixedVector2(player.Position.x, GroundY);
            player.IsGrounded = true;
            player.VerticalVelocity = FixedNumber.ZERO;
            player.AirHorizontalDirection = 0;
            return;
        }

        player.IsGrounded = false;
    }

    /// <summary>
    /// updates the high-level movement state after movement has been resolved
    /// </summary>
    /// <param name="player">the player state to update</param>
    /// <param name="stepWasActiveThisTick">true if a grounded step was active this tick</param>
    /// <param name="crouchHeld">true while crouch is active on the ground</param>
    private void UpdateMovementState(SimPlayerState player, bool stepWasActiveThisTick, bool crouchHeld)
    {
        if (!player.IsGrounded)
        {
            player.MovementState = SimMovementState.Jump;
            return;
        }

        if (crouchHeld)
        {
            player.MovementState = SimMovementState.Crouch;
            return;
        }

        if (stepWasActiveThisTick)
        {
            player.MovementState = SimMovementState.Walk;
            return;
        }

        player.MovementState = SimMovementState.Idle;
    }

    /// <summary>
    /// returns the horizontal input that is allowed after landing from a diagonal jump
    /// </summary>
    /// <param name="player">the player state to check</param>
    /// <param name="horizontalAxis">current horizontal input axis</param>
    /// <returns>allowed horizontal input axis</returns>
    private int GetAllowedGroundHorizontalAxis(SimPlayerState player, int horizontalAxis)
    {
        if (!player.MustReleaseHorizontalAfterLanding)
        {
            return horizontalAxis;
        }

        if (horizontalAxis == 0)
        {
            player.MustReleaseHorizontalAfterLanding = false;
        }

        return 0;
    }
    #endregion
}