using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vuforia;
using Image = UnityEngine.UI.Image;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// AURON AR UI. INSIDE mode is only available while the matching AURON image
/// target is directly TRACKED by Vuforia (not merely EXTENDED_TRACKED). If tracking is lost while
/// Inside is open, the watch is reset and Inside mode exits after a short grace.
/// The ARCamera is never disabled by this script.
/// </summary>
public class AURONUI : MonoBehaviour
{
    [Header("WATCH CONTROLLER")]
    [SerializeField] private AuronWatchExperience AuronWatchExperience;

    [Header("TRY ON CONTROLLER")]
    [SerializeField] private AuronTryOnController AuronTryOnController;

    private Canvas canvas;
    private RectTransform canvasRoot;
    private GameObject homeScreen;
    private GameObject arScreen;
    private CanvasGroup homeGroup;
    private CanvasGroup arGroup;

    private TextMeshProUGUI statusText;
    private TextMeshProUGUI currentModeText;
    private Button threeDButton;
    private Button insideButton;
    private Button tryOnButton;
    private Button resetButton;

    private bool insideActive;
    private float insideTrackingLostSince = -1f;
    private const float InsideTrackingLostGraceSeconds = 0.25f;

    // Cache the assembled watch render state before opening Inside. If Vuforia
    // loses the image, reset and hide the stale model so it cannot float at the
    // last tracked pose. Restore these renderers when the image is tracked again.
    private Renderer[] cachedWatchRenderers;
    private bool[] cachedWatchRendererStates;
    private bool watchRenderersHiddenByTrackingLoss;

    private static readonly Color Black = new Color32(5, 6, 7, 255);
    private static readonly Color Gold = new Color32(205, 170, 108, 255);
    private static readonly Color GoldLight = new Color32(235, 207, 158, 255);
    private static readonly Color Ivory = new Color32(247, 243, 235, 255);
    private static readonly Color Muted = new Color32(177, 173, 165, 255);
    private static readonly Color Glass = new Color(0.02f, 0.025f, 0.03f, 0.88f);
    private static readonly Color SoftGlass = new Color(1f, 1f, 1f, 0.09f);
    private static readonly Color ThreeDSelected = new Color32(205, 170, 108, 255);
    private static readonly Color ReassembleSelected = new Color32(228, 190, 118, 255);
    private static readonly Color TryOnSelected = new Color32(190, 155, 96, 255);
    private static Sprite roundedSprite;

    private void Awake()
    {
        FindControllers();
        EnsureEventSystem();
        CreateCanvas();
        BuildHomeScreen();
        BuildARScreen();
        ShowHomeImmediately();
    }

    private void Update()
    {
        bool tracked = IsAuronWatchImageTracked();

        bool isTryingOn = AuronTryOnController != null && AuronTryOnController.IsTryOnActive;
        if (tracked && watchRenderersHiddenByTrackingLoss && !isTryingOn)
            RestoreWatchRenderersAfterTrackingReturns();

        // The button is disabled if the matching image is not currently tracked.
        // Keep it enabled while already inside, so the tracking-loss grace period
        // can complete without UI-state flicker.
        if (insideButton != null)
            insideButton.interactable = tracked || insideActive;

        if (!insideActive || arScreen == null || !arScreen.activeInHierarchy)
        {
            insideTrackingLostSince = -1f;

            // Restore the normal hint when the target comes back into view.
            if (tracked && currentModeText != null && currentModeText.text == "SCAN WATCH IMAGE")
            {
                currentModeText.text = "3D VIEW";
                if (statusText != null)
                {
                    statusText.text = "●  AR READY";
                    statusText.color = GoldLight;
                }
            }
            return;
        }

        if (tracked)
        {
            insideTrackingLostSince = -1f;
            return;
        }

        if (insideTrackingLostSince < 0f)
            insideTrackingLostSince = Time.unscaledTime;

        if (Time.unscaledTime - insideTrackingLostSince >= InsideTrackingLostGraceSeconds)
            ExitInsideBecauseTrackingWasLost();
    }

    private void FindControllers()
    {
        if (AuronWatchExperience == null)
            AuronWatchExperience = FindFirstObjectByType<AuronWatchExperience>();

        if (AuronTryOnController == null)
            AuronTryOnController = FindFirstObjectByType<AuronTryOnController>();
    }

