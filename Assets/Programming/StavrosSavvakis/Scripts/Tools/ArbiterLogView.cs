using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// shows arbiter messages inside the game
/// </summary>
public class ArbiterLogView : MonoBehaviour
{
    #region variables
    [SerializeField] private TMP_Text logText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private int maxLines = 80;
    private static ArbiterLogView arbiterLogView;
    private List<string> messages;
    #endregion

    #region unity methods
    private void Awake()
    {
        arbiterLogView = this;
        messages = new List<string>();
        RefreshText(true);
    }

    private void OnDestroy()
    {
        if (arbiterLogView == this)
        {
            arbiterLogView = null;
        }
    }
    #endregion

    #region methods
    /// <summary>
    /// adds one arbiter message
    /// </summary>
    /// <param name="message">message text</param>
    public static void AddLog(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        arbiterLogView.messages.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message); // adds the current time before the message so the log is easier to read

        // keeps only the newest messages so the list does not grow forever
        if (arbiterLogView.messages.Count > arbiterLogView.maxLines)
        {
            arbiterLogView.messages.RemoveAt(0);
        }

        arbiterLogView.RefreshText(false); // false means it should not force the view to the bottom if the user is reading older logs
    }

    /// <summary>
    /// refreshes the log after opening the panel
    /// </summary>
    public static void RefreshLogView()
    {
        arbiterLogView.RefreshText(true);
    }

    /// <summary>
    /// clears all messages
    /// </summary>
    public void ClearLogs()
    {
        messages.Clear();
        RefreshText(true);
    }

    /// <summary>
    /// updates the log text and scroll size
    /// </summary>
    /// <param name="moveToBottom">true if it should show the newest message</param>
    private void RefreshText(bool moveToBottom)
    {
        float oldPosition = scrollRect.verticalNormalizedPosition; // saves the current scroll position before changing the text size
        bool shouldMoveToBottom = moveToBottom || oldPosition <= 0.05f; // keeps the view at the bottom only if it was already near the newest messages

        if (messages.Count == 0)
        {
            logText.text = "no arbiter messages yet";
        }
        else
        {
            logText.text = string.Join("\n\n", messages);
        }

        Canvas.ForceUpdateCanvases();
        float textWidth = scrollRect.content.rect.width;

        // uses the viewport width if the content width is not ready yet
        if (textWidth <= 0f)
        {
            textWidth = scrollRect.viewport.rect.width;
        }

        // fallback width in case Unity has not calculated the UI size yet
        if (textWidth <= 0f)
        {
            textWidth = 600f;
        }

        logText.ForceMeshUpdate();
        float finalHeight = logText.GetPreferredValues(logText.text, textWidth, 0f).y + 40f; // calculates how tall the text needs to be so the scroll view can scroll correctly

        // makes sure the content is at least slightly taller than the viewport
        if (finalHeight < scrollRect.viewport.rect.height)
        {
            finalHeight = scrollRect.viewport.rect.height + 1f;
        }

        scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, finalHeight);
        logText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, finalHeight);
        logText.rectTransform.anchoredPosition = Vector2.zero;
        Canvas.ForceUpdateCanvases();

        // either follows the newest log or keeps the user's previous scroll position
        if (shouldMoveToBottom)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
        else
        {
            scrollRect.verticalNormalizedPosition = oldPosition;
        }
    }
    #endregion
}