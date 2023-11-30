using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GUID))]
public class GUIDEditor : Editor
{
    private SerializedProperty type;
    public override void OnInspectorGUI()
    {
        
        serializedObject.Update();
        SerializedProperty guid = serializedObject.FindProperty("guid");
        SerializedProperty update = serializedObject.FindProperty("update");
        GUILayout.Label($"GUID: {guid.stringValue}");
        if (GUILayout.Button("Update"))
            update.boolValue = true;
    }
}
