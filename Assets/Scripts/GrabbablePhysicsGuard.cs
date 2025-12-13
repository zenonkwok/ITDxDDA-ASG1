using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Ensures AR grabbable objects don't carry momentum or drift.
/// - While grabbed: sets Rigidbody non-kinematic (optional) for interactor control.
/// - On release: zeros velocity/angVel, sets Rigidbody kinematic and disables gravity.
/// Also disables XRGrabInteractable throw-on-detach to prevent fling.
/// Attach to objects with XRGrabInteractable + Rigidbody.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class GrabbablePhysicsGuard : MonoBehaviour
{
    Rigidbody rb;
    XRGrabInteractable grab;

    [Header("Behavior")]
    [Tooltip("Set Rigidbody non-kinematic while grabbed for smoother interactor control.")]
    public bool nonKinematicWhileGrabbed = false;

    [Tooltip("Use gravity while grabbed (usually false for AR).")]
    public bool gravityWhileGrabbed = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grab = GetComponent<XRGrabInteractable>();

        // Default AR-friendly physics
        rb.useGravity = false;
        rb.isKinematic = true;

        // Prevent throw velocity being applied on release
        grab.throwOnDetach = false;

        // Prefer kinematic movement for AR
        grab.movementType = XRBaseInteractable.MovementType.Kinematic;
    }

    void OnEnable()
    {
        grab.selectEntered.AddListener(OnGrabbed);
        grab.selectExited.AddListener(OnReleased);
    }

    void OnDisable()
    {
        grab.selectEntered.RemoveListener(OnGrabbed);
        grab.selectExited.RemoveListener(OnReleased);
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        if (nonKinematicWhileGrabbed)
            rb.isKinematic = false;
        rb.useGravity = gravityWhileGrabbed;

        // Clear any residual motion at grab start
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void OnReleased(SelectExitEventArgs args)
    {
        // Stop all motion and freeze
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;
    }
}
