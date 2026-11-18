using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Creates visual-only combat debug volumes for animated fighter bodies and authoritative hurtboxes.
/// </summary>
public sealed class CombatDebugVisualizer : MonoBehaviour
{
    public enum FighterProfile
    {
        Fighter02 = 0,
        Fighter05 = 1
    }

    [Header("References")]
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameManager gameManager;

    [Header("Fighter Profile")]
    [SerializeField] private FighterProfile fighterProfile = FighterProfile.Fighter02;

    [Header("Colours")]
    [SerializeField] private Color bodyColor = new Color(0.0f, 0.85f, 1.0f, 0.25f);
    [SerializeField] private Color inactiveAttackColor = new Color(0.65f, 0.65f, 0.65f, 0.45f);
    [SerializeField] private Color activeAttackColor = new Color(1.0f, 0.05f, 0.05f, 0.65f);
    [SerializeField] private Color hurtboxColor = new Color(0.0f, 0.45f, 1.0f, 0.10f);

    [Header("Body Sizes")]
    [SerializeField] private float headRadius = 0.16f;
    [SerializeField] private float pelvisRadius = 0.22f;
    [SerializeField] private float lowerTorsoRadius = 0.27f;
    [SerializeField] private float upperTorsoRadius = 0.24f;
    [SerializeField] private float upperArmRadius = 0.09f;
    [SerializeField] private float forearmRadius = 0.075f;
    [SerializeField] private float handRadius = 0.10f;
    [SerializeField] private float thighRadius = 0.13f;
    [SerializeField] private float calfRadius = 0.105f;
    [SerializeField] private float footRadius = 0.12f;

    [Header("Hurtboxes")]
    [SerializeField] private bool showHurtboxes = true;
    [SerializeField] private float hurtboxDepth = 0.20f;

    private static bool debugEnabled;

    private readonly List<BodyVolume> bodyVolumes = new List<BodyVolume>();
    private MaterialPropertyBlock propertyBlock;
    private Material overlayMaterial;
    private Transform debugRoot;
    private Transform upperHurtbox;
    private Transform lowerHurtbox;
    private Renderer upperHurtboxRenderer;
    private Renderer lowerHurtboxRenderer;
    private bool isBuilt;

    private void Awake()
    {
        this.propertyBlock = new MaterialPropertyBlock();
        this.RefreshReferences();
    }

    private void OnEnable()
    {
        this.TryBuild();
        this.RefreshVisibility();
    }

    private void OnDisable()
    {
        this.HideObject(this.debugRoot);
    }

    private void LateUpdate()
    {
        if (!debugEnabled)
        {
            return;
        }

        if (!this.isBuilt && !this.TryBuild())
        {
            return;
        }

        this.UpdateBodyVolumes();
        this.UpdateHurtboxes();
    }

    private void OnDestroy()
    {
        if (this.debugRoot != null)
        {
            Destroy(this.debugRoot.gameObject);
        }

        if (this.overlayMaterial != null)
        {
            Destroy(this.overlayMaterial);
        }
    }

    private void OnValidate()
    {
        this.headRadius = this.ClampSize(this.headRadius);
        this.pelvisRadius = this.ClampSize(this.pelvisRadius);
        this.lowerTorsoRadius = this.ClampSize(this.lowerTorsoRadius);
        this.upperTorsoRadius = this.ClampSize(this.upperTorsoRadius);
        this.upperArmRadius = this.ClampSize(this.upperArmRadius);
        this.forearmRadius = this.ClampSize(this.forearmRadius);
        this.handRadius = this.ClampSize(this.handRadius);
        this.thighRadius = this.ClampSize(this.thighRadius);
        this.calfRadius = this.ClampSize(this.calfRadius);
        this.footRadius = this.ClampSize(this.footRadius);
        this.hurtboxDepth = this.ClampSize(this.hurtboxDepth);
    }

    public static void SetDebugVisualsEnabled(bool isEnabled)
    {
        CombatDebugVisualizer[] visualizers;

        debugEnabled = isEnabled;
        visualizers = Object.FindObjectsByType<CombatDebugVisualizer>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (CombatDebugVisualizer visualizer in visualizers)
        {
            visualizer.RefreshVisibility();
        }
    }

