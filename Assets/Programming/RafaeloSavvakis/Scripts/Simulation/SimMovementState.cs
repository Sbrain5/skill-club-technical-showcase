/// <summary>
/// represents the authoritative movement state used by the rollback-compatible gameplay layer
/// </summary>
public enum SimMovementState
{
    Idle = 0,
    Walk = 1,
    Jump = 2,
    Crouch = 3
}