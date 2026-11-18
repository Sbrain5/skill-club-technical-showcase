/// <summary>
/// contains deterministic attack-box and hurtbox definitions
/// </summary>
public partial class GameState
{
    #region hurtbox constants
    private static readonly FixedNumber Fighter02UpperHurtboxOffsetY =
        FixedNumber.FromFloat(1.05f);

    private static readonly FixedNumber Fighter02UpperHurtboxHalfWidth =
        FixedNumber.FromFloat(0.31f);

    private static readonly FixedNumber Fighter02UpperHurtboxHalfHeight =
        FixedNumber.FromFloat(0.52f);

    private static readonly FixedNumber Fighter02LowerHurtboxOffsetY =
        FixedNumber.FromFloat(0.38f);

    private static readonly FixedNumber Fighter02LowerHurtboxHalfWidth =
        FixedNumber.FromFloat(0.29f);

    private static readonly FixedNumber Fighter02LowerHurtboxHalfHeight =
        FixedNumber.FromFloat(0.38f);

    private static readonly FixedNumber Fighter05UpperHurtboxOffsetY =
        FixedNumber.FromFloat(1.15f);

    private static readonly FixedNumber Fighter05UpperHurtboxHalfWidth =
        FixedNumber.FromFloat(0.32f);

    private static readonly FixedNumber Fighter05UpperHurtboxHalfHeight =
        FixedNumber.FromFloat(0.58f);

    private static readonly FixedNumber Fighter05LowerHurtboxOffsetY =
        FixedNumber.FromFloat(0.42f);

    private static readonly FixedNumber Fighter05LowerHurtboxHalfWidth =
        FixedNumber.FromFloat(0.30f);

    private static readonly FixedNumber Fighter05LowerHurtboxHalfHeight =
        FixedNumber.FromFloat(0.42f);
    #endregion

    #region attack box constants
    private static readonly FixedNumber LightAttackForwardOffset =
        FixedNumber.FromFloat(0.66f);

    private static readonly FixedNumber LightAttackHalfWidth =
        FixedNumber.FromFloat(0.22f);

    private static readonly FixedNumber LightAttackHalfHeight =
        FixedNumber.FromFloat(0.20f);

    private static readonly FixedNumber Fighter05MediumAttackForwardOffset =
    FixedNumber.FromFloat(0.70f);

    private static readonly FixedNumber Fighter05MediumAttackHalfWidth =
        FixedNumber.FromFloat(0.24f);

    private static readonly FixedNumber Fighter02MediumAttackForwardOffset =
        FixedNumber.FromFloat(0.75f);

    private static readonly FixedNumber Fighter02MediumAttackHalfWidth =
        FixedNumber.FromFloat(0.30f);

    private static readonly FixedNumber Fighter05MediumAttackHalfHeight =
    FixedNumber.FromFloat(0.24f);

    private static readonly FixedNumber Fighter02MediumAttackHalfHeight =
        FixedNumber.FromFloat(0.30f);

    private static readonly FixedNumber Fighter02HeavyForwardOffset =
        FixedNumber.FromFloat(0.72f);

    private static readonly FixedNumber Fighter02HeavyHalfWidth =
        FixedNumber.FromFloat(0.24f);

    private static readonly FixedNumber Fighter02HeavyHalfHeight =
        FixedNumber.FromFloat(0.24f);

    private static readonly FixedNumber Fighter05HeavyPrimaryHalfWidth =
        FixedNumber.FromFloat(0.24f);

    private static readonly FixedNumber Fighter05HeavyPrimaryHalfHeight =
        FixedNumber.FromFloat(0.22f);

    private static readonly FixedNumber Fighter05HeavySecondaryHalfWidth =
        FixedNumber.FromFloat(0.22f);

    private static readonly FixedNumber Fighter05HeavySecondaryHalfHeight =
    FixedNumber.FromFloat(0.20f);
    #endregion

    #region crouch hurtbox constants
    private static readonly FixedNumber Fighter02CrouchUpperHurtboxOffsetY =
        FixedNumber.FromFloat(0.75f);

    private static readonly FixedNumber Fighter02CrouchUpperHurtboxHalfWidth =
        FixedNumber.FromFloat(0.34f);

    private static readonly FixedNumber Fighter02CrouchUpperHurtboxHalfHeight =
        FixedNumber.FromFloat(0.45f);

    private static readonly FixedNumber Fighter02CrouchLowerHurtboxOffsetY =
        FixedNumber.FromFloat(0.25f);

