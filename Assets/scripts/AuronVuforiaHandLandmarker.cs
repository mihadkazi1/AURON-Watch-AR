using System;
using System.Collections;
using UnityEngine;
using Mediapipe;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity;

public class AuronVuforiaHandLandmarker : MonoBehaviour
{
    [Header("Vuforia Camera")]
    [SerializeField]
    private AuronVuforiaCameraFrame vuforiaCameraFrame;

    [Header("Hand Detection")]
    [SerializeField, Range(1, 2)]
    private int maxHands = 2;

    [Range(0f, 1f)]
    [SerializeField]
    private float minHandDetectionConfidence = 0.45f;

    [Range(0f, 1f)]
    [SerializeField]
    private float minHandPresenceConfidence = 0.45f;

    [Range(0f, 1f)]
    [SerializeField]
    private float minTrackingConfidence = 0.45f;

    [SerializeField, Range(15f, 60f)]
    private float processFPS = 30f;

    [Header("Grace & Continuity")]
    [SerializeField, Range(0.1f, 1.0f)]
    private float handLostGraceSeconds = 0.45f;

    private HandLandmarker handLandmarker;
    private Mediapipe.Unity.Experimental.TextureFramePool textureFramePool;
    private HandLandmarkerResult handResult;
    private IResourceManager resourceManager;

    private bool trackingActive;
    private bool initialized;
    private bool processing;
    private float nextProcessTime;
    private float lastStableHandTime = -999f;
    private long lastTimestamp;
    private bool havePreviousWrist;
    private Vector2 previousWrist;

    private readonly Vector3[] normalizedLandmarks = new Vector3[21];
    private readonly Vector3[] worldLandmarks = new Vector3[21];
    private readonly Vector3[] candidateLandmarks = new Vector3[21];
    private readonly Vector3[] candidateWorldLandmarks = new Vector3[21];
    private readonly Vector3[] bestCandidateLandmarks = new Vector3[21];
    private readonly Vector3[] bestCandidateWorldLandmarks = new Vector3[21];

    public bool HasHand { get; private set; }
    public Vector3 WristNormalized { get; private set; }
    public Vector3 WristWorld { get; private set; }
    public bool IsTrackingActive => trackingActive;
    public int AppliedRotationDegrees { get; private set; } = 0;
    public string DiagnosticStatus { get; private set; } = "INIT...";

    public void SetTrackingActive(bool active)
    {
        if (trackingActive == active)
        {
            if (!active) ResetCandidateStability();
            return;
        }

        trackingActive = active;
        ResetCandidateStability();
        nextProcessTime = Time.unscaledTime;
    }

    public bool TryGetLandmark(int index, out Vector3 normalized, out Vector3 world)
    {
        normalized = Vector3.zero;
        world = Vector3.zero;

        if (!HasHand || index < 0 || index >= 21)
            return false;

        normalized = normalizedLandmarks[index];
        world = worldLandmarks[index];
        return true;
    }

    private IEnumerator Start()
    {
        DiagnosticStatus = "STARTING...";

        if (vuforiaCameraFrame == null)
            vuforiaCameraFrame = FindFirstObjectByType<AuronVuforiaCameraFrame>();

        if (vuforiaCameraFrame == null)
        {
            DiagnosticStatus = "ERR: NO CAM BRIDGE";
            yield break;
        }

        while (vuforiaCameraFrame != null && !vuforiaCameraFrame.HasFrame)
            yield return null;

        if (vuforiaCameraFrame == null || vuforiaCameraFrame.CurrentTexture == null)
        {
            DiagnosticStatus = "ERR: NO CAM TEX";
            yield break;
        }

        Texture2D sourceTexture = vuforiaCameraFrame.CurrentTexture;

#if UNITY_EDITOR
        resourceManager = new LocalResourceManager();
#else
        resourceManager = new StreamingAssetsResourceManager();
#endif

        DiagnosticStatus = "LOADING MODEL...";
        yield return resourceManager.PrepareAssetAsync("hand_landmarker.bytes");

        maxHands = 2;

        try
        {
            var options = new HandLandmarkerOptions(
                new Mediapipe.Tasks.Core.BaseOptions(
                    Mediapipe.Tasks.Core.BaseOptions.Delegate.CPU,
                    modelAssetPath: "hand_landmarker.bytes"
                ),
                runningMode: Mediapipe.Tasks.Vision.Core.RunningMode.VIDEO,
                numHands: maxHands,
                minHandDetectionConfidence: minHandDetectionConfidence,
                minHandPresenceConfidence: minHandPresenceConfidence,
                minTrackingConfidence: minTrackingConfidence
            );

            handLandmarker = HandLandmarker.CreateFromOptions(
                options,
                GpuManager.GpuResources
            );
        }
        catch (Exception ex)
        {
            DiagnosticStatus = "ERR: MODEL MISSING";
            Debug.LogError("[AURON HAND] " + ex.Message);
            yield break;
        }

        textureFramePool = new Mediapipe.Unity.Experimental.TextureFramePool(
            sourceTexture.width,
            sourceTexture.height,
            TextureFormat.RGBA32,
            10
        );

        handResult = HandLandmarkerResult.Alloc(maxHands);
        initialized = true;
        DiagnosticStatus = "READY";
    }

