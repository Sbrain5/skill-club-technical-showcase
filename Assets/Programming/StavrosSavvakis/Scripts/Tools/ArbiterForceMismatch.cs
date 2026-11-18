using UnityEngine;

/// <summary>
/// changes arbiter hashes for mismatch testing
/// </summary>
public class ArbiterForceMismatch : MonoBehaviour
{
    #region variables
    private bool checkpointMismatchEnabled;
    private bool finalMismatchEnabled;
    #endregion

    #region public methods
    /// <summary>
    /// turns checkpoint mismatch on
    /// </summary>
    public void EnableCheckpointMismatch()
    {
        checkpointMismatchEnabled = true; // the next checkpoint hash will be changed
        ArbiterLogView.AddLog("checkpoint mismatch test enabled");
    }

    /// <summary>
    /// turns final mismatch on
    /// </summary>
    public void EnableFinalMismatch()
    {
        finalMismatchEnabled = true; // the final hash will be changed
        ArbiterLogView.AddLog("final mismatch test enabled");
    }

    /// <summary>
    /// resets the local mismatch tests
    /// </summary>
    public void ResetTests()
    {
        checkpointMismatchEnabled = false;
        finalMismatchEnabled = false;
        ArbiterLogView.AddLog("mismatch tests reset for this match");
    }

    /// <summary>
    /// checks checkpoint mismatch
    /// </summary>
    /// <returns>true if checkpoint mismatch is on</returns>
    public bool IsCheckpointMismatchEnabled()
    {
        return checkpointMismatchEnabled; // the next checkpoint hash will be changed
    }

    /// <summary>
    /// checks final mismatch
    /// </summary>
    /// <returns>true if final mismatch is on</returns>
    public bool IsFinalMismatchEnabled()
    {
        return finalMismatchEnabled; // the final hash will be changed
    }

    /// <summary>
    /// changes a checkpoint hash
    /// </summary>
    /// <param name="hash">normal hash</param>
    /// <param name="playerId">local player id</param>
    /// <returns>changed hash</returns>
    public string ChangeCheckpointHash(string hash, ulong playerId)
    {
        return hash + " checkpoint test " + playerId; // the next checkpoint hash will be changed
    }

    /// <summary>
    /// changes a final hash
    /// </summary>
    /// <param name="hash">normal hash</param>
    /// <param name="playerId">local player id</param>
    /// <returns>changed hash</returns>
    public string ChangeFinalHash(string hash, ulong playerId)
    {
        return hash + " final test " + playerId; // the final hash will be changed
    }
    #endregion
}