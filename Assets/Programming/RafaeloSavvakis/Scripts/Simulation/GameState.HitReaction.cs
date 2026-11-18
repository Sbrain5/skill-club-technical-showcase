/// <summary>
/// contains deterministic hit detection, hit reaction, range, and knockback logic
/// </summary>
public partial class GameState
{
    #region hit reaction methods
    /// <summary>
    /// resolves simple deterministic hit reactions between both players
    /// </summary>
    /// <param name="player01">player one state</param>
    /// <param name="player02">player two state</param>
    private void ResolveHitReactions(SimPlayerState player01, SimPlayerState player02)
    {
        bool player01HitsPlayer02;
        bool player02HitsPlayer01;

        player01HitsPlayer02 = this.CanActiveAttackHit(player01, player02);
        player02HitsPlayer01 = this.CanActiveAttackHit(player02, player01);

        if (player01HitsPlayer02)
        {
            this.ApplyBasicHitstun(player02, player01);
        }

        if (player02HitsPlayer01)
        {
            this.ApplyBasicHitstun(player01, player02);
        }
    }

    /// <summary>
    /// checks whether an active attack can place the defender into hitstun
    /// </summary>
    /// <param name="attacker">attacking player state</param>
    /// <param name="defender">defending player state</param>
    /// <returns>true if the attack should trigger hitstun</returns>
    private bool CanActiveAttackHit(SimPlayerState attacker, SimPlayerState defender)
    {
        if (attacker.AttackState == SimAttackState.None)
        {
            return false;
        }

        if (!attacker.IsAttackHitboxActive)
        {
            return false;
        }

        if (attacker.ActiveHitWindowId < 0)
        {
            return false;
        }

        if (defender.IsInHitstun)
        {
            return false;
        }

        if (this.HasAlreadyReceivedThisAttack(defender, attacker))
        {
            return false;
        }

        if (!this.IsDefenderInFront(attacker, defender))
        {
            return false;
        }

        if (!this.ArePlayersCloseEnoughForHit(attacker, defender))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// applies the smallest safe hitstun reaction to the defender
    /// </summary>
    /// <param name="defender">the player receiving the hit</param>
    /// <param name="attacker">the player causing the hit</param>
    private void ApplyBasicHitstun(SimPlayerState defender, SimPlayerState attacker)
    {
        defender.IsInHitstun = true;
        defender.HitstunTicksRemaining = BasicHitstunTicks;
        defender.LastReceivedHitWindowId = attacker.ActiveHitWindowId;
        defender.LastReceivedAttackStartFrame = this.GetAttackStartFrame(attacker);

        this.ApplyBasicDamage(defender, attacker.AttackState);
        this.ResetAttackState(defender);
        this.ResetGroundStepState(defender);
        this.ApplyBasicKnockback(defender, attacker);
    }

    /// <summary>
    /// pushes the defender a small deterministic distance away from the attacker
    /// </summary>
    /// <param name="defender">the player receiving knockback</param>
    /// <param name="attacker">the player causing knockback</param>
    private void ApplyBasicKnockback(SimPlayerState defender, SimPlayerState attacker)
    {
        FixedNumber knockbackDirection;
        FixedNumber targetX;

        knockbackDirection = this.GetKnockbackDirection(defender, attacker);
        targetX = defender.Position.x + (knockbackDirection * BasicKnockbackDistance);
        targetX = FixedNumber.Clamp(targetX, MinStageX, MaxStageX);

        defender.Position = new FixedVector2(targetX, defender.Position.y);
    }

    /// <summary>
    /// gets the horizontal knockback direction away from the attacker
    /// </summary>
    /// <param name="defender">the player receiving knockback</param>
    /// <param name="attacker">the player causing knockback</param>
    /// <returns>-1 for left knockback, 1 for right knockback</returns>
    private FixedNumber GetKnockbackDirection(SimPlayerState defender, SimPlayerState attacker)
    {
        if (defender.Position.x > attacker.Position.x)
        {
            return FixedNumber.ONE;
        }

        if (defender.Position.x < attacker.Position.x)
        {
            return FixedNumber.ZERO - FixedNumber.ONE;
        }

        if (attacker.IsFacingRight)
        {
            return FixedNumber.ONE;
        }

        return FixedNumber.ZERO - FixedNumber.ONE;
    }

    /// <summary>
    /// checks whether the defender already reacted to this exact attack instance
    /// </summary>
    /// <param name="defender">the defending player state</param>
    /// <param name="attacker">the attacking player state</param>
    /// <returns>true if this attack already caused hitstun</returns>
    private bool HasAlreadyReceivedThisAttack(SimPlayerState defender, SimPlayerState attacker)
    {
        int attackStartFrame;

        attackStartFrame = this.GetAttackStartFrame(attacker);

        return defender.LastReceivedHitWindowId == attacker.ActiveHitWindowId &&
               defender.LastReceivedAttackStartFrame == attackStartFrame;
    }

    /// <summary>
    /// calculates the deterministic frame where the current attack started
    /// </summary>
    /// <param name="attacker">the attacking player state</param>
    /// <returns>attack start frame</returns>
    private int GetAttackStartFrame(SimPlayerState attacker)
    {
        return this.Frame - attacker.AttackTick + 1;
    }

    /// <summary>
    /// checks if the defender is in front of the attacker
    /// </summary>
    /// <param name="attacker">attacking player state</param>
    /// <param name="defender">defending player state</param>
    /// <returns>true if the defender is in front</returns>
    private bool IsDefenderInFront(SimPlayerState attacker, SimPlayerState defender)
    {
        if (attacker.IsFacingRight)
        {
            return defender.Position.x >= attacker.Position.x;
        }

        return defender.Position.x <= attacker.Position.x;
    }

    /// <summary>
    /// checks whether the attacker's active deterministic attack boxes overlap the defender's deterministic hurtboxes
    /// </summary>
    /// <param name="attacker">attacking player state</param>
    /// <param name="defender">defending player state</param>
    /// <returns>true when the active attack overlaps a defender hurtbox</returns>
    private bool ArePlayersCloseEnoughForHit(SimPlayerState attacker,SimPlayerState defender)
    {
        SimFighterProfile attackerProfile;
        SimFighterProfile defenderProfile;
        SimCombatBoxSet attackBoxes;
        SimCombatBoxSet hurtboxes;

        if (!attacker.IsAttackHitboxActive)
        {
            return false;
        }

        if (defender.MovementState == SimMovementState.Crouch && attacker.AttackState == SimAttackState.LightAttack)
        {
            return false;
        }


        attackerProfile =
            this.GetFighterProfileForPlayer(attacker);

        defenderProfile =
            this.GetFighterProfileForPlayer(defender);

        attackBoxes =
            this.GetAttackBoxes(
                attacker,
                attackerProfile);

        hurtboxes =
            this.GetHurtboxes(
                defender,
                defenderProfile);

        return attackBoxes.Overlaps(hurtboxes);
    }
    #endregion
}