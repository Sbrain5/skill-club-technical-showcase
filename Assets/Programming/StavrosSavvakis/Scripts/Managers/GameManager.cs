using PurrNet;
using UnityEngine;
using static InputSerialization;

/// <summary>
/// connects input rollback and spawned players in the game scene
/// </summary>
public class GameManager : NetworkBehaviour
{
    #region variables
    [SerializeField] private RollbackBridge rollbackBridge;
    [SerializeField] private InputManager inputManager;
    private PlayerController playerOneController;
    private PlayerController playerTwoController;
    private GameState currentAuthoritativeState; // latest state produced by the rollback simulation
    private GameState finalAuthoritativeState; // first saved MatchOver state used for the final arbiter result
    private int inputDelayFrames = 3; // sends local input a few frames ahead to give remote input time to arrive
    private int sharedStartDelayFrames = 120; // delay before starting rollback after both players are ready
    private int finalRollbackFrame; // rollback frame where the final state was saved
    private int sharedRollbackStartFrame; // network tick frame where both clients should start rollback
    private bool playersReady;
    private bool rollbackStarted;
    private bool playerOneLocal;
    private bool finalStateSaved; // true after the first MatchOver state is saved
    private bool readySentToServer;
    private bool sharedRollbackStartFrameReceived; // true after the server sends the shared rollback start frame
    private bool serverPlayerOneReady;
    private bool serverPlayerTwoReady;
    private bool serverSharedStartFrameSent; // prevents the server from sending the start frame more than once
    #endregion

    #region properties
    /// <summary>
    /// returns the current rollback game state
    /// </summary>
    public GameState CurrentAuthoritativeState
    {
        get
        {
            return this.currentAuthoritativeState;
        }
    }
    #endregion

    #region unity methods
    protected override void OnSpawned(bool asServer)
    {
        if (asServer)
        {
            return;
        }

        this.playerOneLocal = this.isHost;
        this.finalAuthoritativeState = null;
        this.finalRollbackFrame = -1;
        this.finalStateSaved = false;
        this.sharedRollbackStartFrame = -1;
        this.sharedRollbackStartFrameReceived = false;
        this.readySentToServer = false;
    }

    private void Awake()
    {
        Application.runInBackground = true;
    }

    private void Update()
    {
        // wait until both spawned players are found and assigned
        if (!this.playersReady)
        {
            this.TryPreparePlayers();
            return;
        }

        // after the players are ready, prepare the rollback start
        this.TrySendRollbackReadyToServer();
        this.TryStartRollbackBridge();
    }

    protected override void OnDestroy()
    {
        // stop listening to network ticks when this object is destroyed
        if (this.networkManager != null)
        {
            this.networkManager.onTick -= this.RunMatchTick;
        }

        base.OnDestroy();
    }
    #endregion

    #region setup methods
    /// <summary>
    /// finds both spawned players and prepares them for rollback
    /// </summary>
    private void TryPreparePlayers()
    {
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        // wait until both player objects exist
        if (players.Length < 2)
        {
            return;
        }

        this.playerOneLocal = this.isHost;

        // assign the two found players
        this.playerOneController = players[0];
        this.playerTwoController = players[1];

        // the left player is always player one
        if (players[0].transform.position.x > players[1].transform.position.x)
        {
            this.playerOneController = players[1];
            this.playerTwoController = players[0];
        }

        // setup player one
        this.playerOneController.SetPlayerSlot(PlayerController.PlayerSlot.Player01);
        this.playerOneController.SetCharacterIndex(LoadingManager.GetLeftCharacterIndex());
        this.playerOneController.SetLocalPresentation(this.playerOneLocal);

        // setup player two
        this.playerTwoController.SetPlayerSlot(PlayerController.PlayerSlot.Player02);
        this.playerTwoController.SetCharacterIndex(LoadingManager.GetRightCharacterIndex());
        this.playerTwoController.SetLocalPresentation(!this.playerOneLocal);

        // apply the starting simulation state
        this.ApplyStartingStateToPlayers();
        this.playersReady = true;
    }

    /// <summary>
    /// applies the clean starting state before rollback begins
    /// </summary>
    private void ApplyStartingStateToPlayers()
    {
        if (this.playerOneController == null || this.playerTwoController == null)
        {
            return;
        }

        // apply the starting state to both players and save it as the current authoritative state
        GameState startingState = new GameState(this.playerOneLocal);
        this.currentAuthoritativeState = startingState;
        this.playerOneController.ApplyAuthoritativeState(startingState.Player01);
        this.playerTwoController.ApplyAuthoritativeState(startingState.Player02);
        this.playerOneController.ApplyAuthoritativeCombatState(startingState.Player01);
        this.playerTwoController.ApplyAuthoritativeCombatState(startingState.Player02);
    }

    /// <summary>
    /// tells the server that this client is ready for rollback
    /// </summary>
    private void TrySendRollbackReadyToServer()
    {
        if (this.readySentToServer)
        {
            return;
        }

        // send the ready message only once
        this.readySentToServer = true;
        this.SendRollbackReadyToServer(this.playerOneLocal);
    }

    /// <summary>
    /// starts rollback when everything is ready
    /// </summary>
    private void TryStartRollbackBridge()
    {
        // wait until rollback has not started, players are ready, and the server sent the shared start frame
        if (this.rollbackStarted || !this.playersReady || !this.sharedRollbackStartFrameReceived)
        {
            return;
        }

        // start rollback from the same frame on both clients
        this.rollbackStarted = true;
        this.rollbackBridge.StartRollbackBridge(this.playerOneLocal, this.sharedRollbackStartFrame);
        this.networkManager.onTick += this.RunMatchTick;
        Debug.Log("rollback started | host: " + this.isHost + " | player one local: " + this.playerOneLocal + " | start frame: " + this.sharedRollbackStartFrame);
    }
    #endregion

