using TMPro;
using UnityEngine;

/// <summary>
/// shows one message when the main menu opens
/// </summary>
public class TimeoutMessage : MonoBehaviour
{
    #region variables
    public static string pendingMessage;
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TMP_Text messageText;
    #endregion

    #region unity methods
    private void Start()
    {
        if (string.IsNullOrEmpty(pendingMessage))
        {
            messagePanel.SetActive(false);
            return;
        }

        messagePanel.SetActive(true);
        messageText.text = pendingMessage;
        pendingMessage = "";
    }
    #endregion
}