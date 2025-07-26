using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Controls the behavior of a hover dropdown menu.
/// </summary>
/// <remarks>
/// Code made by Ruthvik.R, email: ruthvik.racha2005@gmail.com
/// Modified by Sashim.S, email: sashimsuryawanshi@gmail.com
/// </remarks>
public class HoverDropdown : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public List<GameObject> subButtons; ///< List of sub-buttons in the dropdown menu.
    public static HoverDropdown activeDropdown;
    private Color originalButtonColor; ///< Original color of the main button.
    private Color originalTextColor = Color.black; ///< Original color of the text on the main button.
    private Color hoverButtonColor = new Color(0.2f, 0.2f, 0.2f); ///< Color of the main button when hovered.
    private Color hoverTextColor = Color.white; ///< Color of the text on the main button when hovered.

    /// <summary>
    /// Called at the start of the script instance.
    /// </summary>
    void Start()
    {
        originalButtonColor = GetComponent<Image>().color;
    }

    /// <summary>
    /// Called when the main button of the dropdown menu is clicked.
    /// </summary>
    /// <param name="eventData">The pointer event data associated with the click.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponent<Image>().color = hoverButtonColor;
        GetComponentInChildren<Text>().color = hoverTextColor;

        foreach (GameObject subButton in subButtons)
        {
            subButton.SetActive(true);
            subButton.GetComponent<Image>().color = hoverButtonColor;
            subButton.GetComponentInChildren<Text>().color = hoverTextColor;
        }
    }

    /// This block is added by Sashim.S
    public void OnPointerExit(PointerEventData eventData)
    {
        CloseDropdown();
    }

    /// <summary>
    /// Closes the dropdown menu.
    /// </summary>
    public void CloseDropdown()
    {
        GetComponent<Image>().color = originalButtonColor;
        GetComponentInChildren<Text>().color = originalTextColor;

        foreach (GameObject subButton in subButtons)
        {
            subButton.SetActive(false);
        }
    }

    /// <summary>
    /// Retrieves the name of the main button.
    /// </summary>
    /// <returns>The name of the main button.</returns>
    private string GetMainButtonName()
    {
        return gameObject.name;
    }
}
