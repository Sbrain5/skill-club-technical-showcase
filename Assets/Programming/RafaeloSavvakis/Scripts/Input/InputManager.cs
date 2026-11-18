using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// reads gameplay input from the Unity New Input System
/// </summary>
public sealed class InputManager : MonoBehaviour
{
    #region variables
    [Header("Input Asset")]
    [SerializeField] private InputActionAsset inputActionsAsset;

    [Header("Runtime State")]
    [SerializeField] private Vector2 currentGameplayMove;
    [SerializeField] private bool lightAttackPressedThisFrame;
    [SerializeField] private bool mediumAttackPressedThisFrame;
    [SerializeField] private bool heavyAttackPressedThisFrame;

    private InputActionMap gameplayActionMap;

    private InputAction gameplayMoveAction;
    private InputAction gameplayLightAttackAction;
    private InputAction gameplayMediumAttackAction;
    private InputAction gameplayHeavyAttackAction;
    #endregion

    #region properties
    /// <summary>
    /// gets the current gameplay movement input
    /// </summary>
    public Vector2 CurrentGameplayMove
    {
        get
        {
            return this.currentGameplayMove;
        }
    }
    #endregion

    #region unity methods
    /// <summary>
    /// caches the gameplay action map and actions on startup
    /// </summary>
    private void Awake()
    {
        this.CacheActionMap();
        this.CacheActions();
    }

    /// <summary>
    /// enables the gameplay action map when this object becomes active
    /// </summary>
    private void OnEnable()
    {
        this.EnableActionMap();
    }

    /// <summary>
    /// disables the gameplay action map when this object becomes inactive
    /// </summary>
    private void OnDisable()
    {
        this.DisableActionMap();
    }

    /// <summary>
    /// updates the current gameplay input values each rendered frame
    /// </summary>
    private void Update()
    {
        this.currentGameplayMove = this.gameplayMoveAction.ReadValue<Vector2>();

        if (this.gameplayLightAttackAction.WasPressedThisFrame())
        {
            this.lightAttackPressedThisFrame = true;
        }

        if (this.gameplayMediumAttackAction.WasPressedThisFrame())
        {
            this.mediumAttackPressedThisFrame = true;
        }

        if (this.gameplayHeavyAttackAction.WasPressedThisFrame())
        {
            this.heavyAttackPressedThisFrame = true;
        }
    }
    #endregion

    #region methods
    /// <summary>
    /// consumes the current light attack press once
    /// </summary>
    /// <returns>true if light attack was pressed before being consumed</returns>
    public bool ConsumeLightAttackPressedThisFrame()
    {
        if (!this.lightAttackPressedThisFrame)
        {
            return false;
        }

        this.lightAttackPressedThisFrame = false;
        return true;
    }

    /// <summary>
    /// consumes the current medium attack press once
    /// </summary>
    /// <returns>true if medium attack was pressed before being consumed</returns>
    public bool ConsumeMediumAttackPressedThisFrame()
    {
        if (!this.mediumAttackPressedThisFrame)
        {
            return false;
        }

        this.mediumAttackPressedThisFrame = false;
        return true;
    }

    /// <summary>
    /// consumes the current heavy attack press once
    /// </summary>
    /// <returns>true if heavy attack was pressed before being consumed</returns>
    public bool ConsumeHeavyAttackPressedThisFrame()
    {
        if (!this.heavyAttackPressedThisFrame)
        {
            return false;
        }

        this.heavyAttackPressedThisFrame = false;
        return true;
    }

    /// <summary>
    /// caches the gameplay action map from the assigned input asset
    /// </summary>
    private void CacheActionMap()
    {
        this.gameplayActionMap = this.inputActionsAsset.FindActionMap("Gameplay", false);
    }

    /// <summary>
    /// caches the required gameplay actions from the gameplay action map
    /// </summary>
    private void CacheActions()
    {
        this.gameplayMoveAction = this.gameplayActionMap.FindAction("Move", false);
        this.gameplayLightAttackAction = this.gameplayActionMap.FindAction("LightAttack", false);
        this.gameplayMediumAttackAction = this.gameplayActionMap.FindAction("MediumAttack", false);
        this.gameplayHeavyAttackAction = this.gameplayActionMap.FindAction("HeavyAttack", false);
    }

    /// <summary>
    /// enables the cached gameplay action map
    /// </summary>
    private void EnableActionMap()
    {
        this.gameplayActionMap.Enable();
    }

    /// <summary>
    /// disables the cached gameplay action map
    /// </summary>
    private void DisableActionMap()
    {
        this.gameplayActionMap.Disable();
    }
    #endregion
}