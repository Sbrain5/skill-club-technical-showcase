/// <summary>
/// contains deterministic attack startup, active, recovery, and hit window logic
/// </summary>
public partial class GameState
{
    #region attack methods
    /// <summary>
    /// applies one deterministic attack timing step for a single player
    /// </summary>
    /// <param name="player">the player state to update</param>
    /// <param name="frameInput">the current serialized input for that player</param>
    /// <param name="hitWindowOffset">unique player offset used when creating deterministic hit window ids</param>
    private void ApplyAttackTick(SimPlayerState player, InputSerialization.FrameInput frameInput, int hitWindowOffset)
    {
        if (frameInput.punch == InputSerialization.ButtonState.Pressed)
        {
            this.TryStartAttack(player, SimAttackState.LightAttack, hitWindowOffset);
        }
        else if (frameInput.kick == InputSerialization.ButtonState.Pressed)
        {
            this.TryStartAttack(player, SimAttackState.MediumAttack, hitWindowOffset);
        }
        else if (frameInput.heavyAttack == InputSerialization.ButtonState.Pressed)
        {
            this.TryStartAttack(player, SimAttackState.HeavyAttack, hitWindowOffset);
        }

        if (player.AttackState == SimAttackState.None)
        {
            this.ResetAttackState(player);
            return;
        }

        player.AttackTick++;

        this.UpdateAttackWindow(player);

        if (player.AttackTick >= this.GetTotalAttackTicks(player.AttackState))
        {
            this.FinishAttack(player);
        }
    }

    /// <summary>
    /// advances an active attack while preventing normal movement input
    /// </summary>
    /// <param name="player">the player state to update</param>
    /// <param name="frameInput">the current serialized input for that player</param>
    /// <param name="hitWindowOffset">unique player offset used when creating deterministic hit window ids</param>
    private void ApplyAttackMovementLock(SimPlayerState player, InputSerialization.FrameInput frameInput, int hitWindowOffset)
    {
        this.UpdateAttackHorizontalReleaseLock(player, frameInput);

        this.ApplyVerticalMovement(player);
        this.ApplyGroundRule(player);

        if (player.IsGrounded)
        {
            player.MovementState = SimMovementState.Idle;
        }
        else
        {
            player.MovementState = SimMovementState.Jump;
        }

        this.ApplyAttackTick(player, frameInput, hitWindowOffset);
    }

    /// <summary>
    /// prevents horizontal movement held during an attack from becoming an immediate step after the attack
    /// </summary>
    /// <param name="player">the player state to update</param>
    /// <param name="frameInput">the current serialized input for that player</param>
    private void UpdateAttackHorizontalReleaseLock(SimPlayerState player, InputSerialization.FrameInput frameInput)
    {
        int horizontalAxis;
        InputSerialization.GetAxisFromDirection(frameInput.direction, out horizontalAxis, out _);

        if (horizontalAxis != 0)
        {
            player.RequiresMoveReleaseBeforeNextStep = true;
            return;
        }

        player.RequiresMoveReleaseBeforeNextStep = false;
    }

    /// <summary>
    /// starts a grounded attack if the player is allowed to attack
    /// </summary>
    /// <param name="player">the player state to update</param>
    /// <param name="attackState">attack state to start</param>
    /// <param name="hitWindowOffset">unique player offset used when creating deterministic hit window ids</param>
    private void TryStartAttack(SimPlayerState player, SimAttackState attackState, int hitWindowOffset)
    {
        if (!player.IsGrounded)
        {
            return;
        }

        if (player.AttackState != SimAttackState.None)
        {
            return;
        }

        if (player.IsInHitstun)
        {
            return;
        }

        player.AttackState = attackState;
        player.AttackTick = 0;
        player.IsAttackHitboxActive = false;
        player.ActiveHitWindowId = this.GetHitWindowId(hitWindowOffset, attackState);
        this.ResetGroundStepState(player);
    }

    /// <summary>
    /// updates whether the current attack is inside its active hitbox window
    /// </summary>
    /// <param name="player">the player state to update</param>
    private void UpdateAttackWindow(SimPlayerState player)
    {
        int activeStartTick;
        int activeEndTick;
        activeStartTick = this.GetAttackStartupTicks(player.AttackState) + 1;
        activeEndTick = this.GetAttackStartupTicks(player.AttackState) + this.GetAttackActiveTicks(player.AttackState);
        player.IsAttackHitboxActive = player.AttackTick >= activeStartTick && player.AttackTick <= activeEndTick;
    }

