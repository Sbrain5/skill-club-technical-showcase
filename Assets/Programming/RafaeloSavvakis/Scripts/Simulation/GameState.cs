/// <summary>
/// represents the authoritative gameplay state used by the rollback-compatible gameplay layer
/// </summary>
public partial class GameState
{
    #region variables
    private bool playerOneLocal;

    public int Frame;
    public SimMatchState MatchState;
    public int RoundWinnerSlot;
    public int MatchWinnerSlot;
    public int RoundEndTicksRemaining;
    public int RoundTimerTicksRemaining;
    public bool RoundEndedByTimeUp;
    public int Player01RoundWins;
    public int Player02RoundWins;
    public SimPlayerState Player01;
    public SimPlayerState Player02;
    public SimFighterProfile Player01FighterProfile;
    public SimFighterProfile Player02FighterProfile;
    #endregion

    #region constructors
    /// <summary>
    /// creates a clean starting gameplay state
    /// </summary>
    /// <param name="isPlayerOneLocal">true if player one is local</param>
    public GameState(bool isPlayerOneLocal)
    {
        this.playerOneLocal = isPlayerOneLocal;

        this.Player01FighterProfile =
            this.GetFighterProfileFromCharacterIndex(
                LoadingManager.GetLeftCharacterIndex());

        this.Player02FighterProfile =
            this.GetFighterProfileFromCharacterIndex(
                LoadingManager.GetRightCharacterIndex());

        this.Frame = 0;
        this.MatchState = SimMatchState.Playing;
        this.RoundWinnerSlot = 0;
        this.MatchWinnerSlot = 0;
        this.RoundEndTicksRemaining = 0;
        this.RoundTimerTicksRemaining = RoundTimerStartTicks;
        this.RoundEndedByTimeUp = false;
        this.Player01RoundWins = 0;
        this.Player02RoundWins = 0;
        this.Player01 = new SimPlayerState();
        this.Player02 = new SimPlayerState();

        this.InitializeStartingPlayers();
    }

    /// <summary>
    /// creates a deep copy of another gameplay state
    /// </summary>
    /// <param name="other">the source gameplay state to copy</param>
    public GameState(GameState other)
    {
        this.playerOneLocal = other.playerOneLocal;

        this.Player01FighterProfile =
            other.Player01FighterProfile;

        this.Player02FighterProfile =
            other.Player02FighterProfile;

        this.Frame = other.Frame;
        this.MatchState = other.MatchState;
        this.RoundWinnerSlot = other.RoundWinnerSlot;
        this.MatchWinnerSlot = other.MatchWinnerSlot;
        this.RoundEndTicksRemaining = other.RoundEndTicksRemaining;
        this.RoundTimerTicksRemaining = other.RoundTimerTicksRemaining;
        this.RoundEndedByTimeUp = other.RoundEndedByTimeUp;
        this.Player01RoundWins = other.Player01RoundWins;
        this.Player02RoundWins = other.Player02RoundWins;
        this.Player01 = new SimPlayerState(other.Player01);
        this.Player02 = new SimPlayerState(other.Player02);
    }

    /// <summary>
    /// maps one character-selection index to the deterministic fighter profile
    /// </summary>
    /// <param name="characterIndex">character-selection index</param>
    /// <returns>deterministic fighter profile</returns>
    private SimFighterProfile GetFighterProfileFromCharacterIndex(
        int characterIndex)
    {
        switch (characterIndex)
        {
            case 0:
                return SimFighterProfile.Fighter05;

            case 1:
                return SimFighterProfile.Fighter02;

            default:
                return SimFighterProfile.Fighter05;
        }
    }
    #endregion

    #region public methods
    /// <summary>
    /// advances the authoritative gameplay state by exactly one frame
    /// </summary>
    /// <param name="localInput">the local player's serialized frame input</param>
    /// <param name="remoteInput">the remote player's serialized frame input</param>
    /// <returns>the next authoritative gameplay state</returns>
    public GameState Tick(InputSerialization.FrameInput localInput, InputSerialization.FrameInput remoteInput)
    {
        GameState nextState = new GameState(this);
        InputSerialization.FrameInput player01Input;
        InputSerialization.FrameInput player02Input;

        if (nextState.MatchState != SimMatchState.Playing)
        {
            nextState.ApplyNonPlayingMatchTick();
            nextState.Frame++;
            return nextState;
        }

        nextState.MapInputs(localInput, remoteInput, out player01Input, out player02Input);
        nextState.ApplyPlayerTick(nextState.Player01, player01Input, Player01HitWindowOffset);
        nextState.ApplyPlayerTick(nextState.Player02, player02Input, Player02HitWindowOffset);

        nextState.ResolvePlayerBodyBlocking(this.Player01, this.Player02, nextState.Player01, nextState.Player02);
        nextState.UpdateFacingDirections(nextState.Player01, nextState.Player02);
        nextState.ResolveHitReactions(nextState.Player01, nextState.Player02);
        nextState.CheckRoundOverFromHealth();
        nextState.ApplyRoundTimerTick();
        nextState.ResolvePlayerBodyBlocking(this.Player01, this.Player02, nextState.Player01, nextState.Player02);
        nextState.UpdateFacingDirections(nextState.Player01, nextState.Player02);

        nextState.Frame++;
        return nextState;
    }
    #endregion

