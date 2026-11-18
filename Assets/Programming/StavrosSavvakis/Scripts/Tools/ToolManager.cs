using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// opens the tools menu that contains all the tools buttons
/// </summary>
public class ToolManager : MonoBehaviour
{
    #region variables
    [SerializeField] private GameObject toolPanel;
    [SerializeField] private GameObject rollbackPanel;
    [SerializeField] private GameObject arbiterPanel;
    [SerializeField] private GameObject arbiterMismatchPanel;
    [SerializeField] private GameObject pingPanel;
    [SerializeField] private InputActionReference toggleToolsAction;
    #endregion

    #region unity methods
    private void OnEnable()
    {
        toggleToolsAction.action.performed += OnToggleToolsPressed;
        toggleToolsAction.action.Enable();
    }

    private void OnDisable()
    {
        toggleToolsAction.action.performed -= OnToggleToolsPressed;
        toggleToolsAction.action.Disable();
    }
    #endregion

    #region methods
    /// <summary>
    /// opens or closes the tools menu
    /// </summary>
    /// <param name="context">input context</param>
    private void OnToggleToolsPressed(InputAction.CallbackContext context)
    {
        toolPanel.SetActive(!toolPanel.activeSelf);
    }

    /// <summary>
    /// toggles the rollback panel
    /// </summary>
    public void ToggleRollbackPanel()
    {
        rollbackPanel.SetActive(!rollbackPanel.activeSelf);
    }

    /// <summary>
    /// toggles the arbiter panel
    /// </summary>
    public void ToggleArbiterPanel()
    {
        bool panelOpen = !arbiterPanel.activeSelf;
        arbiterPanel.SetActive(panelOpen);

        if (panelOpen)
        {
            ArbiterLogView.RefreshLogView();
        }
    }

    /// <summary>
    /// toggles the arbiter mismatch panel
    /// </summary>
    public void ToggleArbiterMismatchPanel()
    {
        arbiterMismatchPanel.SetActive(!arbiterMismatchPanel.activeSelf);
    }

    /// <summary>
    /// toggles the ping panel
    /// </summary>
    public void TogglePingPanel()
    {
        pingPanel.SetActive(!pingPanel.activeSelf);
    }
    #endregion
}