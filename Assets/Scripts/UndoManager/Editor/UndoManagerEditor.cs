using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UndoManager))]
public class UndoManagerEditor : Editor {

	public override void OnInspectorGUI()
	{
		UndoManager undoManager = target as UndoManager;
		if (undoManager == null) return;
		DrawDefaultInspector();
		if (undoManager.undoActions != null){
			GUILayout.Label(string.Format("List action count: {0}", undoManager.undoActions.Count));
			for (int i = 0; i < undoManager.undoActions.Count; i++)
			{
				var pos = undoManager.undoActions[i].positionInitial;
				GUILayout.Label(string.Format("Position: {0},{1},{2}", pos.x, pos.y, pos.z));				
			}
		}
		if (GUILayout.Button("<<"))
		{
			undoManager.MoveBackwards();
		}

		if (GUILayout.Button(">>"))
		{
			undoManager.MoveForward();
		}
	}
}
