/// <summary>
/// contains deterministic match flow and round end logic
/// </summary>
public partial class GameState
{
    #region match flow methods
    /// <summary>
    /// applies match flow logic while gameplay is not in the normal playing state
    /// </summary>
    private void ApplyNonPlayingMatchTick()
    {
        if (this.MatchState == SimMatchState.RoundOver)
        {
            this.ApplyRoundOverTick();
            return;
        }

        this.ApplyRoundOverFreeze();
    }

    /// <summary>
    /// keeps the round frozen for a short delay before resetting the next round
    /// </summary>
    private void ApplyRoundOverTick()
    {
        this.ApplyRoundOverFreeze();

        if (this.RoundEndTicksRemaining > 0)
        {
            this.RoundEndTicksRemaining--;
        }

        if (this.RoundEndTicksRemaining <= 0)
        {
            this.ResetRoundState();
        }
    }

    /// <summary>
    /// checks if a player's health reached zero and ends the round if needed
    /// </summary>
    private void CheckRoundOverFromHealth()
    {
        if (this.MatchState != SimMatchState.Playing)
        {
            return;
        }

        if (this.Player01.CurrentHealth <= 0 && this.Player02.CurrentHealth > 0)
        {
            this.EndRound(2);
            return;
        }

        if (this.Player02.CurrentHealth <= 0 && this.Player01.CurrentHealth > 0)
        {
            this.EndRound(1);
            return;
        }

        if (this.Player01.CurrentHealth <= 0 && this.Player02.CurrentHealth <= 0)
        {
            this.EndRound(0);
        }
    }

    /// <summary>
    /// moves the match into a round over state
    /// </summary>
    /// <param name="winnerSlot">the winning player slot, or 0 for no winner</param>
    private void EndRound(int winnerSlot)
    {
        this.EndRound(winnerSlot, false);
    }

    /// <summary>
    /// moves the match into a round over state
    /// </summary>
    /// <param name="winnerSlot">the winning player slot, or 0 for no winner</param>
    /// <param name="endedByTimeUp">true if the round ended because the timer reached zero</param>
    private void EndRound(int winnerSlot, bool endedByTimeUp)
    {
        int matchWinnerSlot;

        this.RoundEndedByTimeUp = endedByTimeUp;
        this.AddRoundWin(winnerSlot);

        this.RoundWinnerSlot = winnerSlot;

        matchWinnerSlot = this.GetMatchWinnerSlot();

        if (matchWinnerSlot > 0)
        {
            this.EndMatch(matchWinnerSlot);
            return;
        }

        this.MatchState = SimMatchState.RoundOver;
        this.MatchWinnerSlot = 0;
        this.RoundEndTicksRemaining = RoundEndDelayTicks;

        this.ApplyRoundOverFreeze();
    }

    /// <summary>
    /// resets both players for the next round
    /// </summary>
    private void ResetRoundState()
    {
        this.MatchState = SimMatchState.Playing;
        this.RoundWinnerSlot = 0;
        this.MatchWinnerSlot = 0;
        this.RoundEndTicksRemaining = 0;
        this.RoundTimerTicksRemaining = RoundTimerStartTicks;
        this.RoundEndedByTimeUp = false;

        this.InitializeStartingPlayers();
    }

    /// <summary>
    /// freezes both players after the round has ended
    /// </summary>
    private void ApplyRoundOverFreeze()
    {
        this.ApplyRoundOverFreezeToPlayer(this.Player01);
        this.ApplyRoundOverFreezeToPlayer(this.Player02);
    }

    /// <summary>
    /// counts down the deterministic round timer while the round is playing
    /// </summary>
    private void ApplyRoundTimerTick()
    {
        if (this.MatchState != SimMatchState.Playing)
        {
            return;
        }

        if (this.RoundTimerTicksRemaining > 0)
        {
            this.RoundTimerTicksRemaining--;
        }

        if (this.RoundTimerTicksRemaining <= 0)
        {
            this.EndRoundFromTimeUp();
        }
    }

    /// <summary>
    /// ends the round when the timer reaches zero
    /// </summary>
    private void EndRoundFromTimeUp()
    {
        if (this.Player01.CurrentHealth > this.Player02.CurrentHealth)
        {
            this.EndRound(1, true);
            return;
        }

        if (this.Player02.CurrentHealth > this.Player01.CurrentHealth)
        {
            this.EndRound(2, true);
            return;
        }

        this.EndRound(0, true);
    }

    /// <summary>
    /// clears gameplay movement while allowing an already-started attack animation to finish
    /// </summary>
    /// <param name="player">the player state to freeze</param>
    private void ApplyRoundOverFreezeToPlayer(SimPlayerState player)
    {
        this.ApplyRoundOverAttackFreeze(player);
        this.ResetGroundStepState(player);

        player.AirHorizontalDirection = 0;
        player.VerticalVelocity = FixedNumber.ZERO;

        if (player.IsGrounded)
        {
            player.MovementState = SimMovementState.Idle;
        }
    }

    /// <summary>
    /// keeps the current attack animation alive during round over while disabling gameplay hitboxes
    /// </summary>
    /// <param name="player">the player state to update</param>
    private void ApplyRoundOverAttackFreeze(SimPlayerState player)
    {
        player.IsAttackHitboxActive = false;
        player.ActiveHitWindowId = -1;

        if (player.AttackState == SimAttackState.None)
        {
            return;
        }

        player.AttackTick++;

        if (player.AttackTick >= this.GetTotalAttackTicks(player.AttackState))
        {
            this.ResetAttackState(player);
        }
    }

    /// <summary>
    /// adds one round win to the correct player
    /// </summary>
    /// <param name="winnerSlot">the winning player slot, or 0 for no winner</param>
    private void AddRoundWin(int winnerSlot)
    {
        if (winnerSlot == 1)
        {
            this.Player01RoundWins++;
            return;
        }

        if (winnerSlot == 2)
        {
            this.Player02RoundWins++;
        }
    }

    /// <summary>
    /// returns the player slot that has won the match, or 0 if the match is not over
    /// </summary>
    /// <returns>match winner slot, or 0 when there is no match winner yet</returns>
    private int GetMatchWinnerSlot()
    {
        if (this.Player01RoundWins >= RoundsNeededToWinMatch)
        {
            return 1;
        }

        if (this.Player02RoundWins >= RoundsNeededToWinMatch)
        {
            return 2;
        }

        return 0;
    }

    /// <summary>
    /// moves the match into the final match over state
    /// </summary>
    /// <param name="winnerSlot">the player slot that won the match</param>
    private void EndMatch(int winnerSlot)
    {
        this.MatchState = SimMatchState.MatchOver;
        this.MatchWinnerSlot = winnerSlot;
        this.RoundEndTicksRemaining = 0;

        this.ApplyRoundOverFreeze();
    }
    #endregion
}