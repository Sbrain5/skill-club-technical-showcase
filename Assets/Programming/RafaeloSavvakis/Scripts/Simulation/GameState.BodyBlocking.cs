/// <summary>
/// contains deterministic player body blocking logic
/// </summary>
public partial class GameState
{
    #region body blocking methods
    /// <summary>
    /// prevents both players from crossing through each other using deterministic body blocking
    /// </summary>
    /// <param name="previousPlayer01">player one state from the previous frame</param>
    /// <param name="previousPlayer02">player two state from the previous frame</param>
    /// <param name="player01">player one state for the current frame</param>
    /// <param name="player02">player two state for the current frame</param>
    private void ResolvePlayerBodyBlocking(SimPlayerState previousPlayer01, SimPlayerState previousPlayer02, SimPlayerState player01, SimPlayerState player02)
    {
        FixedNumber currentDistance;
        FixedNumber player01DeltaX;
        FixedNumber player02DeltaX;
        bool player01MovedRight;
        bool player02MovedLeft;

        currentDistance = player02.Position.x - player01.Position.x;

        if (currentDistance >= PlayerBodyMinimumDistance)
        {
            return;
        }

        player01DeltaX = player01.Position.x - previousPlayer01.Position.x;
        player02DeltaX = player02.Position.x - previousPlayer02.Position.x;

        player01MovedRight = player01DeltaX > FixedNumber.ZERO;
        player02MovedLeft = player02DeltaX < FixedNumber.ZERO;

        if (player01MovedRight && player02MovedLeft)
        {
            this.RestorePreviousBodyPositions(previousPlayer01, previousPlayer02, player01, player02);
            return;
        }

        if (player01MovedRight)
        {
            this.ClampPlayer01AgainstPlayer02(player01, player02);
            return;
        }

        if (player02MovedLeft)
        {
            this.ClampPlayer02AgainstPlayer01(player01, player02);
            return;
        }

        this.RestorePreviousBodyPositions(previousPlayer01, previousPlayer02, player01, player02);
    }

    /// <summary>
    /// restores previous horizontal positions while keeping the new vertical positions
    /// </summary>
    /// <param name="previousPlayer01">player one state from the previous frame</param>
    /// <param name="previousPlayer02">player two state from the previous frame</param>
    /// <param name="player01">player one state for the current frame</param>
    /// <param name="player02">player two state for the current frame</param>
    private void RestorePreviousBodyPositions(SimPlayerState previousPlayer01, SimPlayerState previousPlayer02, SimPlayerState player01, SimPlayerState player02)
    {
        FixedNumber player01X;
        FixedNumber player02X;

        player01X = FixedNumber.Clamp(previousPlayer01.Position.x, MinStageX, MaxStageX);
        player02X = FixedNumber.Clamp(previousPlayer02.Position.x, MinStageX, MaxStageX);

        player01.Position = new FixedVector2(player01X, player01.Position.y);
        player02.Position = new FixedVector2(player02X, player02.Position.y);
    }

    /// <summary>
    /// clamps player one against player two without moving player two
    /// </summary>
    /// <param name="player01">player one state</param>
    /// <param name="player02">player two state</param>
    private void ClampPlayer01AgainstPlayer02(SimPlayerState player01, SimPlayerState player02)
    {
        FixedNumber targetX;

        targetX = player02.Position.x - PlayerBodyMinimumDistance;
        targetX = FixedNumber.Clamp(targetX, MinStageX, MaxStageX);

        player01.Position = new FixedVector2(targetX, player01.Position.y);
    }

    /// <summary>
    /// clamps player two against player one without moving player one
    /// </summary>
    /// <param name="player01">player one state</param>
    /// <param name="player02">player two state</param>
    private void ClampPlayer02AgainstPlayer01(SimPlayerState player01, SimPlayerState player02)
    {
        FixedNumber targetX;

        targetX = player01.Position.x + PlayerBodyMinimumDistance;
        targetX = FixedNumber.Clamp(targetX, MinStageX, MaxStageX);

        player02.Position = new FixedVector2(targetX, player02.Position.y);
    }
    #endregion
}