    private static readonly FixedNumber Fighter02CrouchLowerHurtboxHalfWidth =
        FixedNumber.FromFloat(0.32f);

    private static readonly FixedNumber Fighter02CrouchLowerHurtboxHalfHeight =
        FixedNumber.FromFloat(0.25f);

    private static readonly FixedNumber Fighter05CrouchUpperHurtboxOffsetY =
        FixedNumber.FromFloat(0.78f);

    private static readonly FixedNumber Fighter05CrouchUpperHurtboxHalfWidth =
        FixedNumber.FromFloat(0.36f);

    private static readonly FixedNumber Fighter05CrouchUpperHurtboxHalfHeight =
        FixedNumber.FromFloat(0.45f);

    private static readonly FixedNumber Fighter05CrouchLowerHurtboxOffsetY =
        FixedNumber.FromFloat(0.28f);

    private static readonly FixedNumber Fighter05CrouchLowerHurtboxHalfWidth =
        FixedNumber.FromFloat(0.34f);

    private static readonly FixedNumber Fighter05CrouchLowerHurtboxHalfHeight =
        FixedNumber.FromFloat(0.27f);
    #endregion

    #region air hurtbox constants
    private static readonly FixedNumber Fighter02AirUpperHurtboxOffsetY =
        FixedNumber.FromFloat(2.05f);

    private static readonly FixedNumber Fighter02AirUpperHurtboxHalfWidth =
        FixedNumber.FromFloat(0.30f);

    private static readonly FixedNumber Fighter02AirUpperHurtboxHalfHeight =
        FixedNumber.FromFloat(0.45f);

    private static readonly FixedNumber Fighter02AirLowerHurtboxOffsetY =
        FixedNumber.FromFloat(1.45f);

    private static readonly FixedNumber Fighter02AirLowerHurtboxHalfWidth =
        FixedNumber.FromFloat(0.27f);

    private static readonly FixedNumber Fighter02AirLowerHurtboxHalfHeight =
        FixedNumber.FromFloat(0.30f);

    private static readonly FixedNumber Fighter05AirUpperHurtboxOffsetY =
    FixedNumber.FromFloat(2.45f);

    private static readonly FixedNumber Fighter05AirUpperHurtboxHalfWidth =
        FixedNumber.FromFloat(0.31f);

    private static readonly FixedNumber Fighter05AirUpperHurtboxHalfHeight =
        FixedNumber.FromFloat(0.42f);

    private static readonly FixedNumber Fighter05AirLowerHurtboxOffsetY =
        FixedNumber.FromFloat(1.90f);

    private static readonly FixedNumber Fighter05AirLowerHurtboxHalfWidth =
        FixedNumber.FromFloat(0.28f);

    private static readonly FixedNumber Fighter05AirLowerHurtboxHalfHeight =
        FixedNumber.FromFloat(0.22f);

    #endregion


    #region public methods
    /// <summary>
    /// returns the current deterministic attack boxes for one player
    /// </summary>
    /// <param name="playerSlot">player slot, using 1 or 2</param>
    /// <returns>current attack-box set</returns>
    public SimCombatBoxSet GetPlayerAttackBoxes(int playerSlot)
    {
        if (playerSlot == 1)
        {
            return this.GetAttackBoxes(
                this.Player01,
                this.Player01FighterProfile);
        }

        if (playerSlot == 2)
        {
            return this.GetAttackBoxes(
                this.Player02,
                this.Player02FighterProfile);
        }

        return SimCombatBoxSet.CreateEmpty();
    }

    /// <summary>
    /// returns the deterministic hurtboxes for one player
    /// </summary>
    /// <param name="playerSlot">player slot, using 1 or 2</param>
    /// <returns>current hurtbox set</returns>
    public SimCombatBoxSet GetPlayerHurtboxes(int playerSlot)
    {
        if (playerSlot == 1)
        {
            return this.GetHurtboxes(
                this.Player01,
                this.Player01FighterProfile);
        }

        if (playerSlot == 2)
        {
            return this.GetHurtboxes(
                this.Player02,
                this.Player02FighterProfile);
        }

        return SimCombatBoxSet.CreateEmpty();
    }
    #endregion

