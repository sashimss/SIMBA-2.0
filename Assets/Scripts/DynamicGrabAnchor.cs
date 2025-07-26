using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class DynamicGrabAnchor : MonoBehaviour
{
    private Transform dynamicAttachPoint;
    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        if (args.interactorObject is XRRayInteractor rayInteractor)
        {
            if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                if (dynamicAttachPoint != null)
                    Destroy(dynamicAttachPoint.gameObject);

                GameObject attachGO = new GameObject("DynamicAttachPoint");
                attachGO.transform.position = hit.point;
                attachGO.transform.rotation = Quaternion.LookRotation(-1 * args.interactorObject.transform.forward);
                attachGO.transform.SetParent(transform);
                dynamicAttachPoint = attachGO.transform;

                grabInteractable.attachTransform = dynamicAttachPoint;
            }
        }
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (dynamicAttachPoint != null)
        {
            Destroy(dynamicAttachPoint.gameObject);
            grabInteractable.attachTransform = null;
            dynamicAttachPoint = null;
        }
    }

}