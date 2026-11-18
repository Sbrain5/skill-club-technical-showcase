/// <summary>
/// stores up to two deterministic combat boxes
/// </summary>
public struct SimCombatBoxSet
{
    #region variables
    public bool HasPrimary;
    public bool HasSecondary;
    public SimCombatBox Primary;
    public SimCombatBox Secondary;
    #endregion

    #region factory methods
    /// <summary>
    /// creates an empty combat-box set
    /// </summary>
    /// <returns>empty set</returns>
    public static SimCombatBoxSet CreateEmpty()
    {
        return new SimCombatBoxSet();
    }

    /// <summary>
    /// creates a set containing one combat box
    /// </summary>
    /// <param name="primary">primary combat box</param>
    /// <returns>single-box set</returns>
    public static SimCombatBoxSet CreateSingle(
        SimCombatBox primary)
    {
        SimCombatBoxSet result = new SimCombatBoxSet();

        result.HasPrimary = true;
        result.Primary = primary;

        return result;
    }

    /// <summary>
    /// creates a set containing two combat boxes
    /// </summary>
    /// <param name="primary">primary combat box</param>
    /// <param name="secondary">secondary combat box</param>
    /// <returns>two-box set</returns>
    public static SimCombatBoxSet CreateDouble(
        SimCombatBox primary,
        SimCombatBox secondary)
    {
        SimCombatBoxSet result = new SimCombatBoxSet();

        result.HasPrimary = true;
        result.HasSecondary = true;
        result.Primary = primary;
        result.Secondary = secondary;

        return result;
    }
    #endregion

    #region public methods
    /// <summary>
    /// checks whether any box in this set overlaps any box in another set
    /// </summary>
    /// <param name="other">other combat-box set</param>
    /// <returns>true when any valid pair overlaps</returns>
    public bool Overlaps(SimCombatBoxSet other)
    {
        if (this.HasPrimary &&
            other.HasPrimary &&
            this.Primary.Overlaps(other.Primary))
        {
            return true;
        }

        if (this.HasPrimary &&
            other.HasSecondary &&
            this.Primary.Overlaps(other.Secondary))
        {
            return true;
        }

        if (this.HasSecondary &&
            other.HasPrimary &&
            this.Secondary.Overlaps(other.Primary))
        {
            return true;
        }

        if (this.HasSecondary &&
            other.HasSecondary &&
            this.Secondary.Overlaps(other.Secondary))
        {
            return true;
        }

        return false;
    }
    #endregion
}