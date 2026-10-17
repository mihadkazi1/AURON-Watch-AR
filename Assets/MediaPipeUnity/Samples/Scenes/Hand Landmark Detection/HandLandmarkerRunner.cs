using System.Collections;
using Mediapipe.Unity;
using Mediapipe.Tasks.Vision.HandLandmarker;
using UnityEngine;

namespace Mediapipe.Unity.Sample.HandLandmarkDetection
{
    public class AuronVuforiaHandLandmarker : MonoBehaviour
    {
        // =====================================================
        // REFERENCES
        // =====================================================

        [Header("AURON References")]

        [SerializeField]
        private AuronVuforiaCameraFrame vuforiaCameraFrame;


        // =====================================================
        // DETECTION
        // =====================================================

        [Header("Hand Detection")]

        [SerializeField]
        private int maxHands = 1;

        [Range(0f, 1f)]
        [SerializeField]
        private float minDetectionConfidence = 0.5f;

        [Range(0f, 1f)]
        [SerializeField]
        private float minPresenceConfidence = 0.5f;

        [Range(0f, 1f)]
        [SerializeField]
        private float minTrackingConfidence = 0.5f;

        [SerializeField]
        private float processFPS = 8f;


        // =====================================================
        // DEBUG
        // =====================================================

        [Header("Debug")]

        [SerializeField]
        private bool logWrist = true;

        [SerializeField]
        private float logInterval = 0.5f;


        // =====================================================
        // MEDIAPIPE
        // =====================================================

        private HandLandmarker handLandmarker;

        private Experimental.TextureFramePool textureFramePool;

        private HandLandmarkerResult result;

        private long lastTimestamp;

        private float nextProcessTime;

        private float nextLogTime;

        private bool initialized;

        private bool processing;


        // =====================================================
        // PUBLIC HAND DATA
        // =====================================================

        public bool HasHand { get; private set; }

        public Vector3 WristNormalized { get; private set; }

        public Vector3 WristWorld { get; private set; }


        // =====================================================
        // START
        // =====================================================

        private IEnumerator Start()
        {
            Debug.Log(
                "[AURON HAND] Starting..."
            );


            // -------------------------------------------------
            // Find Vuforia bridge
            // -------------------------------------------------

            if (vuforiaCameraFrame == null)
            {
                vuforiaCameraFrame =
                    FindFirstObjectByType<AuronVuforiaCameraFrame>();
            }


            if (vuforiaCameraFrame == null)
            {
                Debug.LogError(
                    "[AURON HAND] " +
                    "AuronVuforiaCameraFrame not found."
                );

                yield break;
            }


            // -------------------------------------------------
            // Wait for Vuforia frame
            // -------------------------------------------------

            Debug.Log(
                "[AURON HAND] Waiting for Vuforia frame..."
            );


            while (
                !vuforiaCameraFrame.HasFrame
            )
            {
                yield return null;
            }


            Texture2D sourceTexture =
                vuforiaCameraFrame.CurrentTexture;


            if (sourceTexture == null)
            {
                Debug.LogError(
                    "[AURON HAND] " +
                    "Vuforia texture is null."
                );

                yield break;
            }


            // -------------------------------------------------
            // Use the SAME configuration class that your
            // official MediaPipe sample uses.
            // -------------------------------------------------

            HandLandmarkDetectionConfig config =
                new HandLandmarkDetectionConfig();


            config.Delegate =
                Tasks.Core.BaseOptions.Delegate.CPU;

            config.ImageReadMode =
                ImageReadMode.CPU;

            config.RunningMode =
                Tasks.Vision.Core.RunningMode.VIDEO;

            config.NumHands =
                maxHands;

            config.MinHandDetectionConfidence =
                minDetectionConfidence;

            config.MinHandPresenceConfidence =
                minPresenceConfidence;

            config.MinTrackingConfidence =
                minTrackingConfidence;


            // -------------------------------------------------
            // Prepare model
            // -------------------------------------------------

            Debug.Log(
                "[AURON HAND] Preparing hand model..."
            );


            yield return AssetLoader.PrepareAssetAsync(
                config.ModelPath
            );


            // -------------------------------------------------
            // Create MediaPipe options
            // -------------------------------------------------

            HandLandmarkerOptions options =
                config.GetHandLandmarkerOptions();


            // -------------------------------------------------
            // Create Hand Landmarker
            // -------------------------------------------------

            handLandmarker =
                HandLandmarker.CreateFromOptions(
                    options,
                    GpuManager.GpuResources
                );


            // -------------------------------------------------
            // Create texture pool
            // -------------------------------------------------

            textureFramePool =
                new Experimental.TextureFramePool(
                    sourceTexture.width,
                    sourceTexture.height,
                    TextureFormat.RGBA32,
                    10
                );


            // -------------------------------------------------
            // Create result container
            // -------------------------------------------------

            result =
                HandLandmarkerResult.Alloc(
                    maxHands
                );


            initialized = true;


            Debug.Log(
                "[AURON HAND] Hand Landmarker READY."
            );
        }


