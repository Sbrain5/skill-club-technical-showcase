/// <summary>
/// stores deterministic gameplay tuning values used by GameState
/// </summary>
public partial class GameState
{
    #region time constants
    private static readonly FixedNumber FixedTickDelta = FixedNumber.FromFloat(1.0f / 60.0f);
    #endregion

    #region movement constants
    private static readonly FixedNumber AirJumpHorizontalSpeed = FixedNumber.FromFloat(3.2f);
    private static readonly FixedNumber JumpVelocity = FixedNumber.FromFloat(2.0f);
    private static readonly FixedNumber Gravity = FixedNumber.FromFloat(5.0f);
    private static readonly FixedNumber GroundStepSpeed = FixedNumber.FromFloat(2.1667f);

    private const int GroundStepDurationTicks = 18;
    private const int GroundStepRecoveryTicks = 10;
    private const int GroundStepVisualLockoutTicks = 24;
    #endregion

    #region stage constants
    private static readonly FixedNumber GroundY = FixedNumber.ZERO;
    private static readonly FixedNumber MinStageX = FixedNumber.FromFloat(-9.65f);
    private static readonly FixedNumber MaxStageX = FixedNumber.FromFloat(9.65f);
    private static readonly FixedNumber Player01StartX = FixedNumber.FromFloat(-2.0f);
    private static readonly FixedNumber Player02StartX = FixedNumber.FromFloat(2.5f);
    private static readonly FixedNumber PlayerStartZ = FixedNumber.FromFloat(-0.5f);
    private static readonly FixedNumber PlayerBodyMinimumDistance = FixedNumber.FromFloat(0.85f);
    #endregion

    #region attack timing constants
    private const int LightAttackStartupTicks = 7;
    private const int LightAttackActiveTicks = 4;
    private const int LightAttackRecoveryTicks = 30;

    private const int MediumAttackStartupTicks = 10;
    private const int MediumAttackActiveTicks = 5;
    private const int MediumAttackRecoveryTicks = 39;

    private const int HeavyAttackStartupTicks = 14;
    private const int HeavyAttackActiveTicks = 6;
    private const int HeavyAttackRecoveryTicks = 47;
    #endregion

    #region attack damage constants
    private const int MaxPlayerHealth = 100;
    private const int LightAttackDamage = 6;
    private const int MediumAttackDamage = 8;
    private const int HeavyAttackDamage = 10;
    #endregion

    #region hit reaction constants
    private const int BasicHitstunTicks = 49;

    private static readonly FixedNumber BasicKnockbackDistance = FixedNumber.FromFloat(0.15f);
    #endregion

    #region attack range constants
    private static readonly FixedNumber LightAttackHorizontalRange = FixedNumber.FromFloat(1.35f);
    private static readonly FixedNumber MediumAttackHorizontalRange = FixedNumber.FromFloat(1.35f);
    private static readonly FixedNumber HeavyAttackHorizontalRange = FixedNumber.FromFloat(1.35f);

    private static readonly FixedNumber LightAttackVerticalRange = FixedNumber.FromFloat(0.85f);
    private static readonly FixedNumber MediumAttackVerticalRange = FixedNumber.FromFloat(0.85f);
    private static readonly FixedNumber HeavyAttackVerticalRange = FixedNumber.FromFloat(0.85f);
    #endregion

    #region hit window constants
    private const int Player01HitWindowOffset = 1;
    private const int Player02HitWindowOffset = 2;
    private const int LightAttackHitWindowOffset = 10;
    private const int MediumAttackHitWindowOffset = 20;
    private const int HeavyAttackHitWindowOffset = 30;
    #endregion

    #region match flow constants
    private const int RoundEndDelayTicks = 180;
    private const int RoundsNeededToWinMatch = 2;
    private const int RoundTimerStartSeconds = 60;
    private const int RoundTimerStartTicks = RoundTimerStartSeconds * 60;
    #endregion
}