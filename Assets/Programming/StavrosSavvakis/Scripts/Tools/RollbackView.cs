using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static InputSerialization;

/// <summary>
/// shows player one and player two input history on screen
/// </summary>
[System.Serializable]
public class RollbackFrameInfo
{
    #region variables
    public int frame;
    public FrameInput playerOneInput;
    public bool hasPlayerOneInput;
    public FrameInput playerTwoInput;
    public bool hasPlayerTwoInput;
    public bool playerOnePredicted;
    public bool playerOneChanged;
    public bool playerTwoPredicted;
    public bool playerTwoChanged;
    #endregion

    #region constructors
    public RollbackFrameInfo()
    {
        // intentionally left empty
    }

    public RollbackFrameInfo(RollbackFrameInfo other)
    {
        frame = other.frame;
        playerOneInput = other.playerOneInput;
        hasPlayerOneInput = other.hasPlayerOneInput;
        playerTwoInput = other.playerTwoInput;
        hasPlayerTwoInput = other.hasPlayerTwoInput;
        playerOnePredicted = other.playerOnePredicted;
        playerOneChanged = other.playerOneChanged;
        playerTwoPredicted = other.playerTwoPredicted;
        playerTwoChanged = other.playerTwoChanged;
    }
    #endregion
}

/// <summary>
/// holds one input box
/// </summary>
[System.Serializable]
public class RollbackBox
{
    #region variables
    public GameObject boxObject;
    public TMP_Text frameText;
    public Image directionImage;
    public Image punchImage;
    public Image kickImage;
    public Image heavyImage;
    public GameObject predictedMark;
    public GameObject changedMark;
    #endregion

    #region constructor
    public RollbackBox(GameObject newBoxObject)
    {
        boxObject = newBoxObject;
        Transform frameTransform = newBoxObject.transform.Find("FrameText");
        Transform directionTransform = newBoxObject.transform.Find("DirectionImage");
        Transform punchTransform = newBoxObject.transform.Find("PunchImage");
        Transform kickTransform = newBoxObject.transform.Find("KickImage");
        Transform heavyTransform = newBoxObject.transform.Find("HeavyImage");
        Transform predictedTransform = newBoxObject.transform.Find("PredictedMark");
        Transform changedTransform = newBoxObject.transform.Find("ChangedMark");

        if (frameTransform != null)
        {
            frameText = frameTransform.GetComponent<TMP_Text>();
        }

        if (directionTransform != null)
        {
            directionImage = directionTransform.GetComponent<Image>();
        }

        if (punchTransform != null)
        {
            punchImage = punchTransform.GetComponent<Image>();
        }

        if (kickTransform != null)
        {
            kickImage = kickTransform.GetComponent<Image>();
        }

        if (heavyTransform != null)
        {
            heavyImage = heavyTransform.GetComponent<Image>();
        }

        if (predictedTransform != null)
        {
            predictedMark = predictedTransform.gameObject;
        }

        if (changedTransform != null)
        {
            changedMark = changedTransform.gameObject;
        }
    }
    #endregion

    #region  rollbackbox methods
    /// <summary>
    /// fills one box
    /// </summary>
    /// <param name="frame">frame number</param>
    /// <param name="showInput">true if input should be shown</param>
    /// <param name="input">frame input</param>
    /// <param name="directionSprite">direction icon</param>
    /// <param name="punchSprite">punch icon</param>
    /// <param name="kickSprite">kick icon</param>
    /// <param name="heavySprite">heavy icon</param>
    /// <param name="predicted">true if predicted</param>
    /// <param name="changed">true if changed</param>
    public void ShowBox(int frame, bool showInput, FrameInput input, Sprite directionSprite, Sprite punchSprite, Sprite kickSprite, Sprite heavySprite, bool predicted, bool changed)
    {
        boxObject.SetActive(true);

        if (frameText != null)
        {
            frameText.text = frame.ToString();
        }

        if (directionImage != null)
        {
            directionImage.enabled = showInput;
            directionImage.sprite = directionSprite;
        }

        if (punchImage != null)
        {
            punchImage.enabled = showInput && input.punch != ButtonState.None;
            punchImage.sprite = punchSprite;
        }

        if (kickImage != null)
        {
            kickImage.enabled = showInput && input.kick != ButtonState.None;
            kickImage.sprite = kickSprite;
        }

        if (heavyImage != null)
        {
            heavyImage.enabled = showInput && input.heavyAttack != ButtonState.None;
            heavyImage.sprite = heavySprite;
        }

        if (predictedMark != null)
        {
            predictedMark.SetActive(predicted);
        }

        if (changedMark != null)
        {
            changedMark.SetActive(changed);
        }
    }
    #endregion
}

