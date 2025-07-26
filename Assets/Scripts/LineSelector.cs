using System;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class LineSelector : MonoBehaviour
{
    [SerializeField] XRRayInteractor rayInteractor;
    [SerializeField] LineRenderer hoveredLine;
    [SerializeField] Color highlightColor;
    [SerializeField] InputActionReference anchorAction;
    [SerializeField] HingeJointPlacer hingeJointPlacer;
    [SerializeField] FixedJointPlacer fixedJointPlacer;

    private Color originalColor;
    private RaycastHit hit;

    public enum JointType
    {
        NONE,
        FIXED,
        HINGE
    };
    private JointType currJointType = JointType.NONE;

    private void OnEnable()
    {
        if (anchorAction != null && anchorAction.action != null)
        {
            anchorAction.action.performed += onAnchorPerformed;
            anchorAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (anchorAction != null && anchorAction.action != null)
        {
            anchorAction.action.performed -= onAnchorPerformed;
            anchorAction.action.Disable();
        }

    }

    public void SetJointType(JointType joint)
    {
        currJointType = joint;
    }

    public void SetJointTypeByInt(int n)
    {
        SetJointType((JointType)n);
    }

    private void onAnchorPerformed(InputAction.CallbackContext context)
    {
        if (hoveredLine == null || hit.collider == null) return;
        switch (currJointType) {
            case JointType.FIXED:
                fixedJointPlacer.OnLineSelected(hoveredLine.transform, hit);
                break;
            case JointType.HINGE:
                hingeJointPlacer.OnLineSeleced(hoveredLine.transform, hit);
                SetJointType(JointType.NONE);
                break;
        }
    }

    private void Update()
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out hit) && hoveredLine==null)
        {
            hoveredLine = hit.collider.GetComponent<LineRenderer>();
            originalColor = hoveredLine.startColor;
            hoveredLine.startColor = highlightColor;
            hoveredLine.endColor = highlightColor;
        } 
        else if (hoveredLine)
        {
            hoveredLine.startColor = originalColor;
            hoveredLine.endColor= originalColor;
            hoveredLine= null;
        }
    }
}
