using UnityEngine;

public class FCPColorSessionCache : MonoBehaviour
{
    public FlexibleColorPicker fcp;
    private Color cachedColor;
    private bool hasColor = false;

    void OnEnable()
    {
        if (hasColor)
        {
            fcp.color = cachedColor;
        }
    }

    void OnDisable()
    {
        cachedColor = fcp.color;
        hasColor = true;
    }
}
