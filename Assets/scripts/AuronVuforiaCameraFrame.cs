using System;
using UnityEngine;
using Vuforia;

using VuforiaImage = Vuforia.Image;

public class AuronVuforiaCameraFrame : MonoBehaviour
{
    // =========================================================
    // SETTINGS
    // =========================================================

    [Header("Camera Frame")]

    [SerializeField]
    private bool enableFrameCapture = true;

    [Tooltip("Maximum number of camera frames copied per second.")]
    [SerializeField]
    private float targetFPS = 20f;


    [Header("Debug")]

    [SerializeField]
    private bool logFirstFrame = true;


    // =========================================================
    // OUTPUT
    // =========================================================

    private Texture2D cameraTexture;

    private bool formatRegistered;

    private bool firstFrameReceived;

    private float nextFrameTime;


    // =========================================================
    // PUBLIC ACCESS
    // =========================================================

    public Texture2D CurrentTexture
    {
        get
        {
            return cameraTexture;
        }
    }


    public bool HasFrame
    {
        get
        {
            return cameraTexture != null;
        }
    }


    public event Action<Texture2D> OnFrameReady;


    // =========================================================
    // UNITY START
    // =========================================================

    private void Start()
    {
        // -----------------------------------------------------
        // Subscribe to Vuforia lifecycle.
        // -----------------------------------------------------

        if (VuforiaApplication.Instance != null)
        {
            VuforiaApplication.Instance.OnVuforiaStarted +=
                OnVuforiaStarted;

            VuforiaApplication.Instance.OnVuforiaStopped +=
                OnVuforiaStopped;
        }


        // -----------------------------------------------------
        // Subscribe to Vuforia frame/state updates.
        // -----------------------------------------------------

        VuforiaBehaviour vuforiaBehaviour =
            GetVuforiaBehaviour();


        if (vuforiaBehaviour != null)
        {
            vuforiaBehaviour.World.OnStateUpdated +=
                OnVuforiaStateUpdated;
        }


        // -----------------------------------------------------
        // IMPORTANT:
        //
        // IsInitialized belongs to VuforiaApplication.
        // -----------------------------------------------------

        if (
            VuforiaApplication.Instance != null &&
            VuforiaApplication.Instance.IsInitialized
        )
        {
            RegisterFrameFormat();
        }
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        // -----------------------------------------------------
        // Remove Vuforia lifecycle events.
        // -----------------------------------------------------

        if (VuforiaApplication.Instance != null)
        {
            VuforiaApplication.Instance.OnVuforiaStarted -=
                OnVuforiaStarted;

            VuforiaApplication.Instance.OnVuforiaStopped -=
                OnVuforiaStopped;
        }


        // -----------------------------------------------------
        // Remove state update event.
        // -----------------------------------------------------

        VuforiaBehaviour vuforiaBehaviour =
            GetVuforiaBehaviour();


        if (vuforiaBehaviour != null)
        {
            vuforiaBehaviour.World.OnStateUpdated -=
                OnVuforiaStateUpdated;
        }


        // -----------------------------------------------------
        // Release frame format.
        // -----------------------------------------------------

        UnregisterFrameFormat();


        // -----------------------------------------------------
        // Destroy texture.
        // -----------------------------------------------------

        if (cameraTexture != null)
        {
            Destroy(cameraTexture);

            cameraTexture = null;
        }
    }


    // =========================================================
    // GET VUFORIA BEHAVIOUR
    // =========================================================

    private VuforiaBehaviour GetVuforiaBehaviour()
    {
        if (VuforiaApplication.Instance == null)
        {
            return null;
        }


        return
            VuforiaApplication.Instance
                .GetVuforiaBehaviour();
    }


    // =========================================================
    // VUFORIA STARTED
    // =========================================================

    private void OnVuforiaStarted()
    {
        RegisterFrameFormat();
    }


    // =========================================================
    // VUFORIA STOPPED
    // =========================================================

    private void OnVuforiaStopped()
    {
        UnregisterFrameFormat();
    }


    // =========================================================
    // REGISTER RGB888
    // =========================================================