    #region setup methods
    /// <summary>
    /// initializes the clean match start positions and states for both players
    /// </summary>
    private void InitializeStartingPlayers()
    {
        this.Player01.Position = new FixedVector2(Player01StartX, GroundY);
        this.Player01.PositionZ = PlayerStartZ;

        this.Player02.Position = new FixedVector2(Player02StartX, GroundY);
        this.Player02.PositionZ = PlayerStartZ;

        this.Player01.VerticalVelocity = FixedNumber.ZERO;
        this.Player02.VerticalVelocity = FixedNumber.ZERO;

        this.Player01.AirHorizontalDirection = 0;
        this.Player02.AirHorizontalDirection = 0;

        this.Player01.WasJumpHeldLastFrame = false;
        this.Player02.WasJumpHeldLastFrame = false;

        this.Player01.MustReleaseHorizontalAfterLanding = false;
        this.Player02.MustReleaseHorizontalAfterLanding = false;

        this.Player01.IsGrounded = true;
        this.Player02.IsGrounded = true;

        this.Player01.IsFacingRight = true;
        this.Player02.IsFacingRight = false;

        this.Player01.MovementState = SimMovementState.Idle;
        this.Player02.MovementState = SimMovementState.Idle;

        this.ResetGroundStepState(this.Player01);
        this.ResetGroundStepState(this.Player02);

        this.ResetAttackState(this.Player01);
        this.ResetAttackState(this.Player02);

        this.ResetHitstunState(this.Player01);
        this.ResetHitstunState(this.Player02);

        this.ResetHitMemory(this.Player01);
        this.ResetHitMemory(this.Player02);

        this.ResetHealthState(this.Player01);
        this.ResetHealthState(this.Player02);
    }
    #endregion

    #region input methods
    /// <summary>
    /// maps local and remote frame inputs to player one and player two
    /// </summary>
    /// <param name="localInput">the local frame input</param>
    /// <param name="remoteInput">the remote frame input</param>
    /// <param name="player01Input">the mapped player one input</param>
    /// <param name="player02Input">the mapped player two input</param>
    private void MapInputs(
        InputSerialization.FrameInput localInput,
        InputSerialization.FrameInput remoteInput,
        out InputSerialization.FrameInput player01Input,
        out InputSerialization.FrameInput player02Input)
    {
        if (this.playerOneLocal)
        {
            player01Input = localInput;
            player02Input = remoteInput;
            return;
        }

        player01Input = remoteInput;
        player02Input = localInput;
    }
    #endregion

    #region tick methods
    /// <summary>
    /// applies one deterministic movement and combat step for a single player
    /// </summary>
    /// <param name="player">the player state to update</param>
    /// <param name="frameInput">the current serialized input for that player</param>
    /// <param name="hitWindowOffset">unique player offset used when creating deterministic hit window ids</param>
    private void ApplyPlayerTick(SimPlayerState player, InputSerialization.FrameInput frameInput, int hitWindowOffset)
    {
        int horizontalAxis;
        int verticalAxis;
        int movementStateHorizontalAxis;

        bool crouchHeld;
        bool jumpHeld;
        bool jumpRequested;
        bool stepWasActiveThisTick;

        InputSerialization.GetAxisFromDirection(frameInput.direction, out horizontalAxis, out verticalAxis);

        jumpHeld = verticalAxis > 0;
        jumpRequested = jumpHeld && !player.WasJumpHeldLastFrame;

        this.ApplyGroundRule(player);

        if (player.IsInHitstun)
        {
            this.ApplyHitstunTick(player);
            this.ApplyVerticalMovement(player);
            this.ApplyGroundRule(player);
            this.UpdateHitstunMovementState(player);
            player.WasJumpHeldLastFrame = jumpHeld;
            return;
        }

        if (player.AttackState != SimAttackState.None)
        {
            this.ApplyAttackMovementLock(player, frameInput, hitWindowOffset);
            player.WasJumpHeldLastFrame = jumpHeld;
            return;
        }

        crouchHeld = verticalAxis < 0 && player.IsGrounded;

        if (jumpRequested && player.IsGrounded && !crouchHeld)
        {
            player.IsGrounded = false;
            player.VerticalVelocity = JumpVelocity;
            player.AirHorizontalDirection = horizontalAxis;
            player.MovementState = SimMovementState.Jump;

            this.ResetGroundStepState(player);
        }

        this.ApplyGroundStepRecovery(player);
        this.ApplyGroundStepVisualLockout(player);

        movementStateHorizontalAxis = this.GetAllowedGroundHorizontalAxis(player, horizontalAxis);

        if (player.IsGrounded && !crouchHeld)
        {
            this.TryStartGroundStep(player, movementStateHorizontalAxis);
        }

        stepWasActiveThisTick = player.StepTicksRemaining > 0;

        if (player.IsGrounded && !crouchHeld)
        {
            this.ApplyGroundStep(player);
        }

        if (!player.IsGrounded)
        {
            this.ApplyAirJumpMovement(player);
        }

        this.ApplyVerticalMovement(player);
        this.ApplyGroundRule(player);
        this.UpdateMovementState(player, stepWasActiveThisTick, crouchHeld);
        this.ApplyAttackTick(player, frameInput, hitWindowOffset);

        player.WasJumpHeldLastFrame = jumpHeld;
    }
    #endregion
}