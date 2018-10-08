using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Linq;
using UnityEngine.Events;
using System.Reflection;

namespace MRS.FlowManager
{
    [CustomEditor(typeof(FlowManager))]
    public class FlowManagerEditor : Editor
    {
        private ReorderableList m_stages;
        private bool m_debug;

        private void SetUpStages()
        {
            m_stages = new ReorderableList(serializedObject, serializedObject.FindProperty("m_stages"), true, true, true, true);

            m_stages.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "Stages");
            };

            m_stages.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = serializedObject.FindProperty("m_stages").GetArrayElementAtIndex(index);

                // Restart indicator
                if (serializedObject.FindProperty("m_restartEnabled").boolValue)
                {
                    int resetStageIndex = serializedObject.FindProperty("m_loopBackStage").intValue;

                    if (resetStageIndex == index)
                    {
                        GUI.DrawTexture(new Rect(rect.x + 2, rect.y, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight), Resources.Load("ResetArrow") as Texture);
                    }
                }

                // Stage
                EditorGUI.PropertyField(
                    new Rect(rect.x + 22, rect.y, rect.width - 35, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("Name"), GUIContent.none);
            };
        }

        public void OnEnable()
        {
            SetUpStages();
        }

        public override void OnInspectorGUI()
        {
            if (m_debug)
            {
                base.DrawDefaultInspector();
            }
            else
            {
                serializedObject.Update();

                // Restart Options
                GUILayout.Space(10);
                GUIStyle titles = new GUIStyle();
                titles.normal.textColor = Color.white;
                titles.fontSize = 15;
                GUILayout.Label("Restart Options", titles);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_restartEnabled"));
                if (serializedObject.FindProperty("m_restartEnabled").boolValue)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_loopBackStage"));

                // Stages
                GUILayout.Space(10);
                GUILayout.Label("Stages", titles);
                GUILayout.Space(10);

                m_stages.DoLayoutList();

                if (GUILayout.Button("Open Flow Window", GUILayout.Height(50)))
                {
                    FlowEditorWindow.OpenWindow(target.GetInstanceID());
                }

                serializedObject.ApplyModifiedProperties();
            }
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Debug", "Restores the default inspector window"));
            m_debug = EditorGUILayout.Toggle(m_debug);
            GUILayout.EndHorizontal();
        }
    }
}