    private void RefreshReferences()
    {
        if (this.characterAnimator == null)
        {
            this.characterAnimator = this.GetComponent<Animator>();
        }

        if (this.playerController == null)
        {
            this.playerController = this.GetComponentInParent<PlayerController>(true);
        }

        if (this.playerCombat == null && this.playerController != null)
        {
            this.playerCombat = this.playerController.GetComponentInChildren<PlayerCombat>(true);
        }

        if (this.gameManager == null)
        {
            this.gameManager = Object.FindFirstObjectByType<GameManager>();
        }
    }

    private bool TryBuild()
    {
        if (this.isBuilt)
        {
            return true;
        }

        this.RefreshReferences();

        if (this.characterAnimator == null ||
            !this.characterAnimator.isHuman ||
            !this.CreateRootAndMaterial())
        {
            return false;
        }

        this.AddBodyVolumes();
        this.CreateHurtboxObjects();
        this.isBuilt = this.bodyVolumes.Count > 0;
        this.RefreshVisibility();

        return this.isBuilt;
    }

    private bool CreateRootAndMaterial()
    {
        GameObject rootObject;
        Shader shader;

        if (this.debugRoot == null)
        {
            rootObject = new GameObject("CombatDebug_Volumes");
            rootObject.hideFlags = HideFlags.DontSave;
            this.debugRoot = rootObject.transform;
            this.debugRoot.SetParent(
                this.playerController != null ? this.playerController.transform : this.transform,
                true);
        }

        if (this.overlayMaterial != null)
        {
            return true;
        }

        shader = Shader.Find("SkillClub/CombatDebugOverlay");

        if (shader == null)
        {
            return false;
        }

        this.overlayMaterial = new Material(shader);
        this.overlayMaterial.name = "CombatDebug_OverlayMaterial_Runtime";
        this.overlayMaterial.hideFlags = HideFlags.DontSave;
        this.overlayMaterial.SetColor("_BaseColor", this.bodyColor);

        return true;
    }

    private void AddBodyVolumes()
    {
        Transform hips = this.GetBone(HumanBodyBones.Hips);
        Transform chest = this.GetBone(HumanBodyBones.Chest);
        Transform neck = this.GetBone(HumanBodyBones.Neck);
        Transform head = this.GetBone(HumanBodyBones.Head);

        if (chest == null)
        {
            chest = this.GetBone(HumanBodyBones.UpperChest);
        }

        this.AddVolume("Head", PrimitiveType.Sphere, head, null, this.headRadius);
        this.AddVolume("Pelvis", PrimitiveType.Sphere, hips, null, this.pelvisRadius);
        this.AddVolume("LowerTorso", PrimitiveType.Cylinder, hips, chest, this.lowerTorsoRadius);
        this.AddVolume("UpperTorso", PrimitiveType.Cylinder, chest, neck, this.upperTorsoRadius);

        this.AddLimb("Left", HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm,
            HumanBodyBones.LeftHand, "UpperArm", "Forearm", "Hand",
            this.upperArmRadius, this.forearmRadius, this.handRadius);

        this.AddLimb("Right", HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm,
            HumanBodyBones.RightHand, "UpperArm", "Forearm", "Hand",
            this.upperArmRadius, this.forearmRadius, this.handRadius);

        this.AddLimb("Left", HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg,
            HumanBodyBones.LeftFoot, "Thigh", "Calf", "Foot",
            this.thighRadius, this.calfRadius, this.footRadius);

        this.AddLimb("Right", HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg,
            HumanBodyBones.RightFoot, "Thigh", "Calf", "Foot",
            this.thighRadius, this.calfRadius, this.footRadius);
    }

    private void AddLimb(
        string side,
        HumanBodyBones upperBone,
        HumanBodyBones lowerBone,
        HumanBodyBones endBone,
        string upperName,
        string lowerName,
        string endName,
        float upperRadius,
        float lowerRadius,
        float endRadius)
    {
        Transform upper = this.GetBone(upperBone);
        Transform lower = this.GetBone(lowerBone);
        Transform end = this.GetBone(endBone);

        this.AddVolume(side + upperName, PrimitiveType.Cylinder, upper, lower, upperRadius);
        this.AddVolume(side + lowerName, PrimitiveType.Cylinder, lower, end, lowerRadius);
        this.AddVolume(side + endName, PrimitiveType.Sphere, end, null, endRadius);
    }