    #region attack box methods
    /// <summary>
    /// creates the current deterministic attack boxes for one player
    /// </summary>
    private SimCombatBoxSet GetAttackBoxes(SimPlayerState player,SimFighterProfile fighterProfile)
    {
        if (player.AttackState == SimAttackState.None)
        {
            return SimCombatBoxSet.CreateEmpty();
        }

        switch (player.AttackState)
        {
            case SimAttackState.LightAttack:
                return SimCombatBoxSet.CreateSingle(
                    this.CreateRootRelativeBox(
                        player,
                        LightAttackForwardOffset,
                        this.GetLightAttackOffsetY(fighterProfile),
                        LightAttackHalfWidth,
                        LightAttackHalfHeight));

            case SimAttackState.MediumAttack:
                return SimCombatBoxSet.CreateSingle(
                    this.CreateRootRelativeBox(
                        player,
                        this.GetMediumAttackForwardOffset(fighterProfile),
                        this.GetMediumAttackOffsetY(fighterProfile),
                        this.GetMediumAttackHalfWidth(fighterProfile),
                        this.GetMediumAttackHalfHeight(fighterProfile)));


            case SimAttackState.HeavyAttack:
                return this.GetHeavyAttackBoxes(
                    player,
                    fighterProfile);

            default:
                return SimCombatBoxSet.CreateEmpty();
        }
    }

    /// <summary>
    /// returns the fighter-specific light-attack height
    /// </summary>
    private FixedNumber GetLightAttackOffsetY(SimFighterProfile fighterProfile)
    {
        if (fighterProfile == SimFighterProfile.Fighter05)
        {
            return FixedNumber.FromFloat(1.30f);
        }

        return FixedNumber.FromFloat(1.22f);
    }

    /// <summary>
    /// returns the fighter-specific medium-attack height
    /// </summary>
    private FixedNumber GetMediumAttackOffsetY(SimFighterProfile fighterProfile)
    {
        if (fighterProfile == SimFighterProfile.Fighter05)
        {
            return FixedNumber.FromFloat(1.22f);
        }

        return FixedNumber.FromFloat(1.52f);
    }

    /// <summary>
    /// returns the fighter-specific medium-attack forward offset
    /// </summary>
    /// <param name="fighterProfile">fighter profile</param>
    /// <returns>medium-attack forward offset</returns>
    private FixedNumber GetMediumAttackForwardOffset(SimFighterProfile fighterProfile)
    {
        if (fighterProfile == SimFighterProfile.Fighter02)
        {
            return Fighter02MediumAttackForwardOffset;
        }

        return Fighter05MediumAttackForwardOffset;
    }

    /// <summary>
    /// returns the fighter-specific medium-attack half width
    /// </summary>
    /// <param name="fighterProfile">fighter profile</param>
    /// <returns>medium-attack half width</returns>
    private FixedNumber GetMediumAttackHalfWidth(SimFighterProfile fighterProfile)
    {
        if (fighterProfile == SimFighterProfile.Fighter02)
        {
            return Fighter02MediumAttackHalfWidth;
        }

        return Fighter05MediumAttackHalfWidth;
    }

    /// <summary>
    /// returns the fighter-specific medium-attack half height
    /// </summary>
    /// <param name="fighterProfile">fighter profile</param>
    /// <returns>medium-attack half height</returns>
    private FixedNumber GetMediumAttackHalfHeight(SimFighterProfile fighterProfile)
    {
        if (fighterProfile == SimFighterProfile.Fighter02)
        {
            return Fighter02MediumAttackHalfHeight;
        }

        return Fighter05MediumAttackHalfHeight;
    }


    /// <summary>
    /// returns fighter-specific heavy-attack boxes
    /// </summary>
    private SimCombatBoxSet GetHeavyAttackBoxes(SimPlayerState player,SimFighterProfile fighterProfile)
    {
        if (fighterProfile == SimFighterProfile.Fighter02)
        {
            return SimCombatBoxSet.CreateSingle(
                this.CreateRootRelativeBox(
                    player,
                    Fighter02HeavyForwardOffset,
                    FixedNumber.FromFloat(1.24f),
                    Fighter02HeavyHalfWidth,
                    Fighter02HeavyHalfHeight));
        }

        return this.GetFighter05HeavyAttackBoxes(player);
    }

