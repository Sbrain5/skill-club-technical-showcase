using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// shows ping and adds test delay to remote inputs
/// </summary>
public class RollbackPing : MonoBehaviour
{
    #region variables
    [SerializeField] private TMP_Text currentPingText;
    [SerializeField] private TMP_Text pingText;
    [SerializeField] private TMP_Text extraPingText;
    [SerializeField] private TMP_Text rollbackText;
    [SerializeField] private int changeAmount = 50;
    private RollbackBridge rollbackBridge;
    private int realPing;
    private int extraPing;
    #endregion

    #region unity methods
    private void Start()
    {
        StartCoroutine(FindScripts());
        RefreshText();
    }

    private void Update()
    {
        RefreshText();
    }
    #endregion

    #region methods
    /// <summary>
    /// finds the rollback bridge
    /// </summary>
    /// <returns>coroutine</returns>
    private IEnumerator FindScripts()
    {
        while (rollbackBridge == null)
        {
            rollbackBridge = FindFirstObjectByType<RollbackBridge>();
            yield return null;
        }
    }

    /// <summary>
    /// saves the latest ping
    /// </summary>
    /// <param name="newPing">new ping value</param>
    public void SetPing(int newPing)
    {
        realPing = newPing;
        RefreshText();
    }

    /// <summary>
    /// adds more test ping
    /// </summary>
    public void AddExtraPing()
    {
        extraPing += changeAmount;
        RefreshText();
    }

    /// <summary>
    /// removes test ping
    /// </summary>
    public void RemoveExtraPing()
    {
        extraPing = Mathf.Max(0, extraPing - changeAmount);
        RefreshText();
    }

    /// <summary>
    /// resets test ping
    /// </summary>
    public void ResetExtraPing()
    {
        extraPing = 0;
        RefreshText();
    }

    /// <summary>
    /// returns extra ping in seconds
    /// </summary>
    /// <returns>extra ping seconds</returns>
    public float GetExtraPingSeconds()
    {
        return extraPing / 1000f;
    }

    /// <summary>
    /// returns extra ping in milliseconds
    /// </summary>
    /// <returns>extra ping milliseconds</returns>
    public int GetExtraPingMilliseconds()
    {
        return extraPing;
    }

    /// <summary>
    /// returns current test ping
    /// </summary>
    /// <returns>current ping milliseconds</returns>
    public int GetCurrentPingMilliseconds()
    {
        return realPing + extraPing;
    }

    /// <summary>
    /// updates the ping text
    /// </summary>
    private void RefreshText()
    {
        if (currentPingText != null)
        {
            currentPingText.text = "current ping: " + GetCurrentPingMilliseconds() + " ms"; // shows the total ping including extra ping
        }

        if (pingText != null)
        {
            pingText.text = "real ping: " + realPing + " ms"; // shows the real ping without extra ping
        }

        if (extraPingText != null)
        {
            extraPingText.text = "added ping: " + extraPing + " ms"; // shows the extra ping added
        }

        if (rollbackText == null)
        {
            return;
        }

        if (rollbackBridge == null)
        {
            rollbackText.text = "rollback count: waiting"; // shows waiting for rollback bridge
            return;
        }

        rollbackText.text = "rollback count: " + rollbackBridge.GetRollbackCount(); // shows the rollback count from the rollback bridge
    }
    #endregion
}