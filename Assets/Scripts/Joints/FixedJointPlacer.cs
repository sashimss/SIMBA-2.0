using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FixedJointPlacer : MonoBehaviour
{
    private FixedJoint joint;
    private Vector3 lastHitPoint;
    public void OnLineSelected(Transform line, RaycastHit hit)
    {
        Rigidbody rb;
        rb = line.gameObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = line.gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        line.gameObject.GetComponent<MeshCollider>().convex = true;


        if (joint != null)
        {
            XRGrabInteractable inter;
            if (joint.gameObject.TryGetComponent<XRGrabInteractable>(out inter))
            {
                inter.colliders.Add(line.GetComponent<MeshCollider>());
            }

            if (line.gameObject.TryGetComponent<XRGrabInteractable>(out inter))
            {
                inter.colliders.Add(line.GetComponent<MeshCollider>());
            }
            // Connect the bodies
            rb.useGravity = false;
            joint.connectedBody = rb;
            //joint.anchor = (hit.point + lastHitPoint) / 2f;

            // Cleanup
            joint = null;
            ActionPrompt.Instance.ClearPrompt();
            return;
        }
        joint = line.gameObject.AddComponent<FixedJoint>();
        lastHitPoint = hit.point;
        ActionPrompt.Instance.SetActionPrompt("Select the other body to join.");
    }
}
