using System.Collections.Generic;
using static InputSerialization;
using UnityEngine;

/// <summary>
/// rollback simulation that stores inputs and old states
/// </summary>
public class GameSimulation
{
    #region variables
    private GameState currentState;
    private Dictionary<int, GameState> stateHistory;
    private Dictionary<int, FrameInput> localInputs;
    private Dictionary<int, FrameInput> remoteInputs;
    private Dictionary<int, RollbackFrameInfo> frameInfos;
    private HashSet<int> predictedRemoteFrames;
    private HashSet<int> rollbackFrames;
    private FrameInput lastRemoteInput;
    private bool hasLastRemoteInput;
    private int currentFrame;
    private int rollbackCount;
    private int maxSavedFrames;
    private bool playerOneLocal;
    #endregion

    #region constructor
    /// <summary>
    /// creates the rollback simulation
    /// </summary>
    /// <param name="isPlayerOneLocal">true if player one is local</param>
    /// <param name="startFrame">shared start frame</param>
    public GameSimulation(bool isPlayerOneLocal, int startFrame)
    {
        playerOneLocal = isPlayerOneLocal;
        maxSavedFrames = 360;
        StartNewMatch(startFrame);
    }
    #endregion

    #region methods
    /// <summary>
    /// starts a new match
    /// </summary>
    /// <param name="startFrame">shared start frame</param>
    public void StartNewMatch(int startFrame)
    {
        currentState = new GameState(playerOneLocal);
        stateHistory = new Dictionary<int, GameState>();
        localInputs = new Dictionary<int, FrameInput>();
        remoteInputs = new Dictionary<int, FrameInput>();
        frameInfos = new Dictionary<int, RollbackFrameInfo>();
        predictedRemoteFrames = new HashSet<int>();
        rollbackFrames = new HashSet<int>();
        hasLastRemoteInput = false;
        currentFrame = startFrame;
        rollbackCount = 0;
    }

    /// <summary>
    /// saves local input for one frame
    /// </summary>
    /// <param name="input">frame input</param>
    public void SaveLocalInput(FrameInput input)
    {
        localInputs[input.frame] = input;
        RollbackFrameInfo frameInfo = GetFrameInfo(input.frame);

        if (playerOneLocal)
        {
            frameInfo.playerOneInput = input;
            frameInfo.hasPlayerOneInput = true;
        }
        else
        {
            frameInfo.playerTwoInput = input;
            frameInfo.hasPlayerTwoInput = true;
        }
    }

    /// <summary>
    /// saves remote input for one frame
    /// </summary>
    /// <param name="input">frame input</param>
    /// <returns>true if this frame was predicted before the real input arrived</returns>
    public bool SaveRemoteInput(FrameInput input)
    {
        FrameInput savedInput;
        bool wasPredicted = predictedRemoteFrames.Contains(input.frame);
        bool inputChanged = wasPredicted && remoteInputs.TryGetValue(input.frame, out savedInput) && !savedInput.IsSame(input); // real input was different from predicted input

        if (inputChanged)
        {
            rollbackFrames.Add(input.frame);
        }

        predictedRemoteFrames.Remove(input.frame);
        remoteInputs[input.frame] = input;
        lastRemoteInput = input;
        hasLastRemoteInput = true;
        RollbackFrameInfo frameInfo = GetFrameInfo(input.frame);

        if (playerOneLocal)
        {
            frameInfo.playerTwoInput = input;
            frameInfo.hasPlayerTwoInput = true;

            if (wasPredicted && !inputChanged)
            {
                frameInfo.playerTwoPredicted = true; // prediction was correct
            }

            if (inputChanged)
            {
                frameInfo.playerTwoPredicted = false;
                frameInfo.playerTwoChanged = true; // prediction was wrong
            }
        }
        else
        {
            frameInfo.playerOneInput = input;
            frameInfo.hasPlayerOneInput = true;

            if (wasPredicted && !inputChanged)
            {
                frameInfo.playerOnePredicted = true; // prediction was correct
            }

            if (inputChanged)
            {
                frameInfo.playerOnePredicted = false;
                frameInfo.playerOneChanged = true; // prediction was wrong
            }
        }

        return wasPredicted;
    }

    /// <summary>
    /// runs one simulation frame
    /// </summary>
    public void RunFrame()
    {
        if (rollbackFrames.Count > 0)
        {
            RunRollback(); // fix old wrong predicted frames before running the new one
        }

        stateHistory[currentFrame] = new GameState(currentState); // save state before this frame changes it
        currentState = currentState.Tick(GetLocalInput(currentFrame), GetRemoteInput(currentFrame)); // apply inputs and move the game forward by one frame
        currentFrame++;
        CleanOldData();
    }

    /// <summary>
    /// returns rollback viewer frames
    /// </summary>
    /// <returns>frame list</returns>
    public List<RollbackFrameInfo> GetDebugFrames()
    {
        List<int> frameNumbers = new List<int>(frameInfos.Keys);
        List<RollbackFrameInfo> result = new List<RollbackFrameInfo>();
        frameNumbers.Sort(); // put frame numbers in order

        foreach (int frame in frameNumbers)
        {
            result.Add(new RollbackFrameInfo(frameInfos[frame]));
        }

        return result;
    }

