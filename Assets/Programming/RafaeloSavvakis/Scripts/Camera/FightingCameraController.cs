using UnityEngine;

/// <summary>
/// controls the dynamic fighting-game camera using smoothed player presentation positions
/// </summary>
[RequireComponent(typeof(Camera))]
public sealed class FightingCameraController : MonoBehaviour
{
    #region variables
    [Header("Player References")]
    [SerializeField] private bool autoFindPlayers = true;
    [SerializeField] private PlayerController playerOne;
    [SerializeField] private PlayerController playerTwo;

    [Header("Follow Settings")]
    [SerializeField] private float horizontalFollowMultiplier = 1.0f;
    [SerializeField] private float positionSmoothTime = 0.12f;
    [SerializeField] private float rotationSmoothTime = 0.10f;

    [Header("Zoom Settings")]
    [SerializeField] private float maxZoomOutDistance = 6.7f;
    [SerializeField] private float jumpZoomOutMultiplier = 0.45f;
    [SerializeField] private float horizontalFramingPadding = 3.6f;

    [Header("Look Target")]
    [SerializeField] private float lookHeight = 1.2f;
    [SerializeField] private float jumpLookHeightMultiplier = 0.35f;

    private Camera controlledCamera;

    private Vector3 startingPosition;
    private Vector3 startingForward;
    private Vector3 positionVelocity;

    private float currentZoomOutDistance;
    private float zoomVelocity;
    #endregion

    #region unity methods
    /// <summary>
    /// keeps inspector values safe
    /// </summary>
    private void OnValidate()
    {
        if (this.horizontalFollowMultiplier < 0.0f)
        {
            this.horizontalFollowMultiplier = 0.0f;
        }

        if (this.positionSmoothTime < 0.01f)
        {
            this.positionSmoothTime = 0.01f;
        }

        if (this.rotationSmoothTime < 0.01f)
        {
            this.rotationSmoothTime = 0.01f;
        }

        if (this.maxZoomOutDistance < 0.0f)
        {
            this.maxZoomOutDistance = 0.0f;
        }

        if (this.jumpZoomOutMultiplier < 0.0f)
        {
            this.jumpZoomOutMultiplier = 0.0f;
        }

        if (this.horizontalFramingPadding < 0.0f)
        {
            this.horizontalFramingPadding = 0.0f;
        }

        if (this.jumpLookHeightMultiplier < 0.0f)
        {
            this.jumpLookHeightMultiplier = 0.0f;
        }
    }

    /// <summary>
    /// stores the starting camera transform as the base camera pose
    /// </summary>
    private void Awake()
    {
        this.controlledCamera = this.GetComponent<Camera>();
        this.startingPosition = this.transform.position;
        this.startingForward = this.transform.forward;
    }

    /// <summary>
    /// updates the camera after player movement has been applied
    /// </summary>
    private void LateUpdate()
    {
        if (this.autoFindPlayers)
        {
            this.FindPlayersIfNeeded();
        }

        if (this.playerOne == null || this.playerTwo == null)
        {
            return;
        }

        this.UpdateCameraTransform();
    }
    #endregion

    #region methods
    /// <summary>
    /// smoothly updates the camera position and rotation around both players
    /// </summary>
    private void UpdateCameraTransform()
    {
        Vector3 playerOnePosition = this.playerOne.PresentationPosition;
        Vector3 playerTwoPosition = this.playerTwo.PresentationPosition;
        Vector3 midpoint = (playerOnePosition + playerTwoPosition) * 0.5f;

        float highestPlayerY = Mathf.Max(playerOnePosition.y, playerTwoPosition.y);
        float targetZoomOutDistance = this.CalculateTargetZoomOutDistance(playerOnePosition, playerTwoPosition, midpoint, highestPlayerY);

        this.currentZoomOutDistance = Mathf.SmoothDamp(this.currentZoomOutDistance, targetZoomOutDistance, ref this.zoomVelocity, this.positionSmoothTime);

        Vector3 desiredPosition = this.CalculateDesiredPosition(midpoint);
        this.transform.position = Vector3.SmoothDamp(this.transform.position, desiredPosition, ref this.positionVelocity, this.positionSmoothTime);

        this.ApplyCameraRotation(midpoint, highestPlayerY);
    }

