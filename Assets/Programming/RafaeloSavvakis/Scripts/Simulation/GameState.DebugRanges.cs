/// <summary>
/// exposes read-only deterministic attack values for visual debugging tools
/// </summary>
public partial class GameState
{
    #region debug range methods
    /// <summary>
    /// gets the horizontal gameplay range used by one attack
    /// </summary>
    /// <param name="attackState">attack type</param>
    /// <returns>horizontal range in Unity world units</returns>
    public static float GetDebugAttackHorizontalRange(SimAttackState attackState)
    {
        switch (attackState)
        {
            case SimAttackState.LightAttack:
                return LightAttackHorizontalRange.ToFloat;

            case SimAttackState.MediumAttack:
                return MediumAttackHorizontalRange.ToFloat;

            case SimAttackState.HeavyAttack:
                return HeavyAttackHorizontalRange.ToFloat;

            default:
                return 0.0f;
        }
    }

    /// <summary>
    /// gets the vertical gameplay range used by one attack
    /// </summary>
    /// <param name="attackState">attack type</param>
    /// <returns>vertical range in Unity world units</returns>
    public static float GetDebugAttackVerticalRange(SimAttackState attackState)
    {
        switch (attackState)
        {
            case SimAttackState.LightAttack:
                return LightAttackVerticalRange.ToFloat;

            case SimAttackState.MediumAttack:
                return MediumAttackVerticalRange.ToFloat;

            case SimAttackState.HeavyAttack:
                return HeavyAttackVerticalRange.ToFloat;

            default:
                return 0.0f;
        }
    }
    #endregion
}