    /// <summary>
    /// returns the current game state
    /// </summary>
    /// <returns>current game state</returns>
    public GameState GetCurrentState()
    {
        return currentState;
    }

    /// <summary>
    /// returns saved state for one rollback frame
    /// </summary>
    /// <param name="frame">rollback frame</param>
    /// <returns>saved game state or null</returns>
    public GameState GetStateForRollbackFrame(int frame)
    {
        GameState savedState;

        // uses the rollback frame as the dictionary key
        if (stateHistory.TryGetValue(frame, out savedState))
        {
            return new GameState(savedState);
        }

        return null;
    }

    /// <summary>
    /// returns the current frame
    /// </summary>
    /// <returns>current frame</returns>
    public int GetCurrentFrame()
    {
        return currentFrame;
    }

    /// <summary>
    /// returns rollback count
    /// </summary>
    /// <returns>rollback count</returns>
    public int GetRollbackCount()
    {
        return rollbackCount;
    }

    /// <summary>
    /// returns local input for one frame
    /// </summary>
    /// <param name="frame">frame number</param>
    /// <returns>frame input</returns>
    private FrameInput GetLocalInput(int frame)
    {
        FrameInput input;

        if (localInputs.TryGetValue(frame, out input))
        {
            return input;
        }

        return CreateNeutralInput(frame); // if no input was saved use neutral input for this frame
    }

    /// <summary>
    /// returns remote input for one frame
    /// </summary>
    /// <param name="frame">frame number</param>
    /// <returns>frame input</returns>
    private FrameInput GetRemoteInput(int frame)
    {
        FrameInput savedInput;

        if (remoteInputs.TryGetValue(frame, out savedInput))
        {
            return savedInput;
        }

        FrameInput predictedInput = CreateNeutralInput(frame);

        if (hasLastRemoteInput)
        {
            predictedInput.direction = lastRemoteInput.direction;
            predictedInput.punch = ButtonState.None;
            predictedInput.kick = ButtonState.None;
            predictedInput.heavyAttack = ButtonState.None;
        }

        predictedInput.frame = frame;
        remoteInputs[frame] = predictedInput; // save the predicted remote input
        predictedRemoteFrames.Add(frame); // mark it so a later real packet can trigger rollback
        RollbackFrameInfo frameInfo = GetFrameInfo(frame);

        if (playerOneLocal)
        {
            frameInfo.playerTwoInput = predictedInput;
            frameInfo.hasPlayerTwoInput = true;
        }
        else
        {
            frameInfo.playerOneInput = predictedInput;
            frameInfo.hasPlayerOneInput = true;
        }

        return predictedInput;
    }

    /// <summary>
    /// runs rollback from the earliest wrong frame
    /// </summary>
    private void RunRollback()
    {
        int rollbackFrame = GetEarliestRollbackFrame();

        if (!stateHistory.ContainsKey(rollbackFrame))
        {
            rollbackFrames.Clear(); // if we do not have the old saved state rollback cannot happen
            return;
        }

        int targetFrame = currentFrame; // remember where the simulation had already reached
        currentState = new GameState(stateHistory[rollbackFrame]); // load the saved state from the first wrong frame
        currentFrame = rollbackFrame; // go back to the wrong frame
        rollbackFrames.Clear(); // clear current rollback requests because we are handling them now
        rollbackCount++;

        while (currentFrame < targetFrame)
        {
            stateHistory[currentFrame] = new GameState(currentState); // save state again while replaying frames
            currentState = currentState.Tick(GetLocalInput(currentFrame), GetRemoteInput(currentFrame)); // run the frame again with the corrected input data
            currentFrame++;
        }

        CleanOldData();
    }

    /// <summary>
    /// returns the earliest rollback frame
    /// </summary>
    /// <returns>earliest rollback frame</returns>
    private int GetEarliestRollbackFrame()
    {
        int earliestFrame = -1;

        foreach (int frame in rollbackFrames)
        {
            if (earliestFrame < 0 || frame < earliestFrame)
            {
                earliestFrame = frame; // keep the smallest frame because rollback must start from the first wrong frame
            }
        }

        return earliestFrame;
    }

    /// <summary>
    /// removes old saved data
    /// </summary>
    private void CleanOldData()
    {
        int oldFrame = currentFrame - maxSavedFrames;

        if (oldFrame < 0)
        {
            return;
        }

        // remove data that is outside the rollback memory window
        stateHistory.Remove(oldFrame);
        localInputs.Remove(oldFrame);
        remoteInputs.Remove(oldFrame);
        frameInfos.Remove(oldFrame);
        predictedRemoteFrames.Remove(oldFrame);
        rollbackFrames.Remove(oldFrame);
    }

    /// <summary>
    /// gets one frame info or makes it
    /// </summary>
    /// <param name="frame">frame number</param>
    /// <returns>frame info</returns>
    private RollbackFrameInfo GetFrameInfo(int frame)
    {
        RollbackFrameInfo frameInfo;

        if (!frameInfos.TryGetValue(frame, out frameInfo))
        {
            frameInfo = new RollbackFrameInfo();
            frameInfo.frame = frame;
            frameInfos[frame] = frameInfo;
        }

        return frameInfo;
    }
    #endregion
}