/// <summary>
/// updates the rollback input viewer
/// </summary>
public class RollbackView : MonoBehaviour
{
    #region enums
    private enum RollbackViewMode
    {
        RealTime,
        TimedRefresh,
        InputReceived
    }
    #endregion

    #region variables
    [SerializeField] private RollbackBridge rollbackBridge;
    [SerializeField] private TMP_Text currentFrameText;
    [SerializeField] private GameObject boxPrefab;
    [SerializeField] private Transform playerOneParent;
    [SerializeField] private Transform playerTwoParent;
    [SerializeField] private Sprite unknownSprite;
    [SerializeField] private Sprite downBackSprite;
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite downForwardSprite;
    [SerializeField] private Sprite backSprite;
    [SerializeField] private Sprite neutralSprite;
    [SerializeField] private Sprite forwardSprite;
    [SerializeField] private Sprite upBackSprite;
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite upForwardSprite;
    [SerializeField] private Sprite punchSprite;
    [SerializeField] private Sprite kickSprite;
    [SerializeField] private Sprite heavyAttackSprite;
    private List<RollbackBox> playerOneBoxes;
    private List<RollbackBox> playerTwoBoxes;
    private RollbackViewMode viewMode;
    private int shownFrames = 20;
    private float refreshSeconds = 0.4f;
    private float refreshTimer;
    private int lastRemoteInputCount;
    #endregion

    #region unity methods
    private void Awake()
    {
        playerOneBoxes = new List<RollbackBox>();
        playerTwoBoxes = new List<RollbackBox>();
        viewMode = RollbackViewMode.RealTime;
        refreshTimer = 0f;
        lastRemoteInputCount = 0;
    }

    private void Update()
    {
        if (boxPrefab == null || playerOneParent == null || playerTwoParent == null)
        {
            return;
        }

        if (viewMode == RollbackViewMode.RealTime)
        {
            RefreshView();
        }
        else if (viewMode == RollbackViewMode.TimedRefresh)
        {
            refreshTimer += Time.unscaledDeltaTime; // waits before refreshing so the inputs are easier to read

            if (refreshTimer >= refreshSeconds)
            {
                refreshTimer = 0f;
                RefreshView();
            }
        }
        else if (viewMode == RollbackViewMode.InputReceived)
        {
            int newRemoteInputCount = rollbackBridge.GetRemoteInputCount();

            // only updates when a new useful remote input was received
            if (newRemoteInputCount != lastRemoteInputCount)
            {
                lastRemoteInputCount = newRemoteInputCount;
                RefreshInputFrame(rollbackBridge.GetLastRemoteInputFrame()); // shows the exact frame where the remote input arrived
            }
        }
    }
    #endregion

    #region rollbackview methods
    /// <summary>
    /// shows the rollback view every frame
    /// </summary>
    public void UseRealTimeMode()
    {
        viewMode = RollbackViewMode.RealTime;
        RefreshView();
    }

    /// <summary>
    /// shows the rollback view every few seconds
    /// </summary>
    public void UseTimedRefreshMode()
    {
        viewMode = RollbackViewMode.TimedRefresh;
        refreshTimer = 0f;
        RefreshView();
    }

    /// <summary>
    /// shows the rollback view when remote input arrives
    /// </summary>
    public void UseInputReceivedMode()
    {
        viewMode = RollbackViewMode.InputReceived;
        lastRemoteInputCount = rollbackBridge.GetRemoteInputCount(); // saves the current count so the tool waits for the next received input
        RefreshInputFrame(rollbackBridge.GetLastRemoteInputFrame());
    }

