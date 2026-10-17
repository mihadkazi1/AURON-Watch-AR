using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using Vuforia;

public class AuronTryOnController : MonoBehaviour
{
    public enum ModelAxis
    {
        Auto, PositiveX, NegativeX, PositiveY, NegativeY, PositiveZ, NegativeZ
    }

    public enum FrameRotation
    {
        Auto, None, Clockwise90, CounterClockwise90, Rotate180
    }

    [Header("References")]
    [SerializeField] private AuronVuforiaHandLandmarker handLandmarker;
    [SerializeField] private AuronVuforiaCameraFrame vuforiaCameraFrame;
    [SerializeField] private Camera arCamera;

    [Header("Watch sizing")]
    [SerializeField, Min(0.025f)] private float watchWidthMeters = 0.042f;
    [SerializeField, Range(0.5f, 2.5f)] private float sizeMultiplier = 1.25f;
    [SerializeField] private string caseAnchorName = "Empty";
    [SerializeField, Range(-0.04f, 0.06f)] private float wristToWatchMeters = 0.016f;
    [SerializeField, Range(-0.04f, 0.04f)] private float palmSurfaceOffsetMeters = 0.002f;

    [Header("Strap Visibility & Optional Back-Clasp Mask")]
    [SerializeField] private bool hideBackClaspOnly = false;
    [SerializeField, Range(0.25f, 0.85f)] private float innerWristMaskWidth = 0.48f;

    [Header("Hand size / depth estimate")]
    [SerializeField, Min(0.04f)] private float assumedPalmWidthMeters = 0.080f;
    [SerializeField, Min(0.05f)] private float assumedWristToMiddleMeters = 0.095f;
    [SerializeField, Range(0.15f, 1.5f)] private float minimumHandDepthMeters = 0.24f;
    [SerializeField, Range(0.2f, 2.0f)] private float maximumHandDepthMeters = 0.70f;

    [Header("Landmark image mapping")]
    [SerializeField] private FrameRotation frameRotation = FrameRotation.Auto;
    [SerializeField] private bool mirrorLandmarksForDisplay = false;

    [Header("Watch orientation")]
    [SerializeField] private ModelAxis modelFaceAxis = ModelAxis.Auto;
    [SerializeField] private ModelAxis modelTwelveAxis = ModelAxis.Auto;
    [SerializeField] private bool invertFace = false;
    [SerializeField] private bool invertTwelve = false;
    [SerializeField] private bool alignStrapAroundWrist = true;
    [SerializeField] private bool twelveTowardThumb = false;
    [SerializeField] private Vector3 rotationOffset = new Vector3(12f, 0f, 0f);

    [SerializeField, Range(0f, 1f)]
    private float palmRotationInfluence = 0.45f;

    [SerializeField, Range(-0.04f, 0.06f)]
    private float wristPlacementOffsetMeters = 0f;

    [Header("Direct Wrist Following")]
    [SerializeField, Min(1f)] private float positionSmooth = 80f;
    [SerializeField, Min(1f)] private float rotationSmooth = 40f;
    [SerializeField, Min(1f)] private float scaleSmooth = 25f;

    [Header("Tracking")]
    [SerializeField, Min(0.05f)] private float targetScanInterval = 0.15f;
    [SerializeField, Min(0f)] private float handLostDelay = 0.35f;

    private ImageTargetBehaviour currentTarget;
    private Transform currentWatch;
    private ImageTargetBehaviour lastRememberedTarget;
    private Transform lastRememberedWatch;
    private float nextTargetScan;

    private Transform sourceWatchRoot;
    private Transform sourceParent;
    private Vector3 sourceLocalPosition;
    private Quaternion sourceLocalRotation;
    private Vector3 sourceLocalScale;
    private AuronWatchExperience sourceWatchExperience;
    private bool sourceWatchExperienceWasEnabled;
    private Renderer[] sourceRenderers;
    private bool[] sourceRendererStates;

    private GameObject tryOnWatch;
    private Transform tryOnWatchRoot;
    private Transform tryOnCaseAnchor;

    private GameObject wristOccluderObj;
    private Transform wristOccluderTransform;
    private Material occluderMaterial;

    private bool cloneInCameraSpace;
    private bool tryOnActive;
    private float lastValidHandTime = -999f;
    private bool hasSmoothedPose;

    private Vector3 caseCenterLocal;
    private Vector3 fullWatchCenterLocal;
    private Vector3 caseBoundsSizeLocal;
    private Vector3 fullWatchBoundsSizeLocal = Vector3.one * 0.05f;
    private float caseWidthLocal = 0.05f;
    private Vector3 resolvedFaceAxis = Vector3.forward;
    private Vector3 resolvedTwelveAxis = Vector3.up;

    private Vector3 smoothedWristPosition;
    private Quaternion smoothedCaseRotation = Quaternion.identity;
    private Vector3 smoothedForearmDir = Vector3.up;
    private float smoothedRootScale = 1f;
    private float smoothedDepth = 0.36f;

