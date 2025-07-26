using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleGameObjectByInput : MonoBehaviour
{
    /// <summary>
    /// This script toggle a gameobject based on the input action
    /// </summary>
    [SerializeField] InputActionReference toggleAction;
    [SerializeField] GameObject targetObject;

    private void OnEnable()
    {
        if (toggleAction != null && toggleAction.action != null)
        {
            toggleAction.action.performed += OnTogglePerformed;
            toggleAction.action.Enable();
        }
    }
    private void OnDisable()
    {

        if (toggleAction != null && toggleAction.action != null)
        {
            toggleAction.action.performed -= OnTogglePerformed;
            toggleAction.action.Disable();
        }
    }
    private void OnTogglePerformed(InputAction.CallbackContext context)
    {
        if (targetObject != null)
        {
            targetObject.SetActive(!targetObject.activeSelf);
        }
    }
}