    /// <summary>
    /// True only if the image target associated with the current watch experience
    /// is actually tracked by Vuforia. Does not use hand detection for this check.
    /// </summary>
    private bool IsAuronWatchImageTracked()
    {
        ImageTargetBehaviour[] targets = FindObjectsByType<ImageTargetBehaviour>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        for (int i = 0; i < targets.Length; i++)
        {
            ImageTargetBehaviour target = targets[i];
            if (target == null || !TargetBelongsToThisWatch(target))
                continue;

            try
            {
                Status targetStatus = target.TargetStatus.Status;
                // Strictly require the printed image to be seen now. EXTENDED_TRACKED
                // can preserve an old pose after the marker leaves the camera view,
                // which is specifically not sufficient for Inside mode.
                if (targetStatus == Status.TRACKED)
                    return true;
            }
            catch
            {
                // If Vuforia status is temporarily unavailable, treat it as untracked.
            }
        }

        return false;
    }

    private bool TargetBelongsToThisWatch(ImageTargetBehaviour target)
    {
        if (target == null)
            return false;

        if (AuronWatchExperience != null)
        {
            AuronWatchExperience experienceOnTarget =
                target.GetComponentInChildren<AuronWatchExperience>(true);

            if (experienceOnTarget == AuronWatchExperience)
                return true;

            Transform watchTransform = AuronWatchExperience.transform;
            if (watchTransform == target.transform || watchTransform.IsChildOf(target.transform))
                return true;

            return false;
        }

        // Fallback if the watch controller reference is not assigned/found.
        string targetName = target.TargetName;
        return !string.IsNullOrEmpty(targetName) &&
               targetName.ToUpperInvariant().Contains("AURON");
    }

    private void ExitInsideBecauseTrackingWasLost()
    {
        insideTrackingLostSince = -1f;
        if (!insideActive)
            return;

        insideActive = false;

        if (AuronWatchExperience != null)
            AuronWatchExperience.ResetView();

        HideWatchRenderersUntilTrackingReturns();

        SetButtonText(insideButton, "INSIDE");
        if (currentModeText != null)
            currentModeText.text = "SCAN WATCH IMAGE";
        if (statusText != null)
        {
            statusText.text = "●  TARGET NOT TRACKED";
            statusText.color = GoldLight;
        }

        SetMode(threeDButton, true);
        SetMode(insideButton, false);
        SetMode(tryOnButton, false);
        if (insideButton != null)
            insideButton.interactable = false;

        Debug.LogWarning("[AURON UI] Inside mode exited because the AURON image target is no longer tracked.");
    }

    private void CaptureWatchRendererStates()
    {
        if (AuronWatchExperience == null)
            return;

        cachedWatchRenderers = AuronWatchExperience.GetComponentsInChildren<Renderer>(true);
        cachedWatchRendererStates = new bool[cachedWatchRenderers.Length];
        for (int i = 0; i < cachedWatchRenderers.Length; i++)
        {
            Renderer renderer = cachedWatchRenderers[i];
            cachedWatchRendererStates[i] = renderer != null && renderer.enabled;
        }
    }

    private void HideWatchRenderersUntilTrackingReturns()
    {
        if (AuronWatchExperience == null)
            return;

        // If there was no successful Inside entry to cache state from, capture
        // the current reset state before hiding.
        if (cachedWatchRenderers == null || cachedWatchRendererStates == null)
            CaptureWatchRendererStates();

        if (cachedWatchRenderers != null)
        {
            for (int i = 0; i < cachedWatchRenderers.Length; i++)
            {
                if (cachedWatchRenderers[i] != null)
                    cachedWatchRenderers[i].enabled = false;
            }
        }

        watchRenderersHiddenByTrackingLoss = true;
        Debug.LogWarning("[AURON UI] Watch renderers hidden until the image target is directly TRACKED again.");
    }

    private void RestoreWatchRenderersAfterTrackingReturns()
    {
        if (cachedWatchRenderers != null && cachedWatchRendererStates != null)
        {
            int count = Mathf.Min(cachedWatchRenderers.Length, cachedWatchRendererStates.Length);
            for (int i = 0; i < count; i++)
            {
                if (cachedWatchRenderers[i] != null)
                    cachedWatchRenderers[i].enabled = cachedWatchRendererStates[i];
            }
        }

        watchRenderersHiddenByTrackingLoss = false;
        cachedWatchRenderers = null;
        cachedWatchRendererStates = null;
        Debug.Log("[AURON UI] Image target tracked again; watch renderers restored.");
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null)
            return;

