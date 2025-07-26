using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.HapticsUtility;

public class PlaneMaker : MonoBehaviour
{
    [SerializeField] private GameObject Quad;
    [SerializeField] private GameObject plane_pref;
    [SerializeField] private VRControllerSketch Sketcher;
    public InputDevice controller;
    public Transform rightControllerTransform;
    private bool primaryButtonWasPressed = false;
    private void Update()
    {
        controller = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        if (controller.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryPressed) && secondaryPressed)
        {
            Sketcher.ClearPlane();
        }
    }

    public void SelectPlane()
    {
        StartCoroutine(GetPlane());
    }

    public void MakePlane()
    {
        GameObject quad = Instantiate(Quad);

        StartCoroutine(AxisInterface(quad, t =>
        {
            GameObject plane = Instantiate(plane_pref, t.position, t.rotation);
            plane.layer = LayerMask.NameToLayer("Planes");
            Sketcher.SetPlane(plane.transform);
            Destroy(quad);
        }));
    }

    IEnumerator GetPlane()
    {
        primaryButtonWasPressed = true;
        //while ((controller.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryPressed) && !primaryPressed) || primaryButtonWasPressed)
        //{

        //}
        yield return null;
    }

    IEnumerator AxisInterface(GameObject quad, System.Action<Transform> callback)
    {
        primaryButtonWasPressed = true;
        ActionPrompt.Instance.SetActionPrompt("Rotate the controller to orient the Plane of Rotation and press A to finalize.");
        yield return null;
        while ((controller.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryPressed) && !primaryPressed) || primaryButtonWasPressed)
        {
            quad.transform.forward = -1 * rightControllerTransform.forward;
            quad.transform.position = rightControllerTransform.position;
            primaryButtonWasPressed = primaryPressed;
            yield return null;
        }
        callback?.Invoke(quad.transform);
    }
}