    #region tick methods
    /// <summary>
    /// runs one shared network tick
    /// </summary>
    /// <param name="asServer">true on server side</param>
    private void RunMatchTick(bool asServer)
    {
        if (asServer || !this.rollbackStarted)
        {
            return;
        }

        int targetFrame = (int)this.networkManager.tickModule.syncedTick; // use the synced network tick as the target rollback frame

        // wait until the shared rollback start frame is reached
        if (targetFrame < this.sharedRollbackStartFrame)
        {
            return;
        }

        // run rollback until it catches up to the current network frame
        while (this.rollbackBridge.GetCurrentFrame() <= targetFrame)
        {
            int frameToSend = this.rollbackBridge.GetCurrentFrame() + this.inputDelayFrames;
            this.rollbackBridge.SendLocalInput(this.CreateLocalInput(frameToSend));
            this.rollbackBridge.RunFrame();
        }

        GameState currentState = this.rollbackBridge.GetCurrentState();

        if (currentState == null)
        {
            return;
        }

        // save and apply the newest authoritative rollback state
        this.currentAuthoritativeState = currentState;
        this.SaveFinalStateIfNeeded(currentState);
        this.playerOneController.ApplyAuthoritativeState(currentState.Player01);
        this.playerTwoController.ApplyAuthoritativeState(currentState.Player02);
        this.playerOneController.ApplyAuthoritativeCombatState(currentState.Player01);
        this.playerTwoController.ApplyAuthoritativeCombatState(currentState.Player02);
        this.playerOneController.ApplyAuthoritativeMatchVoiceState(currentState);
        this.playerTwoController.ApplyAuthoritativeMatchVoiceState(currentState);
    }
    #endregion

    #region input methods
    /// <summary>
    /// creates one local frame input
    /// </summary>
    /// <param name="frame">frame number</param>
    /// <returns>frame input</returns>
    private FrameInput CreateLocalInput(int frame)
    {
        FrameInput input = new FrameInput(frame);
        Vector2 move = this.inputManager.CurrentGameplayMove;
        int horizontal = 0;
        int vertical = 0;

        // convert movement input into simple -1, 0, or 1 axis values
        if (move.x > 0.5f)
        {
            horizontal = 1;
        }
        else if (move.x < -0.5f)
        {
            horizontal = -1;
        }

        if (move.y > 0.5f)
        {
            vertical = 1;
        }
        else if (move.y < -0.5f)
        {
            vertical = -1;
        }

        input.direction = GetDirectionFromAxis(horizontal, vertical); // convert the axis values into the rollback direction enum

        // save attack presses for this rollback frame
        if (this.inputManager.ConsumeLightAttackPressedThisFrame())
        {
            input.punch = ButtonState.Pressed;
        }

        if (this.inputManager.ConsumeMediumAttackPressedThisFrame())
        {
            input.kick = ButtonState.Pressed;
        }

        if (this.inputManager.ConsumeHeavyAttackPressedThisFrame())
        {
            input.heavyAttack = ButtonState.Pressed;
        }

        return input;
    }
    #endregion

    #region final state methods
    /// <summary>
    /// saves the final state when the match ends
    /// </summary>
    /// <param name="currentState">current game state</param>
    private void SaveFinalStateIfNeeded(GameState currentState)
    {
        if (this.finalStateSaved || currentState.MatchState != SimMatchState.MatchOver)
        {
            return;
        }

        // save the first match over state for the arbiter result
        this.finalAuthoritativeState = new GameState(currentState);
        this.finalRollbackFrame = this.rollbackBridge.GetCurrentFrame();
        this.finalStateSaved = true;
    }

    /// <summary>
    /// checks if final state is saved
    /// </summary>
    /// <returns>true if final state exists</returns>
    public bool HasFinalState()
    {
        return this.finalStateSaved && this.finalAuthoritativeState != null;
    }

    /// <summary>
    /// returns saved final state
    /// </summary>
    /// <returns>final game state</returns>
    public GameState GetFinalState()
    {
        return this.finalAuthoritativeState;
    }

    /// <summary>
    /// returns saved final rollback frame
    /// </summary>
    /// <returns>final rollback frame</returns>
    public int GetFinalRollbackFrame()
    {
        return this.finalRollbackFrame;
    }
    #endregion

    #region rpc methods
    /// <summary>
    /// tells the server that one client is ready
    /// </summary>
    /// <param name="readyClientIsPlayerOne">true if the ready client is player one</param>
    [ServerRpc(requireOwnership: false)]
    private void SendRollbackReadyToServer(bool readyClientIsPlayerOne)
    {
        // save which player is ready on the server
        if (readyClientIsPlayerOne)
        {
            this.serverPlayerOneReady = true;
        }
        else
        {
            this.serverPlayerTwoReady = true;
        }

        // wait until both players are ready and make sure the start frame is only sent once
        if (this.serverSharedStartFrameSent || !this.serverPlayerOneReady || !this.serverPlayerTwoReady)
        {
            return;
        }

        int startFrame = (int)this.networkManager.tickModule.syncedTick + this.sharedStartDelayFrames;
        this.serverSharedStartFrameSent = true;
        this.SendSharedRollbackStartFrameToClients(startFrame);
    }

    /// <summary>
    /// receives the shared rollback start frame
    /// </summary>
    /// <param name="startFrame">shared rollback start frame</param>
    [ObserversRpc]
    private void SendSharedRollbackStartFrameToClients(int startFrame)
    {
        // save the shared start frame that came from the server
        this.sharedRollbackStartFrame = startFrame;
        this.sharedRollbackStartFrameReceived = true;
        this.TryStartRollbackBridge(); // check if rollback can start now
    }
    #endregion
}