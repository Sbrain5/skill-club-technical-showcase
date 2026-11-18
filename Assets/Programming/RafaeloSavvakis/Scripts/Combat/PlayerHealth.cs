using UnityEngine;

/// <summary>
/// mirrors authoritative health values from the deterministic simulation
/// </summary>
public sealed class PlayerHealth : MonoBehaviour
{
    #region variables
    [Header("Runtime State")]
    [SerializeField] private int maxHealth;
    [SerializeField] private int currentHealth;
    #endregion

    #region properties
    /// <summary>
    /// gets the current health value
    /// </summary>
    public int CurrentHealth
    {
        get
        {
            return this.currentHealth;
        }
    }

    /// <summary>
    /// gets the maximum health value
    /// </summary>
    public int MaxHealth
    {
        get
        {
            return this.maxHealth;
        }
    }

    /// <summary>
    /// gets current health as a normalized value from 0 to 1
    /// </summary>
    public float HealthPercent
    {
        get
        {
            if (this.maxHealth <= 0)
            {
                return 0.0f;
            }

            return Mathf.Clamp01((float)this.currentHealth / this.maxHealth);
        }
    }
    #endregion

    #region methods
    /// <summary>
    /// applies authoritative health values from the simulation state
    /// </summary>
    /// <param name="state">authoritative player state</param>
    public void ApplyAuthoritativeHealthState(SimPlayerState state)
    {
        this.maxHealth = state.MaxHealth;
        this.currentHealth = state.CurrentHealth;
    }
    #endregion
}