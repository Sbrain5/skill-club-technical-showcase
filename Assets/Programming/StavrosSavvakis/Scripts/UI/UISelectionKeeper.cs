using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// keeps one ui button selected for controller navigation
/// </summary>
public class UISelectionKeeper : MonoBehaviour
{
    #region variables
    [SerializeField] private Selectable[] defaultButtons;
    private GameObject lastSelectedObject;
    #endregion

    #region unity methods
    private void Start()
    {
        RestoreSelection();
    }

    private void Update()
    {
        if (EventSystem.current == null)
        {
            return;
        }

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

        if (selectedObject != null)
        {
            lastSelectedObject = selectedObject;
            return;
        }

        RestoreSelection();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            return;
        }

        RestoreSelection();
    }
    #endregion

    #region methods
    /// <summary>
    /// restores ui selection
    /// </summary>
    private void RestoreSelection()
    {
        if (EventSystem.current == null)
        {
            return;
        }

        if (lastSelectedObject != null && lastSelectedObject.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(lastSelectedObject);
            return;
        }

        int i = 0;

        while (i < defaultButtons.Length)
        {
            Selectable button = defaultButtons[i];

            if (button != null && button.gameObject.activeInHierarchy && button.interactable)
            {
                EventSystem.current.SetSelectedGameObject(button.gameObject);
                lastSelectedObject = button.gameObject;
                return;
            }

            i++;
        }
    }
    #endregion
}