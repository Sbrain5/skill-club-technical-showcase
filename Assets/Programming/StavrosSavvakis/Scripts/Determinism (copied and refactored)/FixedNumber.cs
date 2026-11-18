/// <summary>
/// fixed number used for deterministic gameplay math
/// </summary>
public class FixedNumber
{
    #region variables
    private const byte POINT = 8;
    private int rawValue;
    public static readonly FixedNumber ZERO = new FixedNumber(0);
    public static readonly FixedNumber ONE = new FixedNumber(1 << POINT);
    #endregion

    #region constructors
    /// <summary>
    /// creates a fixed number from its raw value
    /// </summary>
    /// <param name="value">raw fixed-point value</param>
    private FixedNumber(int value)
    {
        rawValue = value;
    }
    #endregion

    #region properties
    /// <summary>
    /// returns the fixed number as a float
    /// </summary>
    public float ToFloat
    {
        get
        {
            return rawValue / (float)ONE.rawValue;
        }
    }
    #endregion

    #region methods
    /// <summary>
    /// creates a fixed number from a float
    /// </summary>
    /// <param name="number">float value</param>
    /// <returns>fixed number</returns>
    public static FixedNumber FromFloat(float number)
    {
        return new FixedNumber((int)(ONE.rawValue * number));
    }

    /// <summary>
    /// keeps a fixed number between the minimum and maximum values
    /// </summary>
    /// <param name="number">value to clamp</param>
    /// <param name="min">minimum value</param>
    /// <param name="max">maximum value</param>
    /// <returns>clamped fixed number</returns>
    public static FixedNumber Clamp(FixedNumber number, FixedNumber min, FixedNumber max)
    {
        if (number < min)
        {
            return min;
        }

        if (number > max)
        {
            return max;
        }

        return number;
    }
    #endregion

    #region operators
    /// <summary>
    /// adds two fixed numbers
    /// </summary>
    public static FixedNumber operator +(FixedNumber first, FixedNumber second)
    {
        return new FixedNumber(first.rawValue + second.rawValue);
    }

    /// <summary>
    /// subtracts two fixed numbers
    /// </summary>
    public static FixedNumber operator -(FixedNumber first, FixedNumber second)
    {
        return new FixedNumber(first.rawValue - second.rawValue);
    }

    /// <summary>
    /// multiplies an integer by a fixed number
    /// </summary>
    public static FixedNumber operator *(int amount, FixedNumber number)
    {
        return new FixedNumber(number.rawValue * amount);
    }

    /// <summary>
    /// multiplies two fixed numbers
    /// </summary>
    public static FixedNumber operator *(FixedNumber first, FixedNumber second)
    {
        long result;

        result = (long)first.rawValue * second.rawValue;

        return new FixedNumber((int)(result >> POINT));
    }

    /// <summary>
    /// checks if the first value is smaller
    /// </summary>
    public static bool operator <(FixedNumber first, FixedNumber second)
    {
        return first.rawValue < second.rawValue;
    }

    /// <summary>
    /// checks if the first value is bigger
    /// </summary>
    public static bool operator >(FixedNumber first, FixedNumber second)
    {
        return first.rawValue > second.rawValue;
    }

    /// <summary>
    /// checks if the first value is smaller or equal
    /// </summary>
    public static bool operator <=(FixedNumber first, FixedNumber second)
    {
        return first.rawValue <= second.rawValue;
    }

    /// <summary>
    /// checks if the first value is bigger or equal
    /// </summary>
    public static bool operator >=(FixedNumber first, FixedNumber second)
    {
        return first.rawValue >= second.rawValue;
    }
    #endregion

    #region object methods
    /// <summary>
    /// returns the deterministic raw value as a hash code
    /// </summary>
    /// <returns>fixed-number hash code</returns>
    public override int GetHashCode()
    {
        return rawValue.GetHashCode();
    }
    #endregion
}