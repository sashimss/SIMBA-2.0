using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VRControllerGrab : MonoBehaviour
{
    [SerializeField] Transform controllerTransform;
    [SerializeField] float grabRange;
    [SerializeField] LayerMask linesLayerMask;
    [SerializeField] Color highlightColor;
    [SerializeField] Color selectedColor;
    [SerializeField] LineRenderer selectedLine;
    [SerializeField] LineRenderer highlightedLine;
    [SerializeField] XRInteractionManager XRInteractionManager;

    private InputDevice rightController;

    void Start()
    {
        rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    void Update()
    {
        // Raycast to detect lines
        if (Physics.Raycast(controllerTransform.position, controllerTransform.forward, out RaycastHit hit, grabRange, linesLayerMask))
        {
            LineRenderer currentHitLine = hit.collider.GetComponent<LineRenderer>();

            // Ensure we are not re-highlighting the same line or a line that already has a Rigidbody (meaning it's already anchored)
            if (currentHitLine != highlightedLine && (currentHitLine != null && !currentHitLine.GetComponent<Rigidbody>()))
            {
                // Reset previous highlight if a different line is now highlighted
                if (highlightedLine != null)
                {
                    highlightedLine.startColor = Color.white;
                    highlightedLine.endColor = Color.white;
                }
                highlightedLine = currentHitLine;
                highlightedLine.startColor = highlightColor;
                highlightedLine.endColor = highlightColor;
            }
            else if (currentHitLine == null || currentHitLine.GetComponent<Rigidbody>()) // If hit something else or an anchored line
            {
                if (highlightedLine != null)
                {
                    highlightedLine.startColor = Color.white;
                    highlightedLine.endColor = Color.white;
                }
                highlightedLine = null;
            }


            // Primary button pressed: Place an anchor point (Hinge Joint)
            // Only add components if they don't already exist to prevent duplicates
            if (highlightedLine != null && rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryPressed) && primaryPressed)
            {
                // Prevent multiple anchors on the same line
                if (highlightedLine.GetComponent<HingeJoint>() == null)
                {
                    // Add Rigidbody if not already present
                    Rigidbody rb = highlightedLine.GetComponent<Rigidbody>();
                    if (rb == null)
                    {
                        rb = highlightedLine.gameObject.AddComponent<Rigidbody>();
                        rb.useGravity = false; // Set to true if you want gravity to affect the line
                        rb.isKinematic = false; // Must be false for physics and joints to work
                        rb.mass = 1.0f; // Give it a reasonable mass
                    }

                    // Add HingeJoint
                    HingeJoint Joint = highlightedLine.gameObject.AddComponent<HingeJoint>();
                    Joint.anchor = highlightedLine.transform.InverseTransformPoint(hit.point);

                    // *** IMPORTANT FIX: Set the Hinge Joint Axis ***
                    // This defines the axis of rotation for the hinge, in the object's local space.
                    // A common choice for a line drawn mostly on a horizontal plane is Vector3.up (local Y-axis).
                    // If your lines are typically drawn vertically, you might need Vector3.right or Vector3.forward.
                    Joint.axis = Vector3.up; // Start with this, adjust if needed (e.g., Vector3.forward, Vector3.right)

                    // Optional: Set limits if you don't want 360-degree rotation
                    // Joint.useLimits = true;
                    // Joint.limits = new JointLimits { min = -90, max = 90 }; // Example: limit rotation to +/- 90 degrees

                    // Ensure the MeshCollider is convex for physics interactions with a Rigidbody
                    MeshCollider meshCollider = highlightedLine.GetComponent<MeshCollider>();
                    if (meshCollider != null)
                    {
                        meshCollider.convex = true;
                    }
                    else
                    {
                        Debug.LogWarning("MeshCollider not found on line. HingeJoint physics may not behave as expected.");
                    }

                    // Add XRGrabInteractable if not already present
                    XRGrabInteractable inter = highlightedLine.GetComponent<XRGrabInteractable>();
                    if (inter == null)
                    {
                        inter = highlightedLine.gameObject.AddComponent<XRGrabInteractable>();
                        inter.interactionManager = XRInteractionManager;
                        inter.movementType = XRBaseInteractable.MovementType.VelocityTracking; // Or Instantaneous, Kinematic
                        inter.trackRotation = true;
                        // inter.matchAttachRotation = true; // Consider enabling this if you want the grabbed object to precisely match controller rotation
                        // Be aware this might conflict with HingeJoint if not carefully managed.
                    }

                    // Add DynamicGrabAnchor for dynamic attach point if not already present
                    //if (highlightedLine.GetComponent<DynamicGrabAnchor>() == null)
                    //{
                    //    highlightedLine.gameObject.AddComponent<DynamicGrabAnchor>();
                    //}

                    // Update colors and clear highlight
                    highlightedLine.startColor = selectedColor;
                    highlightedLine.endColor = selectedColor;
                    highlightedLine = null; // Clear the highlighted line after anchoring
                }
            }
        }
        else // No line hit by raycast
        {
            if (highlightedLine != null)
            {
                // If no lines are hit, reset the last highlighted line's color
                highlightedLine.startColor = Color.white;
                highlightedLine.endColor = Color.white;
                highlightedLine = null;
            }
        }
    }
}
