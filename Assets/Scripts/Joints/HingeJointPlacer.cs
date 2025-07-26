using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Rendering;
using UnityEngine.InputSystem.Utilities;
using System.IO;
using UnityEditor;

public class HingeJointPlacer : MonoBehaviour
{
    public InputDevice controller;
    public Transform rightControllerTransform;
    public LineSelector lineSelector;
    public GameObject Quad;

    private bool primaryButtonWasPressed = false;

    private void Update()
    {
        controller = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    public void OnLineSeleced(Transform line, RaycastHit hit)
    {
        // Reset the Joint selection
        GameObject quad = Instantiate(Quad, hit.point,
            Quaternion.LookRotation(Camera.main.transform.forward), line);
        Debug.Log("Quad Instantiated");

        if (line.parent == null)
        {
            StartCoroutine(AxisInterface(quad, dir =>
        {
            //GameObject parent = new GameObject("Fixed Object");
            //parent.AddComponent<ArticulationBody>().immovable = true;
            //line.transform.parent = parent.transform;

            // Set the hinge
            //ArticulationBody joint = line.gameObject.AddComponent<ArticulationBody>();
            //joint.jointType = ArticulationJointType.RevoluteJoint;
            //joint.anchorPosition = line.InverseTransformPoint(hit.point);
            //line.gameObject.GetComponent<MeshCollider>().convex = true;
            //joint.anchorRotation = dir;
            //var drive = joint.xDrive;
            //drive.driveType = ArticulationDriveType.Target;
            //joint.xDrive = drive;
            //Debug.Log("Joint Set");

            //// Add Articulatino Joint Grab Interactable
            //joint.gameObject.AddComponent<ArticulationBodyGrabInteractable>();

            // Create Proxy
            //CreateProxy(joint, Vector3.right, Vector3.one * 0.2f);

            SetJoint(line, hit.point, dir, quad);
        }));
        } else
        {
            SetJoint(line, hit.point, line.parent.forward, quad);
        }
    }

    public void SetJoint(Transform line, Vector3 anchor, Vector3 dir, GameObject quad)
    {
        HingeJoint joint = line.gameObject.AddComponent<HingeJoint>();
        joint.anchor = line.transform.InverseTransformPoint(anchor);
        joint.axis = dir;

        Rigidbody rb = line.gameObject.GetComponent<Rigidbody>();
        rb.useGravity = false;

        line.gameObject.GetComponent<MeshCollider>().convex = true;

        // Make the line grabbable
        XRGrabInteractable inter = line.gameObject.AddComponent<XRGrabInteractable> ();
        inter.movementType = XRBaseInteractable.MovementType.VelocityTracking;
        inter.interactionLayers = InteractionLayerMask.GetMask("Lines");

        // Remove Simple interactable
        Destroy(line.gameObject.GetComponent<XRSimpleInteractable>());

        // Add Dynamic attach transform
        line.gameObject.AddComponent<DynamicGrabAnchor>();

        //Activate the quad when grabbed
        inter.selectEntered.AddListener(args =>
        {
            quad.SetActive(true);
        });
        // Deactivate the quad when released
        inter.selectExited.AddListener(args =>
        {
            quad.SetActive(false);
        });

        quad.SetActive(false);
    }

    public void CreateProxy(ArticulationBody targetJoint, Vector3 localOffset, Vector3 colliderSize)
    {
        GameObject proxy = GameObject.CreatePrimitive(PrimitiveType.Cube);
        proxy.name = "Grab Proxy";

        // Position it relative to the target articulation body
        proxy.transform.position = targetJoint.transform.TransformPoint(localOffset);
        proxy.transform.rotation = targetJoint.transform.rotation;
        proxy.transform.parent = targetJoint.transform.parent;

        // Add kinematic Rigidbody
        Rigidbody rb = proxy.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // Add Collider for interaction
        BoxCollider col = proxy.AddComponent<BoxCollider>();
        col.size = colliderSize;
        col.isTrigger = false;  // Can be true if you want no collision

        // Add XR Grab Interactable
        XRGrabInteractable interactable = proxy.AddComponent<XRGrabInteractable>();
        interactable.movementType = XRBaseInteractable.MovementType.Kinematic;  // Default
        interactable.trackPosition = true;
        interactable.trackRotation = true;

        // You can also add a script here to track proxy rotation and apply it to the articulation joint
        var tracker = proxy.AddComponent<ProxyHingeController>();
        tracker.Init();
        tracker.axis = targetJoint.anchorRotation * Vector3.up ;
        tracker.targetJoint = targetJoint;
    }

    IEnumerator AxisInterface(GameObject quad, System.Action<Vector3> callback)
    {
        ActionPrompt.Instance.SetActionPrompt("Rotate the controller to orient the Plane of Rotation and press A to finalize.");
        primaryButtonWasPressed = true;
        yield return null;
        while ((controller.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryPressed) && !primaryPressed) || primaryButtonWasPressed)
        {
            quad.transform.forward = -1* rightControllerTransform.forward;
            primaryButtonWasPressed = primaryPressed;
            yield return null;
        }
        ActionPrompt.Instance.ClearPrompt();
        callback?.Invoke(quad.transform.forward);
    }
}
