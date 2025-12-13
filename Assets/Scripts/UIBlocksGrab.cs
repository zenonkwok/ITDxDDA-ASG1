using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Prevents grabbing when the pointer (touch/cursor) is over screen-space UI
/// by temporarily removing the Grabbable layer from the XR Ray Interactor's
/// interaction layer mask. This cleanly separates UI vs object interactions
/// without needing colliders on every UI button.
/// Attach to the XR Ray Interactor in your AR/XR Origin.
/// </summary>
[RequireComponent(typeof(XRRayInteractor))]
public class UIBlocksGrab : MonoBehaviour
{
    [Header("Layers")]
    [Tooltip("Interaction Layer used for UI-only interactables.")]
    public InteractionLayerMask uiLayerMask; // e.g., UI

    [Tooltip("Interaction Layer used for grabbable objects.")]
    public InteractionLayerMask grabbableLayerMask; // e.g., Grabbable

    XRRayInteractor rayInteractor;
    InteractionLayerMask combinedMask;

    void Awake()
    {
        rayInteractor = GetComponent<XRRayInteractor>();
        combinedMask = uiLayerMask | grabbableLayerMask;
        rayInteractor.interactionLayers = combinedMask;
    }

    void Update()
    {
        // If grab is already active, keep grabbable layers enabled even when hovering UI
        if (rayInteractor.hasSelection)
        {
            rayInteractor.interactionLayers = combinedMask;
            return;
        }

        // Otherwise, when hovering UI, only allow UI interactions
        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1);
        rayInteractor.interactionLayers = overUI ? uiLayerMask : combinedMask;
    }
}
