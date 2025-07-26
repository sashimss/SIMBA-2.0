using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required for UI.Text

/// <summary>
/// A simple action logger for Unity that records and displays game events.
/// Implemented as a Singleton for easy global access.
/// </summary>
/// <remarks>
/// Code made by Sashim, email: sashimsuryawanshi@gmail.com
/// </remarks>
public class ActionLogger : MonoBehaviour
{
    // Static instance of the ActionLogger, ensuring only one exists.
    public static ActionLogger Instance { get; private set; }

    // A list to store all the logged actions.
    private List<string> actionLogs = new List<string>();

    // The maximum number of logs to keep in memory.
    // Oldest logs will be removed if this limit is exceeded.
    [SerializeField]
    private int maxLogs = 50;

    // Optional: Reference to a UI Text element to display logs in-game.
    // If not assigned, logs will only go to the Unity console.
    [SerializeField]
    private Text logDisplayText; // Use UnityEngine.UI.Text directly

    private void Awake()
    {
        // Singleton pattern implementation:
        // Check if an instance already exists.
        if (Instance != null && Instance != this)
        {
            // If another instance already exists, destroy this one.
            // This ensures there's only one ActionLogger in the scene.
            Destroy(gameObject);
            return;
        }

        // If no instance exists, set this as the instance.
        Instance = this;

        // Optional: Ensure the logger persists across scene loads.
        // Uncomment the line below if you want this logger to be a singleton
        // that is not destroyed when a new scene is loaded.
        // This is common for managers like a logger.
         DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Logs an action with a timestamp.
    /// This method can be called from any other script using ActionLogger.Instance.LogAction("Your message").
    /// </summary>
    /// <param name="actionDescription">A string describing the action that occurred.</param>
    public void LogAction(string actionDescription)
    {
        // Create a timestamp for the log entry.
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");

        // Format the log message.
        string logEntry = $"[{timestamp}] {actionDescription}";

        // Add the new log entry to the list.
        actionLogs.Add(logEntry);
        FileManager.Instance.WriteToFile(timestamp, actionDescription);

        // If the number of logs exceeds the maximum, remove the oldest one.
        if (actionLogs.Count > maxLogs)
        {
            actionLogs.RemoveAt(0); // Remove the first (oldest) element.
        }

        // Output the log to the Unity console for debugging.
        Debug.Log($"Action Logged: {logEntry}");

        // Update the UI display if a Text component is assigned.
        UpdateLogDisplay();
    }

    /// <summary>
    /// Updates the UI Text component with the current logs.
    /// This method is called automatically when a new action is logged.
    /// </summary>
    private void UpdateLogDisplay()
    {
        if (logDisplayText != null)
        {
            // Clear the existing text.
            logDisplayText.text = "";

            // Append each log entry to the display text.
            foreach (string log in actionLogs)
            {
                logDisplayText.text += log + "\n";
            }
        }
    }

    /// <summary>
    /// Clears all stored action logs.
    /// </summary>
    public void ClearLogs()
    {
        actionLogs.Clear();
        Debug.Log("Action logs cleared.");
        UpdateLogDisplay(); // Update UI after clearing.
    }
}
