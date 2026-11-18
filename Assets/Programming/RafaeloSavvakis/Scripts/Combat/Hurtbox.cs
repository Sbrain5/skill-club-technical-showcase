using UnityEngine;

/// <summary>
/// defines the area where a player can receive hits
/// </summary>
public sealed class Hurtbox : MonoBehaviour
{
    #region variables
    [Header("Identity")]
    [SerializeField] private PlayerController.PlayerSlot ownerSlot = PlayerController.PlayerSlot.Player01;

    [Header("Box Settings")]
    [SerializeField] private Vector2 localCenter = new Vector2(0.0f, 1.0f);
    [SerializeField] private Vector2 boxSize = new Vector2(0.9f, 2.0f);
    [SerializeField] private bool receivesHits = true;

    [Header("Debug View")]
    [SerializeField] private float debugLineWidth = 0.025f;
    [SerializeField] private float debugZOffset = -0.15f;

    private static bool debugVisualsEnabled;
    private static Material debugLineMaterial;
    private LineRenderer debugLineRenderer;
    #endregion

    #region properties
    /// <summary>
    /// gets the owner slot for this hurtbox
    /// </summary>
    public PlayerController.PlayerSlot OwnerSlot
    {
        get
        {
            return this.ownerSlot;
        }
    }

    /// <summary>
    /// gets whether this hurtbox currently accepts hits
    /// </summary>
    public bool ReceivesHits
    {
        get
        {
            return this.receivesHits;
        }
    }
    #endregion

    #region unity methods
    /// <summary>
    /// keeps inspector values safe
    /// </summary>
    private void OnValidate()
    {
        if (this.boxSize.x < 0.05f)
        {
            this.boxSize.x = 0.05f;
        }

        if (this.boxSize.y < 0.05f)
        {
            this.boxSize.y = 0.05f;
        }
    }

    /// <summary>
    /// draws the hurtbox in the scene view while the object is selected
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Rect worldRect = this.GetWorldRect();

        Vector3 drawCenter = new Vector3(worldRect.center.x, worldRect.center.y, this.transform.position.z);
        Vector3 drawSize = new Vector3(worldRect.size.x, worldRect.size.y, 0.05f);

        Gizmos.color = this.receivesHits ? Color.green : Color.gray;
        Gizmos.DrawWireCube(drawCenter, drawSize);
    }

    /// <summary>
    /// updates the runtime debug visual after normal gameplay updates
    /// </summary>
    private void LateUpdate()
    {
        this.UpdateDebugVisual();
    }
    #endregion

    #region methods
    /// <summary>
    /// sets the owner slot for this hurtbox
    /// </summary>
    /// <param name="slot">player slot</param>
    public void SetOwnerSlot(PlayerController.PlayerSlot slot)
    {
        this.ownerSlot = slot;
    }

    /// <summary>
    /// gets the world rectangle used for hit checks
    /// </summary>
    /// <returns>world rectangle</returns>
    public Rect GetWorldRect()
    {
        Vector3 worldCenter = this.transform.TransformPoint(new Vector3(this.localCenter.x, this.localCenter.y, 0.0f));
        Vector3 worldScale = this.transform.lossyScale;

        float worldWidth = Mathf.Abs(this.boxSize.x * worldScale.x);
        float worldHeight = Mathf.Abs(this.boxSize.y * worldScale.y);

        return new Rect(
            worldCenter.x - (worldWidth * 0.5f),
            worldCenter.y - (worldHeight * 0.5f),
            worldWidth,
            worldHeight);
    }

    /// <summary>
    /// enables or disables all runtime hurtbox debug visuals
    /// </summary>
    /// <param name="isEnabled">true when hurtbox debug visuals should be shown</param>
    public static void SetDebugVisualsEnabled(bool isEnabled)
    {
        Hurtbox[] hurtboxes;

        debugVisualsEnabled = isEnabled;
        hurtboxes = Object.FindObjectsByType<Hurtbox>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Hurtbox hurtbox in hurtboxes)
        {
            hurtbox.UpdateDebugVisual();
        }
    }

    /// <summary>
    /// creates the runtime debug line renderer if it does not exist yet
    /// </summary>
    private void EnsureDebugLineRenderer()
    {
        GameObject debugObject;

        if (this.debugLineRenderer != null)
        {
            return;
        }

        debugObject = new GameObject("Hurtbox_DebugVisual");
        debugObject.transform.SetParent(this.transform);
        debugObject.transform.localPosition = Vector3.zero;
        debugObject.transform.localRotation = Quaternion.identity;
        debugObject.transform.localScale = Vector3.one;

        this.debugLineRenderer = debugObject.AddComponent<LineRenderer>();
        this.debugLineRenderer.useWorldSpace = true;
        this.debugLineRenderer.loop = true;
        this.debugLineRenderer.positionCount = 4;
        this.debugLineRenderer.widthMultiplier = this.debugLineWidth;
        this.debugLineRenderer.material = GetDebugLineMaterial();
        this.debugLineRenderer.enabled = false;
    }

    /// <summary>
    /// updates the runtime hurtbox debug rectangle
    /// </summary>
    private void UpdateDebugVisual()
    {
        Rect worldRect;
        float zPosition;
        Color debugColor;

        this.EnsureDebugLineRenderer();

        if (!debugVisualsEnabled)
        {
            this.debugLineRenderer.enabled = false;
            return;
        }

        worldRect = this.GetWorldRect();
        zPosition = this.transform.position.z + this.debugZOffset;
        debugColor = this.receivesHits ? Color.green : Color.gray;

        this.debugLineRenderer.enabled = true;
        this.debugLineRenderer.startColor = debugColor;
        this.debugLineRenderer.endColor = debugColor;
        this.debugLineRenderer.widthMultiplier = this.debugLineWidth;

        this.debugLineRenderer.SetPosition(0, new Vector3(worldRect.xMin, worldRect.yMin, zPosition));
        this.debugLineRenderer.SetPosition(1, new Vector3(worldRect.xMin, worldRect.yMax, zPosition));
        this.debugLineRenderer.SetPosition(2, new Vector3(worldRect.xMax, worldRect.yMax, zPosition));
        this.debugLineRenderer.SetPosition(3, new Vector3(worldRect.xMax, worldRect.yMin, zPosition));
    }

    /// <summary>
    /// gets the shared material used by runtime debug line renderers
    /// </summary>
    /// <returns>debug line material</returns>
    private static Material GetDebugLineMaterial()
    {
        Shader shader;

        if (debugLineMaterial != null)
        {
            return debugLineMaterial;
        }

        shader = Shader.Find("Sprites/Default");
        debugLineMaterial = new Material(shader);

        return debugLineMaterial;
    }
    #endregion
}