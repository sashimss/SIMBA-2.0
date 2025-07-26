using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Darkens the color of a UI Image component when the pointer hovers over it.
/// </summary>
/// <remarks>
/// Code made by Ruthvik R, email: ruthvik.racha2005@gmail.com
/// </remarks>
public class DarkenOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// Reference to the Image component of the UI element.
    /// </summary>
    private Image buttonImage;

    /// <summary>
    /// Stores the original color of the Image component.
    /// </summary>
    private Color originalColor;

    /// <summary>
    /// Called on the frame when a script is enabled, before any of the Update methods are called.
    /// Initializes the reference to the Image component and stores its original color.
    /// </summary>
    void Start()
    {
        buttonImage = GetComponent<Image>();
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
        }
        Debug.Log("Hover script is started");
    }

    /// <summary>
    /// Called when the pointer enters the UI element.
    /// Darkens the color of the Image component.
    /// </summary>
    /// <param name="eventData">Current event data.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonImage != null)
        {
            buttonImage.color = originalColor * 0.8f; // Darken the color
        }
    }

    /// <summary>
    /// Called when the pointer exits the UI element.
    /// Reverts the color of the Image component to its original color.
    /// </summary>
    /// <param name="eventData">Current event data.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonImage != null)
        {
            buttonImage.color = originalColor; // Revert to original color
        }
    }
}
