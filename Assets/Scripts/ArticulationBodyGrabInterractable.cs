using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ArticulationBodyGrabInteractable : XRBaseInteractable
{
    [Header("Articulation Body Settings")]
    [SerializeField] private ArticulationBody targetArticulationBody;
    [SerializeField] private bool preserveJointSettings = true;
    [SerializeField] private bool useKinematicOnGrab = true;
    [SerializeField] private float grabForce = 1000f;
    [SerializeField] private float grabDamping = 50f;
    [SerializeField] private float followSpeed = 10f;

    [Header("Rotation Control")]
    [SerializeField] private bool allowRotationControl = true;
    [SerializeField] private bool respectJointConstraints = true;
    [SerializeField] private bool moveEntireChain = false;

    [Header("Joint Control")]
    [SerializeField] private bool allowJointMovement = true;
    [SerializeField] private float jointMoveSpeed = 90f; // degrees per second for revolute joints
    [SerializeField] private KeyCode testMovePositive = KeyCode.Q;
    [SerializeField] private KeyCode testMoveNegative = KeyCode.E;

    // Store original joint settings
    private ArticulationDrive originalXDrive;
    private ArticulationDrive originalYDrive;
    private ArticulationDrive originalZDrive;
    private bool originalImmovable;
    private ArticulationJointType jointType;
    private ArticulationBody rootBody;

    // Grab state
    private bool isCurrentlyGrabbed = false;
    private Transform grabberTransform;
    private Vector3 grabOffset;
    private Quaternion grabRotationOffset;
    private OpenXRJointController jointController;

    protected override void Awake()
    {
        base.Awake();

        // Get the articulation body if not assigned
        if (targetArticulationBody == null)
            targetArticulationBody = GetComponent<ArticulationBody>();

        if (targetArticulationBody == null)
        {
            Debug.LogError($"No ArticulationBody found on {gameObject.name}. ArticulationBodyGrabInteractable requires an ArticulationBody component.");
            return;
        }

        // Store original joint settings
        StoreOriginalJointSettings();

        // Find the root of the articulation chain
        rootBody = targetArticulationBody;
        while (rootBody.transform.parent != null)
        {
            rootBody = rootBody.transform.parent.GetComponent<ArticulationBody>();
        }

        // Set up the interactable
        // Remove movement type since we're not inheriting from XRGrabInteractable
    }

    private void StoreOriginalJointSettings()
    {
        if (targetArticulationBody == null) return;

        originalXDrive = targetArticulationBody.xDrive;
        originalYDrive = targetArticulationBody.yDrive;
        originalZDrive = targetArticulationBody.zDrive;
        originalImmovable = targetArticulationBody.immovable;
        jointType = targetArticulationBody.jointType;
    }

    // Method to start grabbing (called by OpenXRJointController)
    public void StartGrab(Transform grabber)
    {
        if (targetArticulationBody == null || isCurrentlyGrabbed) return;

        isCurrentlyGrabbed = true;
        grabberTransform = grabber;

        // Calculate grab offset
        grabOffset = transform.position - grabberTransform.position;
        grabRotationOffset = Quaternion.Inverse(grabberTransform.rotation) * transform.rotation;
        Debug.Log("Grabbed");

        if (useKinematicOnGrab)
        {
            // Make the articulation body kinematic while grabbed
            var drive = targetArticulationBody.xDrive;
            drive.driveType = ArticulationDriveType.Velocity;
            targetArticulationBody.xDrive = drive;
        }
        else
        {
            // Set up drives for force-based grabbing
            SetupGrabDrives();
        }

        // Notify that grab started
        OnGrabStarted();
    }

    // Method to stop grabbing (called by OpenXRJointController)
    public void StopGrab()
    {
        if (targetArticulationBody == null || !isCurrentlyGrabbed) return;

        isCurrentlyGrabbed = false;
        grabberTransform = null;

        if (preserveJointSettings)
        {
            // Restore original joint settings
            RestoreOriginalJointSettings();
        }

        // Notify that grab ended
        OnGrabEnded();
    }

    // Events for other scripts to listen to
    public System.Action OnGrabStarted;
    public System.Action OnGrabEnded;

    private void SetupGrabDrives()
    {
        // Create drives for smooth following
        ArticulationDrive grabDrive = new ArticulationDrive
        {
            stiffness = grabForce,
            damping = grabDamping,
            forceLimit = float.MaxValue
        };

        // Apply to appropriate axes based on joint type
        if (jointType == ArticulationJointType.FixedJoint)
        {
            // For fixed joints, we can control all axes
            targetArticulationBody.xDrive = grabDrive;
            targetArticulationBody.yDrive = grabDrive;
            targetArticulationBody.zDrive = grabDrive;
        }
    }

    private void RestoreOriginalJointSettings()
    {
        if (targetArticulationBody == null) return;

        targetArticulationBody.xDrive = originalXDrive;
        targetArticulationBody.yDrive = originalYDrive;
        targetArticulationBody.zDrive = originalZDrive;
        //targetArticulationBody.immovable = originalImmovable;
    }

    private void Update()
    {
        if (!isCurrentlyGrabbed || grabberTransform == null || targetArticulationBody == null)
            return;

        if (useKinematicOnGrab)
        {
            // Direct kinematic movement
            UpdateKinematicMovement();
        }

        // Handle joint movement if allowed
        if (allowJointMovement)
        {
            HandleJointMovement();
        }
    }

    private void UpdateKinematicMovement()
    {
        // Calculate target position and rotation
        Vector3 targetPosition = grabberTransform.position + grabberTransform.rotation * grabOffset;
        Quaternion targetRotation = grabberTransform.rotation * grabRotationOffset;

        if (respectJointConstraints && jointType == ArticulationJointType.RevoluteJoint)
        {
            // For revolute joints, calculate the rotation around the joint axis
            UpdateRevoluteJointFollowing(targetPosition, targetRotation);
        }
        else if (jointType == ArticulationJointType.FixedJoint || !respectJointConstraints)
        {
            // For fixed joints or when ignoring constraints, move both position and rotation
            if (useKinematicOnGrab)
            {
                if (moveEntireChain)
                {
                    // Move the entire chain
                    Vector3 rootOffset = targetPosition - transform.position;
                    Quaternion rootRotationOffset = targetRotation * Quaternion.Inverse(transform.rotation);
                    rootBody.TeleportRoot(rootBody.transform.position + rootOffset, rootRotationOffset * rootBody.transform.rotation);
                }
                else
                {
                    // Direct teleportation for immediate response
                    targetArticulationBody.TeleportRoot(targetPosition, targetRotation);
                }
            }
            else
            {
                // Smooth movement using interpolation
                Vector3 currentPosition = targetArticulationBody.transform.position;
                Quaternion currentRotation = targetArticulationBody.transform.rotation;

                Vector3 newPosition = Vector3.Lerp(currentPosition, targetPosition, followSpeed * Time.deltaTime);
                Quaternion newRotation = Quaternion.Lerp(currentRotation, targetRotation, followSpeed * Time.deltaTime);

                targetArticulationBody.TeleportRoot(newPosition, newRotation);
            }
        }
        else
        {
            // For other joint types, just move position
            targetArticulationBody.TeleportRoot(targetPosition, targetArticulationBody.transform.rotation);
        }
    }

    private void UpdateRevoluteJointFollowing(Vector3 targetPosition, Quaternion targetRotation)
    {
        // Get the joint axis in world space
        Vector3 jointAxis = GetJointAxis();
        Vector3 jointPosition = GetJointPosition();

        // Calculate the desired rotation to make the object follow the controller
        Vector3 currentDirection = (transform.position - jointPosition).normalized;
        Vector3 targetDirection = (targetPosition - jointPosition).normalized;

        // Project both directions onto the plane perpendicular to the joint axis
        Vector3 currentProjected = Vector3.ProjectOnPlane(currentDirection, jointAxis);
        Vector3 targetProjected = Vector3.ProjectOnPlane(targetDirection, jointAxis);

        if (currentProjected.magnitude > 0.001f && targetProjected.magnitude > 0.001f)
        {
            // Calculate the angle between the projected vectors
            float angle = Vector3.SignedAngle(currentProjected, targetProjected, jointAxis);

            // Get current joint position and calculate new target
            float currentJointAngle = targetArticulationBody.jointPosition[0];
            float newJointAngle = currentJointAngle + (angle * Mathf.Deg2Rad);

            // Apply joint limits if they exist
            ArticulationDrive drive = targetArticulationBody.xDrive;
            if (drive.lowerLimit < drive.upperLimit)
            {
                newJointAngle = Mathf.Clamp(newJointAngle, drive.lowerLimit, drive.upperLimit);
            }

            // Set the joint target
            drive.target = newJointAngle;
            targetArticulationBody.xDrive = drive;
        }
    }

    private Vector3 GetJointAxis()
    {
        // Determine which axis the revolute joint rotates around
        // Check the motion settings to see which axis is free
        if (targetArticulationBody.twistLock == ArticulationDofLock.FreeMotion)
            return targetArticulationBody.transform.TransformDirection(Vector3.forward); // Z-axis (twist)
        else if (targetArticulationBody.swingYLock == ArticulationDofLock.FreeMotion)
            return targetArticulationBody.transform.TransformDirection(Vector3.up); // Y-axis
        else if (targetArticulationBody.swingZLock == ArticulationDofLock.FreeMotion)
            return targetArticulationBody.transform.TransformDirection(Vector3.right); // X-axis
        else
            // Default to Z-axis if we can't determine
            return targetArticulationBody.transform.TransformDirection(Vector3.forward);
    }

    private Vector3 GetJointPosition()
    {
        // The joint position is at the anchor point
        if (targetArticulationBody.transform.parent != null)
        {
            // Use the anchor position from the parent
            return targetArticulationBody.transform.parent.TransformPoint(targetArticulationBody.parentAnchorPosition);
        }
        else
        {
            // If this is the root, use the anchor position
            return targetArticulationBody.transform.TransformPoint(targetArticulationBody.anchorPosition);
        }
    }

    private void HandleJointMovement()
    {
        float moveInput = 0f;

        // Get input for joint movement (you can replace this with OpenXR input)
        //if (Input.GetKey(testMovePositive))
        //    moveInput = 1f;
        //else if (Input.GetKey(testMoveNegative))
        //    moveInput = -1f;

        // You can also get input from the grabbing hand controller
        // For example, using thumbstick or trigger values

        if (Mathf.Abs(moveInput) > 0.1f)
        {
            MoveJoint(moveInput);
        }
    }

    private void MoveJoint(float input)
    {
        if (jointType != ArticulationJointType.RevoluteJoint) return;

        // Calculate movement amount
        float moveAmount = input * jointMoveSpeed * Time.deltaTime;

        // Get current joint position
        float currentPosition = targetArticulationBody.jointPosition[0];
        float newPosition = currentPosition + Mathf.Deg2Rad * moveAmount;

        // Check joint limits
        ArticulationDrive drive = targetArticulationBody.xDrive;
        if (drive.lowerLimit < drive.upperLimit)
        {
            newPosition = Mathf.Clamp(newPosition, drive.lowerLimit, drive.upperLimit);
        }

        // Set target position
        drive.target = newPosition;
        targetArticulationBody.xDrive = drive;
    }

    // Method to temporarily disable joint constraints for free movement
    public void SetFreeMovement(bool enabled)
    {
        if (targetArticulationBody == null) return;

        if (enabled)
        {
            // Store current settings
            StoreOriginalJointSettings();

            // Temporarily change to fixed joint for free movement
            targetArticulationBody.jointType = ArticulationJointType.FixedJoint;
            targetArticulationBody.immovable = true;
        }
        else
        {
            // Restore original joint type and settings
            targetArticulationBody.jointType = jointType;
            RestoreOriginalJointSettings();
        }
    }

    // Method to set joint to specific angle (in degrees)
    public void SetJointAngle(float angleInDegrees)
    {
        if (targetArticulationBody == null || jointType != ArticulationJointType.RevoluteJoint)
            return;

        float angleInRadians = Mathf.Deg2Rad * angleInDegrees;

        ArticulationDrive drive = targetArticulationBody.xDrive;

        // Check limits
        if (drive.lowerLimit < drive.upperLimit)
        {
            angleInRadians = Mathf.Clamp(angleInRadians, drive.lowerLimit, drive.upperLimit);
        }

        drive.target = angleInRadians;
        targetArticulationBody.xDrive = drive;
    }

    // Get current joint angle in degrees
    public float GetJointAngle()
    {
        if (targetArticulationBody == null || jointType != ArticulationJointType.RevoluteJoint)
            return 0f;

        return Mathf.Rad2Deg * targetArticulationBody.jointPosition[0];
    }

    // Method to move joint from external scripts (e.g., using OpenXR input)
    public void MoveJointWithInput(float normalizedInput)
    {
        if (!isCurrentlyGrabbed || !allowJointMovement) return;
        MoveJoint(normalizedInput);
    }

    // Properties
    public bool IsGrabbed => isCurrentlyGrabbed;
    public ArticulationBody ArticulationBody => targetArticulationBody;
    public ArticulationJointType JointType => jointType;
    public bool RespectJointConstraints
    {
        get => respectJointConstraints;
        set => respectJointConstraints = value;
    }
}