    private void AddVolume(
        string volumeName,
        PrimitiveType primitiveType,
        Transform startBone,
        Transform endBone,
        float radius)
    {
        GameObject volumeObject;

        if (startBone == null ||
            primitiveType == PrimitiveType.Cylinder && endBone == null)
        {
            return;
        }

        volumeObject = this.CreatePrimitive(volumeName, primitiveType);

        this.bodyVolumes.Add(new BodyVolume
        {
            Name = volumeName,
            VisualTransform = volumeObject.transform,
            VisualRenderer = volumeObject.GetComponent<Renderer>(),
            StartBone = startBone,
            EndBone = endBone,
            Radius = radius,
            IsSphere = primitiveType == PrimitiveType.Sphere
        });
    }

    private GameObject CreatePrimitive(string objectName, PrimitiveType primitiveType)
    {
        GameObject createdObject = GameObject.CreatePrimitive(primitiveType);
        Collider objectCollider = createdObject.GetComponent<Collider>();
        Renderer objectRenderer = createdObject.GetComponent<Renderer>();

        createdObject.name = objectName;
        createdObject.hideFlags = HideFlags.DontSave;
        createdObject.transform.SetParent(this.debugRoot, true);
        Destroy(objectCollider);
        objectRenderer.sharedMaterial = this.overlayMaterial;
        objectRenderer.shadowCastingMode = ShadowCastingMode.Off;
        objectRenderer.receiveShadows = false;

        return createdObject;
    }

    private Transform GetBone(HumanBodyBones bone)
    {
        return this.characterAnimator == null
            ? null
            : this.characterAnimator.GetBoneTransform(bone);
    }

    private void UpdateBodyVolumes()
    {
        foreach (BodyVolume volume in this.bodyVolumes)
        {
            this.UpdateBodyVolume(volume);
        }
    }

    private void UpdateBodyVolume(BodyVolume volume)
    {
        Vector3 direction;
        float diameter;
        float length;
        diameter = volume.Radius * 2.0f;

        if (volume.IsSphere)
        {
            volume.VisualTransform.position = volume.StartBone.position;
            volume.VisualTransform.rotation = volume.StartBone.rotation;
            this.SetWorldScale(volume.VisualTransform, new Vector3(diameter, diameter, diameter));
        }
        else
        {
            direction = volume.EndBone.position - volume.StartBone.position;
            length = direction.magnitude;

            if (length <= 0.0001f)
            {
                return;
            }

            volume.VisualTransform.position = volume.StartBone.position + direction * 0.5f;
            volume.VisualTransform.rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            this.SetWorldScale(volume.VisualTransform, new Vector3(diameter, length * 0.5f, diameter));
        }

        this.SetRendererColour(volume.VisualRenderer, this.GetBodyVolumeColor(volume.Name));
    }

    private Color GetBodyVolumeColor(string volumeName)
    {
        if (!this.IsCurrentAttackVolume(volumeName))
        {
            return this.bodyColor;
        }

        return this.playerCombat != null && this.playerCombat.IsAttackHitboxActive
            ? this.activeAttackColor
            : this.inactiveAttackColor;
    }

    private bool IsCurrentAttackVolume(string volumeName)
    {
        if (this.playerCombat == null || !this.playerCombat.IsAttacking)
        {
            return false;
        }

        switch (this.playerCombat.CurrentAttackState)
        {
            case SimAttackState.LightAttack:
                return volumeName == "LeftForearm" || volumeName == "LeftHand";

            case SimAttackState.MediumAttack:
                return volumeName == "LeftCalf" || volumeName == "LeftFoot";

            case SimAttackState.HeavyAttack:
                return this.IsHeavyAttackVolume(volumeName);

            default:
                return false;
        }
    }

    private bool IsHeavyAttackVolume(string volumeName)
    {
        if (this.fighterProfile == FighterProfile.Fighter02)
        {
            return volumeName == "RightForearm" || volumeName == "RightHand";
        }

        return volumeName == "LeftCalf" ||
               volumeName == "LeftFoot" ||
               volumeName == "RightCalf" ||
               volumeName == "RightFoot";
    }

