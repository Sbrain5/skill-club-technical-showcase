using UnityEngine;

/// <summary>
/// Applies a visual-only head look-at correction so the fighter looks toward the opponent.
/// This does not affect gameplay, hitboxes, movement, or rollback state.
/// </summary>
public sealed class FighterHeadLookAt : MonoBehaviour
{
    #region variables
    [Header("References")]
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private PlayerController ownerController;

    [Header("Look Settings")]
    [SerializeField] private bool enableHeadLookAt = true;
    [SerializeField] private float lookHeightOffset = 1.35f;

    [Header("Look Weights")]
    [SerializeField] private float totalWeight = 0.55f;
    [SerializeField] private float bodyWeight = 0.05f;
    [SerializeField] private float headWeight = 0.65f;
    [SerializeField] private float eyesWeight = 0.0f;
    [SerializeField] private float clampWeight = 0.65f;

    private PlayerController opponentController;
    #endregion

    #region unity methods
    /// <summary>
    /// Keeps inspector values within valid ranges
    /// </summary>
    private void OnValidate()
    {
        this.totalWeight = Mathf.Clamp01(this.totalWeight);
        this.bodyWeight = Mathf.Clamp01(this.bodyWeight);
        this.headWeight = Mathf.Clamp01(this.headWeight);
        this.eyesWeight = Mathf.Clamp01(this.eyesWeight);
        this.clampWeight = Mathf.Clamp01(this.clampWeight);

        if (this.lookHeightOffset < 0.0f)
        {
            this.lookHeightOffset = 0.0f;
        }
    }

    /// <summary>
    /// Applies the final visual-only look-at correction after Animator pose evaluation
    /// </summary>
    /// <param name="layerIndex">Animator layer index.</param>
    private void OnAnimatorIK(int layerIndex)
    {
        Vector3 lookTargetPosition;

        if (!this.enableHeadLookAt)
        {
            return;
        }

        this.RefreshOpponentIfNeeded();

        if (this.opponentController == null)
        {
            return;
        }

        lookTargetPosition = this.opponentController.PresentationPosition;
        lookTargetPosition.y += this.lookHeightOffset;

        this.characterAnimator.SetLookAtWeight(
            this.totalWeight,
            this.bodyWeight,
            this.headWeight,
            this.eyesWeight,
            this.clampWeight
        );

        this.characterAnimator.SetLookAtPosition(lookTargetPosition);
    }
    #endregion

    #region methods
    /// <summary>
    /// Finds the opposite player controller when the cached reference is missing
    /// </summary>
    private void RefreshOpponentIfNeeded()
    {
        PlayerController[] playerControllers;

        if (this.opponentController != null)
        {
            return;
        }

        playerControllers = Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (PlayerController playerController in playerControllers)
        {
            if (playerController == this.ownerController)
            {
                continue;
            }

            this.opponentController = playerController;
            return;
        }
    }
    #endregion
}