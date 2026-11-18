/// <summary>
/// stores a deterministic two-dimensional position
/// </summary>
public struct FixedVector2
{
    #region variables
    public FixedNumber x;
    public FixedNumber y;
    #endregion

    #region constructors
    /// <summary>
    /// creates a fixed vector from two fixed numbers
    /// </summary>
    /// <param name="xValue">horizontal value</param>
    /// <param name="yValue">vertical value</param>
    public FixedVector2(FixedNumber xValue, FixedNumber yValue)
    {
        x = xValue;
        y = yValue;
    }
    #endregion
}