    private void Update()
    {
        if (!trackingActive || !initialized || processing || Time.unscaledTime < nextProcessTime)
            return;

        nextProcessTime = Time.unscaledTime + (1f / Mathf.Clamp(processFPS, 15f, 60f));
        StartCoroutine(ProcessCurrentFrame());
    }

    private IEnumerator ProcessCurrentFrame()
    {
        if (!trackingActive || processing)
            yield break;

        processing = true;

        Texture2D sourceTexture = vuforiaCameraFrame != null ? vuforiaCameraFrame.CurrentTexture : null;
        if (sourceTexture == null || textureFramePool == null || handLandmarker == null)
        {
            ResetCandidateStability();
            processing = false;
            yield break;
        }

        if (textureFramePool.textureWidth != sourceTexture.width ||
            textureFramePool.textureHeight != sourceTexture.height)
        {
            textureFramePool.Dispose();
            textureFramePool = new Mediapipe.Unity.Experimental.TextureFramePool(
                sourceTexture.width,
                sourceTexture.height,
                TextureFormat.RGBA32,
                10
            );
        }

        if (!textureFramePool.TryGetTextureFrame(out var textureFrame))
        {
            processing = false;
            yield break;
        }

        yield return new WaitForEndOfFrame();

        if (!trackingActive)
        {
            textureFrame.Release();
            ResetCandidateStability();
            processing = false;
            yield break;
        }

        textureFrame.ReadTextureOnCPU(sourceTexture, false, false);
        Image image = textureFrame.BuildCPUImage();
        textureFrame.Release();

        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (timestamp <= lastTimestamp)
            timestamp = lastTimestamp + 1;
        lastTimestamp = timestamp;

#if UNITY_EDITOR
        int rotation = 0;
#else
        bool texLandscape = sourceTexture.width > sourceTexture.height;
        bool screenPortrait = UnityEngine.Screen.height > UnityEngine.Screen.width;
        int rotation = (texLandscape && screenPortrait) ? 270 : 0;
#endif
        AppliedRotationDegrees = rotation;

        var imageProcessingOptions =
            new Mediapipe.Tasks.Vision.Core.ImageProcessingOptions(rotationDegrees: rotation);

        bool detected = handLandmarker.TryDetectForVideo(
            image,
            timestamp,
            imageProcessingOptions,
            ref handResult
        );

        int bestScore = -1;
        float bestDist = float.MaxValue;

        if (detected && handResult.handLandmarks != null && handResult.handLandmarks.Count > 0)
        {
            int count = handResult.handLandmarks.Count;
            for (int c = 0; c < count; c++)
            {
                var lms = handResult.handLandmarks[c].landmarks;
                if (lms == null || lms.Count < 21) continue;

                for (int i = 0; i < 21; i++)
                {
                    var lm = lms[i];
                    candidateLandmarks[i] = new Vector3(lm.x, lm.y, lm.z);
                    candidateWorldLandmarks[i] = Vector3.zero;
                }

                if (handResult.handWorldLandmarks != null && c < handResult.handWorldLandmarks.Count)
                {
                    var world = handResult.handWorldLandmarks[c].landmarks;
                    if (world != null && world.Count >= 21)
                    {
                        for (int i = 0; i < 21; i++)
                        {
                            var w = world[i];
                            candidateWorldLandmarks[i] = new Vector3(w.x, w.y, w.z);
                        }
                    }
                }

                // Score >= 0 means valid wrist & knuckle geometry (0 = closed fist, 1..4 = open fingers).
                // Score == -1 means implausible geometry (face/background noise).
                int score = ScorePlausibleHandOrFist(candidateLandmarks, sourceTexture);
                if (score < 0) continue;

                Vector2 wrist2D = new Vector2(candidateLandmarks[0].x, candidateLandmarks[0].y);
                float dist = havePreviousWrist ? Vector2.Distance(wrist2D, previousWrist) : 0f;

                // If we already locked onto a wrist, prioritize staying on the nearest wrist (even when closing into a fist!)
                bool isSameTrackedWrist = havePreviousWrist && dist < 0.28f;
                if (isSameTrackedWrist)
                {
                    if (dist < bestDist)
                    {
                        bestScore = Mathf.Max(bestScore, score);
                        bestDist = dist;
                        Array.Copy(candidateLandmarks, bestCandidateLandmarks, 21);
                        Array.Copy(candidateWorldLandmarks, bestCandidateWorldLandmarks, 21);
                    }
                }
                else if (score > bestScore || (score == bestScore && dist < bestDist))
                {
                    bestScore = score;
                    bestDist = dist;
                    Array.Copy(candidateLandmarks, bestCandidateLandmarks, 21);
                    Array.Copy(candidateWorldLandmarks, bestCandidateWorldLandmarks, 21);
                }
            }
        }

        // Accept score >= 0 so both a closed fist (score == 0) and open hand (score 1..4) keep the watch visible!
        if (bestScore >= 0)
        {
            Array.Copy(bestCandidateLandmarks, normalizedLandmarks, 21);
            Array.Copy(bestCandidateWorldLandmarks, worldLandmarks, 21);
            WristNormalized = normalizedLandmarks[0];
            WristWorld = worldLandmarks[0];

            previousWrist = new Vector2(WristNormalized.x, WristNormalized.y);
            havePreviousWrist = true;
            HasHand = true;
            lastStableHandTime = Time.unscaledTime;
            DiagnosticStatus = "WRIST TRACKING";
        }
        else
        {
            if (Time.unscaledTime - lastStableHandTime > handLostGraceSeconds)
            {
                HasHand = false;
                havePreviousWrist = false;
                DiagnosticStatus = "SHOW WRIST";
            }
        }

        image.Dispose();
        processing = false;
    }

