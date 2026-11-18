/// <summary>
/// represents one deterministic axis-aligned combat rectangle
/// </summary>
public struct SimCombatBox
{
    #region variables
    public FixedVector2 Center;
    public FixedVector2 HalfSize;
    #endregion

    #region constructors
    /// <summary>
    /// creates one deterministic combat box
    /// </summary>
    /// <param name="center">world-space box centre</param>
    /// <param name="halfSize">half width and half height</param>
    public SimCombatBox(
        FixedVector2 center,
        FixedVector2 halfSize)
    {
        this.Center = center;
        this.HalfSize = halfSize;
    }
    #endregion

    #region properties
    public FixedNumber Left
    {
        get
        {
            return this.Center.x - this.HalfSize.x;
        }
    }

    public FixedNumber Right
    {
        get
        {
            return this.Center.x + this.HalfSize.x;
        }
    }

    public FixedNumber Bottom
    {
        get
        {
            return this.Center.y - this.HalfSize.y;
        }
    }

    public FixedNumber Top
    {
        get
        {
            return this.Center.y + this.HalfSize.y;
        }
    }
    #endregion

    #region public methods
    /// <summary>
    /// checks whether this box overlaps another deterministic box
    /// </summary>
    /// <param name="other">other combat box</param>
    /// <returns>true when both boxes overlap</returns>
    public bool Overlaps(SimCombatBox other)
    {
        if (this.Right < other.Left ||
            this.Left > other.Right)
        {
            return false;
        }

        if (this.Top < other.Bottom ||
            this.Bottom > other.Top)
        {
            return false;
        }

        return true;
    }
    #endregion
}