using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class OpenXRJointController : MonoBehaviour
{
    [Header("Input References")]
    [SerializeField] private InputActionProperty gripAction;
    [SerializeField] private InputActionProperty triggerAction;
    [SerializeField] private InputActionProperty thumbstickAction;

    [Header("Grab Settings")]
    [SerializeField] private float grabThreshold = 0.5f;
    [SerializeField] private float releaseThreshold = 0.3f;
    [SerializeField] private LayerMask interactableLayerMask = -1;
    [SerializeField] private float grabRange = 0.1f;

    [Header("Movement Settings")]
    [SerializeField] private bool allowFreeRotation = false;
    [SerializeField] private bool moveEntireChain = false;

    [Header("Joint Control Settings")]
    [SerializeField] private bool useTriggerForJointControl = true;
    [SerializeField] private bool useThumbstickForJointControl = false;
    [SerializeField] private float inputDeadzone = 0.1f;
    [SerializeField] private float inputSensitivity = 1f;

    private ArticulationBodyGrabInteractable currentGrabbedArticulation;
    private bool wasGripPressed = false;
    private Collider[] nearbyColliders = new Collider[10];

    [Header("Visual Feedback")]
    [SerializeField] private Transform grabPoint;
    [SerializeField] private LineRenderer grabLineRenderer;

    private void Awake()
    {
        // Set up grab point if not assigned
        if (grabPoint == null)
            grabPoint = transform;

        // Set up line renderer for visual feedback
        if (grabLineRenderer == null)
            grabLineRenderer = GetComponent<LineRenderer>();

        if (grabLineRenderer != null)
        {
            grabLineRenderer.enabled = false;
            grabLineRenderer.startWidth = 0.01f;
            grabLineRenderer.endWidth = 0.01f;
            grabLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            grabLineRenderer.startColor = Color.blue;
        }
    }

    private void OnEnable()
    {
        // Enable input actions
        gripAction.action?.Enable();
        triggerAction.action?.Enable();
        thumbstickAction.action?.Enable();
    }

    private void OnDisable()
    {
        // Disable input actions
        gripAction.action?.Disable();
        triggerAction.action?.Disable();
        thumbstickAction.action?.Disable();

        // Release any grabbed object
        if (currentGrabbedArticulation != null)
        {
            currentGrabbedArticulation.StopGrab();
            currentGrabbedArticulation = null;
        }
    }

    private void Update()
    {
        HandleGripInput();

        if (currentGrabbedArticulation != null && currentGrabbedArticulation.IsGrabbed)
        {
            HandleJointControl();
        }

        UpdateVisualFeedback();

        // Keyboard input for testing (remove in production)
        //HandleKeyboardInput();
    }

    private void HandleGripInput()
    {
        // Safety check for input action
        if (gripAction.action == null)
        {
            Debug.LogWarning($"Grip action not assigned on {gameObject.name}. Please assign the grip input action in the inspector.");
            return;
        }

        float gripValue = 0f;
        try
        {
            gripValue = gripAction.action.ReadValue<float>();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to read grip input: {e.Message}");
            return;
        }

        bool gripPressed = gripValue > grabThreshold;
        bool gripReleased = gripValue < releaseThreshold;

        // Start grab
        if (gripPressed && !wasGripPressed && currentGrabbedArticulation == null)
        {
            TryGrabNearbyArticulationBody();
        }
        // Release grab
        else if (gripReleased && wasGripPressed && currentGrabbedArticulation != null)
        {
            currentGrabbedArticulation.StopGrab();
            currentGrabbedArticulation = null;
        }

        wasGripPressed = gripPressed;
    }

    private void TryGrabNearbyArticulationBody()
    {
        // Find nearby articulation body interactables
        int hitCount = Physics.OverlapSphereNonAlloc(grabPoint.position, grabRange, nearbyColliders, interactableLayerMask);

        ArticulationBodyGrabInteractable closestInteractable = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            var interactable = nearbyColliders[i].GetComponent<ArticulationBodyGrabInteractable>();
            if (interactable != null && !interactable.IsGrabbed)
            {
                float distance = Vector3.Distance(grabPoint.position, nearbyColliders[i].transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }

        // Grab the closest interactable
        if (closestInteractable != null)
        {
            currentGrabbedArticulation = closestInteractable;

            // Configure movement settings
            if (allowFreeRotation)
            {
                currentGrabbedArticulation.RespectJointConstraints = false;
            }

            currentGrabbedArticulation.StartGrab(transform);
        }
    }

    private void HandleJointControl()
    {
        float inputValue = 0f;

        if (useTriggerForJointControl && triggerAction.action != null && gripAction.action != null)
        {
            try
            {
                // Use trigger pressure for joint control
                float triggerValue = triggerAction.action.ReadValue<float>();
                float gripValueForJoint = gripAction.action.ReadValue<float>();

                // Trigger moves positive, grip moves negative
                inputValue = triggerValue - gripValueForJoint;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to read trigger/grip input for joint control: {e.Message}");
                return;
            }
        }
        else if (useThumbstickForJointControl && thumbstickAction.action != null)
        {
            try
            {
                // Use thumbstick for joint control
                Vector2 thumbstick = thumbstickAction.action.ReadValue<Vector2>();
                inputValue = thumbstick.y; // Use Y axis of thumbstick
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to read thumbstick input: {e.Message}");
                return;
            }
        }

        // Apply deadzone
        if (Mathf.Abs(inputValue) < inputDeadzone)
            inputValue = 0f;

        // Apply sensitivity
        inputValue *= inputSensitivity;

        // Send input to the articulation body grab interactable
        if (Mathf.Abs(inputValue) > 0f)
        {
            currentGrabbedArticulation.MoveJointWithInput(inputValue);
        }
    }

    private void UpdateVisualFeedback()
    {
        if (grabLineRenderer == null) return;

        // Show line to nearby grabbable objects when grip is being pressed
        float gripValue = 0f;

        if (gripAction.action != null)
        {
            try
            {
                gripValue = gripAction.action.ReadValue<float>();
            }
            catch (System.Exception)
            {
                // Silently handle input errors for visual feedback
                gripValue = 0f;
            }
        }

        if (gripValue > 0.1f && currentGrabbedArticulation == null)
        {
            // Find closest grabbable object
            int hitCount = Physics.OverlapSphereNonAlloc(grabPoint.position, grabRange, nearbyColliders, interactableLayerMask);

            ArticulationBodyGrabInteractable closestInteractable = null;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                var interactable = nearbyColliders[i].GetComponent<ArticulationBodyGrabInteractable>();
                if (interactable != null && !interactable.IsGrabbed)
                {
                    float distance = Vector3.Distance(grabPoint.position, nearbyColliders[i].transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestInteractable = interactable;
                    }
                }
            }

            // Show line to closest object
            if (closestInteractable != null)
            {
                grabLineRenderer.enabled = true;
                grabLineRenderer.positionCount = 2;
                grabLineRenderer.SetPosition(0, grabPoint.position);
                grabLineRenderer.SetPosition(1, closestInteractable.transform.position);
                grabLineRenderer.startColor = gripValue > grabThreshold ? Color.green : Color.yellow;
            }
            else
            {
                grabLineRenderer.enabled = false;
            }
        }
        else if (currentGrabbedArticulation != null)
        {
            // Show line to grabbed object
            grabLineRenderer.enabled = true;
            grabLineRenderer.positionCount = 2;
            grabLineRenderer.SetPosition(0, grabPoint.position);
            grabLineRenderer.SetPosition(1, currentGrabbedArticulation.transform.position);
            grabLineRenderer.startColor = Color.green;
        }
        else
        {
            grabLineRenderer.enabled = false;
        }
    }

    // Public method to get current grab state
    public bool IsGrabbing => currentGrabbedArticulation != null;

    // Public method to get currently grabbed object
    public ArticulationBodyGrabInteractable GrabbedObject => currentGrabbedArticulation;

    // Helper method to validate input actions setup
    [ContextMenu("Validate Input Actions")]
    public void ValidateInputActions()
    {
        bool allValid = true;

        if (gripAction.action == null)
        {
            Debug.LogError($"Grip Action not assigned on {gameObject.name}");
            allValid = false;
        }

        if (useTriggerForJointControl && triggerAction.action == null)
        {
            Debug.LogError($"Trigger Action not assigned on {gameObject.name} but is required for joint control");
            allValid = false;
        }

        if (useThumbstickForJointControl && thumbstickAction.action == null)
        {
            Debug.LogError($"Thumbstick Action not assigned on {gameObject.name} but is required for joint control");
            allValid = false;
        }

        if (allValid)
        {
            Debug.Log($"All input actions properly configured on {gameObject.name}");
        }
    }

    // Alternative keyboard input for testing (remove in production)
    private void HandleKeyboardInput()
    {
        if (Application.isEditor)
        {
            // Test grab with Space key
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (currentGrabbedArticulation == null)
                    TryGrabNearbyArticulationBody();
                else
                {
                    currentGrabbedArticulation.StopGrab();
                    currentGrabbedArticulation = null;
                }
            }
        }
    }
}