    private void CreateHurtboxObjects()
    {
        if (this.upperHurtbox != null)
        {
            return;
        }

        this.CreateHurtboxObject("AuthoritativeHurtbox_Upper", out this.upperHurtbox, out this.upperHurtboxRenderer);
        this.CreateHurtboxObject("AuthoritativeHurtbox_Lower", out this.lowerHurtbox, out this.lowerHurtboxRenderer);
    }

    private void CreateHurtboxObject(string objectName, out Transform targetTransform, out Renderer targetRenderer)
    {
        GameObject createdObject = this.CreatePrimitive(objectName, PrimitiveType.Cube);

        targetTransform = createdObject.transform;
        targetRenderer = createdObject.GetComponent<Renderer>();
        createdObject.SetActive(false);
    }

    private void UpdateHurtboxes()
    {
        GameState state;
        SimCombatBoxSet hurtboxes;
        int slot;

        this.RefreshReferences();

        if (!this.showHurtboxes || this.gameManager == null || this.playerController == null)
        {
            this.HideHurtboxes();
            return;
        }

        state = this.gameManager.CurrentAuthoritativeState;

        if (state == null)
        {
            this.HideHurtboxes();
            return;
        }

        slot = this.playerController.AssignedPlayerSlot == PlayerController.PlayerSlot.Player01 ? 1 : 2;
        hurtboxes = state.GetPlayerHurtboxes(slot);

        this.UpdateHurtbox(this.upperHurtbox, this.upperHurtboxRenderer, hurtboxes.HasPrimary, hurtboxes.Primary);
        this.UpdateHurtbox(this.lowerHurtbox, this.lowerHurtboxRenderer, hurtboxes.HasSecondary, hurtboxes.Secondary);
    }

    private void UpdateHurtbox(
        Transform boxTransform,
        Renderer boxRenderer,
        bool isVisible,
        SimCombatBox box)
    {
        if (!isVisible || boxTransform == null || boxRenderer == null || this.playerController == null)
        {
            this.HideObject(boxTransform);
            return;
        }

        boxTransform.gameObject.SetActive(true);
        boxTransform.position = new Vector3(
            box.Center.x.ToFloat,
            box.Center.y.ToFloat,
            this.playerController.transform.position.z);
        boxTransform.rotation = Quaternion.identity;

        this.SetWorldScale(boxTransform, new Vector3(
            box.HalfSize.x.ToFloat * 2.0f,
            box.HalfSize.y.ToFloat * 2.0f,
            this.hurtboxDepth));
        this.SetRendererColour(boxRenderer, this.hurtboxColor);
    }

    private void HideHurtboxes()
    {
        this.HideObject(this.upperHurtbox);
        this.HideObject(this.lowerHurtbox);
    }

    private void HideObject(Transform targetTransform)
    {
        if (targetTransform != null)
        {
            targetTransform.gameObject.SetActive(false);
        }
    }

    private void SetRendererColour(Renderer targetRenderer, Color targetColor)
    {
        targetRenderer.GetPropertyBlock(this.propertyBlock);
        this.propertyBlock.SetColor("_BaseColor", targetColor);
        targetRenderer.SetPropertyBlock(this.propertyBlock);
    }

    private void SetWorldScale(Transform targetTransform, Vector3 desiredWorldScale)
    {
        Vector3 parentScale;
        parentScale = targetTransform.parent.lossyScale;
        targetTransform.localScale = new Vector3(
            this.SafeDivide(desiredWorldScale.x, parentScale.x),
            this.SafeDivide(desiredWorldScale.y, parentScale.y),
            this.SafeDivide(desiredWorldScale.z, parentScale.z));
    }

    private float SafeDivide(float value, float divisor)
    {
        return Mathf.Abs(divisor) <= 0.0001f ? value : value / divisor;
    }

    private float ClampSize(float value)
    {
        return Mathf.Max(0.02f, value);
    }

    private void RefreshVisibility()
    {
        if (this.debugRoot != null)
        {
            this.debugRoot.gameObject.SetActive(debugEnabled && this.isActiveAndEnabled);
        }
    }

    private sealed class BodyVolume
    {
        public string Name;
        public Transform VisualTransform;
        public Renderer VisualRenderer;
        public Transform StartBone;
        public Transform EndBone;
        public float Radius;
        public bool IsSphere;
    }
}