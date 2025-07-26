using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// Manages file operations for logging object click events in a Unity application.
/// This singleton class handles writing, clearing, and opening a CSV file to store click data.
/// </summary>
/// <remarks>
/// Code made by Saisri, email: saisribogapathi@gmail.com
/// Modified by Sashim, email: sashimsuryawanshi@gmail.com
/// </remarks>
public class FileManager : MonoBehaviour
{
    private static FileManager instance;
    private string filepath;
    private List<(string Timestamp, string Description)> clickedObjects;
    private readonly object fileLock = new object();

    /// <summary>
    /// Gets the singleton instance of the FileManager.
    /// Creates a new instance if none exists and marks it to persist across scenes.
    /// </summary>
    public static FileManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("FileManager");
                instance = go.AddComponent<FileManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    /// <summary>
    /// Initializes the FileManager, ensuring singleton behavior and setting up the file path.
    /// Clears the existing file and initializes the click data list.
    /// </summary>
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        filepath = Path.Combine(Application.persistentDataPath, "Log.csv");
        clickedObjects = new List<(string, string)>();
        ClearFile();
    }
    /// <summary>
    /// Clears the contents of the CSV file and the in-memory click data list.
    /// Creates a new file with the header row for the CSV structure.
    /// </summary>
    public void ClearFile()
    {
        lock (fileLock)
        {
            try
            {
                File.WriteAllText(filepath, "Index,Timestamp,Description\n");
                clickedObjects.Clear();
                Debug.Log($"Cleared file: {filepath}");
            }
            catch (IOException ex)
            {
                Debug.LogError($"Error clearing file: {ex.Message}");
            }
        }
    }
    /// <summary>
    /// Appends a new click event to the CSV file and the in-memory list.
    /// </summary>
    /// <param name="timestamp">The timestamp of the click event.</param>
    /// <param name="actionDescription">The description of the action performed.</param>
    public void WriteToFile(string timestamp, string actionDescription)
    {
        lock (fileLock)
        {
            try
            {
                clickedObjects.Add((timestamp, actionDescription));
                string formattedText = $"{clickedObjects.Count},{timestamp},{actionDescription}\n";
                File.AppendAllText(filepath, formattedText);
                Debug.Log($"Appended to {filepath}: {formattedText}");
            }
            catch (IOException ex)
            {
                Debug.LogError($"Error writing to file: {ex.Message}");
            }
        }
    }
    /// <summary>
    /// Opens the CSV file using the default system application associated with .csv files.
    /// </summary>
    public void OpenFile()
    {
        try
        {
            Application.OpenURL("file:///" + filepath);
            Debug.Log($"Opening file: {filepath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error opening file: {ex.Message}");
        }
    }
    /// <summary>
    /// Gets the total number of click events recorded in the in-memory list.
    /// </summary>
    /// <returns>The count of recorded click events.</returns>
    public int GetClickedObjectsCount()
    {
        return clickedObjects.Count;
    }
}