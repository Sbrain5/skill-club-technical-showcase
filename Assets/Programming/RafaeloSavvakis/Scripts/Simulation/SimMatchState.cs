/// <summary>
/// represents the authoritative match state used by the rollback-compatible gameplay layer
/// </summary>
public enum SimMatchState
{
    Playing = 0,
    RoundOver = 1,
    MatchOver = 2
}