    /// <summary>
    /// Validates the core palm/knuckle structure (wrist 0 to knuckles 5, 9, 13, 17).
    /// Returns -1 if implausible (e.g. tiny face false positive), 0 for a valid closed fist,
    /// or 1..4 for extended fingers.
    /// </summary>
    private int ScorePlausibleHandOrFist(Vector3[] pts, Texture2D tex)
    {
        if (pts == null || pts.Length < 21) return -1;

        float aspect = (tex != null && tex.height > 0) ? (float)tex.width / tex.height : 1f;
        Vector2 p0 = new Vector2(pts[0].x * aspect, pts[0].y);
        Vector2 p5 = new Vector2(pts[5].x * aspect, pts[5].y);
        Vector2 p9 = new Vector2(pts[9].x * aspect, pts[9].y);
        Vector2 p13 = new Vector2(pts[13].x * aspect, pts[13].y);
        Vector2 p17 = new Vector2(pts[17].x * aspect, pts[17].y);

        float handLength = Vector2.Distance(p0, p9);
        float palmWidth = Vector2.Distance(p5, p17);

        // Require a real foreground hand size so small background faces are ignored
        if (handLength < 0.045f || palmWidth < 0.030f) return -1;

        float ratio = palmWidth / handLength;
        if (ratio < 0.22f || ratio > 1.85f) return -1;

        // Check that knuckles 5 -> 9 -> 13 -> 17 are ordered sequentially across the hand
        float d5_9 = Vector2.Distance(p5, p9);
        float d9_13 = Vector2.Distance(p9, p13);
        float d13_17 = Vector2.Distance(p13, p17);
        if (d5_9 < 0.004f || d9_13 < 0.004f || d13_17 < 0.004f) return -1;

        int extended = 0;
        if (IsFingerExtended(pts, 5, 8, p0, handLength, aspect)) extended++;
        if (IsFingerExtended(pts, 9, 12, p0, handLength, aspect)) extended++;
        if (IsFingerExtended(pts, 13, 16, p0, handLength, aspect)) extended++;
        if (IsFingerExtended(pts, 17, 20, p0, handLength, aspect)) extended++;

        // Returns 0 when all fingers are curled into a fist, or 1..4 when fingers are open
        return extended;
    }

    private bool IsFingerExtended(Vector3[] pts, int mcpIdx, int tipIdx, Vector2 wrist, float handLen, float aspect)
    {
        Vector2 mcp = new Vector2(pts[mcpIdx].x * aspect, pts[mcpIdx].y);
        Vector2 tip = new Vector2(pts[tipIdx].x * aspect, pts[tipIdx].y);

        float mcpDist = Vector2.Distance(mcp, wrist);
        float tipDist = Vector2.Distance(tip, wrist);
        float segLen = Vector2.Distance(tip, mcp);

        return segLen >= handLen * 0.10f && tipDist >= mcpDist + handLen * 0.02f;
    }

    private void ResetCandidateStability()
    {
        HasHand = false;
        WristNormalized = Vector3.zero;
        WristWorld = Vector3.zero;
        Array.Clear(normalizedLandmarks, 0, normalizedLandmarks.Length);
        Array.Clear(worldLandmarks, 0, worldLandmarks.Length);
        havePreviousWrist = false;
        previousWrist = Vector2.zero;
        lastStableHandTime = -999f;
    }

    private void OnDestroy()
    {
        initialized = false;
        processing = false;
        ResetCandidateStability();

        if (textureFramePool != null)
        {
            textureFramePool.Dispose();
            textureFramePool = null;
        }

        if (handLandmarker != null)
        {
            IDisposable disposable = handLandmarker as IDisposable;
            if (disposable != null)
                disposable.Dispose();
            handLandmarker = null;
        }

        resourceManager = null;
    }
}