    /// <summary>
    /// calculates the target zoom-out distance needed to keep both players visible
    /// </summary>
    /// <param name="playerOnePosition">first player world position</param>
    /// <param name="playerTwoPosition">second player world position</param>
    /// <param name="midpoint">midpoint between both players</param>
    /// <param name="highestPlayerY">highest player vertical position</param>
    /// <returns>target zoom-out distance</returns>
    private float CalculateTargetZoomOutDistance(Vector3 playerOnePosition, Vector3 playerTwoPosition, Vector3 midpoint, float highestPlayerY)
    {
        float targetZoomOutDistance;

        targetZoomOutDistance = this.CalculateHorizontalFitZoom(playerOnePosition, playerTwoPosition, midpoint);
        targetZoomOutDistance += Mathf.Max(0.0f, highestPlayerY) * this.jumpZoomOutMultiplier;
        targetZoomOutDistance = Mathf.Clamp(targetZoomOutDistance, 0.0f, this.maxZoomOutDistance);

        return targetZoomOutDistance;
    }

    /// <summary>
    /// calculates the minimum zoom needed to fit both players horizontally inside the camera view
    /// </summary>
    /// <param name="playerOnePosition">first player world position</param>
    /// <param name="playerTwoPosition">second player world position</param>
    /// <param name="midpoint">midpoint between both players</param>
    /// <returns>required zoom-out distance</returns>
    private float CalculateHorizontalFitZoom(Vector3 playerOnePosition, Vector3 playerTwoPosition, Vector3 midpoint)
    {
        float playerWidth;
        float requiredWorldWidth;
        float verticalFovRadians;
        float horizontalFovRadians;
        float requiredCameraDistance;
        float currentBaseDistance;
        float requiredZoomOutDistance;

        playerWidth = Mathf.Abs(playerOnePosition.x - playerTwoPosition.x);
        requiredWorldWidth = playerWidth + this.horizontalFramingPadding;

        verticalFovRadians = this.controlledCamera.fieldOfView * Mathf.Deg2Rad;
        horizontalFovRadians = 2.0f * Mathf.Atan(Mathf.Tan(verticalFovRadians * 0.5f) * this.controlledCamera.aspect);

        requiredCameraDistance = requiredWorldWidth / (2.0f * Mathf.Tan(horizontalFovRadians * 0.5f));
        currentBaseDistance = Mathf.Abs(Vector3.Dot(midpoint - this.startingPosition, this.startingForward));
        requiredZoomOutDistance = Mathf.Max(0.0f, requiredCameraDistance - currentBaseDistance);

        return requiredZoomOutDistance;
    }

    /// <summary>
    /// calculates the desired camera position from the player midpoint
    /// </summary>
    /// <param name="midpoint">midpoint between both players</param>
    /// <returns>desired camera position</returns>
    private Vector3 CalculateDesiredPosition(Vector3 midpoint)
    {
        Vector3 desiredPosition;
        float cameraX;

        cameraX = this.startingPosition.x + (midpoint.x * this.horizontalFollowMultiplier);

        desiredPosition = this.startingPosition;
        desiredPosition.x = cameraX;
        desiredPosition -= this.startingForward * this.currentZoomOutDistance;

        return desiredPosition;
    }

    /// <summary>
    /// smoothly rotates the camera toward the shared player look target
    /// </summary>
    /// <param name="midpoint">midpoint between both players</param>
    /// <param name="highestPlayerY">highest player vertical position</param>
    private void ApplyCameraRotation(Vector3 midpoint, float highestPlayerY)
    {
        Vector3 desiredLookTarget;
        Quaternion desiredRotation;

        desiredLookTarget = midpoint;
        desiredLookTarget.y = this.lookHeight + (Mathf.Max(0.0f, highestPlayerY) * this.jumpLookHeightMultiplier);

        desiredRotation = Quaternion.LookRotation(desiredLookTarget - this.transform.position, Vector3.up);

        this.transform.rotation = Quaternion.Slerp(
            this.transform.rotation,
            desiredRotation,
            Time.deltaTime / Mathf.Max(0.001f, this.rotationSmoothTime)
        );
    }

    /// <summary>
    /// finds spawned Player01 and Player02 controllers when references are missing
    /// </summary>
    private void FindPlayersIfNeeded()
    {
        PlayerController[] playerControllers;

        if (this.playerOne != null && this.playerTwo != null)
        {
            return;
        }

        playerControllers = Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (PlayerController playerController in playerControllers)
        {
            if (playerController.AssignedPlayerSlot == PlayerController.PlayerSlot.Player01)
            {
                this.playerOne = playerController;
                continue;
            }

            if (playerController.AssignedPlayerSlot == PlayerController.PlayerSlot.Player02)
            {
                this.playerTwo = playerController;
            }
        }
    }
    #endregion
}