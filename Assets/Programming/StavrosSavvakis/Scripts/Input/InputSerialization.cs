/// <summary>
/// frame input data and methods for rollback input
/// </summary>
public static class InputSerialization
{
    #region enums
    public enum DirectionInput
    {
        Unknown,
        DownBack,
        Down,
        DownForward,
        Back,
        Neutral,
        Forward,
        UpBack,
        Up,
        UpForward
    }

    public enum ButtonState
    {
        None,
        Pressed,
        Held,
        Released
    }
    #endregion

    #region frame input
    /// <summary>
    /// one frame of player input
    /// </summary>
    public struct FrameInput
    {
        public int frame;
        public DirectionInput direction;
        public ButtonState punch;
        public ButtonState kick;
        public ButtonState heavyAttack;

        /// <summary>
        /// creates one frame input
        /// </summary>
        /// <param name="frameNumber">frame number</param>
        public FrameInput(int frameNumber)
        {
            frame = frameNumber;
            direction = DirectionInput.Unknown;
            punch = ButtonState.None;
            kick = ButtonState.None;
            heavyAttack = ButtonState.None;
        }

        /// <summary>
        /// checks if two frame inputs are the same
        /// </summary>
        /// <param name="other">other input</param>
        /// <returns>true if same</returns>
        public bool IsSame(FrameInput other)
        {
            return frame == other.frame && direction == other.direction && punch == other.punch && kick == other.kick && heavyAttack == other.heavyAttack;
        }

        /// <summary>
        /// returns input text
        /// </summary>
        /// <returns>input text</returns>
        public string GetText()
        {
            return "frame: " + frame + " direction: " + direction + " punch: " + punch + " kick: " + kick + " heavy attack: " + heavyAttack;
        }
    }
    #endregion

    #region methods
    /// <summary>
    /// creates neutral input for one frame
    /// </summary>
    /// <param name="frameNumber">frame number</param>
    /// <returns>neutral input</returns>
    public static FrameInput CreateNeutralInput(int frameNumber)
    {
        FrameInput input = new FrameInput(frameNumber);
        input.direction = DirectionInput.Neutral;
        return input;
    }

    /// <summary>
    /// checks if axis value is valid
    /// </summary>
    /// <param name="value">axis value</param>
    /// <returns>true if valid</returns>
    private static bool IsAxisValid(int value)
    {
        return value >= -1 && value <= 1;
    }

    /// <summary>
    /// converts axis values to direction
    /// </summary>
    /// <param name="horizontal">horizontal input</param>
    /// <param name="vertical">vertical input</param>
    /// <returns>direction value</returns>
    public static DirectionInput GetDirectionFromAxis(int horizontal, int vertical)
    {
        if (!IsAxisValid(horizontal) || !IsAxisValid(vertical))
        {
            return DirectionInput.Unknown;
        }

        if (horizontal == -1 && vertical == -1)
        {
            return DirectionInput.DownBack;
        }

        if (horizontal == 0 && vertical == -1)
        {
            return DirectionInput.Down;
        }

        if (horizontal == 1 && vertical == -1)
        {
            return DirectionInput.DownForward;
        }

        if (horizontal == -1 && vertical == 0)
        {
            return DirectionInput.Back;
        }

        if (horizontal == 0 && vertical == 0)
        {
            return DirectionInput.Neutral;
        }

        if (horizontal == 1 && vertical == 0)
        {
            return DirectionInput.Forward;
        }

        if (horizontal == -1 && vertical == 1)
        {
            return DirectionInput.UpBack;
        }

        if (horizontal == 0 && vertical == 1)
        {
            return DirectionInput.Up;
        }

        if (horizontal == 1 && vertical == 1)
        {
            return DirectionInput.UpForward;
        }

        return DirectionInput.Unknown;
    }

    /// <summary>
    /// converts direction to axis values
    /// </summary>
    /// <param name="direction">direction value</param>
    /// <param name="horizontal">horizontal result</param>
    /// <param name="vertical">vertical result</param>
    public static void GetAxisFromDirection(DirectionInput direction, out int horizontal, out int vertical)
    {
        horizontal = 0;
        vertical = 0;

        switch (direction)
        {
            case DirectionInput.DownBack:
                horizontal = -1; // back
                vertical = -1; // down
                break;

            case DirectionInput.Down:
                horizontal = 0; // no horizontal movement
                vertical = -1; // down
                break;

            case DirectionInput.DownForward:
                horizontal = 1; // forward
                vertical = -1; // down
                break;

            case DirectionInput.Back:
                horizontal = -1; // back
                vertical = 0; // no vertical movement
                break;

            case DirectionInput.Neutral:
                horizontal = 0; // no horizontal movement
                vertical = 0; // no vertical movement
                break;

            case DirectionInput.Forward:
                horizontal = 1; // forward
                vertical = 0; // no vertical movement
                break;

            case DirectionInput.UpBack:
                horizontal = -1; // back
                vertical = 1; // up
                break;

            case DirectionInput.Up:
                horizontal = 0; // no horizontal movement
                vertical = 1; // up
                break;

            case DirectionInput.UpForward:
                horizontal = 1; // forward
                vertical = 1; // up
                break;

            default:
                horizontal = 0; // unknown falls back to neutral
                vertical = 0; // unknown falls back to neutral
                break;
        }
    }

    /// <summary>
    /// flips left and right direction
    /// </summary>
    /// <param name="direction">direction value</param>
    /// <returns>flipped direction</returns>
    public static DirectionInput FlipHorizontal(DirectionInput direction)
    {
        switch (direction)
        {
            case DirectionInput.DownBack:
                return DirectionInput.DownForward; // back becomes forward

            case DirectionInput.DownForward:
                return DirectionInput.DownBack; // forward becomes back

            case DirectionInput.Back:
                return DirectionInput.Forward; // back becomes forward

            case DirectionInput.Forward:
                return DirectionInput.Back; // forward becomes back

            case DirectionInput.UpBack:
                return DirectionInput.UpForward; // back becomes forward

            case DirectionInput.UpForward:
                return DirectionInput.UpBack; // forward becomes back
        }

        return direction; // neutral up down and unknown stay the same
    }
    #endregion
}