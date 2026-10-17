using UnityEngine;
using Vuforia;

public class AuronTargetWatchVisibility : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("Try On Controller")]

    [SerializeField]
    private AuronTryOnController tryOnController;


    // =========================================================
    // VUFORIA
    // =========================================================

    private ObserverBehaviour observer;


    // =========================================================
    // WATCH RENDERERS
    // =========================================================

    private Renderer[] watchRenderers;

    private bool[] originalRendererStates;


    // =========================================================
    // STATE
    // =========================================================

    private bool currentVisible = true;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        observer =
            GetComponent<ObserverBehaviour>();


        if (tryOnController == null)
        {
            tryOnController =
                FindFirstObjectByType<
                    AuronTryOnController
                >();
        }


        watchRenderers =
            GetComponentsInChildren<Renderer>(
                true
            );


        originalRendererStates =
            new bool[
                watchRenderers.Length
            ];


        for (
            int i = 0;
            i < watchRenderers.Length;
            i++
        )
        {
            if (
                watchRenderers[i] != null
            )
            {
                originalRendererStates[i] =
                    watchRenderers[i].enabled;
            }
        }
    }


    private void OnEnable()
    {
        if (observer != null)
        {
            observer.OnTargetStatusChanged +=
                OnTargetStatusChanged;
        }


        Refresh();
    }


    private void OnDisable()
    {
        if (observer != null)
        {
            observer.OnTargetStatusChanged -=
                OnTargetStatusChanged;
        }
    }


    // =========================================================
    // UPDATE
    //
    // We continuously check Vuforia status.
    // This makes the watch disappear reliably when the image
    // target is moved out of view.
    // =========================================================

    private void Update()
    {
        // -----------------------------------------------------
        // TRY ON:
        //
        // Image target should NOT control the watch.
        // -----------------------------------------------------

        if (
            tryOnController != null &&
            tryOnController.IsTryOnActive
        )
        {
            return;
        }


        if (observer == null)
        {
            return;
        }


        TargetStatus status =
            observer.TargetStatus;


        bool tracked =
            status.Status ==
            Status.TRACKED;


        bool extendedTracked =
            status.Status ==
            Status.EXTENDED_TRACKED;


        bool shouldShow =
            tracked ||
            extendedTracked;


        if (
            shouldShow !=
            currentVisible
        )
        {
            SetVisible(
                shouldShow
            );


            Debug.Log(
                "[AURON TARGET] Watch " +
                (
                    shouldShow
                        ? "VISIBLE"
                        : "HIDDEN"
                ) +
                " | Status = " +
                status.Status
            );
        }
    }


    // =========================================================
    // VUFORIA EVENT
    // =========================================================

    private void OnTargetStatusChanged(
        ObserverBehaviour behaviour,
        TargetStatus status
    )
    {
        // -----------------------------------------------------
        // TRY ON does not depend on image target.
        // -----------------------------------------------------

        if (
            tryOnController != null &&
            tryOnController.IsTryOnActive
        )
        {
            return;
        }


        bool tracked =
            status.Status ==
            Status.TRACKED;


        bool extendedTracked =
            status.Status ==
            Status.EXTENDED_TRACKED;


        bool shouldShow =
            tracked ||
            extendedTracked;


        SetVisible(
            shouldShow
        );


        Debug.Log(
            "[AURON TARGET] " +
            behaviour.TargetName +
            " = " +
            status.Status
        );
    }


    // =========================================================
    // REFRESH
    // =========================================================

    public void Refresh()
    {
        if (observer == null)
        {
            return;
        }


        if (
            tryOnController != null &&
            tryOnController.IsTryOnActive
        )
        {
            return;
        }


        TargetStatus status =
            observer.TargetStatus;


        bool tracked =
            status.Status ==
            Status.TRACKED;


        bool extendedTracked =
            status.Status ==
            Status.EXTENDED_TRACKED;


        bool shouldShow =
            tracked ||
            extendedTracked;


        SetVisible(
            shouldShow
        );
    }


    // =========================================================
    // SHOW / HIDE
    // =========================================================

    private void SetVisible(
        bool visible
    )
    {
        currentVisible =
            visible;


        if (
            watchRenderers == null
        )
        {
            return;
        }


        for (
            int i = 0;
            i < watchRenderers.Length;
            i++
        )
        {
            if (
                watchRenderers[i] == null
            )
            {
                continue;
            }


            if (visible)
            {
                watchRenderers[i].enabled =
                    originalRendererStates[i];
            }
            else
            {
                watchRenderers[i].enabled =
                    false;
            }
        }
    }
}