    public bool IsTryOnActive => tryOnActive;

    public string GetStatusString()
    {
        if (handLandmarker == null) return "ERR: NO LANDMARKER";
        return handLandmarker.DiagnosticStatus;
    }

    public void SetCaseSizeMillimeters(float millimeters)
    {
        float ratio = Mathf.Clamp(millimeters / 42f, 0.75f, 1.35f);
        watchWidthMeters = millimeters * 0.001f;
        sizeMultiplier = 1.25f * ratio;
    }

    private void Awake()
    {
        FindReferences();
    }

    private void Update()
    {
        if (!tryOnActive)
        {
            if (Time.unscaledTime >= nextTargetScan)
            {
                nextTargetScan = Time.unscaledTime + targetScanInterval;
                FindTrackedWatch();
            }
            return;
        }

        HideSourceRenderers();

        if (tryOnWatchRoot == null || tryOnWatch == null)
        {
            StopTryOn();
            return;
        }

        if (TryCalculateHandPose(
                out Vector3 wristCorePosition,
                out Quaternion caseRotation,
                out Vector3 forearmDirCam,
                out float estimatedWristWidthMeters))
        {
            bool wasHidden = !tryOnWatch.activeSelf;
            lastValidHandTime = Time.unscaledTime;

            if (!cloneInCameraSpace)
            {
                MoveCloneToCameraSpace();
                hasSmoothedPose = false;
            }
            else if (wasHidden)
            {
                hasSmoothedPose = false;
            }

            float rootScale = CalculateRootScaleForWrist(estimatedWristWidthMeters);
            SetSmoothedCameraPose(wristCorePosition, caseRotation, forearmDirCam, rootScale, estimatedWristWidthMeters);

            if (wasHidden)
            {
                tryOnWatch.SetActive(true);
                if (wristOccluderObj != null)
                    wristOccluderObj.SetActive(hideBackClaspOnly);
                ForceEnableAllCloneRenderers();
            }

            return;
        }

        if (tryOnWatch.activeSelf && cloneInCameraSpace &&
            Time.unscaledTime - lastValidHandTime <= Mathf.Max(handLostDelay, 0.30f))
        {
            return;
        }

        HideTryOnCloneUntilHandReturns();
    }

    private void FindReferences()
    {
        if (arCamera == null) arCamera = Camera.main;
        if (arCamera == null) arCamera = FindFirstObjectByType<Camera>();
        if (handLandmarker == null)
            handLandmarker = FindFirstObjectByType<AuronVuforiaHandLandmarker>();
        if (vuforiaCameraFrame == null)
            vuforiaCameraFrame = FindFirstObjectByType<AuronVuforiaCameraFrame>();
    }

    private void FindTrackedWatch()
    {
        currentTarget = null;
        currentWatch = null;

        ImageTargetBehaviour[] targets =
            FindObjectsByType<ImageTargetBehaviour>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

        for (int i = 0; i < targets.Length; i++)
        {
            ImageTargetBehaviour target = targets[i];
            if (target == null || !IsTracked(target)) continue;

            Transform watch = FindWatchUnderTarget(target);
            if (watch == null) continue;

            currentTarget = target;
            currentWatch = watch;
            lastRememberedTarget = target;
            lastRememberedWatch = watch;
            return;
        }
    }

    private Transform ResolveWatchForTryOn()
    {
        FindTrackedWatch();
        if (currentWatch != null)
            return currentWatch;

        if (lastRememberedWatch != null)
        {
            currentTarget = lastRememberedTarget;
            currentWatch = lastRememberedWatch;
            return currentWatch;
        }

        AuronWatchExperience exp = FindFirstObjectByType<AuronWatchExperience>(FindObjectsInactive.Include);
        if (exp != null)
        {
            currentWatch = exp.transform;
            currentTarget = exp.GetComponentInParent<ImageTargetBehaviour>();
            lastRememberedWatch = currentWatch;
            lastRememberedTarget = currentTarget;
            return currentWatch;
        }

        ImageTargetBehaviour[] allTargets = FindObjectsByType<ImageTargetBehaviour>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < allTargets.Length; i++)
        {
            Transform w = FindWatchUnderTarget(allTargets[i]);
            if (w != null)
            {
                currentTarget = allTargets[i];
                currentWatch = w;
                lastRememberedTarget = currentTarget;
                lastRememberedWatch = currentWatch;
                return currentWatch;
            }
        }

