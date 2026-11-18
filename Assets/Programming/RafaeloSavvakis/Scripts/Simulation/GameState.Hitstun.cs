/// <summary>
/// contains deterministic hitstun state countdown and reset logic
/// </summary>
public partial class GameState
{
    #region hitstun methods
    /// <summary>
    /// advances hitstun by one deterministic gameplay tick
    /// </summary>
    /// <param name="player">the player state to update</param>
    private void ApplyHitstunTick(SimPlayerState player)
    {
        this.ResetAttackState(player);

        if (player.HitstunTicksRemaining > 0)
        {
            player.HitstunTicksRemaining--;
        }

        if (player.HitstunTicksRemaining <= 0)
        {
            this.ResetHitstunState(player);
        }
    }

    /// <summary>
    /// updates the movement state while the player is locked in hitstun
    /// </summary>
    /// <param name="player">the player state to update</param>
    private void UpdateHitstunMovementState(SimPlayerState player)
    {
        if (!player.IsGrounded)
        {
            player.MovementState = SimMovementState.Jump;
            return;
        }

        player.MovementState = SimMovementState.Idle;
    }

    /// <summary>
    /// resets hitstun-only state to the inactive values
    /// </summary>
    /// <param name="player">the player state to update</param>
    private void ResetHitstunState(SimPlayerState player)
    {
        player.IsInHitstun = false;
        player.HitstunTicksRemaining = 0;
    }

    /// <summary>
    /// resets remembered hit reaction data for a clean match start
    /// </summary>
    /// <param name="player">the player state to update</param>
    private void ResetHitMemory(SimPlayerState player)
    {
        player.LastReceivedHitWindowId = -1;
        player.LastReceivedAttackStartFrame = -1;
    }
    #endregion
}