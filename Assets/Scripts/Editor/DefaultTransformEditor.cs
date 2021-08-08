using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DefaultTransform))]
public class DefaultTransformEditor: Editor{
	public override void OnInspectorGUI(){
		base.OnInspectorGUI();
		var obj = target as DefaultTransform;
		if (!target)
			return;
		if (GUILayout.Button("Record")){
			Undo.RecordObject(obj, "Record");
			obj.record();
		}
	}
}
