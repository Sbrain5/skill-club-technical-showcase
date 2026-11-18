/// <summary>
/// contains deterministic facing direction logic
/// </summary>
public partial class GameState
{
    #region facing methods
    /// <summary>
    /// updates both players so they face each other based on world x positions
    /// </summary>
    /// <param name="player01">player one state</param>
    /// <param name="player02">player two state</param>
    private void UpdateFacingDirections(SimPlayerState player01, SimPlayerState player02)
    {
        if (player01.Position.x < player02.Position.x)
        {
            player01.IsFacingRight = true;
            player02.IsFacingRight = false;
            return;
        }

        if (player01.Position.x > player02.Position.x)
        {
            player01.IsFacingRight = false;
            player02.IsFacingRight = true;
        }
    }
    #endregion
}