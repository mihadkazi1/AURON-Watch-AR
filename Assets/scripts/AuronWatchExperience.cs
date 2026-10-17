using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class AuronWatchExperience : MonoBehaviour
{
    [Header("Watch Setup")]
    [Tooltip("Assign the 'watch moroccan' root.")]
    [SerializeField] private Transform watchRoot;

    [Tooltip("Assign ARCamera. If empty, it will be found automatically.")]
    [SerializeField] private Camera arCamera;

    [Header("Inside / Cinematic Explosion")]
    [Tooltip("Main animation duration.")]
    [SerializeField] private float animationDuration = 2.2f;

    [Tooltip("Distance of each layer as a percentage of the watch size. 0.13 is a good cinematic starting value.")]
    [SerializeField] private float layerSpacing = 0.13f;

    [Tooltip("How far Empty.013 moves to the right.")]
    [SerializeField] private float crownDistance = 0.16f;

    [Tooltip("Small vertical presentation lift. This helps the layers remain visually separated.")]
    [SerializeField] private float presentationLift = 0.12f;

    [Tooltip("Delay between the start of each mechanical layer.")]
    [SerializeField] private float layerStagger = 0.08f;

    [Tooltip("Small cinematic rotation of the watch during Inside mode.")]
    [SerializeField] private float presentationTiltX = 8f;

    [Tooltip("Small cinematic rotation of the watch during Inside mode.")]
    [SerializeField] private float presentationTiltY = -18f;

    [Tooltip("Controls which end of innerPartNames is treated as the front dial vs. the backplate. Toggle this if the backplate moves to the front.")]
    [SerializeField] private bool frontPartMovesFarthest = false;

    [Header("3D Rotation / Zoom")]
    [SerializeField] private float rotationSpeed = 0.20f;
    [SerializeField] private float mouseRotationSpeed = 4.0f;
    [SerializeField] private float zoomSpeed = 0.0015f;
    [SerializeField] private float mouseZoomSpeed = 0.0015f;
    [SerializeField] private float minZoom = 0.70f;
    [SerializeField] private float maxZoom = 1.35f;

    [Header("Animator Safety")]
    [Tooltip("Disable any Animator while this script controls the mechanism.")]
    [SerializeField] private bool disableAnimatorsWhileControlling = true;

    private readonly string[] innerPartNames =
    {
        "Cube.003",
        "Empty.001",
        "Empty.002",
        "Empty.003",
        "Empty.004",
        "Empty.005",
        "Empty.006",
        "Empty.007",
        "Empty.008",
        "Empty.009",
        "Empty.010",
        "Empty.011",
        "Empty.012"
    };

    private const string BODY_NAME = "Empty";
    private const string CROWN_NAME = "Empty.013";
    private const string STRAP_NAME = "Empty.014";

    private Transform body;
    private Transform crown;
    private Transform strap;

    private readonly List<Transform> innerParts = new List<Transform>();

    private readonly Dictionary<Transform, Vector3> originalLocalPositions = new Dictionary<Transform, Vector3>();
    private readonly Dictionary<Transform, Quaternion> originalLocalRotations = new Dictionary<Transform, Quaternion>();
    private readonly Dictionary<Transform, Vector3> originalLocalScales = new Dictionary<Transform, Vector3>();

    private Vector3 originalRootPosition;
    private Quaternion originalRootRotation;
    private Vector3 originalRootScale;

    private readonly Dictionary<Animator, bool> originalAnimatorStates = new Dictionary<Animator, bool>();

    private Coroutine animationCoroutine;
    private bool isAnimating;
    private bool isInsideMode;
    private float targetZoom = 1f;

    private Vector2 lastTouchPosition;
    private bool touchRotating;
    private float lastPinchDistance;
    private bool pinching;

    private Vector2 lastMousePosition;
    private bool mouseRotating;

    public bool IsInsideMode => isInsideMode;

    private void Awake()
    {
        if (watchRoot == null)
            watchRoot = transform;

        CacheOriginalTransforms();
        CacheAnimators();
    }

    private void Start()
    {
        FindCamera();
        RestoreWatchImmediately();
    }

    private void Update()
    {
        if (isAnimating)
            return;

        HandleTouchInput();
        HandleMouseInput();
    }

    private void CacheOriginalTransforms()
    {
        if (watchRoot == null)
        {
            Debug.LogError("[AURON] Watch Root is missing.");
            return;
        }

        innerParts.Clear();
        originalLocalPositions.Clear();
        originalLocalRotations.Clear();
        originalLocalScales.Clear();

        originalRootPosition = watchRoot.localPosition;
        originalRootRotation = watchRoot.localRotation;
        originalRootScale = watchRoot.localScale;

        body = FindAnywhere(BODY_NAME);
        CacheTransform(body);

        crown = FindAnywhere(CROWN_NAME);
        CacheTransform(crown);

        strap = FindAnywhere(STRAP_NAME);
        CacheTransform(strap);

        foreach (string partName in innerPartNames)
        {
            Transform part = FindAnywhere(partName);
            if (part != null)
            {
                innerParts.Add(part);
                CacheTransform(part);
            }
            else
            {
                Debug.LogWarning($"[AURON] Could not find: {partName}");
            }
        }

        Debug.Log($"[AURON] Watch cached. Inner parts found: {innerParts.Count}/13");
    }

    private void CacheTransform(Transform target)
    {
        if (target == null) return;

        if (!originalLocalPositions.ContainsKey(target))
            originalLocalPositions.Add(target, target.localPosition);

        if (!originalLocalRotations.ContainsKey(target))
            originalLocalRotations.Add(target, target.localRotation);

        if (!originalLocalScales.ContainsKey(target))
            originalLocalScales.Add(target, target.localScale);
    }

    private Transform FindAnywhere(string objectName)
    {
        if (watchRoot == null) return null;

        Transform[] objects = watchRoot.GetComponentsInChildren<Transform>(true);
        foreach (Transform item in objects)
        {
            if (item.name == objectName)
                return item;
        }
        return null;
    }

    private void FindCamera()
    {
        if (arCamera != null) return;
        arCamera = Camera.main;
        if (arCamera != null) return;

        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Camera cam in cameras)
        {
            if (cam != null && cam.isActiveAndEnabled)
            {
                arCamera = cam;
                break;
            }
        }
    }

    private void CacheAnimators()
    {
        originalAnimatorStates.Clear();
        if (watchRoot == null) return;

        Animator[] animators = watchRoot.GetComponentsInChildren<Animator>(true);
        foreach (Animator animator in animators)
        {
            if (animator == null) continue;
            originalAnimatorStates[animator] = animator.enabled;
        }
    }

    private void SetAnimatorsEnabled(bool enabled)
    {
        if (!disableAnimatorsWhileControlling) return;

        foreach (KeyValuePair<Animator, bool> item in originalAnimatorStates)
        {
            if (item.Key != null)
                item.Key.enabled = enabled;
        }
    }

    private void RestoreAnimatorStates()
    {
        foreach (KeyValuePair<Animator, bool> item in originalAnimatorStates)
        {
            if (item.Key != null)
                item.Key.enabled = item.Value;
        }
    }

    public void Show3DView()
    {
        StartRestoreAnimation();
    }

    public void CloseWatch()
    {
        StartRestoreAnimation();
    }

    public void ShowInside()
    {
        StartInsideAnimation();
    }

    public void Reassemble()
    {
        if (watchRoot == null) return;
        StopCurrentAnimation();
        animationCoroutine = StartCoroutine(ReassembleAnimation());
    }

    public void ResetView()
    {
        if (watchRoot == null) return;

        StopCurrentAnimation();
        isAnimating = false;
        isInsideMode = false;
        targetZoom = 1f;

        SetAnimatorsEnabled(false);
        RestoreOriginalTransforms();

        watchRoot.localPosition = originalRootPosition;
        watchRoot.localRotation = originalRootRotation;
        watchRoot.localScale = originalRootScale;

        RestoreAnimatorStates();
    }

    /// <summary>
    /// Resets the watch back to its normal 1x size and original upright rotation when exiting Auto-Turntable.
    /// </summary>
    public void ResetToNormalSizeAndPose()
    {
        if (watchRoot == null) return;
        targetZoom = 1f;
        watchRoot.localPosition = originalRootPosition;
        watchRoot.localRotation = isInsideMode
            ? originalRootRotation * Quaternion.Euler(presentationTiltX, presentationTiltY, 0f)
            : originalRootRotation;
        watchRoot.localScale = originalRootScale;
    }

    /// <summary>
    /// Allows the interactive slider in AuronLuxuryFeatures to scrub the explosion from 0% (closed) to 100% (open).
    /// </summary>
    public void SetManualExplosionProgress(float progress01)
    {
        if (watchRoot == null || !isInsideMode) return;

        StopCurrentAnimation();
        SetAnimatorsEnabled(false);
        FindCamera();
        if (arCamera == null) return;

        float t = Mathf.Clamp01(progress01);
        float watchSize = CalculateWatchSize();
        if (watchSize <= 0.0001f) watchSize = 1f;

        Vector3 towardUser = -arCamera.transform.forward.normalized;
        Vector3 screenUp = arCamera.transform.up.normalized;
        Vector3 screenRight = arCamera.transform.right.normalized;

        List<Transform> frontToBack = GetOrderedFrontToBackParts();
        int count = frontToBack.Count;

        for (int i = 0; i < count; i++)
        {
            Transform part = frontToBack[i];
            if (part == null || !originalLocalPositions.ContainsKey(part)) continue;

            int layerMultiplier = count - i;
            float distance = watchSize * layerSpacing * layerMultiplier * t;
            Vector3 worldOffset = (towardUser * distance) + (screenUp * (distance * presentationLift));
            Vector3 localOffset = WorldOffsetToParentLocal(part, worldOffset);

            part.localPosition = originalLocalPositions[part] + localOffset;
        }

        if (crown != null && originalLocalPositions.ContainsKey(crown))
        {
            float crownMove = watchSize * crownDistance * t;
            Vector3 localOffset = WorldOffsetToParentLocal(crown, screenRight * crownMove);
            crown.localPosition = originalLocalPositions[crown] + localOffset;
        }
    }

    /// <summary>
    /// Pulls the winding crown (Empty.013) out or pushes it back in for Feature 2 (Manual Winding).
    /// </summary>
    public void SetCrownPulledOut(bool pulledOut)
    {
        if (crown == null || !originalLocalPositions.ContainsKey(crown)) return;
        FindCamera();
        Vector3 rightDir = arCamera != null ? arCamera.transform.right.normalized : Vector3.right;
        float watchSize = CalculateWatchSize();
        float pullDist = pulledOut ? (watchSize * 0.075f) : 0f;
        Vector3 localOffset = WorldOffsetToParentLocal(crown, rightDir * pullDist);
        crown.localPosition = originalLocalPositions[crown] + localOffset;
    }


    private void StopCurrentAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        isAnimating = false;
    }

    private void StartRestoreAnimation()
    {
        if (watchRoot == null) return;
        StopCurrentAnimation();
        animationCoroutine = StartCoroutine(RestoreWatchAnimation());
        isInsideMode = false;
    }

    private IEnumerator RestoreWatchAnimation()
    {
        isAnimating = true;
        SetAnimatorsEnabled(false);

        Dictionary<Transform, Vector3> startPositions = GetCurrentPositions();
        Dictionary<Transform, Quaternion> startRotations = GetCurrentRotations();

        Quaternion startRootRotation = watchRoot.localRotation;
        Vector3 startRootPosition = watchRoot.localPosition;
        Vector3 startRootScale = watchRoot.localScale;

        float time = 0f;
        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / animationDuration);
            float smoothT = EaseInOut(t);

            watchRoot.localPosition = Vector3.Lerp(startRootPosition, originalRootPosition, smoothT);
            watchRoot.localRotation = Quaternion.Slerp(startRootRotation, originalRootRotation, smoothT);
            watchRoot.localScale = Vector3.Lerp(startRootScale, originalRootScale * targetZoom, smoothT);

            foreach (KeyValuePair<Transform, Vector3> item in startPositions)
            {
                Transform part = item.Key;
                if (part == null || !originalLocalPositions.ContainsKey(part)) continue;

                part.localPosition = Vector3.Lerp(item.Value, originalLocalPositions[part], smoothT);

                if (originalLocalRotations.ContainsKey(part))
                    part.localRotation = Quaternion.Slerp(startRotations[part], originalLocalRotations[part], smoothT);
            }

            yield return null;
        }

        RestoreOriginalTransforms();
        watchRoot.localPosition = originalRootPosition;
        watchRoot.localRotation = originalRootRotation;
        watchRoot.localScale = originalRootScale * targetZoom;

        RestoreAnimatorStates();
        isAnimating = false;
        isInsideMode = false;
        animationCoroutine = null;
    }

    // =========================================================
    // LAYER ORDER HELPER
    // Returns the list of parts ordered from FRONT (index 0) to BACK/BOTTOM (index count - 1)
    // =========================================================
    private List<Transform> GetOrderedFrontToBackParts()
    {
        List<Transform> ordered = new List<Transform>(innerParts);
        if (!frontPartMovesFarthest)
        {
            // When false, Empty.012 is treated as the front-most part and Cube.003 as the bottom back-plate
            ordered.Reverse();
        }
        return ordered;
    }

    // =========================================================
    // REASSEMBLE — BOTTOM PART RETURNS FIRST, FRONT PART LAST
    // =========================================================

    private IEnumerator ReassembleAnimation()
    {
        isAnimating = true;
        SetAnimatorsEnabled(false);

        Dictionary<Transform, Vector3> startPositions = GetCurrentPositions();
        Dictionary<Transform, Quaternion> startRotations = GetCurrentRotations();

        Vector3 startRootPosition = watchRoot.localPosition;
        Quaternion startRootRotation = watchRoot.localRotation;
        Vector3 finalRootScale = originalRootScale * targetZoom;

        float partDuration = Mathf.Max(0.1f, animationDuration);
        float stagger = Mathf.Max(0f, layerStagger);

        List<Transform> frontToBack = GetOrderedFrontToBackParts();
        int count = frontToBack.Count;
        float longestDelay = Mathf.Max(0, count - 1) * stagger;
        float totalDuration = partDuration + longestDelay;
        float elapsed = 0f;

        // PHASE 1: Reassemble the components.
        // frontToBack[count - 1] is the BOTTOM part -> delay = 0 (returns FIRST into the watch body)
        // frontToBack[0] is the FRONT part -> delay = (count - 1) * stagger (returns LAST to close the watch)
        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;
            float currentTime = Mathf.Min(elapsed, totalDuration);

            watchRoot.localPosition = startRootPosition;
            watchRoot.localRotation = startRootRotation;
            watchRoot.localScale = finalRootScale;

            for (int i = count - 1; i >= 0; i--)
            {
                Transform part = frontToBack[i];
                if (part == null || !originalLocalPositions.ContainsKey(part) || !startPositions.ContainsKey(part))
                    continue;

                float reverseDelay = (count - 1 - i) * stagger;
                float partT = Mathf.Clamp01((currentTime - reverseDelay) / partDuration);
                float smoothT = EaseInOut(partT);

                part.localPosition = Vector3.Lerp(startPositions[part], originalLocalPositions[part], smoothT);

                if (startRotations.ContainsKey(part) && originalLocalRotations.ContainsKey(part))
                    part.localRotation = Quaternion.Slerp(startRotations[part], originalLocalRotations[part], smoothT);

                if (originalLocalScales.ContainsKey(part))
                    part.localScale = originalLocalScales[part];
            }

            if (crown != null && startPositions.ContainsKey(crown) && originalLocalPositions.ContainsKey(crown))
            {
                float crownT = Mathf.Clamp01(currentTime / partDuration);
                float smoothCrownT = EaseInOut(crownT);
                crown.localPosition = Vector3.Lerp(startPositions[crown], originalLocalPositions[crown], smoothCrownT);

                if (originalLocalRotations.ContainsKey(crown))
                    crown.localRotation = originalLocalRotations[crown];
                if (originalLocalScales.ContainsKey(crown))
                    crown.localScale = originalLocalScales[crown];
            }

            if (body != null) RestoreTransform(body);
            if (strap != null) RestoreTransform(strap);

            yield return null;
        }

        RestoreOriginalTransforms();
        watchRoot.localPosition = startRootPosition;
        watchRoot.localRotation = startRootRotation;
        watchRoot.localScale = finalRootScale;

        // PHASE 2: Rotate the complete, assembled watch back to its normal pose.
        float rootReturnDuration = Mathf.Max(0.25f, partDuration * 0.25f);
        float rootReturnTime = 0f;

        while (rootReturnTime < rootReturnDuration)
        {
            rootReturnTime += Time.deltaTime;
            float rootT = Mathf.Clamp01(rootReturnTime / rootReturnDuration);
            float smoothRootT = EaseInOut(rootT);

            watchRoot.localPosition = Vector3.Lerp(startRootPosition, originalRootPosition, smoothRootT);
            watchRoot.localRotation = Quaternion.Slerp(startRootRotation, originalRootRotation, smoothRootT);
            watchRoot.localScale = finalRootScale;

            yield return null;
        }

        RestoreOriginalTransforms();
        watchRoot.localPosition = originalRootPosition;
        watchRoot.localRotation = originalRootRotation;
        watchRoot.localScale = finalRootScale;

        RestoreAnimatorStates();
        isAnimating = false;
        isInsideMode = false;
        animationCoroutine = null;
    }

    // =========================================================
    // INSIDE EXPLOSION — FRONT PART MOVES FARTHEST, BOTTOM LEAST
    // =========================================================

    private void StartInsideAnimation()
    {
        if (watchRoot == null) return;
        StopCurrentAnimation();
        animationCoroutine = StartCoroutine(ExplodeWatch());
        isInsideMode = true;
    }

    private IEnumerator ExplodeWatch()
    {
        isAnimating = true;
        SetAnimatorsEnabled(false);

        // Snap parts to their clean assembled positions before calculating world bounds & directions
        RestoreOriginalTransforms();

        watchRoot.localPosition = originalRootPosition;
        watchRoot.localRotation = originalRootRotation;
        watchRoot.localScale = originalRootScale * targetZoom;

        FindCamera();
        if (arCamera == null)
        {
            Debug.LogError("[AURON] ARCamera not found.");
            isAnimating = false;
            animationCoroutine = null;
            yield break;
        }

        float watchSize = CalculateWatchSize();
        if (watchSize <= 0.0001f) watchSize = 1f;

        Vector3 towardUser = -arCamera.transform.forward;
        towardUser.Normalize();

        Vector3 screenUp = arCamera.transform.up;
        screenUp.Normalize();

        Vector3 screenRight = arCamera.transform.right;
        screenRight.Normalize();

        // Use deterministic structural hierarchy order instead of camera-angle bounds sorting
        List<Transform> frontToBack = GetOrderedFrontToBackParts();
        int count = frontToBack.Count;

        Dictionary<Transform, Vector3> targetPositions = new Dictionary<Transform, Vector3>();

        for (int i = 0; i < count; i++)
        {
            Transform part = frontToBack[i];
            if (part == null || !originalLocalPositions.ContainsKey(part))
                continue;

            // i = 0 is the FRONT-MOST part -> moves FARTHEST (count - i)
            // i = count - 1 is the BOTTOM/BACK part -> moves LEAST (1)
            int layerMultiplier = count - i;

            float distance = watchSize * layerSpacing * layerMultiplier;
            Vector3 worldOffset = towardUser * distance;
            worldOffset += screenUp * (distance * presentationLift);

            Vector3 localOffset = WorldOffsetToParentLocal(part, worldOffset);
            targetPositions[part] = originalLocalPositions[part] + localOffset;
        }

        if (crown != null && originalLocalPositions.ContainsKey(crown))
        {
            float crownMove = watchSize * crownDistance;
            Vector3 worldOffset = screenRight * crownMove;
            Vector3 localOffset = WorldOffsetToParentLocal(crown, worldOffset);
            targetPositions[crown] = originalLocalPositions[crown] + localOffset;
        }

        if (body != null && originalLocalPositions.ContainsKey(body))
            targetPositions[body] = originalLocalPositions[body];

        if (strap != null && originalLocalPositions.ContainsKey(strap))
            targetPositions[strap] = originalLocalPositions[strap];

        Dictionary<Transform, Vector3> startPositions = GetCurrentPositions();

        Quaternion cinematicRotation = originalRootRotation * Quaternion.Euler(presentationTiltX, presentationTiltY, 0f);
        float totalAnimationTime = animationDuration + (Mathf.Max(0, count - 1) * layerStagger);
        float animationTime = 0f;

        while (animationTime < totalAnimationTime)
        {
            animationTime += Time.deltaTime;

            float rootT = Mathf.Clamp01(animationTime / animationDuration);
            float smoothRootT = EaseInOut(rootT);

            watchRoot.localRotation = Quaternion.Slerp(originalRootRotation, cinematicRotation, smoothRootT);
            watchRoot.localPosition = originalRootPosition;
            watchRoot.localScale = originalRootScale * targetZoom;

            for (int i = 0; i < count; i++)
            {
                Transform part = frontToBack[i];
                if (part == null || !targetPositions.ContainsKey(part)) continue;

                // i = 0 (front part) starts first (delay = 0)
                // i = count - 1 (bottom part) starts last
                float startDelay = i * layerStagger;
                float partTime = animationTime - startDelay;
                float partT = Mathf.Clamp01(partTime / animationDuration);
                float smoothPartT = EaseInOut(partT);

                Vector3 startPosition = startPositions.ContainsKey(part) ? startPositions[part] : part.localPosition;
                part.localPosition = Vector3.Lerp(startPosition, targetPositions[part], smoothPartT);

                if (originalLocalRotations.ContainsKey(part))
                    part.localRotation = originalLocalRotations[part];
            }

            if (crown != null && targetPositions.ContainsKey(crown))
            {
                float crownT = Mathf.Clamp01(animationTime / animationDuration);
                float smoothCrownT = EaseInOut(crownT);
                crown.localPosition = Vector3.Lerp(startPositions[crown], targetPositions[crown], smoothCrownT);
                crown.localRotation = originalLocalRotations[crown];
            }

            if (body != null) RestoreTransform(body);
            if (strap != null) RestoreTransform(strap);

            yield return null;
        }

        foreach (KeyValuePair<Transform, Vector3> item in targetPositions)
        {
            if (item.Key == null) continue;
            item.Key.localPosition = item.Value;
            if (originalLocalRotations.ContainsKey(item.Key))
                item.Key.localRotation = originalLocalRotations[item.Key];
        }

        if (body != null) RestoreTransform(body);
        if (strap != null) RestoreTransform(strap);

        watchRoot.localRotation = cinematicRotation;
        watchRoot.localPosition = originalRootPosition;
        watchRoot.localScale = originalRootScale * targetZoom;

        isAnimating = false;
        animationCoroutine = null;
    }

    private Vector3 WorldOffsetToParentLocal(Transform part, Vector3 worldOffset)
    {
        if (part == null || part.parent == null) return worldOffset;
        return part.parent.InverseTransformVector(worldOffset);
    }

    private float CalculateWatchSize()
    {
        if (watchRoot == null) return 1f;
        Renderer[] renderers = watchRoot.GetComponentsInChildren<Renderer>(true);
        Bounds bounds = new Bounds(watchRoot.position, Vector3.zero);
        bool foundRenderer = false;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;
            Transform rendererTransform = renderer.transform;
            if (strap != null && (rendererTransform == strap || rendererTransform.IsChildOf(strap)))
                continue;

            if (!foundRenderer)
            {
                bounds = renderer.bounds;
                foundRenderer = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        if (!foundRenderer) return 1f;
        return Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
    }

    private Dictionary<Transform, Vector3> GetCurrentPositions()
    {
        Dictionary<Transform, Vector3> result = new Dictionary<Transform, Vector3>();
        AddCurrentPosition(result, body);
        AddCurrentPosition(result, crown);
        AddCurrentPosition(result, strap);
        foreach (Transform part in innerParts)
            AddCurrentPosition(result, part);
        return result;
    }

    private void AddCurrentPosition(Dictionary<Transform, Vector3> dictionary, Transform target)
    {
        if (target != null) dictionary[target] = target.localPosition;
    }

    private Dictionary<Transform, Quaternion> GetCurrentRotations()
    {
        Dictionary<Transform, Quaternion> result = new Dictionary<Transform, Quaternion>();
        AddCurrentRotation(result, body);
        AddCurrentRotation(result, crown);
        AddCurrentRotation(result, strap);
        foreach (Transform part in innerParts)
            AddCurrentRotation(result, part);
        return result;
    }

    private void AddCurrentRotation(Dictionary<Transform, Quaternion> dictionary, Transform target)
    {
        if (target != null) dictionary[target] = target.localRotation;
    }

    private void RestoreOriginalTransforms()
    {
        foreach (Transform part in innerParts)
            RestoreTransform(part);
        RestoreTransform(body);
        RestoreTransform(crown);
        RestoreTransform(strap);
    }

    private void RestoreTransform(Transform target)
    {
        if (target == null) return;
        if (originalLocalPositions.ContainsKey(target))
            target.localPosition = originalLocalPositions[target];
        if (originalLocalRotations.ContainsKey(target))
            target.localRotation = originalLocalRotations[target];
        if (originalLocalScales.ContainsKey(target))
            target.localScale = originalLocalScales[target];
    }

    private void RestoreWatchImmediately()
    {
        if (watchRoot == null) return;
        StopCurrentAnimation();
        isAnimating = false;
        isInsideMode = false;
        targetZoom = 1f;

        watchRoot.localPosition = originalRootPosition;
        watchRoot.localRotation = originalRootRotation;
        watchRoot.localScale = originalRootScale;

        RestoreOriginalTransforms();
        RestoreAnimatorStates();
    }

    private void HandleTouchInput()
    {
        if (Touchscreen.current == null) return;

        List<TouchControl> activeTouches = new List<TouchControl>();
        foreach (TouchControl touch in Touchscreen.current.touches)
        {
            if (touch != null && touch.press.isPressed)
                activeTouches.Add(touch);
        }

        if (activeTouches.Count >= 2)
        {
            Vector2 p0 = activeTouches[0].position.ReadValue();
            Vector2 p1 = activeTouches[1].position.ReadValue();
            float currentDistance = Vector2.Distance(p0, p1);

            if (!pinching)
            {
                lastPinchDistance = currentDistance;
                pinching = true;
            }
            else
            {
                float difference = currentDistance - lastPinchDistance;
                targetZoom = Mathf.Clamp(targetZoom + difference * zoomSpeed, minZoom, maxZoom);
                ApplyZoom();
                lastPinchDistance = currentDistance;
            }

            touchRotating = false;
            return;
        }

        pinching = false;

        if (activeTouches.Count == 1)
        {
            Vector2 currentPosition = activeTouches[0].position.ReadValue();
            if (!touchRotating)
            {
                lastTouchPosition = currentPosition;
                touchRotating = true;
            }
            else
            {
                Vector2 delta = currentPosition - lastTouchPosition;
                RotateWatch(delta);
                lastTouchPosition = currentPosition;
            }
        }
        else
        {
            touchRotating = false;
        }
    }

    private void HandleMouseInput()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 currentMousePosition = Mouse.current.position.ReadValue();
            if (!mouseRotating)
            {
                lastMousePosition = currentMousePosition;
                mouseRotating = true;
            }
            else
            {
                Vector2 delta = currentMousePosition - lastMousePosition;
                RotateWatch(delta * mouseRotationSpeed);
                lastMousePosition = currentMousePosition;
            }
        }
        else
        {
            mouseRotating = false;
        }

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetZoom = Mathf.Clamp(targetZoom + scroll * mouseZoomSpeed, minZoom, maxZoom);
            ApplyZoom();
        }
    }

    private void RotateWatch(Vector2 delta)
    {
        if (watchRoot == null) return;

        FindCamera();
        if (arCamera != null)
        {
            watchRoot.Rotate(arCamera.transform.up, -delta.x * rotationSpeed, Space.World);
            watchRoot.Rotate(arCamera.transform.right, delta.y * rotationSpeed, Space.World);
        }
        else
        {
            watchRoot.Rotate(Vector3.up, -delta.x * rotationSpeed, Space.Self);
            watchRoot.Rotate(Vector3.right, delta.y * rotationSpeed, Space.Self);
        }
    }

    private void ApplyZoom()
    {
        if (watchRoot == null) return;
        watchRoot.localScale = originalRootScale * targetZoom;
    }

    private float EaseInOut(float t)
    {
        return t * t * (3f - 2f * t);
    }
}