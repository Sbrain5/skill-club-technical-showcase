using UnityEngine;

/// <summary>
/// handles player animation parameters and one-shot hit reaction animation restarts
/// </summary>
public sealed partial class PlayerController
{
    #region animation variables
    private const string MovementStateParameter = "MovementState";
    private const string AttackStateParameter = "AttackState";
    private const string IsInHitstunParameter = "IsInHitstun";
    private const string HitReactionTriggerParameter = "HitReactionTrigger";
    private const string HitReactionStateName = "HitReaction";
    private const string IsKOParameter = "IsKO";
    private const string KOTriggerParameter = "KOTrigger";

    private int lastPlayedHitWindowId = -1;
    private int lastPlayedAttackStartFrame = -1;
    private bool hasPlayedKOAnimation;
    #endregion

    #region animation methods
    /// <summary>
    /// applies authoritative gameplay state values to all active character animators
    /// </summary>
    /// <param name="state">the authoritative simulated player state</param>
    private void ApplyAnimatorState(SimPlayerState state)
    {
        bool isKO;
        bool shouldPlayHitReaction;
        bool shouldPlayKO;
        isKO = state.CurrentHealth <= 0;

        if (!state.IsInHitstun)
        {
            this.lastPlayedHitWindowId = -1;
            this.lastPlayedAttackStartFrame = -1;
        }

        if (!isKO)
        {
            this.hasPlayedKOAnimation = false;
        }

        shouldPlayKO = isKO && !this.hasPlayedKOAnimation;
        shouldPlayHitReaction = !isKO && this.ShouldPlayHitReaction(state);

        foreach (Animator characterAnimator in this.characterAnimators)
        {
            if (!characterAnimator.isActiveAndEnabled)
            {
                continue;
            }

            characterAnimator.SetInteger(MovementStateParameter, this.GetAnimatorMovementState(state));
            characterAnimator.SetInteger(AttackStateParameter, (int)state.AttackState);
            characterAnimator.SetBool(IsInHitstunParameter, !isKO && state.IsInHitstun);
            characterAnimator.SetBool(IsKOParameter, isKO);

            if (shouldPlayKO)
            {
                characterAnimator.ResetTrigger(HitReactionTriggerParameter);
                characterAnimator.ResetTrigger(KOTriggerParameter);
                characterAnimator.SetTrigger(KOTriggerParameter);
                continue;
            }

            if (shouldPlayHitReaction)
            {
                characterAnimator.ResetTrigger(HitReactionTriggerParameter);
                characterAnimator.SetTrigger(HitReactionTriggerParameter);
                characterAnimator.Play(HitReactionStateName, 0, 0.0f);
            }
        }

        if (shouldPlayKO)
        {
            this.hasPlayedKOAnimation = true;
        }

        if (shouldPlayHitReaction)
        {
            this.lastPlayedHitWindowId = state.LastReceivedHitWindowId;
            this.lastPlayedAttackStartFrame = state.LastReceivedAttackStartFrame;
        }
    }

    /// <summary>
    /// checks if a new real hit instance should trigger the hit reaction animation
    /// </summary>
    /// <param name="state">the authoritative simulated player state</param>
    /// <returns>true if a new hit reaction animation should play</returns>
    private bool ShouldPlayHitReaction(SimPlayerState state)
    {
        if (!state.IsInHitstun)
        {
            return false;
        }

        if (state.LastReceivedHitWindowId < 0)
        {
            return false;
        }

        if (state.LastReceivedAttackStartFrame < 0)
        {
            return false;
        }

        return state.LastReceivedHitWindowId != this.lastPlayedHitWindowId ||
               state.LastReceivedAttackStartFrame != this.lastPlayedAttackStartFrame;
    }

    /// <summary>
    /// converts simulation movement state into animator movement state
    /// </summary>
    /// <param name="state">authoritative simulation player state</param>
    /// <returns>animator movement state value</returns>
    private int GetAnimatorMovementState(SimPlayerState state)
    {
        if (state.MovementState == SimMovementState.Idle)
        {
            return 0;
        }

        if (state.MovementState == SimMovementState.Walk)
        {
            return this.GetStepAnimatorMovementState(state);
        }

        if (state.MovementState == SimMovementState.Jump)
        {
            return 2;
        }

        if (state.MovementState == SimMovementState.Crouch)
        {
            return 3;
        }

        return 0;
    }

    /// <summary>
    /// converts step direction and facing direction into forward or backward animation value
    /// </summary>
    /// <param name="state">authoritative simulation player state</param>
    /// <returns>1 for forward step, -1 for backward step, 0 for none</returns>
    private int GetStepAnimatorMovementState(SimPlayerState state)
    {
        bool movingRight;
        bool movingForward;

        if (state.StepDirection == 0)
        {
            return 0;
        }

        movingRight = state.StepDirection > 0;

        if (state.IsFacingRight)
        {
            movingForward = movingRight;
        }
        else
        {
            movingForward = !movingRight;
        }

        if (movingForward)
        {
            return 1;
        }

        return -1;
    }
    #endregion
}