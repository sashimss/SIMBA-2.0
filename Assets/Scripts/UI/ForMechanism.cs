using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Controls the behavior of a main button with dropdown functionality.
/// </summary>
/// <remarks>
/// Code made by Ruthvik.R, email: ruthvik.racha2005@gmail.com
/// Changes made by C.Harshita, email: chemboliharshita@gmail.com
/// </remarks>
public class ForMechanism : MonoBehaviour, IPointerClickHandler
{
    public List<GameObject> subButtons; ///< List of sub-buttons associated with this main button.
    private static ForMechanism activeMainButton; ///< Reference to the active main button.
    private bool isDropdownActive = false; ///< Flag indicating whether the dropdown is currently active.
    private int clickCount = 0; ///< Counter to track the number of clicks on the main button.
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
    /// Called when the main button is clicked.
    /// </summary>
    /// <param name="eventData">The pointer event data associated with the click.</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // Close the active main button if another main button is clicked
        if (activeMainButton != null && activeMainButton != this)
        {
            activeMainButton.CloseDropdown();
        }

        isDropdownActive = !isDropdownActive;
        clickCount++;

        if (isDropdownActive)
        {
            GetComponent<Image>().color = hoverButtonColor;
            GetComponentInChildren<Text>().color = hoverTextColor;

            foreach (GameObject subButton in subButtons)
            {
                subButton.SetActive(true);
                subButton.GetComponent<Image>().color = hoverButtonColor;
                subButton.GetComponentInChildren<Text>().color = hoverTextColor;
            }

            // Set this main button as the active one
            activeMainButton = this;
        }
        else
        {
            CloseDropdown();
        }

        if (isDropdownActive)
        {
            Debug.Log("#" + clickCount + " button clicked: " + GetMainButtonName());
        }
        else
        {
            Debug.Log("#" + clickCount + " button unclicked: " + GetMainButtonName());
        }
    }

    /// <summary>
    /// Closes the dropdown associated with the main button.
    /// </summary>
    private void CloseDropdown()
    {
        GetComponent<Image>().color = originalButtonColor;
        GetComponentInChildren<Text>().color = originalTextColor;

        foreach (GameObject subButton in subButtons)
        {
            subButton.SetActive(false);
        }

        // Check if there are sub buttons in the HoverDropdown script
        if (HoverDropdown.activeDropdown != null) // This block is added by Harshita
        {
            foreach (GameObject subButton in HoverDropdown.activeDropdown.subButtons)
            {
                subButton.SetActive(false);
            }
            HoverDropdown.activeDropdown.CloseDropdown();
        }

        // If this main button is the active one, set activeMainButton to null
        if (activeMainButton == this)
        {
            activeMainButton = null;
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