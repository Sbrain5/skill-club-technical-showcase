using UnityEngine;

/// <summary>
/// handles visual-only player presentation such as smoothing, facing, and crouch scaling
/// </summary>
public sealed partial class PlayerController
{
    #region properties
    /// <summary>
    /// gets the smoothed visual presentation position for camera and visual-only systems
    /// </summary>
    public Vector3 PresentationPosition
    {
        get
        {
            return this.transform.position;
        }
    }
    #endregion

    #region methods
    /// <summary>
    /// sets whether this player is locally controlled for presentation smoothing
    /// </summary>
    /// <param name="isLocalPlayer">true if this controller represents the local player</param>
    public void SetLocalPresentation(bool isLocalPlayer)
    {
        this.isLocalPresentation = isLocalPlayer;
        this.fighterVoice.SetVoiceEnabled(this.isLocalPresentation);
        this.ResetVisualPresentation();
    }

    /// <summary>
    /// caches the visual root's default local position
    /// </summary>
    private void CacheVisualRestPosition()
    {
        this.visualRestLocalPosition = this.visualRoot.localPosition;
    }

    /// <summary>
    /// snaps the visual model back to its default local position
    /// </summary>
    private void ResetVisualPresentation()
    {
        this.visualSmoothVelocity = Vector3.zero;
        this.hasReceivedAuthoritativeState = false;
        this.visualRoot.localPosition = this.visualRestLocalPosition;
    }

    /// <summary>
    /// applies visual-only correction after the authoritative root position changes
    /// </summary>
    /// <param name="previousRootPosition">root position before authoritative update</param>
    /// <param name="previousVisualWorldPosition">visual world position before authoritative update</param>
    /// <param name="newRootPosition">root position after authoritative update</param>
    private void ApplyVisualPresentationCorrection(Vector3 previousRootPosition, Vector3 previousVisualWorldPosition, Vector3 newRootPosition)
    {
        float rootCorrectionDistance;
        float smoothTime;
        Vector3 correctedLocalPosition;

        if (!this.enableVisualSmoothing)
        {
            return;
        }

        smoothTime = this.GetCurrentVisualSmoothTime();

        if (smoothTime <= 0.0f)
        {
            this.visualRoot.localPosition = this.visualRestLocalPosition;
            this.hasReceivedAuthoritativeState = true;
            return;
        }

        if (!this.hasReceivedAuthoritativeState)
        {
            this.visualRoot.localPosition = this.visualRestLocalPosition;
            this.hasReceivedAuthoritativeState = true;
            return;
        }

        rootCorrectionDistance = Vector3.Distance(previousRootPosition, newRootPosition);

        if (rootCorrectionDistance >= this.visualSnapDistance)
        {
            this.visualRoot.localPosition = this.visualRestLocalPosition;
            this.visualSmoothVelocity = Vector3.zero;
            return;
        }

        this.visualRoot.position = previousVisualWorldPosition;

        correctedLocalPosition = this.visualRoot.localPosition;
        correctedLocalPosition.y = this.visualRestLocalPosition.y;
        this.visualRoot.localPosition = correctedLocalPosition;

        if (Vector3.Distance(this.visualRoot.localPosition, this.visualRestLocalPosition) >= this.visualSnapDistance)
        {
            this.visualRoot.localPosition = this.visualRestLocalPosition;
            this.visualSmoothVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// smooths the visual root back to the authoritative gameplay root
    /// </summary>
    private void UpdateVisualPresentationSmoothing()
    {
        float smoothTime;
        Vector3 smoothedLocalPosition;

        if (!this.enableVisualSmoothing)
        {
            return;
        }

        smoothTime = this.GetCurrentVisualSmoothTime();

        if (smoothTime <= 0.0f)
        {
            this.visualRoot.localPosition = this.visualRestLocalPosition;
            return;
        }

        smoothedLocalPosition = Vector3.SmoothDamp(
            this.visualRoot.localPosition,
            this.visualRestLocalPosition,
            ref this.visualSmoothVelocity,
            smoothTime
        );

        smoothedLocalPosition.y = this.visualRestLocalPosition.y;
        this.visualRoot.localPosition = smoothedLocalPosition;
    }

    /// <summary>
    /// gets the active smoothing time based on local or remote ownership
    /// </summary>
    /// <returns>visual smoothing time</returns>
    private float GetCurrentVisualSmoothTime()
    {
        if (this.isLocalPresentation)
        {
            return this.localVisualSmoothTime;
        }

        return this.remoteVisualSmoothTime;
    }

    /// <summary>
    /// applies simple facing rotation to the assigned visual pivot
    /// </summary>
    /// <param name="isFacingRight">true if facing right</param>
    private void ApplyVisualFacing(bool isFacingRight)
    {
        Vector3 currentEulerAngles = this.visualRoot.localEulerAngles;
        currentEulerAngles.y = isFacingRight ? 0.0f : 180.0f;
        this.visualRoot.localEulerAngles = currentEulerAngles;
    }

    /// <summary>
    /// applies a simple visual crouch cue by scaling the visual pivot on Y
    /// </summary>
    /// <param name="crouchActive">true while crouch is active</param>
    /// <param name="isGrounded">true while grounded</param>
    private void ApplyCrouchVisual(bool crouchActive, bool isGrounded)
    {
        Vector3 localScale = this.visualRoot.localScale;

        if (crouchActive && isGrounded)
        {
            localScale.y = this.crouchVisualScaleY;
        }
        else
        {
            localScale.y = 1.0f;
        }

        this.visualRoot.localScale = localScale;
    }
    #endregion
}