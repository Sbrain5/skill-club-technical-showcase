using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// defines an attack area that can register one valid opponent hurtbox hit per hit window
/// </summary>
public sealed class Hitbox : MonoBehaviour
{
    #region variables
    [Header("Identity")]
    [SerializeField] private PlayerController.PlayerSlot ownerSlot = PlayerController.PlayerSlot.Player01;

    [Header("Box Settings")]
    [SerializeField] private Vector2 localCenter = new Vector2(0.0f, 1.0f);
    [SerializeField] private Vector2 boxSize = new Vector2(0.8f, 0.8f);

    [Header("Hit Window")]
    [SerializeField] private bool isActive;
    [SerializeField] private int currentHitWindowId = -1;

    [Header("Debug View")]
    [SerializeField] private float debugLineWidth = 0.025f;
    [SerializeField] private float debugZOffset = -0.15f;

    private static bool debugVisualsEnabled;
    private static Material debugLineMaterial;
    private LineRenderer debugLineRenderer;

    private readonly List<Hurtbox> hurtboxesHitThisWindow = new List<Hurtbox>();
    #endregion

    #region properties
    /// <summary>
    /// gets the owner slot for this hitbox
    /// </summary>
    public PlayerController.PlayerSlot OwnerSlot
    {
        get
        {
            return this.ownerSlot;
        }
    }

    /// <summary>
    /// gets whether this hitbox is currently active through a hit window
    /// </summary>
    public bool IsHitWindowOpen
    {
        get
        {
            return this.isActive;
        }
    }

    /// <summary>
    /// gets the currently active hit window id
    /// </summary>
    public int CurrentHitWindowId
    {
        get
        {
            return this.currentHitWindowId;
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
    /// draws the hitbox in the scene view while the object is selected
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Rect worldRect = this.GetWorldRect();

        Vector3 drawCenter = new Vector3(worldRect.center.x, worldRect.center.y, this.transform.position.z);
        Vector3 drawSize = new Vector3(worldRect.size.x, worldRect.size.y, 0.05f);

        Gizmos.color = this.isActive ? Color.red : Color.gray;
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
    /// sets the owner slot for this hitbox
    /// </summary>
    /// <param name="slot">player slot</param>
    public void SetOwnerSlot(PlayerController.PlayerSlot slot)
    {
        this.ownerSlot = slot;
    }

    /// <summary>
    /// opens a gameplay hit window
    /// </summary>
    /// <param name="hitWindowId">unique id for the current hit window</param>
    public void OpenHitWindow(int hitWindowId)
    {
        if (hitWindowId < 0)
        {
            return;
        }

        if (this.isActive && this.currentHitWindowId == hitWindowId)
        {
            return;
        }

        this.isActive = true;
        this.currentHitWindowId = hitWindowId;
        this.ClearHitWindow();
    }

    /// <summary>
    /// closes the current gameplay hit window
    /// </summary>
    public void CloseHitWindow()
    {
        this.isActive = false;
        this.currentHitWindowId = -1;
    }

    /// <summary>
    /// clears the one-hit-per-window memory
    /// </summary>
    public void ClearHitWindow()
    {
        this.hurtboxesHitThisWindow.Clear();
    }

    /// <summary>
    /// tries to register the first valid opponent hurtbox hit during the current hit window
    /// </summary>
    /// <returns>true if a new opponent hit was registered</returns>
    public bool TryRegisterFirstOpponentHit()
    {
        Hurtbox[] hurtboxes;

        hurtboxes = Object.FindObjectsByType<Hurtbox>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Hurtbox hurtbox in hurtboxes)
        {
            if (this.TryRegisterHit(hurtbox))
            {
                return true;
            }
        }

        return false;
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
    /// tries to register one valid hit against one hurtbox
    /// </summary>
    /// <param name="hurtbox">target hurtbox</param>
    /// <returns>true if a new hit was registered</returns>
    private bool TryRegisterHit(Hurtbox hurtbox)
    {
        if (!this.Overlaps(hurtbox))
        {
            return false;
        }

        if (this.HasAlreadyHit(hurtbox))
        {
            return false;
        }

        this.hurtboxesHitThisWindow.Add(hurtbox);
        return true;
    }

    /// <summary>
    /// checks if this hitbox already hit the target hurtbox during the current window
    /// </summary>
    /// <param name="hurtbox">target hurtbox</param>
    /// <returns>true if this hurtbox was already hit</returns>
    private bool HasAlreadyHit(Hurtbox hurtbox)
    {
        return this.hurtboxesHitThisWindow.Contains(hurtbox);
    }

    /// <summary>
    /// checks whether this hitbox overlaps a valid opponent hurtbox
    /// </summary>
    /// <param name="hurtbox">target hurtbox</param>
    /// <returns>true if overlap exists</returns>
    private bool Overlaps(Hurtbox hurtbox)
    {
        Rect hitboxRect;
        Rect hurtboxRect;

        if (!this.isActive)
        {
            return false;
        }

        if (!hurtbox.ReceivesHits)
        {
            return false;
        }

        if (hurtbox.OwnerSlot == this.ownerSlot)
        {
            return false;
        }

        hitboxRect = this.GetWorldRect();
        hurtboxRect = hurtbox.GetWorldRect();

        return hitboxRect.Overlaps(hurtboxRect);
    }

    /// <summary>
    /// enables or disables all runtime hitbox debug visuals
    /// </summary>
    /// <param name="isEnabled">true when hitbox debug visuals should be shown</param>
    public static void SetDebugVisualsEnabled(bool isEnabled)
    {
        Hitbox[] hitboxes;

        debugVisualsEnabled = isEnabled;
        hitboxes = Object.FindObjectsByType<Hitbox>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Hitbox hitbox in hitboxes)
        {
            hitbox.UpdateDebugVisual();
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

        debugObject = new GameObject("Hitbox_DebugVisual");
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
    /// updates the runtime hitbox debug rectangle
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
        debugColor = this.isActive ? Color.red : Color.gray;

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