    /// <summary>
    /// finishes the current attack and resets attack-only state
    /// </summary>
    /// <param name="player">the player state to update</param>
    private void FinishAttack(SimPlayerState player)
    {
        this.ResetAttackState(player);
    }

    /// <summary>
    /// resets attack-only state to the inactive values
    /// </summary>
    /// <param name="player">the player state to update</param>
    private void ResetAttackState(SimPlayerState player)
    {
        player.AttackState = SimAttackState.None;
        player.AttackTick = 0;
        player.IsAttackHitboxActive = false;
        player.ActiveHitWindowId = -1;
    }

    /// <summary>
    /// creates a deterministic hit window id from the player offset and attack type
    /// </summary>
    /// <param name="hitWindowOffset">unique player offset</param>
    /// <param name="attackState">the attack state used to create an attack-specific id</param>
    /// <returns>deterministic hit window id</returns>
    private int GetHitWindowId(int hitWindowOffset, SimAttackState attackState)
    {
        return this.GetAttackHitWindowOffset(attackState) + hitWindowOffset;
    }
    #endregion

    #region attack timing methods
    /// <summary>
    /// gets the startup duration for the selected attack
    /// </summary>
    /// <param name="attackState">the current attack state</param>
    /// <returns>startup duration in deterministic gameplay ticks</returns>
    private int GetAttackStartupTicks(SimAttackState attackState)
    {
        switch (attackState)
        {
            case SimAttackState.LightAttack:
                return LightAttackStartupTicks;

            case SimAttackState.MediumAttack:
                return MediumAttackStartupTicks;

            case SimAttackState.HeavyAttack:
                return HeavyAttackStartupTicks;

            default:
                return 0;
        }
    }

    /// <summary>
    /// gets the active hitbox duration for the selected attack
    /// </summary>
    /// <param name="attackState">the current attack state</param>
    /// <returns>active duration in deterministic gameplay ticks</returns>
    private int GetAttackActiveTicks(SimAttackState attackState)
    {
        switch (attackState)
        {
            case SimAttackState.LightAttack:
                return LightAttackActiveTicks;

            case SimAttackState.MediumAttack:
                return MediumAttackActiveTicks;

            case SimAttackState.HeavyAttack:
                return HeavyAttackActiveTicks;

            default:
                return 0;
        }
    }

    /// <summary>
    /// gets the recovery duration for the selected attack
    /// </summary>
    /// <param name="attackState">the current attack state</param>
    /// <returns>recovery duration in deterministic gameplay ticks</returns>
    private int GetAttackRecoveryTicks(SimAttackState attackState)
    {
        switch (attackState)
        {
            case SimAttackState.LightAttack:
                return LightAttackRecoveryTicks;

            case SimAttackState.MediumAttack:
                return MediumAttackRecoveryTicks;

            case SimAttackState.HeavyAttack:
                return HeavyAttackRecoveryTicks;

            default:
                return 0;
        }
    }

    /// <summary>
    /// gets the attack-specific hit window offset
    /// </summary>
    /// <param name="attackState">the current attack state</param>
    /// <returns>attack-specific hit window offset</returns>
    private int GetAttackHitWindowOffset(SimAttackState attackState)
    {
        switch (attackState)
        {
            case SimAttackState.LightAttack:
                return LightAttackHitWindowOffset;

            case SimAttackState.MediumAttack:
                return MediumAttackHitWindowOffset;

            case SimAttackState.HeavyAttack:
                return HeavyAttackHitWindowOffset;

            default:
                return 0;
        }
    }

    /// <summary>
    /// gets the total attack duration in deterministic gameplay ticks
    /// </summary>
    /// <param name="attackState">the current attack state</param>
    /// <returns>total attack ticks</returns>
    private int GetTotalAttackTicks(SimAttackState attackState)
    {
        return this.GetAttackStartupTicks(attackState) + this.GetAttackActiveTicks(attackState) + this.GetAttackRecoveryTicks(attackState);
    }
    #endregion
}