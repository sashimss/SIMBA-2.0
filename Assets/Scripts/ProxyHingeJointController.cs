using UnityEngine;

public class ProxyHingeController : MonoBehaviour
{
    public ArticulationBody targetJoint;
    public Vector3 axis = Vector3.up;  // Local axis of hinge
    private Quaternion initialRotation;

    public void Init()
    {
        initialRotation = transform.rotation;
    }

    void Update()
    {
        Vector3 hingeAxisWorld = targetJoint.transform.TransformDirection(axis);
        Vector3 baseForward = initialRotation * Vector3.forward;
        Vector3 currentForward = transform.forward;

        float angle = Vector3.SignedAngle(baseForward, currentForward, hingeAxisWorld);

        var drive = targetJoint.xDrive;
        drive.target = angle;
        targetJoint.xDrive = drive;
    }
}
