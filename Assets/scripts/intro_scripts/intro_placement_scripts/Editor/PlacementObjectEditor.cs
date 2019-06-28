using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlacementObject))]
public class PlacementObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var obj = (PlacementObject) target;

        DrawDefaultInspector();

        if (GUILayout.Button("Generate"))
        {
            obj.GenerateData();
        }
    }
}