        GameObject eventSystem = new GameObject("AURON_EVENT_SYSTEM");
        eventSystem.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        eventSystem.AddComponent<InputSystemUIInputModule>();
#else
        eventSystem.AddComponent<StandaloneInputModule>();
#endif
    }

    private void CreateCanvas()
    {
        GameObject canvasObject = new GameObject(
            "AURON_CANVAS", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));

        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject rootObject = new GameObject("UI_ROOT", typeof(RectTransform));
        canvasRoot = rootObject.GetComponent<RectTransform>();
        canvasRoot.SetParent(canvas.transform, false);
        Stretch(canvasRoot);
    }

    private void BuildHomeScreen()
    {
        homeScreen = CreateScreen("HOME");
        homeGroup = homeScreen.AddComponent<CanvasGroup>();
        RectTransform root = homeScreen.GetComponent<RectTransform>();
        Stretch(root);

        Image background = CreateImage(root, Black);
        Stretch(background.rectTransform);

        TextMeshProUGUI logo = CreateText(root, "AURON", 55f, Ivory, TextAlignmentOptions.Center);
        logo.fontStyle = FontStyles.Bold;
        logo.characterSpacing = 11f;
        SetAnchors(logo.rectTransform, 0.05f, 0.535f, 0.95f, 0.625f);

        Image line = CreateImage(root, Gold);
        SetAnchors(line.rectTransform, 0.34f, 0.505f, 0.66f, 0.510f);

        Button exploreButton = CreatePrimaryButton(root, "EXPLORE AR WATCH");
        SetAnchors(exploreButton.GetComponent<RectTransform>(), 0.26f, 0.395f, 0.74f, 0.455f);
        exploreButton.onClick.AddListener(OpenARExperience);
    }

    private void BuildARScreen()
    {
        arScreen = CreateScreen("AR_EXPERIENCE");
        arGroup = arScreen.AddComponent<CanvasGroup>();
        RectTransform root = arScreen.GetComponent<RectTransform>();
        Stretch(root);

        Image topBar = CreateImage(root, Glass);
        topBar.sprite = RoundedSprite();
        topBar.type = Image.Type.Sliced;
        SetAnchors(topBar.rectTransform, 0.025f, 0.905f, 0.975f, 0.98f);

        Button backButton = CreateIconButton(root, "‹");
        SetAnchors(backButton.GetComponent<RectTransform>(), 0.045f, 0.918f, 0.145f, 0.965f);
        backButton.onClick.AddListener(CloseARExperience);

        TextMeshProUGUI logo = CreateText(root, "AURON", 30f, Ivory, TextAlignmentOptions.Left);
        logo.fontStyle = FontStyles.Bold;
        logo.characterSpacing = 4.5f;
        SetAnchors(logo.rectTransform, 0.17f, 0.925f, 0.52f, 0.965f);

        statusText = CreateText(root, "●  AR READY", 17f, GoldLight, TextAlignmentOptions.Right);
        statusText.fontStyle = FontStyles.Bold;
        statusText.characterSpacing = 1.3f;
        SetAnchors(statusText.rectTransform, 0.58f, 0.925f, 0.955f, 0.965f);

        resetButton = CreateResetButton(root);
        SetAnchors(resetButton.GetComponent<RectTransform>(), 0.82f, 0.245f, 0.965f, 0.305f);
        resetButton.onClick.AddListener(ResetWatch);

        Image dock = CreateImage(root, new Color(0.015f, 0.020f, 0.025f, 0.95f));
        dock.sprite = RoundedSprite();
        dock.type = Image.Type.Sliced;
        RectTransform dockRect = dock.rectTransform;
        SetAnchors(dockRect, 0.02f, 0.025f, 0.98f, 0.22f);

        threeDButton = CreateModeButton(dockRect, "3D VIEW", true);
        SetAnchors(threeDButton.GetComponent<RectTransform>(), 0.025f, 0.52f, 0.315f, 0.90f);
        threeDButton.onClick.AddListener(Show3D);

        insideButton = CreateModeButton(dockRect, "INSIDE", false);
        SetAnchors(insideButton.GetComponent<RectTransform>(), 0.345f, 0.52f, 0.655f, 0.90f);
        insideButton.onClick.AddListener(ShowInsideOrReassemble);

        tryOnButton = CreateModeButton(dockRect, "TRY ON", false);
        SetAnchors(tryOnButton.GetComponent<RectTransform>(), 0.685f, 0.52f, 0.975f, 0.90f);
        tryOnButton.onClick.AddListener(ShowTryOn);

        currentModeText = CreateText(dockRect, "3D VIEW", 20f, Muted, TextAlignmentOptions.Center);
        currentModeText.fontStyle = FontStyles.Bold;
        currentModeText.characterSpacing = 1.3f;
        currentModeText.horizontalAlignment = HorizontalAlignmentOptions.Center;
        currentModeText.verticalAlignment = VerticalAlignmentOptions.Middle;
        currentModeText.textWrappingMode = TextWrappingModes.NoWrap;
        SetAnchors(currentModeText.rectTransform, 0.25f, 0.10f, 0.75f, 0.40f);
    }

    private void OpenARExperience()
    {
        Debug.Log("AURON: Opening AR Experience");
        if (AuronTryOnController != null)
            AuronTryOnController.StopTryOn();
        if (AuronWatchExperience != null)
            AuronWatchExperience.ResetView();

        insideActive = false;
        insideTrackingLostSince = -1f;
        homeScreen.SetActive(false);
        homeGroup.alpha = 0f;
        homeGroup.interactable = false;
        homeGroup.blocksRaycasts = false;
        arScreen.SetActive(true);
        arGroup.alpha = 1f;
        arGroup.interactable = true;
        arGroup.blocksRaycasts = true;
        Show3D();
    }

    private void CloseARExperience()
    {
        Debug.Log("AURON: Returning Home");
        if (AuronTryOnController != null)
            AuronTryOnController.StopTryOn();
        if (AuronWatchExperience != null)
            AuronWatchExperience.ResetView();

        insideActive = false;
        insideTrackingLostSince = -1f;
        SetButtonText(insideButton, "INSIDE");
        arScreen.SetActive(false);
        arGroup.alpha = 0f;
        arGroup.interactable = false;
        arGroup.blocksRaycasts = false;
        homeScreen.SetActive(true);
        homeGroup.alpha = 1f;
        homeGroup.interactable = true;
        homeGroup.blocksRaycasts = true;
    }

    private void ShowHomeImmediately()
    {
        homeScreen.SetActive(true);
        arScreen.SetActive(false);
        homeGroup.alpha = 1f;
        arGroup.alpha = 0f;
        homeGroup.interactable = true;
        homeGroup.blocksRaycasts = true;
        arGroup.interactable = false;
        arGroup.blocksRaycasts = false;
        insideActive = false;
        insideTrackingLostSince = -1f;
        SetButtonText(insideButton, "INSIDE");

        if (AuronTryOnController != null)
            AuronTryOnController.StopTryOn();
        if (AuronWatchExperience != null)
            AuronWatchExperience.ResetView();
    }

    private void Show3D()
    {
        Debug.Log("AURON: 3D View");
        if (AuronTryOnController != null)
            AuronTryOnController.StopTryOn();

        insideActive = false;
        insideTrackingLostSince = -1f;
        SetButtonText(insideButton, "INSIDE");
        currentModeText.text = "3D VIEW";
        statusText.text = "●  AR READY";
        statusText.color = GoldLight;
        SetMode(threeDButton, true);
        SetMode(insideButton, false);
        SetMode(tryOnButton, false);

        if (AuronWatchExperience != null)
            AuronWatchExperience.Show3DView();
    }

    private void ShowInsideOrReassemble()
    {
        // Gate both entering Inside and reassembling on current image tracking.
        if (!IsAuronWatchImageTracked())
        {
            if (insideActive)
                ExitInsideBecauseTrackingWasLost();
            else
            {
                if (currentModeText != null)
                    currentModeText.text = "SCAN WATCH IMAGE";
                if (statusText != null)
                {
                    statusText.text = "●  TARGET NOT TRACKED";
                    statusText.color = GoldLight;
                }
                if (insideButton != null)
                    insideButton.interactable = false;
                Debug.LogWarning("[AURON UI] Cannot enter Inside mode until the AURON image target is tracked.");
            }
            return;
        }

        if (insideActive)
        {
            ReassembleWatch();
            return;
        }

        if (AuronTryOnController != null)
            AuronTryOnController.StopTryOn();

        CaptureWatchRendererStates();

        insideActive = true;
        insideTrackingLostSince = -1f;
        Debug.Log("AURON: Inside Mode (image tracked)");
        currentModeText.text = "INNER MECHANISM";
        statusText.text = "●  EXPLORING";
        statusText.color = GoldLight;
        SetButtonText(insideButton, "REASSEMBLE");
        SetMode(threeDButton, false);
        SetMode(insideButton, true);
        SetMode(tryOnButton, false);
        if (insideButton != null)
            insideButton.interactable = true;

        if (AuronWatchExperience != null)
            AuronWatchExperience.ShowInside();
    }

    private void ReassembleWatch()
    {
        // If image tracking was lost, the gate above will prevent reassembling
        // against a stale target transform.
        Debug.Log("AURON: Reassembling Watch");
        insideActive = false;
        insideTrackingLostSince = -1f;
        SetButtonText(insideButton, "INSIDE");
        currentModeText.text = "3D VIEW";
        statusText.text = "●  AR READY";
        statusText.color = GoldLight;
        SetMode(threeDButton, true);
        SetMode(insideButton, false);
        SetMode(tryOnButton, false);

        if (AuronWatchExperience != null)
            AuronWatchExperience.Reassemble();
    }

    private void ResetWatch()
    {
        Debug.Log("AURON: Reset Watch");
        if (AuronTryOnController != null)
            AuronTryOnController.StopTryOn();
        if (AuronWatchExperience != null)
            AuronWatchExperience.ResetView();

        insideActive = false;
        insideTrackingLostSince = -1f;
        SetButtonText(insideButton, "INSIDE");
        currentModeText.text = "3D VIEW";
        statusText.text = "●  AR READY";
        statusText.color = GoldLight;
        SetMode(threeDButton, true);
        SetMode(insideButton, false);
        SetMode(tryOnButton, false);
    }

    private void ShowTryOn()
    {
        Debug.Log("AURON: TRY ON selected");
        insideActive = false;
        insideTrackingLostSince = -1f;
        SetButtonText(insideButton, "INSIDE");
        currentModeText.text = "TRY ON YOUR WRIST";
        statusText.text = "●  WRIST TRACKING";
        statusText.color = GoldLight;
        SetMode(threeDButton, false);
        SetMode(insideButton, false);
        SetMode(tryOnButton, true);

        if (AuronTryOnController != null)
        {
            bool started = AuronTryOnController.StartTryOn();
            if (!started)
            {
                currentModeText.text = "SCAN WATCH IMAGE";
                statusText.text = "●  TARGET NOT TRACKED";
                SetMode(tryOnButton, false);
                SetMode(threeDButton, true);
            }
        }
        else
        {
            Debug.LogError("[AURON UI] AuronTryOnController was not found.");
        }
    }

    private void SetMode(Button button, bool selected)
    {
        if (button == null)
            return;

        Image image = button.GetComponent<Image>();
        TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
        if (image == null || label == null)
            return;

        Color backgroundColor;
        Color textColor;

        if (!selected)
        {
            backgroundColor = SoftGlass;
            textColor = Ivory;
        }
        else if (button == insideButton && insideActive)
        {
            backgroundColor = ReassembleSelected;
            textColor = Black;
        }
        else if (button == tryOnButton)
        {
            backgroundColor = TryOnSelected;
            textColor = Black;
        }
        else
        {
            backgroundColor = ThreeDSelected;
            textColor = Black;
        }

        image.color = backgroundColor;
        label.color = textColor;

        ColorBlock colors = button.colors;
        colors.normalColor = backgroundColor;
        colors.highlightedColor = Color.Lerp(backgroundColor, Color.white, 0.08f);
        colors.pressedColor = Color.Lerp(backgroundColor, Black, 0.12f);
        colors.selectedColor = backgroundColor;
        colors.disabledColor = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0.35f);
        colors.fadeDuration = 0.10f;
        colors.colorMultiplier = 1f;
        button.colors = colors;
    }

    private void SetButtonText(Button button, string value)
    {
        if (button == null)
            return;

        TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
        {
            label.text = value;
            label.alignment = TextAlignmentOptions.Center;
            label.verticalAlignment = VerticalAlignmentOptions.Middle;
            label.horizontalAlignment = HorizontalAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
        }
    }

    private Button CreatePrimaryButton(RectTransform parent, string label)
    {
        GameObject obj = new GameObject("EXPLORE_AR_BUTTON", typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        Image image = obj.GetComponent<Image>();
        image.sprite = RoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = GoldLight;
        Button button = obj.GetComponent<Button>();
        ConfigureButton(button, GoldLight);

        TextMeshProUGUI text = CreateText(rect, label, 30f, Black, TextAlignmentOptions.Center);
        text.fontStyle = FontStyles.Bold;
        text.characterSpacing = 1.8f;
        Stretch(text.rectTransform);
        text.verticalAlignment = VerticalAlignmentOptions.Middle;
        return button;
    }

    private Button CreateModeButton(RectTransform parent, string label, bool selected)
    {
        GameObject obj = new GameObject("MODE_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        Image image = obj.GetComponent<Image>();
        image.sprite = RoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = selected ? ThreeDSelected : SoftGlass;
        Button button = obj.GetComponent<Button>();
        ConfigureButton(button, image.color);

        TextMeshProUGUI text = CreateText(rect, label, 23f, selected ? Black : Ivory, TextAlignmentOptions.Center);
        text.fontStyle = FontStyles.Bold;
        text.characterSpacing = 1.4f;
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        text.horizontalAlignment = HorizontalAlignmentOptions.Center;
        text.verticalAlignment = VerticalAlignmentOptions.Middle;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.margin = Vector4.zero;
        return button;
    }

    private Button CreateResetButton(RectTransform parent)
    {
        GameObject obj = new GameObject("RESET_BUTTON", typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        Image image = obj.GetComponent<Image>();
        image.sprite = RoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = Glass;
        Button button = obj.GetComponent<Button>();
        ConfigureButton(button, Glass);

        TextMeshProUGUI text = CreateText(rect, "RESET", 17f, GoldLight, TextAlignmentOptions.Center);
        text.fontStyle = FontStyles.Bold;
        text.characterSpacing = 1.2f;
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;
        return button;
    }

    private Button CreateIconButton(RectTransform parent, string icon)
    {
        GameObject obj = new GameObject("BACK_BUTTON", typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        Image image = obj.GetComponent<Image>();
        image.sprite = RoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = SoftGlass;
        Button button = obj.GetComponent<Button>();
        ConfigureButton(button, SoftGlass);
        TextMeshProUGUI text = CreateText(rect, icon, 29f, Ivory, TextAlignmentOptions.Center);
        Stretch(text.rectTransform);
        text.verticalAlignment = VerticalAlignmentOptions.Middle;
        return button;
    }

    private TextMeshProUGUI CreateText(RectTransform parent, string value, float fontSize, Color color, TextAlignmentOptions alignment)
    {
        GameObject obj = new GameObject("TEXT", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.horizontalAlignment = HorizontalAlignmentOptions.Center;
        text.verticalAlignment = VerticalAlignmentOptions.Middle;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        text.margin = Vector4.zero;
        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;
        return text;
    }

    private Image CreateImage(RectTransform parent, Color color)
    {
        GameObject obj = new GameObject("IMAGE", typeof(RectTransform), typeof(Image));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        Image image = obj.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private GameObject CreateScreen(string screenName)
    {
        GameObject screen = new GameObject(screenName, typeof(RectTransform));
        RectTransform rect = screen.GetComponent<RectTransform>();
        rect.SetParent(canvasRoot, false);
        Stretch(rect);
        return screen;
    }

    private void ConfigureButton(Button button, Color baseColor)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = baseColor;
        colors.highlightedColor = Color.Lerp(baseColor, Color.white, 0.08f);
        colors.pressedColor = Color.Lerp(baseColor, Black, 0.12f);
        colors.selectedColor = baseColor;
        colors.disabledColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.35f);
        colors.fadeDuration = 0.10f;
        colors.colorMultiplier = 1f;
        button.colors = colors;
        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetAnchors(RectTransform rect, float minX, float minY, float maxX, float maxY)
    {
        rect.anchorMin = new Vector2(minX, minY);
        rect.anchorMax = new Vector2(maxX, maxY);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Sprite RoundedSprite()
    {
        if (roundedSprite != null)
            return roundedSprite;

        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        float radius = size * 0.22f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(radius - x, 0f, x - (size - 1 - radius));
                float dy = Mathf.Max(radius - y, 0f, y - (size - 1 - radius));
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = distance <= radius ? 1f : 0f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        roundedSprite = Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            size,
            0,
            SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius));
        return roundedSprite;
    }
}
