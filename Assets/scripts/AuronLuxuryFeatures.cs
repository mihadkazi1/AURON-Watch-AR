using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AuronLuxuryFeatures : MonoBehaviour
{
    [Header("Controllers (Auto-Found if Empty)")]
    [SerializeField] private AuronWatchExperience watchExperience;
    [SerializeField] private AuronTryOnController tryOnController;
    [SerializeField] private Camera arCamera;

    [Header("1. Mechanical Gear Movement")]
    [SerializeField] private bool animateGears = true;
    [SerializeField] private float baseGearSpeed = 18f;

    [Header("2. Super-LumiNova (Night Glow)")]
    [SerializeField] private Color lumeGlowColor = new Color(0.05f, 0.95f, 0.82f, 1f);
    [SerializeField, Range(0.5f, 4f)] private float lumeIntensity = 2.2f;

    [Header("3. Watch Audio (OFF by Default — Turns On When SFX Clicked)")]
    [SerializeField] private bool enableTickingSound = false;
    [SerializeField, Range(0.1f, 1f)] private float tickVolume = 0.85f;
    [SerializeField] private AudioClip customWatchClickSound;
    [SerializeField] private AudioClip customReassembleSound;

    [Header("4. Auto-Turntable Showcase Mode")]
    [SerializeField] private bool enableAutoTurntable = true;
    [Tooltip("Seconds of no touch before the watch smoothly rotates on its own.")]
    [SerializeField] private float idleSecondsBeforeTurntable = 4.5f;
    [SerializeField] private float turntableSpeed = 18f;

    private struct OriginalMaterialData
    {
        public Color color;
        public float metallic;
        public float smoothness;
        public bool hadEmission;
        public Color emissionColor;
    }

    private readonly Dictionary<Material, OriginalMaterialData> originalMaterials =
        new Dictionary<Material, OriginalMaterialData>();

    private readonly Dictionary<string, (string title, string spec)> partDescriptions =
        new Dictionary<string, (string title, string spec)>
        {
            { "Cube.003",  ("GEOMETRIC SKELETON DIAL", "Laser-cut Moroccan lattice • Sapphire anti-reflective") },
            { "Empty",     ("AURON MONOBLOC CASE", "42mm Grade-5 brushed titanium • 10 ATM water resistance") },
            { "Empty.001", ("PRIMARY MINUTE WHEEL", "Rhodium-plated brass • Diamond-beveled teeth") },
            { "Empty.002", ("ESCAPEMENT BRIDGE", "Hand-chamfered openworked bridge • 4N Gold finish") },
            { "Empty.003", ("TOURBILLON CAGE UPPER", "60-second flying tourbillon • Ultra-light carriage") },
            { "Empty.004", ("BALANCE WHEEL", "28,800 vph (4 Hz) • Variable-inertia glucydur balance") },
            { "Empty.005", ("ESCAPE WHEEL & PALLET", "Silicon anti-magnetic escapement • Low-friction impulse") },
            { "Empty.006", ("FOURTH WHEEL TRAIN", "Coaxial seconds drive gear • Circular satin finish") },
            { "Empty.007", ("THIRD WHEEL", "High-efficiency torque transmission • 72h power reserve") },
            { "Empty.008", ("CENTER WHEEL", "Main timekeeping arbor • Ruby jewel bearings") },
            { "Empty.009", ("MAINSPRING BARREL", "Twin-barrel nivaflex spring • 72-hour autonomy") },
            { "Empty.010", ("KEYLESS WORKS & YOKE", "Precision winding & time-setting clutch mechanism") },
            { "Empty.011", ("AUTOMATIC ROTOR GEAR", "Bi-directional pawl-winding reduction gear") },
            { "Empty.012", ("SAPPHIRE EXHIBITION CASEBACK", "Engraved serial • Anti-glare inner coating") },
            { "Empty.013", ("SCREW-DOWN CROWN", "Fluted grip with embossed AURON emblem") },
            { "Empty.014", ("INTEGRATED LINK BRACELET", "Articulated ergonomic links • Butterfly folding clasp") }
        };

    private RectTransform arScreenRoot;
    private GameObject featureUIRoot;
    private GameObject infoBannerObj;
    private TextMeshProUGUI infoTitleText;
    private TextMeshProUGUI infoSpecText;

    private GameObject flashOverlayObj;
    private Image flashImage;

    private Button lumeButton;
    private Button sfxButton;
    private Button snapButton;
    private Button windButton;

    // Crown Winding State
    private bool isCrownPulledOut;
    private float windingSpeedBoost = 0f;
    private Vector2 lastWindPointerPos;
    private bool isDraggingWind;
    private float windAccumulator;

    // 4-Size Wrist Case Selector (36MM | 40MM | 42MM | 46MM — Visible in Try-On Mode)
    private GameObject wristSizeBarObj;
    private readonly List<Button> sizeButtons = new List<Button>();
    private int selectedSizeIndex = 2; // Default 42MM (index 2)

    // Auto-Turntable State
    private float lastUserInteractionTime;
    private bool isTurntableActive;

    private bool isLumeActive;
    private int currentFinishIndex = 0;
    private float infoBannerHideTime;
    private bool wasInsideModeLastFrame;
    private Coroutine reassembleSoundCoroutine;

    private AudioSource audioSource;
    private AudioClip watchClickClipA;
    private AudioClip watchClickClipB;
    private AudioClip gearSnapClip;
    private AudioClip caseLockClip;
    private AudioClip ratchetWindClip;
    private AudioClip uiClickClip;
    private AudioClip shutterClip;
    private float nextTickTime;
    private bool tickToggle;

    private Light sceneLight;
    private float originalLightIntensity = 1f;

    private IEnumerator Start()
    {
        enableTickingSound = false;
        lastUserInteractionTime = Time.unscaledTime;

        yield return null;

        FindReferences();
        SetupWatchAudio();
        CacheOriginalWatchMaterials();
        BuildLuxuryOverlayUI();
    }

    private void FindReferences()
    {
        if (watchExperience == null)
            watchExperience = FindFirstObjectByType<AuronWatchExperience>(FindObjectsInactive.Include);
        if (tryOnController == null)
            tryOnController = FindFirstObjectByType<AuronTryOnController>(FindObjectsInactive.Include);
        if (arCamera == null)
            arCamera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();

        sceneLight = FindFirstObjectByType<Light>();
        if (sceneLight != null)
            originalLightIntensity = sceneLight.intensity;
    }

    private void EnsureAudioListener()
    {
        if (FindFirstObjectByType<AudioListener>() == null)
        {
            if (arCamera != null)
                arCamera.gameObject.AddComponent<AudioListener>();
            else
                gameObject.AddComponent<AudioListener>();
        }
        AudioListener.volume = 1f;
        AudioListener.pause = false;
    }

    private void Update()
    {
        bool isInside = watchExperience != null && watchExperience.IsInsideMode;
        bool isTryOn = tryOnController != null && tryOnController.IsTryOnActive;

        // Show the 4-size selector bar ONLY during Try-On mode
        if (wristSizeBarObj != null && wristSizeBarObj.activeSelf != isTryOn)
        {
            wristSizeBarObj.SetActive(isTryOn);
        }

        if ((isInside || isTryOn) && isCrownPulledOut)
        {
            SetCrownWindingMode(false);
        }

        if (windingSpeedBoost > 0f)
            windingSpeedBoost = Mathf.MoveTowards(windingSpeedBoost, 0f, Time.deltaTime * 8f);

        if (animateGears)
        {
            AnimateWatchGears(watchExperience != null ? watchExperience.transform : null);

            GameObject tryOnClone = GameObject.Find("AURON_TRY_ON_WATCH");
            if (tryOnClone != null && tryOnClone.activeInHierarchy)
            {
                AnimateWatchGears(tryOnClone.transform);
                ApplyCurrentFinishAndLumeToRoot(tryOnClone.transform);
            }
        }

        if (watchExperience != null)
        {
            if (wasInsideModeLastFrame && !isInside)
            {
                PlayReassembleSoundSequence();
            }
            wasInsideModeLastFrame = isInside;
        }

        HandleCrownWindingInput(isInside, isTryOn);
        HandleAutoTurntable(isInside, isTryOn);
        HandleTickingAudio();
        HandleInsidePartTapInspection();

        if (infoBannerObj != null && infoBannerObj.activeSelf && Time.unscaledTime > infoBannerHideTime)
        {
            infoBannerObj.SetActive(false);
        }
    }

    // =========================================================
    // CROWN-PULL WINDING & RATCHET MECHANISM
    // =========================================================

    public void ToggleCrownWindingMode()
    {
        bool isInside = watchExperience != null && watchExperience.IsInsideMode;
        bool isTryOn = tryOnController != null && tryOnController.IsTryOnActive;

        if (isInside || isTryOn)
        {
            ShowInfoBanner("SWITCH TO 3D VIEW TO WIND", "Crown winding is available in 3D View mode", 2.2f);
            return;
        }

        SetCrownWindingMode(!isCrownPulledOut);
    }

    private void SetCrownWindingMode(bool pulledOut)
    {
        isCrownPulledOut = pulledOut;
        EnsureAudioListener();

        if (watchExperience != null)
            watchExperience.SetCrownPulledOut(pulledOut);

        if (audioSource != null && gearSnapClip != null)
        {
            audioSource.pitch = pulledOut ? 1.15f : 0.90f;
            audioSource.PlayOneShot(gearSnapClip, 0.9f);
        }

        UpdateToolbarButtonState(windButton, isCrownPulledOut, new Color32(205, 170, 108, 255));

        if (pulledOut)
        {
            ShowInfoBanner("CROWN PULLED OUT — MANUAL WINDING", "Swipe Up/Down on screen to wind Calibre AU-01 mainspring", 3.5f);
        }
        else
        {
            ShowInfoBanner("CROWN LOCKED", "Screw-down crown sealed to 10 ATM", 2.0f);
        }
    }

    private void HandleCrownWindingInput(bool isInside, bool isTryOn)
    {
        bool pointerDown = false;
        bool pointerPressedThisFrame = false;
        Vector2 pointerPos = Vector2.zero;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            pointerDown = true;
            pointerPressedThisFrame = Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
            pointerPos = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            pointerDown = true;
            pointerPressedThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
            pointerPos = Mouse.current.position.ReadValue();
        }

        if (pointerDown)
        {
            // If Auto-Turntable was active and the user touches the screen, immediately restore normal size and pose!
            if (isTurntableActive && watchExperience != null)
            {
                isTurntableActive = false;
                watchExperience.ResetToNormalSizeAndPose();
            }
            lastUserInteractionTime = Time.unscaledTime;
        }

        if (pointerPressedThisFrame && !isInside && !isTryOn && watchExperience != null && arCamera != null)
        {
            Transform crownT = FindDeepChild(watchExperience.transform, "Empty.013");
            if (crownT != null)
            {
                Renderer r = crownT.GetComponentInChildren<Renderer>();
                Vector3 center = r != null ? r.bounds.center : crownT.position;
                Vector3 sp = arCamera.WorldToScreenPoint(center);
                if (sp.z > 0f && Vector2.Distance(pointerPos, new Vector2(sp.x, sp.y)) < 85f)
                {
                    SetCrownWindingMode(!isCrownPulledOut);
                    return;
                }
            }
        }

        if (!isCrownPulledOut || !pointerDown)
        {
            isDraggingWind = false;
            return;
        }

        if (!isDraggingWind)
        {
            isDraggingWind = true;
            lastWindPointerPos = pointerPos;
            return;
        }

        float deltaY = pointerPos.y - lastWindPointerPos.y;
        lastWindPointerPos = pointerPos;

        if (Mathf.Abs(deltaY) > 1.5f)
        {
            windingSpeedBoost = Mathf.Clamp(windingSpeedBoost + Mathf.Abs(deltaY) * 0.25f, 0f, 12f);
            windAccumulator += Mathf.Abs(deltaY);

            if (watchExperience != null)
            {
                Transform crownT = FindDeepChild(watchExperience.transform, "Empty.013");
                if (crownT != null)
                    crownT.Rotate(Vector3.right, deltaY * 1.8f, Space.Self);
            }

            if (windAccumulator >= 18f)
            {
                windAccumulator = 0f;
                if (audioSource != null && ratchetWindClip != null)
                {
                    audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.12f);
                    audioSource.PlayOneShot(ratchetWindClip, 0.75f);
                }
            }
        }
    }

    // =========================================================
    // 4-SIZE WRIST CASE SELECTOR (36MM | 40MM | 42MM | 46MM)
    // =========================================================

    public void SelectWristCaseSize(int index)
    {
        selectedSizeIndex = index;
        PlayUIClick();

        float[] mmSizes = { 36f, 40f, 42f, 46f };
        float chosenMM = mmSizes[Mathf.Clamp(index, 0, mmSizes.Length - 1)];

        if (tryOnController != null)
            tryOnController.SetCaseSizeMillimeters(chosenMM);

        for (int i = 0; i < sizeButtons.Count; i++)
        {
            UpdateToolbarButtonState(sizeButtons[i], i == index, new Color32(205, 170, 108, 255));
        }

        ShowInfoBanner($"CASE DIAMETER: {chosenMM:0}MM", "Calibrated live on your wrist", 2.0f);
    }

    // =========================================================
    // AUTO-TURNTABLE SHOWCASE MODE (RESETS TO NORMAL SIZE ON TOUCH)
    // =========================================================

    private void HandleAutoTurntable(bool isInside, bool isTryOn)
    {
        if (!enableAutoTurntable || isTryOn || isCrownPulledOut || watchExperience == null)
        {
            isTurntableActive = false;
            return;
        }

        if (Time.unscaledTime - lastUserInteractionTime < idleSecondsBeforeTurntable)
        {
            isTurntableActive = false;
            return;
        }

        isTurntableActive = true;
        Vector3 axis = arCamera != null ? arCamera.transform.up : Vector3.up;
        watchExperience.transform.Rotate(axis, turntableSpeed * Time.deltaTime, Space.World);
    }

    // =========================================================
    // 1. LIVE MECHANICAL GEAR ANIMATION
    // =========================================================

    private void AnimateWatchGears(Transform root)
    {
        if (root == null) return;

        float dt = Time.deltaTime;
        float speedFactor = 1f + windingSpeedBoost;
        Transform[] children = root.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < children.Length; i++)
        {
            Transform t = children[i];
            if (t == null) continue;

            string n = t.name;
            if (n.StartsWith("Empty.00") || n == "Empty.010" || n == "Empty.011")
            {
                int index = 1;
                if (int.TryParse(n.Substring(n.Length - 2), out int parsed))
                    index = parsed;

                float direction = (index % 2 == 0) ? 1f : -1f;
                float speedMultiplier = (index == 4) ? 3.5f : (1.2f / Mathf.Max(1f, index * 0.35f));
                t.Rotate(Vector3.forward, direction * baseGearSpeed * speedMultiplier * speedFactor * dt, Space.Self);
            }
        }
    }

    // =========================================================
    // 2. MATERIAL CUSTOMIZER & 3. SUPER-LUMINOVA LUME MODE
    // =========================================================

    private void CacheOriginalWatchMaterials()
    {
        if (watchExperience == null) return;

        Renderer[] renderers = watchExperience.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            if (r == null) continue;
            foreach (Material m in r.materials)
            {
                if (m == null || originalMaterials.ContainsKey(m)) continue;

                OriginalMaterialData data = new OriginalMaterialData
                {
                    color = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : (m.HasProperty("_Color") ? m.color : Color.white),
                    metallic = m.HasProperty("_Metallic") ? m.GetFloat("_Metallic") : 0.8f,
                    smoothness = m.HasProperty("_Smoothness") ? m.GetFloat("_Smoothness") : (m.HasProperty("_Glossiness") ? m.GetFloat("_Glossiness") : 0.7f),
                    hadEmission = m.IsKeywordEnabled("_EMISSION"),
                    emissionColor = m.HasProperty("_EmissionColor") ? m.GetColor("_EmissionColor") : Color.black
                };
                originalMaterials[m] = data;
            }
        }
    }

    public void SelectFinish(int finishIndex)
    {
        currentFinishIndex = finishIndex;
        PlayUIClick();

        if (watchExperience != null)
            ApplyCurrentFinishAndLumeToRoot(watchExperience.transform);

        GameObject tryOnClone = GameObject.Find("AURON_TRY_ON_WATCH");
        if (tryOnClone != null)
            ApplyCurrentFinishAndLumeToRoot(tryOnClone.transform);

        string[] finishNames = { "OBSIDIAN BLACK", "18K ROSE GOLD", "ROYAL GOLD", "PLATINUM SILVER" };
        ShowInfoBanner(finishNames[Mathf.Clamp(finishIndex, 0, finishNames.Length - 1)], "Applied to Case & Integrated Bracelet", 2.2f);
    }

    public void ToggleLumeMode()
    {
        isLumeActive = !isLumeActive;
        PlayUIClick();

        if (sceneLight != null)
            sceneLight.intensity = isLumeActive ? originalLightIntensity * 0.25f : originalLightIntensity;

        if (watchExperience != null)
            ApplyCurrentFinishAndLumeToRoot(watchExperience.transform);

        GameObject tryOnClone = GameObject.Find("AURON_TRY_ON_WATCH");
        if (tryOnClone != null)
            ApplyCurrentFinishAndLumeToRoot(tryOnClone.transform);

        UpdateToolbarButtonState(lumeButton, isLumeActive, new Color32(15, 225, 195, 255));
        ShowInfoBanner(
            isLumeActive ? "SUPER-LUMINOVA™ ACTIVE" : "DAYLIGHT OPTICS RESTORED",
            isLumeActive ? "Grade X1 luminescent pigment on skeleton dial & movement" : "Standard studio illumination",
            2.4f);
    }

    private void ApplyCurrentFinishAndLumeToRoot(Transform root)
    {
        if (root == null) return;

        Color targetCaseColor = new Color32(28, 30, 34, 255);
        float targetMetallic = 0.85f;
        float targetSmoothness = 0.75f;

        if (currentFinishIndex == 1)
        {
            targetCaseColor = new Color32(218, 155, 128, 255);
            targetMetallic = 0.92f;
            targetSmoothness = 0.85f;
        }
        else if (currentFinishIndex == 2)
        {
            targetCaseColor = new Color32(232, 192, 102, 255);
            targetMetallic = 0.94f;
            targetSmoothness = 0.86f;
        }
        else if (currentFinishIndex == 3)
        {
            targetCaseColor = new Color32(215, 220, 226, 255);
            targetMetallic = 0.95f;
            targetSmoothness = 0.88f;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            if (r == null) continue;
            string objName = r.transform.name;
            bool isCaseOrStrap = (objName == "Empty" || objName == "Empty.013" || objName == "Empty.014" ||
                                  (r.transform.parent != null && (r.transform.parent.name == "Empty" || r.transform.parent.name == "Empty.014")));
            bool isDialOrGear = (objName == "Cube.003" || objName == "Empty.003" || objName == "Empty.004");

            foreach (Material m in r.materials)
            {
                if (m == null) continue;

                if (isCaseOrStrap)
                {
                    if (currentFinishIndex == 0 && originalMaterials.TryGetValue(m, out OriginalMaterialData orig))
                    {
                        SetMaterialColor(m, orig.color);
                    }
                    else if (currentFinishIndex > 0)
                    {
                        SetMaterialColor(m, targetCaseColor);
                        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", targetMetallic);
                        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", targetSmoothness);
                        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", targetSmoothness);
                    }
                }

                if (isDialOrGear)
                {
                    if (isLumeActive)
                    {
                        m.EnableKeyword("_EMISSION");
                        Color hdrLume = lumeGlowColor * lumeIntensity;
                        if (m.HasProperty("_EmissionColor"))
                            m.SetColor("_EmissionColor", hdrLume);
                        SetMaterialColor(m, Color.Lerp(Color.white, lumeGlowColor, 0.65f));
                    }
                    else
                    {
                        if (originalMaterials.TryGetValue(m, out OriginalMaterialData orig))
                        {
                            SetMaterialColor(m, orig.color);
                            if (!orig.hadEmission) m.DisableKeyword("_EMISSION");
                            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", orig.emissionColor);
                        }
                        else
                        {
                            m.DisableKeyword("_EMISSION");
                            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", Color.black);
                        }
                    }
                }
            }
        }
    }

    private void SetMaterialColor(Material m, Color c)
    {
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color")) m.color = c;
    }

    // =========================================================
    // 4. INTERACTIVE PART CALLOUTS IN "INSIDE" MODE
    // =========================================================

    private void HandleInsidePartTapInspection()
    {
        if (watchExperience == null || !watchExperience.IsInsideMode || arCamera == null)
            return;

        bool tapped = false;
        Vector2 screenPos = Vector2.zero;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            tapped = true;
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            tapped = true;
            screenPos = Mouse.current.position.ReadValue();
        }

        if (!tapped) return;

        if (screenPos.y < UnityEngine.Screen.height * 0.30f || screenPos.y > UnityEngine.Screen.height * 0.85f)
            return;

        Transform bestPart = null;
        float bestScreenDist = 110f;

        foreach (var kvp in partDescriptions)
        {
            Transform part = FindDeepChild(watchExperience.transform, kvp.Key);
            if (part == null) continue;

            Renderer r = part.GetComponentInChildren<Renderer>();
            Vector3 worldCenter = r != null ? r.bounds.center : part.position;
            Vector3 sp = arCamera.WorldToScreenPoint(worldCenter);
            if (sp.z <= 0f) continue;

            float d = Vector2.Distance(screenPos, new Vector2(sp.x, sp.y));
            if (d < bestScreenDist)
            {
                bestScreenDist = d;
                bestPart = part;
            }
        }

        if (bestPart != null && partDescriptions.TryGetValue(bestPart.name, out var info))
        {
            PlayUIClick();
            ShowInfoBanner(info.title, info.spec, 4.0f);
            StartCoroutine(FlashPartHighlight(bestPart));
        }
    }

    private IEnumerator FlashPartHighlight(Transform part)
    {
        if (part == null) yield break;
        Renderer[] renderers = part.GetComponentsInChildren<Renderer>();
        Color highlightGold = new Color(1f, 0.82f, 0.35f, 1f) * 2.0f;

        foreach (Renderer r in renderers)
        {
            if (r == null) continue;
            foreach (Material m in r.materials)
            {
                m.EnableKeyword("_EMISSION");
                if (m.HasProperty("_EmissionColor"))
                    m.SetColor("_EmissionColor", highlightGold);
            }
        }

        yield return new WaitForSeconds(0.65f);

        if (watchExperience != null)
            ApplyCurrentFinishAndLumeToRoot(watchExperience.transform);
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        if (parent == null) return null;
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindDeepChild(parent.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }

    // =========================================================
    // 5. BRANDED LUXURY BOUTIQUE FRAME ON SAVED PHOTOS
    // =========================================================

    public void CaptureARSnapshot()
    {
        StartCoroutine(CaptureSnapshotCoroutine());
    }

    private IEnumerator CaptureSnapshotCoroutine()
    {
        EnsureAudioListener();
        if (audioSource != null && shutterClip != null)
            audioSource.PlayOneShot(shutterClip, 1f);

        Canvas mainCanvas = arScreenRoot != null ? arScreenRoot.GetComponentInParent<Canvas>() : null;

        if (mainCanvas != null)
            mainCanvas.enabled = false;

        yield return new WaitForEndOfFrame();

        Texture2D rawScreenshot = ScreenCapture.CaptureScreenshotAsTexture();

        if (mainCanvas != null)
            mainCanvas.enabled = true;

        ApplyLuxuryBoutiqueFrame(rawScreenshot);

        string filename = $"AURON_AR_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        SaveImageToGallery(rawScreenshot, filename);

        Destroy(rawScreenshot);

        if (flashOverlayObj != null && flashImage != null)
        {
            flashOverlayObj.SetActive(true);
            float t = 0f;
            while (t < 0.35f)
            {
                t += Time.unscaledDeltaTime;
                flashImage.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.85f, 0f, t / 0.35f));
                yield return null;
            }
            flashOverlayObj.SetActive(false);
        }

        ShowInfoBanner("BOUTIQUE EDITION PHOTO SAVED", $"Saved {filename} with AURON frame to Gallery", 3.5f);
    }

    private void ApplyLuxuryBoutiqueFrame(Texture2D tex)
    {
        if (tex == null) return;
        int w = tex.width;
        int h = tex.height;

        Color32 obsidian = new Color32(8, 10, 12, 255);
        Color32 gold = new Color32(205, 170, 108, 255);
        Color32[] pixels = tex.GetPixels32();

        int topBarH = Mathf.RoundToInt(h * 0.045f);
        int bottomBarH = Mathf.RoundToInt(h * 0.065f);
        int sideBorderW = Mathf.Max(6, Mathf.RoundToInt(w * 0.014f));
        int goldLineThickness = Mathf.Max(2, Mathf.RoundToInt(h * 0.0022f));

        for (int y = 0; y < h; y++)
        {
            bool inBottomBar = y < bottomBarH;
            bool inTopBar = y >= (h - topBarH);
            bool onBottomGoldLine = Mathf.Abs(y - bottomBarH) <= goldLineThickness;
            bool onTopGoldLine = Mathf.Abs(y - (h - topBarH)) <= goldLineThickness;

            for (int x = 0; x < w; x++)
            {
                bool inSideBorder = (x < sideBorderW || x >= w - sideBorderW);
                bool onLeftGoldLine = Mathf.Abs(x - sideBorderW) <= goldLineThickness;
                bool onRightGoldLine = Mathf.Abs(x - (w - sideBorderW)) <= goldLineThickness;

                if (onBottomGoldLine || onTopGoldLine || ((onLeftGoldLine || onRightGoldLine) && !inBottomBar && !inTopBar))
                {
                    pixels[y * w + x] = gold;
                }
                else if (inBottomBar || inTopBar || inSideBorder)
                {
                    pixels[y * w + x] = obsidian;
                }
            }
        }

        int centerX = w / 2;
        int centerY = bottomBarH / 2;
        int diamondRadius = Mathf.Max(8, Mathf.RoundToInt(bottomBarH * 0.22f));
        for (int dy = -diamondRadius; dy <= diamondRadius; dy++)
        {
            for (int dx = -diamondRadius; dx <= diamondRadius; dx++)
            {
                int manhattan = Mathf.Abs(dx) + Mathf.Abs(dy);
                if (manhattan == diamondRadius || manhattan == diamondRadius - 1 || manhattan <= 3)
                {
                    int px = Mathf.Clamp(centerX + dx, 0, w - 1);
                    int py = Mathf.Clamp(centerY + dy, 0, h - 1);
                    pixels[py * w + px] = gold;
                }
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
    }

    private void SaveImageToGallery(Texture2D tex, string filename)
    {
        byte[] pngData = tex.EncodeToPNG();

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject resolver = activity.Call<AndroidJavaObject>("getContentResolver"))
            using (AndroidJavaClass mediaStoreImages = new AndroidJavaClass("android.provider.MediaStore$Images$Media"))
            using (AndroidJavaObject values = new AndroidJavaObject("android.content.ContentValues"))
            {
                values.Call("put", "_display_name", filename);
                values.Call("put", "mime_type", "image/png");
                values.Call("put", "relative_path", "Pictures/AURON");

                AndroidJavaObject externalUri = mediaStoreImages.GetStatic<AndroidJavaObject>("EXTERNAL_CONTENT_URI");
                AndroidJavaObject uri = resolver.Call<AndroidJavaObject>("insert", externalUri, values);

                if (uri != null)
                {
                    using (AndroidJavaObject outStream = resolver.Call<AndroidJavaObject>("openOutputStream", uri))
                    {
                        outStream.Call("write", pngData);
                        outStream.Call("flush");
                        outStream.Call("close");
                    }
                    Debug.Log("[AURON SNAPSHOT] Saved directly to Android Gallery: Pictures/AURON/" + filename);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[AURON SNAPSHOT] MediaStore fallback: " + ex.Message);
        }
#endif
        try
        {
            string picturesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "AURON");
            if (!Directory.Exists(picturesPath))
                Directory.CreateDirectory(picturesPath);
            string fullPath = Path.Combine(picturesPath, filename);
            File.WriteAllBytes(fullPath, pngData);
            Debug.Log("[AURON SNAPSHOT] Saved to: " + fullPath);
        }
        catch
        {
            string fallback = Path.Combine(Application.persistentDataPath, filename);
            File.WriteAllBytes(fallback, pngData);
        }
    }

    // =========================================================
    // 6. AUTHENTIC WATCH CLICK-CLICK, WINDING & REASSEMBLE AUDIO
    // =========================================================

    private void SetupWatchAudio()
    {
        EnsureAudioListener();

        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;

        watchClickClipA = CreateWatchClickSound(3600f, 5200f, 0.018f);
        watchClickClipB = CreateWatchClickSound(3200f, 4800f, 0.018f);
        gearSnapClip = CreateMechanicalSnapSound(950f, 2400f, 0.045f);
        caseLockClip = CreateMechanicalSnapSound(520f, 1650f, 0.095f);
        ratchetWindClip = CreateWatchClickSound(2800f, 4400f, 0.014f);
        uiClickClip = CreateWatchClickSound(2200f, 3400f, 0.015f);
        shutterClip = CreateMechanicalSnapSound(700f, 1400f, 0.075f);
    }

    private AudioClip CreateWatchClickSound(float freq1, float freq2, float duration)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.Max(128, Mathf.RoundToInt(sampleRate * duration));
        float[] samples = new float[sampleCount];
        System.Random rng = new System.Random(1337);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float click1 = Mathf.Exp(-t * 95f);
            float click2 = t >= 0.32f ? Mathf.Exp(-(t - 0.32f) * 110f) * 0.80f : 0f;
            float env = click1 + click2;

            float noise = ((float)rng.NextDouble() * 2f - 1f) * 0.70f;
            float metalRing = Mathf.Sin(2f * Mathf.PI * freq1 * i / sampleRate) * 0.45f +
                              Mathf.Sin(2f * Mathf.PI * freq2 * i / sampleRate) * 0.35f;

            samples[i] = Mathf.Clamp((noise + metalRing) * env, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("AuronWatchClick", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateMechanicalSnapSound(float lowFreq, float highFreq, float duration)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.Max(256, Mathf.RoundToInt(sampleRate * duration));
        float[] samples = new float[sampleCount];
        System.Random rng = new System.Random(42);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float env = Mathf.Exp(-t * 38f);
            float body = Mathf.Sin(2f * Mathf.PI * lowFreq * i / sampleRate) * 0.6f;
            float click = Mathf.Sin(2f * Mathf.PI * highFreq * i / sampleRate) * 0.4f;
            float crisp = ((float)rng.NextDouble() * 2f - 1f) * 0.35f * Mathf.Exp(-t * 85f);

            samples[i] = Mathf.Clamp((body + click + crisp) * env, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("AuronMechSnap", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void HandleTickingAudio()
    {
        if (!enableTickingSound || audioSource == null)
            return;

        if (arScreenRoot == null || !arScreenRoot.gameObject.activeInHierarchy)
            return;

        float interval = (watchExperience != null && watchExperience.IsInsideMode) ? 0.166f : 0.25f;

        if (Time.unscaledTime >= nextTickTime)
        {
            nextTickTime = Time.unscaledTime + interval;
            tickToggle = !tickToggle;
            audioSource.pitch = 1.0f;

            if (customWatchClickSound != null)
            {
                audioSource.PlayOneShot(customWatchClickSound, tickVolume);
            }
            else
            {
                AudioClip clip = tickToggle ? watchClickClipA : watchClickClipB;
                if (clip != null)
                    audioSource.PlayOneShot(clip, tickVolume);
            }
        }
    }

    public void PlayReassembleSoundSequence()
    {
        EnsureAudioListener();
        if (reassembleSoundCoroutine != null)
            StopCoroutine(reassembleSoundCoroutine);
        reassembleSoundCoroutine = StartCoroutine(ReassembleSoundCoroutine());
    }

    private IEnumerator ReassembleSoundCoroutine()
    {
        if (audioSource == null) yield break;

        if (customReassembleSound != null)
        {
            audioSource.PlayOneShot(customReassembleSound, 0.95f);
            yield break;
        }

        int layers = 13;
        float stagger = 0.085f;

        for (int i = 0; i < layers; i++)
        {
            audioSource.pitch = Mathf.Lerp(0.88f, 1.22f, (float)i / (layers - 1));
            if (gearSnapClip != null)
                audioSource.PlayOneShot(gearSnapClip, 0.75f);
            yield return new WaitForSeconds(stagger);
        }

        yield return new WaitForSeconds(0.35f);
        audioSource.pitch = 1.0f;
        if (caseLockClip != null)
            audioSource.PlayOneShot(caseLockClip, 1.0f);

        reassembleSoundCoroutine = null;
    }

    public void ToggleTickingAudio()
    {
        EnsureAudioListener();
        enableTickingSound = !enableTickingSound;
        audioSource.pitch = 1.0f;
        PlayUIClick();
        UpdateToolbarButtonState(sfxButton, enableTickingSound, new Color32(205, 170, 108, 255));
        ShowInfoBanner(
            enableTickingSound ? "WATCH CLICK SOUND ON" : "WATCH SOUND OFF",
            enableTickingSound ? "Mechanical escapement click-click active" : "Silent viewing mode",
            2.0f);
    }

    private void PlayUIClick()
    {
        lastUserInteractionTime = Time.unscaledTime;
        EnsureAudioListener();
        if (audioSource != null && uiClickClip != null)
        {
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(uiClickClip, 0.85f);
        }
    }

    // =========================================================
    // UI BUILDER (YOUR EXACT PREVIOUS UI + 4 TRY-ON SIZES, NO EXPLODE BAR)
    // =========================================================

    private void BuildLuxuryOverlayUI()
    {
        GameObject arExpObj = GameObject.Find("AR_EXPERIENCE");
        if (arExpObj == null)
        {
            Canvas c = FindFirstObjectByType<Canvas>();
            if (c != null)
            {
                Transform found = FindDeepChild(c.transform, "AR_EXPERIENCE");
                if (found != null) arExpObj = found.gameObject;
            }
        }

        if (arExpObj == null) return;

        arScreenRoot = arExpObj.GetComponent<RectTransform>();

        featureUIRoot = new GameObject("AURON_LUXURY_FEATURES_UI", typeof(RectTransform));
        RectTransform rootRect = featureUIRoot.GetComponent<RectTransform>();
        rootRect.SetParent(arScreenRoot, false);
        Stretch(rootRect);

        // 1. Finish Customizer Swatches (Left side, above bottom dock)
        GameObject swatchBar = new GameObject("FINISH_SWATCH_BAR", typeof(RectTransform), typeof(Image));
        RectTransform swatchRect = swatchBar.GetComponent<RectTransform>();
        swatchRect.SetParent(rootRect, false);
        SetAnchors(swatchRect, 0.03f, 0.245f, 0.56f, 0.305f);
        Image swatchBg = swatchBar.GetComponent<Image>();
        swatchBg.color = new Color(0.02f, 0.025f, 0.03f, 0.88f);

        Color[] swatchColors =
        {
            new Color32(35, 38, 42, 255),    // Obsidian Black
            new Color32(218, 155, 128, 255), // 18K Rose Gold
            new Color32(232, 192, 102, 255), // Royal Gold
            new Color32(215, 220, 226, 255)  // Platinum Silver
        };
        string[] swatchLabels = { "BLK", "ROSE", "GOLD", "PLAT" };

        for (int i = 0; i < 4; i++)
        {
            int idx = i;
            float minX = 0.04f + i * 0.24f;
            float maxX = minX + 0.20f;
            Button b = CreatePillButton(swatchRect, swatchLabels[i], swatchColors[i], idx == 0 ? Color.white : Color.black);
            SetAnchors(b.GetComponent<RectTransform>(), minX, 0.16f, maxX, 0.84f);
            b.onClick.AddListener(() => SelectFinish(idx));
        }

        // 2. Top Action Pills (LUME, SNAP, SFX, WIND)
        lumeButton = CreatePillButton(rootRect, "LUME", new Color(0.02f, 0.025f, 0.03f, 0.88f), new Color32(247, 243, 235, 255));
        SetAnchors(lumeButton.GetComponent<RectTransform>(), 0.03f, 0.835f, 0.21f, 0.89f);
        lumeButton.onClick.AddListener(ToggleLumeMode);

        snapButton = CreatePillButton(rootRect, "SNAP", new Color(0.02f, 0.025f, 0.03f, 0.88f), new Color32(235, 207, 158, 255));
        SetAnchors(snapButton.GetComponent<RectTransform>(), 0.23f, 0.835f, 0.41f, 0.89f);
        snapButton.onClick.AddListener(CaptureARSnapshot);

        sfxButton = CreatePillButton(rootRect, "SFX", new Color(0.02f, 0.025f, 0.03f, 0.88f), new Color32(247, 243, 235, 255));
        SetAnchors(sfxButton.GetComponent<RectTransform>(), 0.43f, 0.835f, 0.60f, 0.89f);
        sfxButton.onClick.AddListener(ToggleTickingAudio);

        windButton = CreatePillButton(rootRect, "WIND", new Color(0.02f, 0.025f, 0.03f, 0.88f), new Color32(235, 207, 158, 255));
        SetAnchors(windButton.GetComponent<RectTransform>(), 0.62f, 0.835f, 0.80f, 0.89f);
        windButton.onClick.AddListener(ToggleCrownWindingMode);

        // 3. 4-Size Wrist Case Selector Bar (36MM | 40MM | 42MM | 46MM — Visible only in TRY ON Mode)
        BuildWristSizeSelectorUI(rootRect);

        // 4. Floating Horology Callout Banner
        infoBannerObj = new GameObject("HOROLOGY_INFO_BANNER", typeof(RectTransform), typeof(Image));
        RectTransform bannerRect = infoBannerObj.GetComponent<RectTransform>();
        bannerRect.SetParent(rootRect, false);
        SetAnchors(bannerRect, 0.05f, 0.73f, 0.95f, 0.82f);
        Image bannerBg = infoBannerObj.GetComponent<Image>();
        bannerBg.color = new Color(0.02f, 0.025f, 0.03f, 0.92f);

        infoTitleText = CreateTMP(bannerRect, "CAL. AU-01", 20f, new Color32(235, 207, 158, 255));
        infoTitleText.fontStyle = FontStyles.Bold;
        SetAnchors(infoTitleText.rectTransform, 0.04f, 0.50f, 0.96f, 0.92f);

        infoSpecText = CreateTMP(bannerRect, "Tap any exploded part in Inside Mode to inspect", 16f, new Color32(220, 216, 208, 255));
        SetAnchors(infoSpecText.rectTransform, 0.04f, 0.08f, 0.96f, 0.50f);

        infoBannerObj.SetActive(false);

        // 5. Camera Flash Overlay
        flashOverlayObj = new GameObject("CAMERA_FLASH", typeof(RectTransform), typeof(Image));
        RectTransform flashRect = flashOverlayObj.GetComponent<RectTransform>();
        flashRect.SetParent(rootRect, false);
        Stretch(flashRect);
        flashImage = flashOverlayObj.GetComponent<Image>();
        flashImage.color = new Color(1f, 1f, 1f, 0f);
        flashImage.raycastTarget = false;
        flashOverlayObj.SetActive(false);
    }

    private void BuildWristSizeSelectorUI(RectTransform parent)
    {
        wristSizeBarObj = new GameObject("WRIST_SIZE_BAR", typeof(RectTransform), typeof(Image));
        RectTransform rect = wristSizeBarObj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        SetAnchors(rect, 0.03f, 0.318f, 0.68f, 0.375f);

        Image bg = wristSizeBarObj.GetComponent<Image>();
        bg.color = new Color(0.02f, 0.025f, 0.03f, 0.88f);

        string[] labels = { "36MM", "40MM", "42MM", "46MM" };
        sizeButtons.Clear();

        for (int i = 0; i < 4; i++)
        {
            int idx = i;
            float minX = 0.03f + i * 0.24f;
            float maxX = minX + 0.22f;
            bool isDefault = (idx == 2); // 42MM default

            Button b = CreatePillButton(
                rect,
                labels[i],
                isDefault ? new Color32(205, 170, 108, 255) : new Color(0.02f, 0.025f, 0.03f, 0.88f),
                isDefault ? Color.black : new Color32(247, 243, 235, 255));
            SetAnchors(b.GetComponent<RectTransform>(), minX, 0.14f, maxX, 0.86f);
            b.onClick.AddListener(() => SelectWristCaseSize(idx));
            sizeButtons.Add(b);
        }

        wristSizeBarObj.SetActive(false);
    }

    private void ShowInfoBanner(string title, string spec, float duration)
    {
        if (infoBannerObj == null) return;
        infoTitleText.text = title;
        infoSpecText.text = spec;
        infoBannerObj.SetActive(true);
        infoBannerHideTime = Time.unscaledTime + duration;
    }

    private void UpdateToolbarButtonState(Button btn, bool active, Color activeColor)
    {
        if (btn == null) return;
        Image img = btn.GetComponent<Image>();
        TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (img != null)
            img.color = active ? activeColor : new Color(0.02f, 0.025f, 0.03f, 0.88f);
        if (txt != null)
            txt.color = active ? Color.black : new Color32(247, 243, 235, 255);
    }

    private Button CreatePillButton(RectTransform parent, string label, Color bgColor, Color textColor)
    {
        GameObject obj = new GameObject("BTN_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        Image img = obj.GetComponent<Image>();
        img.color = bgColor;
        Button btn = obj.GetComponent<Button>();

        TextMeshProUGUI txt = CreateTMP(rect, label, 15f, textColor);
        txt.fontStyle = FontStyles.Bold;
        Stretch(txt.rectTransform);
        return btn;
    }

    private TextMeshProUGUI CreateTMP(RectTransform parent, string content, float size, Color color)
    {
        GameObject obj = new GameObject("LABEL", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;
        return tmp;
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
}