    /// <summary>
    /// creates two tick-dependent boxes for Fighter 05's somersault
    /// </summary>
    private SimCombatBoxSet GetFighter05HeavyAttackBoxes(SimPlayerState player)
    {
        int activeStartTick;
        int relativeActiveTick;
        SimCombatBox primary;
        SimCombatBox secondary;

        activeStartTick =
            HeavyAttackStartupTicks + 1;

        relativeActiveTick =
            player.AttackTick - activeStartTick;

        if (relativeActiveTick < 0)
        {
            relativeActiveTick = 0;
        }

        if (relativeActiveTick >= HeavyAttackActiveTicks)
        {
            relativeActiveTick =
                HeavyAttackActiveTicks - 1;
        }

        if (relativeActiveTick <= 1)
        {
            primary = this.CreateRootRelativeBox(
                player,
                FixedNumber.FromFloat(0.96f),
                FixedNumber.FromFloat(1.45f),
                Fighter05HeavyPrimaryHalfWidth,
                Fighter05HeavyPrimaryHalfHeight);

            secondary = this.CreateRootRelativeBox(
                player,
                FixedNumber.FromFloat(0.82f),
                FixedNumber.FromFloat(0.92f),
                Fighter05HeavySecondaryHalfWidth,
                Fighter05HeavySecondaryHalfHeight);

            return SimCombatBoxSet.CreateDouble(
                primary,
                secondary);
        }

        if (relativeActiveTick <= 3)
        {
            primary = this.CreateRootRelativeBox(
                player,
                FixedNumber.FromFloat(0.98f),
                FixedNumber.FromFloat(1.55f),
                Fighter05HeavyPrimaryHalfWidth,
                Fighter05HeavyPrimaryHalfHeight);

            secondary = this.CreateRootRelativeBox(
                player,
                FixedNumber.FromFloat(0.90f),
                FixedNumber.FromFloat(1.15f),
                Fighter05HeavySecondaryHalfWidth,
                Fighter05HeavySecondaryHalfHeight);

            return SimCombatBoxSet.CreateDouble(
                primary,
                secondary);
        }

        primary = this.CreateRootRelativeBox(
            player,
            FixedNumber.FromFloat(0.92f),
            FixedNumber.FromFloat(1.28f),
            Fighter05HeavyPrimaryHalfWidth,
            Fighter05HeavyPrimaryHalfHeight);

        secondary = this.CreateRootRelativeBox(
            player,
            FixedNumber.FromFloat(0.78f),
            FixedNumber.FromFloat(0.78f),
            Fighter05HeavySecondaryHalfWidth,
            Fighter05HeavySecondaryHalfHeight);

        return SimCombatBoxSet.CreateDouble(
            primary,
            secondary);
    }
    #endregion

    #region hurtbox methods
    /// <summary>
    /// creates deterministic body hurtboxes based on the player's current pose
    /// </summary>
    private SimCombatBoxSet GetHurtboxes(SimPlayerState player,SimFighterProfile fighterProfile)
    {
        if (!player.IsGrounded)
        {
            return this.GetAirHurtboxes(
                player,
                fighterProfile);
        }

        if (player.MovementState == SimMovementState.Crouch)
        {
            return this.GetCrouchHurtboxes(
                player,
                fighterProfile);
        }

        return this.GetStandingHurtboxes(
            player,
            fighterProfile);
    }

    /// <summary>
    /// creates airborne hurtboxes that move upward with the jumping fighter
    /// </summary>
    private SimCombatBoxSet GetAirHurtboxes(SimPlayerState player,SimFighterProfile fighterProfile)
    {
        SimCombatBox upperHurtbox;
        SimCombatBox lowerHurtbox;

        if (fighterProfile == SimFighterProfile.Fighter05)
        {
            upperHurtbox = this.CreateRootRelativeBox(
                player,
                FixedNumber.ZERO,
                Fighter05AirUpperHurtboxOffsetY,
                Fighter05AirUpperHurtboxHalfWidth,
                Fighter05AirUpperHurtboxHalfHeight);

            lowerHurtbox = this.CreateRootRelativeBox(
                player,
                FixedNumber.ZERO,
                Fighter05AirLowerHurtboxOffsetY,
                Fighter05AirLowerHurtboxHalfWidth,
                Fighter05AirLowerHurtboxHalfHeight);
        }
        else
        {
            upperHurtbox = this.CreateRootRelativeBox(
                player,
                FixedNumber.ZERO,
                Fighter02AirUpperHurtboxOffsetY,
                Fighter02AirUpperHurtboxHalfWidth,
                Fighter02AirUpperHurtboxHalfHeight);

            lowerHurtbox = this.CreateRootRelativeBox(
                player,
                FixedNumber.ZERO,
                Fighter02AirLowerHurtboxOffsetY,
                Fighter02AirLowerHurtboxHalfWidth,
                Fighter02AirLowerHurtboxHalfHeight);
        }

        return SimCombatBoxSet.CreateDouble(
            upperHurtbox,
            lowerHurtbox);
    }

