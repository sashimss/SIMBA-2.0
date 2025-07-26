using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Controls the behavior of a button.
/// </summary>
/// <remarks>
/// Code made by Ruthvik.R, email: ruthvik.racha2005@gmail.com
/// Changes made by C.Harshita, email: chemboliharshita@gmail.com
/// Code modified by Saisri, email: saisribogapathi@gmail.com
/// Modified by Sashim, email: sashimsuryawanshi@gmail.com
/// </remarks>
public class NewButton : MonoBehaviour
{
    private Button button; ///< Reference to the Button component attached to this GameObject.
    private Scene currentScene; ///< Reference to the current active scene.

    /// <summary>
    /// Called when the script instance is being loaded.
    /// </summary>
    void Start()
    {
        // Get the Button component attached to this GameObject
        button = GetComponent<Button>();

        // Add a listener to the button click event
        button.onClick.AddListener(OnButtonClick);

        // Get the current active scene
        currentScene = SceneManager.GetActiveScene(); //Added by Harshita
    }

    /// <summary>
    /// Called when the button associated with this script is clicked.
    /// </summary>
    void OnButtonClick()
    {
        // Print the name of the button to the console
        Debug.Log("Button Pressed: " + gameObject.name);
        // Logs the Name of the Game Object, UNIX time stamp into the CSV File
        // Added by Saisri
        ActionLogger.Instance.LogAction("Created new File");
        //FileManager.Instance.OpenFile();
    }

    /// <summary>
    /// Reloads the current scene.
    /// </summary>
    /// <remarks>
    /// Method written by C.Harshita, email: chemboliharshita@gmail.com
    /// </remarks>
    public void ReloadScene()
    {
        // Reload the current scene
        SceneManager.LoadScene(currentScene.name);
    }
}