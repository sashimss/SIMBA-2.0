using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.HapticsUtility;

/// <summary>
/// When trigger is held down, appends points to LineRenderer component when the controller's
/// position changes more than the thresold.
/// </summary>
/// <remarks>
/// Code made by Sashim, email:sashimsuryawanshi@gmail.com  
/// </remarks>
public class VRControllerSketch : MonoBehaviour
{
    [SerializeField] Transform controller_transform;
    [SerializeField] float width;
    [SerializeField] float positionChangeThreshold = 0.01f; // Minimum movement to trigger a log
    [SerializeField] Material lineMat;
    [SerializeField] GameObject MenuUI;

    public float widthSliderValue { get; set; } = .2f;
    public Color sketchColor { get; set; } = Color.white;
    public Transform currentPlane = null;

    private Vector3 lastRightHandPosition;
    private int lineCount;
    private LineRenderer currentLine;

    private void Start()
    {
        Debug.Log(LayerMask.GetMask("String"));
    }

    void Update()
    {
        // Sketch if UI is inactive
        if (!MenuUI.activeSelf || !EventSystem.current.IsPointerOverGameObject())
        {
            SketchWithController(XRNode.RightHand, ref lastRightHandPosition);
        }
    }

    void SketchWithController(XRNode node, ref Vector3 lastPosition)
    {
        InputDevice controller = InputDevices.GetDeviceAtXRNode(node);

        if (controller.isValid)
        {
            if (controller.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
            {
                
                if (triggerValue > .1f)
                {
                    Vector3 position = GetPointPosition();
                    // trigger pressed check if line exists
                    if (!currentLine)
                    {
                        // Make line if it doesn't exists
                        currentLine = (new GameObject($"Line {lineCount}")).AddComponent<LineRenderer>();
                        currentLine.useWorldSpace = false;
                        currentLine.positionCount = 1;
                        currentLine.widthMultiplier = width * widthSliderValue;
                        currentLine.material = lineMat;
                        currentLine.startColor = sketchColor;

                        // Set plane
                        if (currentPlane != null)
                        {
                            currentLine.transform.parent = currentPlane;
                        }

                        currentLine.transform.position = position;
                        currentLine.SetPosition(0, currentLine.transform.InverseTransformPoint(position));
                        lastPosition = position;
                    }
                    else
                    {
                        // update the line if it exists
                        if (Vector3.Distance(lastPosition, position) > positionChangeThreshold)
                        {
                            currentLine.positionCount++;
                            Vector3[] vertices = new Vector3[currentLine.positionCount];
                            currentLine.GetPositions(vertices);
                            vertices[vertices.Length - 1] = currentLine.transform.InverseTransformPoint(position);
                            currentLine.SetPositions(vertices);
                            lastPosition = position;
                        }
                    }
                }
                else if (currentLine)
                {
                    //trigger released
                    currentLine.gameObject.layer = LayerMask.NameToLayer("Lines");
                    currentLine.gameObject.AddComponent<XRSimpleInteractable>().interactionLayers = InteractionLayerMask.GetMask("Lines");
                    Undo_B.AddLine(currentLine.gameObject);

                    // Bake the mesh
                    Mesh mesh = new Mesh();
                    currentLine.BakeMesh(mesh);
                    currentLine.gameObject.AddComponent<MeshCollider>().sharedMesh = mesh;
                    currentLine.gameObject.GetComponent<MeshCollider>().convex = true ;

                    // Log action
                    ActionLogger.Instance.LogAction($"Created Line {lineCount}");

                    // Cleanup
                    currentLine.endColor = sketchColor;
                    lastPosition = Vector3.zero;
                    currentLine = null;
                    lineCount++;
                }

            }
        }
    }

    public Vector3 GetPointPosition()
    {
        if (currentPlane != null &&
            Physics.Raycast(controller_transform.position, controller_transform.forward, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Planes") ))
        {
            return hit.point + currentPlane.forward * 0.01f ;
        }
        return controller_transform.position ;
        
    }

    public void SetPlane(Transform plane)
    {
        currentPlane = plane;
    }

    public void ClearPlane()
    {
        currentPlane = null;
    }

    public void LogWidth()
    {
        ActionLogger.Instance.LogAction($"Width changed to {widthSliderValue}");
    }

    public void LogColor()
    {
        ActionLogger.Instance.LogAction($"Color changed to {sketchColor}");
    }
}