        // =====================================================
        // UPDATE
        // =====================================================

        private void Update()
        {
            if (!initialized)
                return;


            if (processing)
                return;


            if (
                Time.unscaledTime <
                nextProcessTime
            )
            {
                return;
            }


            nextProcessTime =
                Time.unscaledTime +
                (
                    1f /
                    Mathf.Max(
                        1f,
                        processFPS
                    )
                );


            StartCoroutine(
                ProcessFrame()
            );
        }


        // =====================================================
        // PROCESS FRAME
        // =====================================================

        private IEnumerator ProcessFrame()
        {
            if (processing)
                yield break;


            processing = true;


            Texture2D sourceTexture =
                vuforiaCameraFrame.CurrentTexture;


            if (sourceTexture == null)
            {
                processing = false;
                yield break;
            }


            if (textureFramePool == null)
            {
                processing = false;
                yield break;
            }


            // -------------------------------------------------
            // Get reusable texture frame
            // -------------------------------------------------

            if (
                !textureFramePool.TryGetTextureFrame(
                    out var textureFrame
                )
            )
            {
                processing = false;
                yield break;
            }


            // -------------------------------------------------
            // Wait until Vuforia's texture has updated.
            // -------------------------------------------------

            yield return new WaitForEndOfFrame();


            // -------------------------------------------------
            // Read Vuforia Texture2D
            // -------------------------------------------------

            textureFrame.ReadTextureOnCPU(
                sourceTexture,
                false,
                false
            );


            // -------------------------------------------------
            // Build MediaPipe Image
            // -------------------------------------------------

            Image image =
                textureFrame.BuildCPUImage();


            textureFrame.Release();


            // -------------------------------------------------
            // Timestamp
            // -------------------------------------------------

            long timestamp =
                System.DateTimeOffset.UtcNow
                    .ToUnixTimeMilliseconds();


            if (
                timestamp <=
                lastTimestamp
            )
            {
                timestamp =
                    lastTimestamp + 1;
            }


            lastTimestamp =
                timestamp;


            // -------------------------------------------------
            // Image processing
            // -------------------------------------------------

            var imageProcessingOptions =
                new Tasks.Vision.Core.ImageProcessingOptions(
                    rotationDegrees: 0
                );


            // -------------------------------------------------
            // Hand detection
            // -------------------------------------------------

            bool detected =
                handLandmarker.TryDetectForVideo(
                    image,
                    timestamp,
                    imageProcessingOptions,
                    ref result
                );


            // -------------------------------------------------
            // READ RESULT
            // -------------------------------------------------

            if (
                detected &&
                result.handLandmarks != null &&
                result.handLandmarks.Count > 0
            )
            {
                var landmarks =
                    result
                        .handLandmarks[0]
                        .landmarks;


                if (
                    landmarks != null &&
                    landmarks.Count >= 21
                )
                {
                    // MediaPipe landmark 0 = WRIST
                    var wrist =
                        landmarks[0];


                    WristNormalized =
                        new Vector3(
                            wrist.x,
                            wrist.y,
                            wrist.z
                        );


                    // -------------------------------------------------
                    // World landmark
                    // -------------------------------------------------

                    if (
                        result.handWorldLandmarks != null &&
                        result.handWorldLandmarks.Count > 0
                    )
                    {
                        var worldLandmarks =
                            result
                                .handWorldLandmarks[0]
                                .landmarks;


                        if (
                            worldLandmarks != null &&
                            worldLandmarks.Count >= 21
                        )
                        {
                            var worldWrist =
                                worldLandmarks[0];


                            WristWorld =
                                new Vector3(
                                    worldWrist.x,
                                    worldWrist.y,
                                    worldWrist.z
                                );
                        }
                    }


                    HasHand = true;


                    // -------------------------------------------------
                    // Console
                    // -------------------------------------------------

                    if (
                        logWrist &&
                        Time.unscaledTime >=
                        nextLogTime
                    )
                    {
                        nextLogTime =
                            Time.unscaledTime +
                            logInterval;


                        Debug.Log(
                            "[AURON HAND] " +
                            "Wrist  " +
                            $"X={WristNormalized.x:F3}  " +
                            $"Y={WristNormalized.y:F3}  " +
                            $"Z={WristNormalized.z:F3}"
                        );
                    }
                }
                else
                {
                    HasHand = false;
                }
            }
            else
            {
                HasHand = false;
            }


            processing = false;
        }


        // =====================================================
        // DESTROY
        // =====================================================

        private void OnDestroy()
        {
            initialized = false;

            processing = false;

            HasHand = false;


            if (textureFramePool != null)
            {
                textureFramePool.Dispose();

                textureFramePool = null;
            }


            if (handLandmarker != null)
            {
                handLandmarker = null;
            }
        }
    }
}