    /// <summary>
    /// refreshes the rollback frame boxes
    /// </summary>
    private void RefreshView()
    {
        currentFrameText.text = rollbackBridge.GetCurrentFrame().ToString();
        List<RollbackFrameInfo> frameList = rollbackBridge.GetDebugFrames();

        // keeps only the newest frames that fit in the tool
        if (frameList.Count > shownFrames)
        {
            frameList = frameList.GetRange(frameList.Count - shownFrames, shownFrames);
        }

        // creates the boxes once and then reuses them
        while (playerOneBoxes.Count < shownFrames)
        {
            playerOneBoxes.Add(new RollbackBox(Instantiate(boxPrefab, playerOneParent)));
            playerTwoBoxes.Add(new RollbackBox(Instantiate(boxPrefab, playerTwoParent)));
        }

        int i = 0;

        while (i < shownFrames)
        {
            // hides boxes that do not have frame data yet
            if (i >= frameList.Count)
            {
                playerOneBoxes[i].boxObject.SetActive(false);
                playerTwoBoxes[i].boxObject.SetActive(false);
                i++;
                continue;
            }

            RollbackFrameInfo frameInfo = frameList[i];
            playerOneBoxes[i].ShowBox(frameInfo.frame, frameInfo.hasPlayerOneInput, frameInfo.playerOneInput, GetDirectionSprite(frameInfo.playerOneInput.direction), punchSprite,
                                      kickSprite, heavyAttackSprite, frameInfo.playerOnePredicted, frameInfo.playerOneChanged);
            playerTwoBoxes[i].ShowBox(frameInfo.frame, frameInfo.hasPlayerTwoInput, frameInfo.playerTwoInput, GetDirectionSprite(frameInfo.playerTwoInput.direction), punchSprite,
                                      kickSprite, heavyAttackSprite, frameInfo.playerTwoPredicted, frameInfo.playerTwoChanged);
            i++;
        }
    }

    /// <summary>
    /// shows only the frame where remote input was received
    /// </summary>
    /// <param name="inputFrame">received input frame</param>
    private void RefreshInputFrame(int inputFrame)
    {
        currentFrameText.text = inputFrame.ToString();
        List<RollbackFrameInfo> frameList = rollbackBridge.GetDebugFrames();
        RollbackFrameInfo foundFrame = null;

        // finds the exact frame that received the remote input
        foreach (RollbackFrameInfo frameInfo in frameList)
        {
            if (frameInfo.frame == inputFrame)
            {
                foundFrame = frameInfo;
                break;
            }
        }

        // creates the boxes once and then reuses them
        while (playerOneBoxes.Count < shownFrames)
        {
            playerOneBoxes.Add(new RollbackBox(Instantiate(boxPrefab, playerOneParent)));
            playerTwoBoxes.Add(new RollbackBox(Instantiate(boxPrefab, playerTwoParent)));
        }

        int i = 0;

        // hides all boxes because this mode only shows one frame
        while (i < shownFrames)
        {
            playerOneBoxes[i].boxObject.SetActive(false);
            playerTwoBoxes[i].boxObject.SetActive(false);
            i++;
        }

        if (foundFrame == null)
        {
            return;
        }

        playerOneBoxes[0].ShowBox(foundFrame.frame, foundFrame.hasPlayerOneInput, foundFrame.playerOneInput, GetDirectionSprite(foundFrame.playerOneInput.direction), punchSprite,
                                  kickSprite, heavyAttackSprite, foundFrame.playerOnePredicted, foundFrame.playerOneChanged);

        playerTwoBoxes[0].ShowBox(foundFrame.frame, foundFrame.hasPlayerTwoInput, foundFrame.playerTwoInput, GetDirectionSprite(foundFrame.playerTwoInput.direction), punchSprite,
                                  kickSprite, heavyAttackSprite, foundFrame.playerTwoPredicted, foundFrame.playerTwoChanged);
    }

    /// <summary>
    /// returns one direction sprite
    /// </summary>
    /// <param name="direction">direction value</param>
    /// <returns>direction sprite</returns>
    private Sprite GetDirectionSprite(DirectionInput direction)
    {
        switch (direction)
        {
            case DirectionInput.DownBack:
                return downBackSprite;

            case DirectionInput.Down:
                return downSprite;

            case DirectionInput.DownForward:
                return downForwardSprite;

            case DirectionInput.Back:
                return backSprite;

            case DirectionInput.Neutral:
                return neutralSprite;

            case DirectionInput.Forward:
                return forwardSprite;

            case DirectionInput.UpBack:
                return upBackSprite;

            case DirectionInput.Up:
                return upSprite;

            case DirectionInput.UpForward:
                return upForwardSprite;

            default:
                return unknownSprite;
        }
    }
    #endregion
}