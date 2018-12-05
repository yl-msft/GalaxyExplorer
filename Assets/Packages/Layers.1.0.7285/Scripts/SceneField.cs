// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MRS.Layers
{
#if UNITY_EDITOR
    /// <summary>
    /// A custom inspector for SceneFields.
    /// </summary>
    [CustomPropertyDrawer(typeof(SceneField))]
    public class SceneFieldPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, GUIContent.none, property);
            SerializedProperty asset = property.FindPropertyRelative("asset");
            SerializedProperty path = property.FindPropertyRelative("path");

            if (asset != null)
            {
                EditorGUI.BeginChangeCheck();

                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
                UnityEngine.Object value = EditorGUI.ObjectField(position, asset.objectReferenceValue, typeof(SceneAsset), false);

                if (EditorGUI.EndChangeCheck())
                {
                    asset.objectReferenceValue = value;
                    path.stringValue = AssetDatabase.GetAssetPath(value);
                }
            }

            EditorGUI.EndProperty();
        }
    }
#endif

    /// <summary>
    /// A serializable object for Unity scene files.
    /// </summary>
    [Serializable]
    public struct SceneField
    {
        [SerializeField, HideInInspector]
        private UnityEngine.Object asset;
        [SerializeField, HideInInspector]
        private string path;

        // Allows SceneField to work with the existing Unity methods (LoadLevel/LoadScene).
        public static implicit operator string(SceneField sceneField)
        {
            return sceneField.Path;
        }

        public string Path
        {
#if UNITY_EDITOR
            get { return AssetDatabase.GetAssetPath(asset); }
#else
            get { return path; }
#endif
        }
    }
}