        return null;
    }

    private bool IsTracked(ImageTargetBehaviour target)
    {
        try
        {
            Status status = target.TargetStatus.Status;
            return status == Status.TRACKED ||
                   status == Status.EXTENDED_TRACKED;
        }
        catch
        {
            return false;
        }
    }

    private Transform FindWatchUnderTarget(ImageTargetBehaviour target)
    {
        if (target == null) return null;

        AuronWatchExperience experience =
            target.GetComponentInChildren<AuronWatchExperience>(true);

        if (experience != null) return experience.transform;

        for (int i = 0; i < target.transform.childCount; i++)
        {
            Transform child = target.transform.GetChild(i);

            if (child != null &&
                child.GetComponentInChildren<Renderer>(true) != null)
            {
                return child;
            }
        }

        return null;
    }

    public bool StartTryOn()
    {
        FindReferences();

        if (tryOnActive)
        {
            if (handLandmarker != null)
                handLandmarker.SetTrackingActive(true);

            return true;
        }

        if (handLandmarker != null)
            handLandmarker.SetTrackingActive(false);

        Transform watchToClone = ResolveWatchForTryOn();

        if (watchToClone == null || arCamera == null || handLandmarker == null)
        {
            Debug.LogError("[AURON TRY ON] Missing watch, ARCamera, or HandLandmarker.");
            return false;
        }

        if (arCamera.nearClipPlane > 0.01f)
            arCamera.nearClipPlane = 0.01f;

        sourceWatchRoot = watchToClone;
        sourceParent = sourceWatchRoot.parent;
        sourceLocalPosition = sourceWatchRoot.localPosition;
        sourceLocalRotation = sourceWatchRoot.localRotation;
        sourceLocalScale = sourceWatchRoot.localScale;

        sourceWatchExperience =
            sourceWatchRoot.GetComponent<AuronWatchExperience>();

        if (sourceWatchExperience == null)
        {
            sourceWatchExperience =
                sourceWatchRoot.GetComponentInChildren<AuronWatchExperience>(true);
        }

        if (sourceWatchExperience != null)
        {
            sourceWatchExperienceWasEnabled = sourceWatchExperience.enabled;
            sourceWatchExperience.enabled = true;
            sourceWatchExperience.ResetView();
            sourceWatchExperience.enabled = false;

            sourceParent = sourceWatchRoot.parent;
            sourceLocalPosition = sourceWatchRoot.localPosition;
            sourceLocalRotation = sourceWatchRoot.localRotation;
            sourceLocalScale = sourceWatchRoot.localScale;
        }

        CacheSourceRenderers();

        if (!CreateTryOnCopy())
        {
            RestoreSourceWatch();
            return false;
        }

        MeasureWatchCase();
        ResolveModelAxes();
        CreateInvisibleWristOccluder();
        HideSourceRenderers();

        tryOnActive = true;
        cloneInCameraSpace = false;
        lastValidHandTime = -999f;
        hasSmoothedPose = false;

        MoveCloneToCameraSpace();
        tryOnWatch.SetActive(false);
        if (wristOccluderObj != null)
            wristOccluderObj.SetActive(false);

        handLandmarker.SetTrackingActive(true);
        return true;
    }

    public void StopTryOn()
    {
        tryOnActive = false;

        if (handLandmarker != null)
            handLandmarker.SetTrackingActive(false);

        cloneInCameraSpace = false;
        hasSmoothedPose = false;

        DestroyTryOnCopy();
        RestoreSourceWatch();

        currentTarget = null;
        currentWatch = null;
        sourceWatchRoot = null;
        sourceParent = null;
        sourceWatchExperience = null;
        sourceRenderers = null;
        sourceRendererStates = null;

        RefreshTargetVisibility();
    }

    private void CacheSourceRenderers()
    {
        sourceRenderers =
            sourceWatchRoot.GetComponentsInChildren<Renderer>(true);

        sourceRendererStates = new bool[sourceRenderers.Length];

        for (int i = 0; i < sourceRenderers.Length; i++)
        {
            Renderer r = sourceRenderers[i];
            sourceRendererStates[i] = r != null;
        }
    }

    private void HideSourceRenderers()
    {
        if (sourceRenderers == null) return;

        for (int i = 0; i < sourceRenderers.Length; i++)
        {
            if (sourceRenderers[i] != null && sourceRenderers[i].enabled)
                sourceRenderers[i].enabled = false;
        }
    }

    private void RestoreSourceWatch()
    {
        if (sourceRenderers != null && sourceRendererStates != null)
        {
            int count = Mathf.Min(
                sourceRenderers.Length,
                sourceRendererStates.Length);

            for (int i = 0; i < count; i++)
            {
                if (sourceRenderers[i] != null)
                    sourceRenderers[i].enabled = sourceRendererStates[i];
            }
        }

        if (sourceWatchRoot != null)
        {
            sourceWatchRoot.gameObject.SetActive(true);
            sourceWatchRoot.localPosition = sourceLocalPosition;
            sourceWatchRoot.localRotation = sourceLocalRotation;
            sourceWatchRoot.localScale = sourceLocalScale;
        }

        if (sourceWatchExperience != null)
        {
            sourceWatchExperience.enabled = sourceWatchExperienceWasEnabled;

            if (sourceWatchExperience.enabled)
                sourceWatchExperience.ResetView();
        }
    }

    private bool CreateTryOnCopy()
    {
        if (sourceWatchRoot == null) return false;

        DestroyTryOnCopy();

        tryOnWatch = Instantiate(sourceWatchRoot.gameObject);
        tryOnWatch.name = "AURON_TRY_ON_WATCH";
        tryOnWatchRoot = tryOnWatch.transform;
        tryOnWatchRoot.SetParent(sourceParent, false);
        tryOnWatchRoot.localPosition = sourceLocalPosition;
        tryOnWatchRoot.localRotation = sourceLocalRotation;
        tryOnWatchRoot.localScale = sourceLocalScale;

        MonoBehaviour[] behaviours =
            tryOnWatch.GetComponentsInChildren<MonoBehaviour>(true);

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] != null)
                behaviours[i].enabled = false;
        }

        Animator[] animators =
            tryOnWatch.GetComponentsInChildren<Animator>(true);

        for (int i = 0; i < animators.Length; i++)
        {
            if (animators[i] != null)
                animators[i].enabled = false;
        }

        Collider[] colliders =
            tryOnWatch.GetComponentsInChildren<Collider>(true);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
                colliders[i].enabled = false;
        }

        Rigidbody[] bodies =
            tryOnWatch.GetComponentsInChildren<Rigidbody>(true);

        for (int i = 0; i < bodies.Length; i++)
        {
            if (bodies[i] == null) continue;

            bodies[i].isKinematic = true;
            bodies[i].useGravity = false;
            bodies[i].detectCollisions = false;
        }

        ForceEnableAllCloneRenderers();

        tryOnCaseAnchor = FindCaseAnchor(tryOnWatchRoot);
        return true;
    }

    private void CreateInvisibleWristOccluder()
    {
        if (wristOccluderObj != null)
            Destroy(wristOccluderObj);

        if (arCamera == null || !hideBackClaspOnly) return;

        Shader depthMaskShader = Shader.Find("AURON/WristOccluder");
        if (depthMaskShader == null)
            return;

        wristOccluderObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wristOccluderObj.name = "AURON_WRIST_OCCLUDER";
        Destroy(wristOccluderObj.GetComponent<Collider>());

        wristOccluderTransform = wristOccluderObj.transform;
        wristOccluderTransform.SetParent(arCamera.transform, false);
        wristOccluderObj.layer = 0;

        occluderMaterial = new Material(depthMaskShader);
        occluderMaterial.renderQueue = (int)RenderQueue.Geometry - 10;

        Renderer r = wristOccluderObj.GetComponent<Renderer>();
        if (r != null)
        {
            r.sharedMaterial = occluderMaterial;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
        }
    }

    private void ForceEnableAllCloneRenderers()
    {
        if (tryOnWatch == null) return;

        Transform[] allTransforms = tryOnWatch.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < allTransforms.Length; i++)
        {
            if (allTransforms[i] != null)
            {
                allTransforms[i].gameObject.SetActive(true);
                allTransforms[i].gameObject.layer = 0;
            }
        }

        Renderer[] cloneRenderers =
            tryOnWatch.GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < cloneRenderers.Length; i++)
        {
            if (cloneRenderers[i] != null)
            {
                cloneRenderers[i].enabled = true;
                cloneRenderers[i].gameObject.SetActive(true);
            }
        }
    }

    private void DestroyTryOnCopy()
    {
        if (tryOnWatch != null)
            Destroy(tryOnWatch);
        if (wristOccluderObj != null)
            Destroy(wristOccluderObj);
        if (occluderMaterial != null)
            Destroy(occluderMaterial);

        tryOnWatch = null;
        tryOnWatchRoot = null;
        tryOnCaseAnchor = null;
        wristOccluderObj = null;
        wristOccluderTransform = null;
        occluderMaterial = null;
    }

    private void MoveCloneToCameraSpace()
    {
        if (tryOnWatchRoot == null || arCamera == null) return;

        tryOnWatchRoot.SetParent(arCamera.transform, false);
        tryOnWatchRoot.localPosition = Vector3.zero;
        tryOnWatchRoot.localRotation = Quaternion.identity;
        tryOnWatchRoot.localScale = Vector3.one;
        cloneInCameraSpace = true;
    }

    private void HideTryOnCloneUntilHandReturns()
    {
        if (tryOnWatch != null && tryOnWatch.activeSelf)
            tryOnWatch.SetActive(false);
        if (wristOccluderObj != null && wristOccluderObj.activeSelf)
            wristOccluderObj.SetActive(false);

        if (!cloneInCameraSpace)
            hasSmoothedPose = false;
    }

    private Transform FindCaseAnchor(Transform root)
    {
        if (root == null) return null;

        Transform exact = FindDeepChild(root, caseAnchorName);
        if (exact != null) return exact;

        string[] fallbackNames =
        {
            "TRY_ON_ANCHOR", "WatchCase", "Watch_Case",
            "Case", "case", "Dial", "dial"
        };

        for (int i = 0; i < fallbackNames.Length; i++)
        {
            Transform found = FindDeepChild(root, fallbackNames[i]);
            if (found != null) return found;
        }

        return root;
    }

    private Transform FindDeepChild(Transform parent, string childName)
    {
        if (parent == null) return null;
        if (parent.name == childName) return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindDeepChild(parent.GetChild(i), childName);
            if (found != null) return found;
        }

        return null;
    }

    private void MeasureWatchCase()
    {
        if (tryOnWatchRoot == null) return;

        Transform anchor =
            tryOnCaseAnchor != null ? tryOnCaseAnchor : tryOnWatchRoot;

        Renderer[] renderers = anchor.GetComponentsInChildren<Renderer>(true);
        bool hasBounds = false;
        Bounds localBounds = new Bounds(Vector3.zero, Vector3.zero);

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;

            MeshFilter filter = renderer.GetComponent<MeshFilter>();
            if (filter != null && filter.sharedMesh != null)
            {
                EncapsulateLocalBounds(
                    tryOnWatchRoot,
                    filter.transform,
                    filter.sharedMesh.bounds,
                    ref localBounds,
                    ref hasBounds);
            }
        }

        if (hasBounds)
        {
            caseCenterLocal = localBounds.center;
            caseBoundsSizeLocal = localBounds.size;
        }
        else
        {
            caseCenterLocal = Vector3.zero;
            caseBoundsSizeLocal = Vector3.one * 0.05f;
        }

        Bounds fullBounds = MeasureFullWatchBoundsLocal(out bool hasFull);
        if (hasFull)
        {
            fullWatchCenterLocal = fullBounds.center;
            fullWatchBoundsSizeLocal = fullBounds.size;
        }
        else
        {
            fullWatchCenterLocal = caseCenterLocal;
            fullWatchBoundsSizeLocal = caseBoundsSizeLocal;
        }

        Vector3 face = ResolveAxis(modelFaceAxis, caseBoundsSizeLocal, true);

        float x = Mathf.Abs(caseBoundsSizeLocal.x);
        float y = Mathf.Abs(caseBoundsSizeLocal.y);
        float z = Mathf.Abs(caseBoundsSizeLocal.z);

        if (Mathf.Abs(face.x) > 0.5f)
            caseWidthLocal = Mathf.Max(y, z);
        else if (Mathf.Abs(face.y) > 0.5f)
            caseWidthLocal = Mathf.Max(x, z);
        else
            caseWidthLocal = Mathf.Max(x, y);

        if (caseWidthLocal < 0.00001f)
            caseWidthLocal = 0.05f;
    }

    private Bounds MeasureFullWatchBoundsLocal(out bool hasBounds)
    {
        hasBounds = false;
        Bounds combined = new Bounds(Vector3.zero, Vector3.zero);
        if (tryOnWatchRoot == null) return combined;

        Renderer[] renderers =
            tryOnWatchRoot.GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null) continue;

            MeshFilter filter = renderer.GetComponent<MeshFilter>();
            if (filter != null && filter.sharedMesh != null)
            {
                EncapsulateLocalBounds(
                    tryOnWatchRoot,
                    filter.transform,
                    filter.sharedMesh.bounds,
                    ref combined,
                    ref hasBounds);
            }
        }

        return combined;
    }

    private void EncapsulateLocalBounds(
        Transform root,
        Transform itemTransform,
        Bounds itemBounds,
        ref Bounds combined,
        ref bool hasBounds)
    {
        Vector3[] corners = BoundsCorners(itemBounds);

        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 world = itemTransform.TransformPoint(corners[i]);
            Vector3 local = root.InverseTransformPoint(world);

            if (!hasBounds)
            {
                combined = new Bounds(local, Vector3.zero);
                hasBounds = true;
            }
            else
            {
                combined.Encapsulate(local);
            }
        }
    }

    private Vector3[] BoundsCorners(Bounds b)
    {
        Vector3 min = b.min;
        Vector3 max = b.max;

        return new Vector3[]
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(max.x, max.y, min.z),
            new Vector3(min.x, min.y, max.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, max.y, max.z)
        };
    }

    private void ResolveModelAxes()
    {
        resolvedFaceAxis =
            ResolveAxis(modelFaceAxis, caseBoundsSizeLocal, true);

        resolvedTwelveAxis =
            ResolveTwelveAxis(
                modelTwelveAxis,
                fullWatchBoundsSizeLocal,
                resolvedFaceAxis);

        Vector3 strapCenterToDial = caseCenterLocal - fullWatchCenterLocal;
        if (strapCenterToDial.sqrMagnitude > 0.00001f)
        {
            if (Vector3.Dot(resolvedFaceAxis, strapCenterToDial) < 0f)
                resolvedFaceAxis = -resolvedFaceAxis;
        }

        if (invertFace) resolvedFaceAxis = -resolvedFaceAxis;
        if (invertTwelve) resolvedTwelveAxis = -resolvedTwelveAxis;
    }

    private Vector3 ResolveAxis(
        ModelAxis axis,
        Vector3 size,
        bool chooseThinnest)
    {
        if (axis != ModelAxis.Auto)
            return AxisVector(axis);

        float x = Mathf.Abs(size.x);
        float y = Mathf.Abs(size.y);
        float z = Mathf.Abs(size.z);

        if (chooseThinnest)
        {
            if (x <= y && x <= z) return Vector3.right;
            if (y <= x && y <= z) return Vector3.up;
            return Vector3.forward;
        }

        if (x >= y && x >= z) return Vector3.right;
        if (y >= x && y >= z) return Vector3.up;

        return Vector3.forward;
    }

    private Vector3 ResolveTwelveAxis(
        ModelAxis axis,
        Vector3 size,
        Vector3 face)
    {
        if (axis != ModelAxis.Auto)
            return AxisVector(axis);

        float x = Mathf.Abs(size.x);
        float y = Mathf.Abs(size.y);
        float z = Mathf.Abs(size.z);

        if (Mathf.Abs(face.x) > 0.5f)
            return y >= z ? Vector3.up : Vector3.forward;

        if (Mathf.Abs(face.y) > 0.5f)
            return x >= z ? Vector3.right : Vector3.forward;

        return x >= y ? Vector3.right : Vector3.up;
    }

    private Vector3 AxisVector(ModelAxis axis)
    {
        switch (axis)
        {
            case ModelAxis.PositiveX: return Vector3.right;
            case ModelAxis.NegativeX: return Vector3.left;
            case ModelAxis.PositiveY: return Vector3.up;
            case ModelAxis.NegativeY: return Vector3.down;
            case ModelAxis.PositiveZ: return Vector3.forward;
            case ModelAxis.NegativeZ: return Vector3.back;
            default: return Vector3.forward;
        }
    }

    private Quaternion GetModelAlignment()
    {
        Vector3 face =
            resolvedFaceAxis.sqrMagnitude > 0.000001f
                ? resolvedFaceAxis.normalized
                : Vector3.forward;

        Vector3 twelve =
            resolvedTwelveAxis.sqrMagnitude > 0.000001f
                ? resolvedTwelveAxis.normalized
                : Vector3.up;

        Quaternion faceRotation =
            Quaternion.FromToRotation(face, Vector3.forward);

        Vector3 adjustedTwelve = faceRotation * twelve;
        adjustedTwelve =
            Vector3.ProjectOnPlane(adjustedTwelve, Vector3.forward);

        if (adjustedTwelve.sqrMagnitude < 0.000001f)
            adjustedTwelve = Vector3.up;

        Quaternion twelveRotation =
            Quaternion.FromToRotation(
                adjustedTwelve.normalized,
                Vector3.up);

        return twelveRotation * faceRotation;
    }

    private bool TryGetViewportLandmark(
        int index,
        out Vector2 viewportPoint)
    {
        viewportPoint = Vector2.zero;

        if (handLandmarker == null || !handLandmarker.HasHand)
            return false;

        if (!handLandmarker.TryGetLandmark(
                index,
                out Vector3 normalized,
                out _))
        {
            return false;
        }

        if (!IsFinite(normalized.x) || !IsFinite(normalized.y))
            return false;

        Vector2 uv = new Vector2(normalized.x, normalized.y);

        // On Android Portrait (AppliedRotationDegrees == 270), the sensor is rotated 90 degrees CW relative to the screen.
        // In Unity Editor Webcam (AppliedRotationDegrees == 0), the webcam is NOT rotated!
        bool isRotatedSensor = (handLandmarker.AppliedRotationDegrees == 270 || handLandmarker.AppliedRotationDegrees == 90);
        if (isRotatedSensor)
        {
            uv = new Vector2(1f - uv.y, uv.x);
        }

        if (mirrorLandmarksForDisplay)
            uv.x = 1f - uv.x;

        uv = ApplyCenterCrop(uv, isRotatedSensor);

        viewportPoint = new Vector2(uv.x, 1f - uv.y);

        return IsFinite(viewportPoint.x) &&
               IsFinite(viewportPoint.y);
    }

    private Vector2 ApplyCenterCrop(Vector2 uv, bool swapSensorDimensions)
    {
        if (arCamera == null) return uv;

        Texture tex = vuforiaCameraFrame != null
            ? vuforiaCameraFrame.CurrentTexture
            : null;

        if (tex == null || tex.width <= 0 || tex.height <= 0)
            return uv;

        float imageAspect = swapSensorDimensions
            ? (float)tex.height / tex.width
            : (float)tex.width / tex.height;

        float viewportAspect = Mathf.Max(0.01f, arCamera.aspect);

        if (imageAspect > viewportAspect)
        {
            float visibleWidth = viewportAspect / imageAspect;
            uv.x = (uv.x - 0.5f) / visibleWidth + 0.5f;
        }
        else if (imageAspect < viewportAspect)
        {
            float visibleHeight = imageAspect / viewportAspect;
            uv.y = (uv.y - 0.5f) / visibleHeight + 0.5f;
        }

        return uv;
    }

    private bool TryCalculateHandPose(
        out Vector3 wristCorePosition,
        out Quaternion caseRotation,
        out Vector3 forearmDirCam,
        out float estimatedWristWidthMeters)
    {
        wristCorePosition = Vector3.zero;
        caseRotation = Quaternion.identity;
        forearmDirCam = Vector3.up;
        estimatedWristWidthMeters = watchWidthMeters * sizeMultiplier;

        if (arCamera == null || handLandmarker == null || !handLandmarker.HasHand)
            return false;

        if (!TryGetViewportLandmark(0, out Vector2 p0) ||
            !TryGetViewportLandmark(5, out Vector2 p5) ||
            !TryGetViewportLandmark(9, out Vector2 p9) ||
            !TryGetViewportLandmark(13, out Vector2 p13) ||
            !TryGetViewportLandmark(17, out Vector2 p17))
        {
            return false;
        }

        float tanHalfFov = Mathf.Tan(arCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float aspect = Mathf.Max(0.01f, arCamera.aspect);

        if (tanHalfFov <= 0.0001f)
            return false;

        float projectedPalmWidth = ViewportPlaneDistance(p5, p17, aspect);
        float projectedHandLength = ViewportPlaneDistance(p0, p9, aspect);

        if (projectedHandLength < 0.015f || projectedPalmWidth < 0.012f)
            return false;

        float depthFromWidth = projectedPalmWidth > 0.001f
            ? assumedPalmWidthMeters / (2f * tanHalfFov * projectedPalmWidth)
            : maximumHandDepthMeters;

        float depthFromLength = projectedHandLength > 0.001f
            ? assumedWristToMiddleMeters / (2f * tanHalfFov * projectedHandLength)
            : maximumHandDepthMeters;

        float rawDepth = Mathf.Min(depthFromWidth, depthFromLength);
        float clampedDepth = Mathf.Clamp(rawDepth, minimumHandDepthMeters, maximumHandDepthMeters);

        if (!hasSmoothedPose)
            smoothedDepth = clampedDepth;
        else
            smoothedDepth = Mathf.Lerp(smoothedDepth, clampedDepth, 1f - Mathf.Exp(-22f * Time.unscaledDeltaTime));

        float depth = smoothedDepth;

        Vector3 wristCam = ViewportToCameraLocalAtDepth(p0, depth, depth);
        Vector3 indexCam = ViewportToCameraLocalAtDepth(p5, depth, depth);
        Vector3 middleCam = ViewportToCameraLocalAtDepth(p9, depth, depth);
        Vector3 ringCam = ViewportToCameraLocalAtDepth(p13, depth, depth);
        Vector3 pinkyCam = ViewportToCameraLocalAtDepth(p17, depth, depth);

        Vector3 palmCenterCam = (indexCam + middleCam + ringCam + pinkyCam) * 0.25f;
        Vector3 alongArm = (palmCenterCam - wristCam);
        if (alongArm.sqrMagnitude < 0.000001f)
            alongArm = Vector3.up;
        alongArm.Normalize();
        forearmDirCam = alongArm;

        Vector3 acrossWrist = (indexCam - pinkyCam);
        if (acrossWrist.sqrMagnitude < 0.000001f)
            acrossWrist = Vector3.Cross(alongArm, Vector3.back);
        acrossWrist.Normalize();

        Vector3 estimatedPalmNormal = Vector3.Cross(alongArm, acrossWrist);
        Vector3 cameraFacingNormal = Vector3.back;

        if (estimatedPalmNormal.sqrMagnitude > 0.000001f)
        {
            estimatedPalmNormal.Normalize();
            if (Vector3.Dot(estimatedPalmNormal, cameraFacingNormal) < 0f)
                estimatedPalmNormal = -estimatedPalmNormal;
        }
        else
        {
            estimatedPalmNormal = cameraFacingNormal;
        }

        Vector3 faceNormal = Vector3.Slerp(
            cameraFacingNormal,
            estimatedPalmNormal,
            palmRotationInfluence).normalized;

        Vector3 desiredUpAxis = alignStrapAroundWrist ? acrossWrist : alongArm;
        if (twelveTowardThumb)
            desiredUpAxis = -desiredUpAxis;

        Vector3 dialUp = Vector3.ProjectOnPlane(desiredUpAxis, faceNormal);
        if (dialUp.sqrMagnitude < 0.000001f)
            dialUp = Vector3.ProjectOnPlane(acrossWrist, faceNormal);
        if (dialUp.sqrMagnitude < 0.000001f)
            dialUp = Vector3.up;

        dialUp.Normalize();

        caseRotation = Quaternion.LookRotation(faceNormal, dialUp);

        Vector3 towardForearm = -alongArm;
        float forearmOffset = wristToWatchMeters + wristPlacementOffsetMeters;

        wristCorePosition =
            wristCam +
            towardForearm * forearmOffset +
            faceNormal * palmSurfaceOffsetMeters;

        float palm3DWidth = Vector3.Distance(indexCam, pinkyCam);
        float wrist3DWidth = Mathf.Clamp(palm3DWidth * 0.75f, 0.038f, 0.064f);
        estimatedWristWidthMeters = wrist3DWidth * sizeMultiplier;

        return true;
    }

    private Vector3 ViewportToCameraLocalAtDepth(
        Vector2 viewport,
        float baseDepth,
        float desiredDepth)
    {
        float safeX = Mathf.Clamp(viewport.x, 0.002f, 0.998f);
        float safeY = Mathf.Clamp(viewport.y, 0.002f, 0.998f);
        float safeBaseDepth = Mathf.Max(0.05f, baseDepth);

        float safeDepth = Mathf.Clamp(
            desiredDepth,
            minimumHandDepthMeters,
            maximumHandDepthMeters);

        Vector3 worldAtBaseDepth = arCamera.ViewportToWorldPoint(
            new Vector3(safeX, safeY, safeBaseDepth));

        Vector3 localAtBaseDepth =
            arCamera.transform.InverseTransformPoint(worldAtBaseDepth);

        float depthRatio = safeDepth / safeBaseDepth;

        return new Vector3(
            localAtBaseDepth.x * depthRatio,
            localAtBaseDepth.y * depthRatio,
            safeDepth);
    }

    private float ViewportPlaneDistance(
        Vector2 a,
        Vector2 b,
        float aspect)
    {
        Vector2 delta = b - a;

        return Mathf.Sqrt(
            (delta.x * aspect) * (delta.x * aspect) +
            delta.y * delta.y);
    }

    private float CalculateRootScaleForWrist(float targetCaseWidthMeters)
    {
        if (caseWidthLocal <= 0.00001f)
            return 1f;

        return targetCaseWidthMeters / caseWidthLocal;
    }

    private void SetSmoothedCameraPose(
        Vector3 targetWristPosition,
        Quaternion targetCaseRotation,
        Vector3 targetForearmDir,
        float targetRootScale,
        float wristWidthMeters)
    {
        if (tryOnWatchRoot == null) return;

        Quaternion finalRotation =
            targetCaseRotation *
            GetModelAlignment() *
            Quaternion.Euler(rotationOffset);

        if (!hasSmoothedPose)
        {
            smoothedWristPosition = targetWristPosition;
            smoothedCaseRotation = finalRotation;
            smoothedForearmDir = targetForearmDir;
            smoothedRootScale = targetRootScale;
            hasSmoothedPose = true;
        }
        else
        {
            float dt = Time.unscaledDeltaTime;

            float pt = 1f - Mathf.Exp(-80f * dt);
            float rt = 1f - Mathf.Exp(-40f * dt);
            float st = 1f - Mathf.Exp(-25f * dt);

            smoothedWristPosition = Vector3.Lerp(smoothedWristPosition, targetWristPosition, pt);
            smoothedCaseRotation = Quaternion.Slerp(smoothedCaseRotation, finalRotation, rt);
            smoothedForearmDir = Vector3.Slerp(smoothedForearmDir, targetForearmDir, rt).normalized;
            smoothedRootScale = Mathf.Lerp(smoothedRootScale, targetRootScale, st);
        }

        Vector3 scaledCenter = Vector3.Scale(caseCenterLocal, Vector3.one * smoothedRootScale);

        Vector3 rootPosition =
            smoothedWristPosition -
            smoothedCaseRotation * scaledCenter;

        tryOnWatchRoot.localPosition = rootPosition;
        tryOnWatchRoot.localRotation = smoothedCaseRotation;
        tryOnWatchRoot.localScale = Vector3.one * smoothedRootScale;

        if (wristOccluderTransform != null && wristOccluderObj != null)
        {
            wristOccluderObj.SetActive(hideBackClaspOnly);
            wristOccluderTransform.localPosition = smoothedWristPosition + new Vector3(0f, 0f, wristWidthMeters * 0.35f);
            wristOccluderTransform.localRotation = Quaternion.FromToRotation(Vector3.up, smoothedForearmDir);

            float cylinderDiameter = wristWidthMeters * innerWristMaskWidth;
            wristOccluderTransform.localScale = new Vector3(
                cylinderDiameter,
                wristWidthMeters * 1.8f,
                cylinderDiameter * 0.55f
            );
        }
    }

    private bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private void RefreshTargetVisibility()
    {
        try
        {
            MonoBehaviour[] components = FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (int i = 0; i < components.Length; i++)
            {
                MonoBehaviour component = components[i];

                if (component == null ||
                    !component.GetType().Name.Contains("AuronTargetWatchVisibility"))
                {
                    continue;
                }

                MethodInfo refresh = component.GetType().GetMethod(
                    "Refresh",
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance);

                if (refresh != null)
                    refresh.Invoke(component, null);
            }
        }
        catch
        {
        }
    }

    private void OnDestroy()
    {
        tryOnActive = false;

        if (handLandmarker != null)
            handLandmarker.SetTrackingActive(false);

        DestroyTryOnCopy();
        RestoreSourceWatch();
    }
}