    /// <summary>
    /// creates normal standing hurtboxes
    /// </summary>
    private SimCombatBoxSet GetStandingHurtboxes(SimPlayerState player,SimFighterProfile fighterProfile)
    {
        SimCombatBox upperHurtbox;
        SimCombatBox lowerHurtbox;

        if (fighterProfile == SimFighterProfile.Fighter05)
        {
            upperHurtbox = this.CreateRootRelativeBox(
                player,
                FixedNumber.ZERO,
                Fighter05UpperHurtboxOffsetY,
                Fighter05UpperHurtboxHalfWidth,
                Fighter05UpperHurtboxHalfHeight);

            lowerHurtbox = this.CreateRootRelativeBox(
                player,
                FixedNumber.ZERO,
                Fighter05LowerHurtboxOffsetY,
                Fighter05LowerHurtboxHalfWidth,
                Fighter05LowerHurtboxHalfHeight);
        }
        else
        {
            upperHurtbox = this.CreateRootRelativeBox(
                player,
                FixedNumber.ZERO,
                Fighter02UpperHurtboxOffsetY,
                Fighter02UpperHurtboxHalfWidth,
                Fighter02UpperHurtboxHalfHeight);

            lowerHurtbox = this.CreateRootRelativeBox(
                player,
                FixedNumber.ZERO,
                Fighter02LowerHurtboxOffsetY,
                Fighter02LowerHurtboxHalfWidth,
                Fighter02LowerHurtboxHalfHeight);
        }

        return SimCombatBoxSet.CreateDouble(
            upperHurtbox,
            lowerHurtbox);
    }

    /// <summary>
    /// creates lower crouching hurtboxes so high attacks can pass over the fighter
    /// </summary>
    private SimCombatBoxSet GetCrouchHurtboxes(SimPlayerState player,SimFighterProfile fighterProfile)
    {
        SimCombatBox upperHurtbox;
        SimCombatBox lowerHurtbox;

        if (fighterProfile == SimFighterProfile.Fighter05)
        {
            upperHurtbox = this.CreateRootRelativeBox(
                player,
                FixedNumber.ZERO,
                Fighter05CrouchUpperHurtboxOffsetY,
                Fighter05CrouchUpperHurtboxHalfWidth,
                Fighter05CrouchUpperHurtboxHalfHeight);

            lowerHurtbox = this.CreateRootRelativeBox(
                player,
                FixedNumber.ZERO,
                Fighter05CrouchLowerHurtboxOffsetY,
                Fighter05CrouchLowerHurtboxHalfWidth,
                Fighter05CrouchLowerHurtboxHalfHeight);
        }
        else
        {
            upperHurtbox = this.CreateRootRelativeBox(
                player,
                FixedNumber.ZERO,
                Fighter02CrouchUpperHurtboxOffsetY,
                Fighter02CrouchUpperHurtboxHalfWidth,
                Fighter02CrouchUpperHurtboxHalfHeight);

            lowerHurtbox = this.CreateRootRelativeBox(
                player,
                FixedNumber.ZERO,
                Fighter02CrouchLowerHurtboxOffsetY,
                Fighter02CrouchLowerHurtboxHalfWidth,
                Fighter02CrouchLowerHurtboxHalfHeight);
        }

        return SimCombatBoxSet.CreateDouble(
            upperHurtbox,
            lowerHurtbox);
    }
    #endregion

    #region creation methods
    /// <summary>
    /// creates a deterministic box relative to one player's root
    /// </summary>
    private SimCombatBox CreateRootRelativeBox(
        SimPlayerState player,
        FixedNumber forwardOffset,
        FixedNumber verticalOffset,
        FixedNumber halfWidth,
        FixedNumber halfHeight)
    {
        FixedNumber facingDirection;
        FixedNumber centerX;
        FixedNumber centerY;

        facingDirection =
            player.IsFacingRight
                ? FixedNumber.ONE
                : FixedNumber.ZERO - FixedNumber.ONE;

        centerX =
            player.Position.x +
            (forwardOffset * facingDirection);

        centerY =
            player.Position.y +
            verticalOffset;

        return new SimCombatBox(
            new FixedVector2(centerX, centerY),
            new FixedVector2(halfWidth, halfHeight));
    }

    /// <summary>
    /// returns the deterministic fighter profile for one player state reference
    /// </summary>
    /// <param name="player">player state reference</param>
    /// <returns>fighter profile for the matching player</returns>
    private SimFighterProfile GetFighterProfileForPlayer(SimPlayerState player)
    {
        if (player == this.Player01)
        {
            return this.Player01FighterProfile;
        }

        if (player == this.Player02)
        {
            return this.Player02FighterProfile;
        }

        return SimFighterProfile.Fighter05;
    }
    #endregion
}