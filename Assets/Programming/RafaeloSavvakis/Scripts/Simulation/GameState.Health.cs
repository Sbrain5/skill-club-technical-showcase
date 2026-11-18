/// <summary>
/// contains deterministic health and damage logic
/// </summary>
public partial class GameState
{
    #region health methods
    /// <summary>
    /// applies simple deterministic damage to the defender
    /// </summary>
    /// <param name="defender">the player receiving damage</param>
    /// <param name="attackState">the attack that caused the damage</param>
    private void ApplyBasicDamage(SimPlayerState defender, SimAttackState attackState)
    {
        int damageAmount;

        damageAmount = this.GetAttackDamage(attackState);
        defender.CurrentHealth -= damageAmount;

        if (defender.CurrentHealth < 0)
        {
            defender.CurrentHealth = 0;
        }
    }

    /// <summary>
    /// returns the damage value for the current attack type
    /// </summary>
    /// <param name="attackState">the attack state to check</param>
    /// <returns>damage amount</returns>
    private int GetAttackDamage(SimAttackState attackState)
    {
        switch (attackState)
        {
            case SimAttackState.LightAttack:
                return LightAttackDamage;

            case SimAttackState.MediumAttack:
                return MediumAttackDamage;

            case SimAttackState.HeavyAttack:
                return HeavyAttackDamage;

            default:
                return 0;
        }
    }

    /// <summary>
    /// resets health state for a new match start
    /// </summary>
    /// <param name="player">the player state to update</param>
    private void ResetHealthState(SimPlayerState player)
    {
        player.MaxHealth = MaxPlayerHealth;
        player.CurrentHealth = MaxPlayerHealth;
    }
    #endregion
}