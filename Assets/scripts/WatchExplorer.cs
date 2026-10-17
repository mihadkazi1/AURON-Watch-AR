using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class WatchExplorer : MonoBehaviour
{
    [Header("AUTO WATCH")]
    [SerializeField] private string watchContainerName = "group23";
    [SerializeField] private string strapName = "Watch_Strap";

    [Header("EXPLODED VIEW")]
    [SerializeField] private int automaticLayers = 7;
    [SerializeField] private float layerSpacing = 0.012f;
    [SerializeField] private float animationDuration = 1.35f;
    [SerializeField] private float layerDelay = 0.08f;

    [Header("EXPLOSION DIRECTION")]
    [SerializeField] private Vector3 explosionAxis = Vector3.up;

    [Header("3D INTERACTION")]
    [SerializeField] private Transform interactionRoot;

    [SerializeField] private float rotationSpeed = 0.20f;
    [SerializeField] private float pinchSensitivity = 0.003f;

    [SerializeField] private float mouseZoomSensitivity = 0.08f;

    [SerializeField] private float minimumScale = 0.60f;
    [SerializeField] private float maximumScale = 1.80f;

    [SerializeField] private float minimumPitch = -35f;
    [SerializeField] private float maximumPitch = 35f;

    [SerializeField] private float interactionSmoothness = 12f;

    private class AutoPart
    {
        public Transform transform;

        public Vector3 originalPosition;
        public Quaternion originalRotation;

        public int layer;

        public bool isStrap;
    }

    private readonly List<AutoPart> autoParts = new();

    private Transform watchContainer;

    private Quaternion originalRootRotation;
    private Quaternion targetRootRotation;

    private Vector3 originalRootScale;

    private float targetScale = 1f;

    private bool exploded;
    private bool initialized;
    private bool animationRunning;

    private Coroutine animationCoroutine;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        FindWatchContainer();

        if (watchContainer == null)
        {
            Debug.LogError(
                "WatchExplorer: Could not find group23."
            );

            return;
        }

        // group23 becomes the interaction root automatically
        interactionRoot = watchContainer;

        originalRootRotation =
            interactionRoot.localRotation;

        targetRootRotation =
            originalRootRotation;

        originalRootScale =
            interactionRoot.localScale;

        BuildAutomaticParts();

        initialized = true;

        Debug.Log(
            $"WatchExplorer: Found {autoParts.Count} watch parts."
        );
    }

    // =========================================================
    // FIND GROUP23
    // =========================================================

    private void FindWatchContainer()
    {
        watchContainer =
            FindChildRecursive(
                transform,
                watchContainerName
            );
    }

    private Transform FindChildRecursive(
        Transform parent,
        string objectName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == objectName)
                return child;

            Transform result =
                FindChildRecursive(
                    child,
                    objectName
                );

            if (result != null)
                return result;
        }

        return null;
    }

    // =========================================================
    // AUTOMATIC PART DETECTION
    // =========================================================

    private void BuildAutomaticParts()
    {
        autoParts.Clear();

        Vector3 axis =
            explosionAxis.normalized;

        List<Transform> candidates = new();

        foreach (Transform child in watchContainer)
        {
            if (!ContainsRenderer(child))
                continue;

            candidates.Add(child);
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning(
                "WatchExplorer: No renderable children found."
            );

            return;
        }

        // -----------------------------------------------------
        // Find depth range along explosion axis
        // -----------------------------------------------------

        float minimumDepth = float.MaxValue;
        float maximumDepth = float.MinValue;

        Dictionary<Transform, float> depths = new();

        foreach (Transform child in candidates)
        {
            Vector3 center =
                GetRendererCenterLocal(child);

            float depth =
                Vector3.Dot(center, axis);

            depths[child] = depth;

            minimumDepth =
                Mathf.Min(
                    minimumDepth,
                    depth
                );

            maximumDepth =
                Mathf.Max(
                    maximumDepth,
                    depth
                );
        }

        float depthRange =
            maximumDepth -
            minimumDepth;

        if (depthRange < 0.00001f)
            depthRange = 0.00001f;

        // -----------------------------------------------------
        // Create automatic layers
        // -----------------------------------------------------

        foreach (Transform child in candidates)
        {
            bool strap =
                child.name
                    .ToLower()
                    .Contains(
                        strapName.ToLower()
                    );

            AutoPart part = new AutoPart();

            part.transform = child;

            part.originalPosition =
                child.localPosition;

            part.originalRotation =
                child.localRotation;

            part.isStrap = strap;

            // Strap never moves
            if (strap)
            {
                part.layer = 0;
            }
            else
            {
                float normalized =
                    (depths[child] -
                     minimumDepth) /
                    depthRange;

                int layer =
                    Mathf.FloorToInt(
                        normalized *
                        automaticLayers
                    );

                layer =
                    Mathf.Clamp(
                        layer,
                        0,
                        automaticLayers - 1
                    );

                part.layer = layer;
            }

            autoParts.Add(part);
        }
    }

    // =========================================================
    // RENDERER HELPERS
    // =========================================================

    private bool ContainsRenderer(
        Transform root)
    {
        return root.GetComponentInChildren<Renderer>(
            true
        ) != null;
    }

    private Vector3 GetRendererCenterLocal(
        Transform root)
    {
        Renderer[] renderers =
            root.GetComponentsInChildren<Renderer>(
                true
            );

        if (renderers.Length == 0)
            return root.localPosition;

        Bounds bounds =
            renderers[0].bounds;

        for (int i = 1;
             i < renderers.Length;
             i++)
        {
            bounds.Encapsulate(
                renderers[i].bounds
            );
        }

        return watchContainer.InverseTransformPoint(
            bounds.center
        );
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!initialized)
            return;

        HandleTouchInput();
        HandleMouseInput();

        // Smooth rotation
        interactionRoot.localRotation =
            Quaternion.Slerp(
                interactionRoot.localRotation,
                targetRootRotation,
                Time.deltaTime *
                interactionSmoothness
            );

        // Smooth zoom
        Vector3 desiredScale =
            originalRootScale *
            targetScale;

        interactionRoot.localScale =
            Vector3.Lerp(
                interactionRoot.localScale,
                desiredScale,
                Time.deltaTime *
                interactionSmoothness
            );
    }

    // =========================================================
    // INSIDE
    // =========================================================

    public void OpenWatch()
    {
        if (animationRunning)
            return;

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        exploded = true;

        animationCoroutine =
            StartCoroutine(
                AnimateWatch(true)
            );
    }

    // =========================================================
    // 3D VIEW
    // =========================================================

    public void CloseWatch()
    {
        if (animationRunning)
            return;

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        exploded = false;

        animationCoroutine =
            StartCoroutine(
                AnimateWatch(false)
            );
    }

    // =========================================================
    // ANIMATION
    // =========================================================

    private IEnumerator AnimateWatch(
        bool open)
    {
        animationRunning = true;

        Vector3 axis =
            explosionAxis.normalized;

        float totalTime =
            animationDuration +
            automaticLayers *
            layerDelay;

        float elapsed = 0f;

        while (elapsed < totalTime)
        {
            elapsed += Time.deltaTime;

            foreach (AutoPart part in autoParts)
            {
                if (part.transform == null)
                    continue;

                // Strap stays exactly where it is
                if (part.isStrap)
                {
                    part.transform.localPosition =
                        part.originalPosition;

                    part.transform.localRotation =
                        part.originalRotation;

                    continue;
                }

                float delay =
                    open
                        ? part.layer * layerDelay
                        : (automaticLayers -
                           1 -
                           part.layer) *
                          layerDelay;

                float progress =
                    Mathf.Clamp01(
                        (elapsed - delay) /
                        animationDuration
                    );

                float eased =
                    EaseInOutCubic(progress);

                Vector3 explodedPosition =
                    part.originalPosition +
                    axis *
                    (part.layer *
                     layerSpacing);

                if (open)
                {
                    part.transform.localPosition =
                        Vector3.Lerp(
                            part.originalPosition,
                            explodedPosition,
                            eased
                        );
                }
                else
                {
                    part.transform.localPosition =
                        Vector3.Lerp(
                            explodedPosition,
                            part.originalPosition,
                            eased
                        );
                }

                part.transform.localRotation =
                    part.originalRotation;
            }

            yield return null;
        }

        // Exact final positions
        foreach (AutoPart part in autoParts)
        {
            if (part.transform == null)
                continue;

            if (part.isStrap)
            {
                part.transform.localPosition =
                    part.originalPosition;

                part.transform.localRotation =
                    part.originalRotation;

                continue;
            }

            Vector3 finalPosition =
                part.originalPosition;

            if (exploded)
            {
                finalPosition +=
                    axis *
                    (part.layer *
                     layerSpacing);
            }

            part.transform.localPosition =
                finalPosition;

            part.transform.localRotation =
                part.originalRotation;
        }

        animationRunning = false;
        animationCoroutine = null;
    }

    // =========================================================
    // TOUCH
    // =========================================================

    private void HandleTouchInput()
    {
        if (Touchscreen.current == null)
            return;

        if (animationRunning)
            return;

        List<TouchControl> touches = new();

        foreach (TouchControl touch
                 in Touchscreen.current.touches)
        {
            if (touch.press.isPressed)
                touches.Add(touch);
        }

        // ONE FINGER = ROTATE
        if (touches.Count == 1)
        {
            TouchControl touch =
                touches[0];

            int fingerId =
                touch.touchId.ReadValue();

            if (IsTouchOverUI(fingerId))
                return;

            Vector2 delta =
                touch.delta.ReadValue();

            if (delta.sqrMagnitude > 0.01f)
            {
                RotateWatch(
                    delta.x *
                    rotationSpeed,

                    -delta.y *
                    rotationSpeed
                );
            }
        }

        // TWO FINGERS = ZOOM
        if (touches.Count >= 2)
        {
            TouchControl first =
                touches[0];

            TouchControl second =
                touches[1];

            Vector2 firstCurrent =
                first.position.ReadValue();

            Vector2 secondCurrent =
                second.position.ReadValue();

            Vector2 firstPrevious =
                firstCurrent -
                first.delta.ReadValue();

            Vector2 secondPrevious =
                secondCurrent -
                second.delta.ReadValue();

            float previousDistance =
                Vector2.Distance(
                    firstPrevious,
                    secondPrevious
                );

            float currentDistance =
                Vector2.Distance(
                    firstCurrent,
                    secondCurrent
                );

            float difference =
                currentDistance -
                previousDistance;

            targetScale +=
                difference *
                pinchSensitivity;

            targetScale =
                Mathf.Clamp(
                    targetScale,
                    minimumScale,
                    maximumScale
                );
        }
    }

    // =========================================================
    // MOUSE
    // =========================================================

    private void HandleMouseInput()
    {
        if (Mouse.current == null)
            return;

        if (animationRunning)
            return;

        // LEFT DRAG = ROTATE
        if (Mouse.current.leftButton.isPressed)
        {
            if (EventSystem.current != null &&
                EventSystem.current
                    .IsPointerOverGameObject())
            {
                return;
            }

            Vector2 delta =
                Mouse.current.delta.ReadValue();

            RotateWatch(
                delta.x * 0.5f,
                -delta.y * 0.5f
            );
        }

        // WHEEL = ZOOM
        float scroll =
            Mouse.current
                .scroll
                .ReadValue()
                .y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetScale +=
                scroll *
                mouseZoomSensitivity *
                0.01f;

            targetScale =
                Mathf.Clamp(
                    targetScale,
                    minimumScale,
                    maximumScale
                );
        }
    }

    // =========================================================
    // ROTATION
    // =========================================================

    private void RotateWatch(
        float yaw,
        float pitch)
    {
        Vector3 current =
            targetRootRotation.eulerAngles;

        float currentPitch =
            current.x;

        if (currentPitch > 180f)
            currentPitch -= 360f;

        currentPitch += pitch;

        currentPitch =
            Mathf.Clamp(
                currentPitch,
                minimumPitch,
                maximumPitch
            );

        targetRootRotation =
            Quaternion.Euler(
                currentPitch,
                current.y + yaw,
                0f
            );
    }

    // =========================================================
    // RESET
    // =========================================================

    public void ResetInteraction()
    {
        targetRootRotation =
            originalRootRotation;

        targetScale = 1f;
    }

    // =========================================================
    // UI HELPER
    // =========================================================

    private bool IsTouchOverUI(
        int fingerId)
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current
            .IsPointerOverGameObject(
                fingerId
            );
    }

    // =========================================================
    // EASING
    // =========================================================

    private float EaseInOutCubic(
        float t)
    {
        if (t < 0.5f)
            return 4f * t * t * t;

        return 1f -
               Mathf.Pow(
                   -2f * t + 2f,
                   3f
               ) /
               2f;
    }
}