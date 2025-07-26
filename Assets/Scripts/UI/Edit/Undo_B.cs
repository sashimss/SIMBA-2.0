using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Class responsible for undoing the last action by deactivating the last drawn line.
/// </summary>
/// <remarks>
/// Code written by C.Harshita, email: chemboliharshita@gmail.com
/// Modified by Sashim, email: sashimsuryawanshi@gmail.com
/// </remarks>
public class Undo_B : MonoBehaviour
{
    public static Stack<GameObject> undoStack = new Stack<GameObject>(); ///< Stack for undo operations
    public Redo_B redo_obj; ///< Reference to the Redo_B class

    ///<summary>
    /// Adds a Line GameObject to the undoStack
    /// </summary>
    public static void AddLine(GameObject LineGO)
    {
        undoStack.Push(LineGO);
    }

    /// <summary>
    /// Undoes the last action by deactivating the last drawn line.
    /// </summary>
    public void UndoLine()
    {
        if (undoStack.Count > 0)
        {
            GameObject lastLine = undoStack.Pop();
            redo_obj.redoStack.Push(lastLine);
            lastLine.SetActive(false);
            ActionLogger.Instance.LogAction($"Undo {lastLine.name}");
        }
    }
}