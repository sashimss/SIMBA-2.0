using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Enlarges and boldens the text when the pointer hovers over a UI element.
/// </summary>
/// <remarks>
/// Code made by Ruthvik R, email: ruthvik.racha2005@gmail.com
/// </remarks>
public class TextZoomAndBoldenOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// Reference to the Text component.
    /// </summary>
    private Text uiText;

    /// <summary>
    /// Stores the original scale of the text.
    /// </summary>
    private Vector3 originalScale;

    /// <summary>
    /// Called on the frame when a script is enabled, before any of the Update methods are called.
    /// Initializes the reference to the Text component and stores its original scale.
    /// </summary>
    void Start()
    {
        uiText = GetComponentInChildren<Text>();
        if (uiText != null)
        {
            originalScale = uiText.transform.localScale;
        }
    }

    /// <summary>
    /// Called when the pointer enters the UI element.
    /// Enlarges and boldens the text.
    /// </summary>
    /// <param name="eventData">Current event data.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (uiText != null)
        {
            uiText.fontStyle = FontStyle.Bold;
            uiText.transform.localScale = originalScale * 1.1f; // Scale up
        }
    }

    /// <summary>
    /// Called when the pointer exits the UI element.
    /// Reverts the text to its original scale and style.
    /// </summary>
    /// <param name="eventData">Current event data.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (uiText != null)
        {
            uiText.fontStyle = FontStyle.Normal;
            uiText.transform.localScale = originalScale; // Revert to original scale
        }
    }
}