    private void RegisterFrameFormat()
    {
        if (!enableFrameCapture)
        {
            return;
        }


        if (formatRegistered)
        {
            return;
        }


        VuforiaBehaviour vuforiaBehaviour =
            GetVuforiaBehaviour();


        if (vuforiaBehaviour == null)
        {
            Debug.LogWarning(
                "[AURON] VuforiaBehaviour not found."
            );

            return;
        }


        // -----------------------------------------------------
        // Make sure the Vuforia camera is active.
        // -----------------------------------------------------

        if (
            vuforiaBehaviour.CameraDevice == null
        )
        {
            Debug.LogWarning(
                "[AURON] Vuforia CameraDevice not available yet."
            );

            return;
        }


        bool success =
            vuforiaBehaviour.CameraDevice.SetFrameFormat(
                PixelFormat.RGB888,
                true
            );


        if (success)
        {
            formatRegistered = true;


            Debug.Log(
                "[AURON] Vuforia RGB888 camera frame enabled."
            );
        }
        else
        {
            Debug.LogError(
                "[AURON] Failed to enable Vuforia RGB888 camera frame."
            );
        }
    }


    // =========================================================
    // UNREGISTER RGB888
    // =========================================================

    private void UnregisterFrameFormat()
    {
        if (!formatRegistered)
        {
            return;
        }


        VuforiaBehaviour vuforiaBehaviour =
            GetVuforiaBehaviour();


        if (vuforiaBehaviour != null)
        {
            try
            {
                vuforiaBehaviour.CameraDevice.SetFrameFormat(
                    PixelFormat.RGB888,
                    false
                );
            }
            catch
            {
                // Vuforia may already be shutting down.
            }
        }


        formatRegistered = false;
    }


    // =========================================================
    // VUFORIA STATE UPDATE
    // =========================================================

    private void OnVuforiaStateUpdated()
    {
        if (!enableFrameCapture)
        {
            return;
        }


        if (!formatRegistered)
        {
            return;
        }


        // -----------------------------------------------------
        // Limit frame copying to targetFPS.
        // -----------------------------------------------------

        if (
            Time.unscaledTime <
            nextFrameTime
        )
        {
            return;
        }


        float interval =
            1f /
            Mathf.Max(
                1f,
                targetFPS
            );


        nextFrameTime =
            Time.unscaledTime +
            interval;


        ReadCameraFrame();
    }


    // =========================================================
    // READ VUFORIA CAMERA FRAME
    // =========================================================

    private void ReadCameraFrame()
    {
        VuforiaBehaviour vuforiaBehaviour =
            GetVuforiaBehaviour();


        if (vuforiaBehaviour == null)
        {
            return;
        }


        // -----------------------------------------------------
        // Get RGB888 frame.
        //
        // Vuforia documents GetCameraImage(format) as returning
        // null when the requested image is not currently
        // available.
        // -----------------------------------------------------

        VuforiaImage image =
            vuforiaBehaviour.CameraDevice.GetCameraImage(
                PixelFormat.RGB888
            );


        if (image == null)
        {
            return;
        }


        // -----------------------------------------------------
        // Check dimensions.
        // -----------------------------------------------------

        if (
            image.Width <= 0 ||
            image.Height <= 0
        )
        {
            return;
        }


        // -----------------------------------------------------
        // Create texture if necessary.
        // -----------------------------------------------------

        if (
            cameraTexture == null ||
            cameraTexture.width != image.Width ||
            cameraTexture.height != image.Height
        )
        {
            if (cameraTexture != null)
            {
                Destroy(cameraTexture);
            }


            cameraTexture =
                new Texture2D(
                    image.Width,
                    image.Height,
                    TextureFormat.RGB24,
                    false
                );


            cameraTexture.name =
                "AURON_VUFORIA_CAMERA_TEXTURE";


            cameraTexture.wrapMode =
                TextureWrapMode.Clamp;


            cameraTexture.filterMode =
                FilterMode.Bilinear;


            Debug.Log(
                "[AURON] Camera texture created: " +
                image.Width +
                " x " +
                image.Height
            );
        }


        // -----------------------------------------------------
        // Copy Vuforia frame into Unity texture.
        // -----------------------------------------------------

        image.CopyToTexture(
            cameraTexture,
            true
        );


        // -----------------------------------------------------
        // First frame received.
        // -----------------------------------------------------

        if (!firstFrameReceived)
        {
            firstFrameReceived = true;


            if (logFirstFrame)
            {
                Debug.Log(
                    "[AURON] First Vuforia camera frame received."
                );
            }
        }


        // -----------------------------------------------------
        // Send frame to future MediaPipe bridge.
        // -----------------------------------------------------

        OnFrameReady?.Invoke(
            cameraTexture
        );
    }
}