using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Class responsible for redoing the last undone action by reactivating the last undone line.
/// </summary>
/// <remarks>
/// Code written by C.Harshita, email: chemboliharshita@gmail.com
/// Modified by Sashim, email: sashimsuryawanshi@gmail.com
/// </remarks>
public class Redo_B : MonoBehaviour
{
    public Stack<GameObject> redoStack = new Stack<GameObject>(); ///< Stack for redo operations

    /// <summary>
    /// Redoes the last undone action by reactivating the last undone line.
    /// </summary>
    public void RedoLine()
    {
        if (redoStack.Count > 0)
        {
            GameObject lastUndoneLine = redoStack.Pop();
            Undo_B.AddLine(lastUndoneLine);
            lastUndoneLine.SetActive(true);
            ActionLogger.Instance.LogAction($"Undo {lastUndoneLine.name}");
        }
    }
}