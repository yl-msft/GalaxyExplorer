// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MRS.FlowManager
{
    public class FlowEditorWindow : EditorWindow
    {

        private static int m_flowManageInstanceID;

        private SerializedObject m_flowManager;
        private Vector2 m_scrollPosition;
        private List<FlowStageEditor> m_flowStages = new List<FlowStageEditor>();

        // Used for repainting the ui
        private float m_updateTimer;
        private float m_startTime;

        private Texture m_backgroundTex;
        private Texture m_arrowTex;
        private Texture m_loopbackTex;

        public static void OpenWindow(int _flowManagerInstanceID)
        {
            FlowEditorWindow window = (FlowEditorWindow)EditorWindow.GetWindow(typeof(FlowEditorWindow), false, "Flow");
            m_flowManageInstanceID = _flowManagerInstanceID;

            // Save this so it persists after entering playmode
            EditorPrefs.SetInt("FlowManagerInstanceID", m_flowManageInstanceID);

            window.Init();
            window.Show();
        }

        public void Init()
        {
            if (EditorPrefs.HasKey("FlowManagerInstanceID"))
            {
                FlowManager flowManager = EditorUtility.InstanceIDToObject(EditorPrefs.GetInt("FlowManagerInstanceID")) as FlowManager;
                if (flowManager)
                {
                    m_flowManager = new SerializedObject(flowManager);
                }
                else
                {
                    EditorPrefs.DeleteKey("FlowManagerInstanceID");
                }
            }

            if (m_flowManager == null)
                return;

            SerializedProperty stagesArray = m_flowManager.FindProperty("m_stages");

            m_flowStages.Clear();
            for (int i = 0; i < stagesArray.arraySize; i++)
            {
                SerializedProperty stage = stagesArray.GetArrayElementAtIndex(i);
                SerializedProperty events = stage.FindPropertyRelative("Events");
                SerializedProperty exitEvents = stage.FindPropertyRelative("ExitEvents");
                m_flowStages.Add(new FlowStageEditor(m_flowManager, stage, events, exitEvents, i));
            }

            EditorApplication.update += Update;

            m_startTime = Time.time;
        }

        private void OnDestroy()
        {
            EditorUtility.DisplayDialog("Warning", "Closing the flow window can cause a desync with custom events.  To avoid this select a new GameObject in the tree hierarchy or keep the flow window docked.", "Ok");
        }

        private void OnEnable()
        {
            m_backgroundTex = Resources.Load("background") as Texture;
            m_arrowTex = Resources.Load("Arrow") as Texture;
            m_loopbackTex = Resources.Load("ResetArrow") as Texture;
            Init();
        }

        private void OnGUI()
        {
            // Draw background 
            Rect uvDrawRect = new Rect(0, 0, 50 * (position.width / position.height), 50);
            GUI.DrawTextureWithTexCoords(new Rect(0, 0, position.width, position.height), m_backgroundTex, uvDrawRect);

            FlowManager flowManager = EditorUtility.InstanceIDToObject(EditorPrefs.GetInt("FlowManagerInstanceID")) as FlowManager;

            if ((flowManager == null) || (m_flowManager == null))
            {
                return;
            }

            SerializedProperty stagesArray = m_flowManager.FindProperty("m_stages");

            if (stagesArray == null)
                return;

            m_flowManager.Update();

            m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition);

            GUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();

            for (int i = 0; i < stagesArray.arraySize; i++)
            {
                if (i != 0)
                {
                    GUILayout.BeginVertical();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(m_arrowTex, GUILayout.Width(20), GUILayout.Height(20));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndVertical();
                }

                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();

                // Restart indicator
                if (m_flowManager.FindProperty("m_restartEnabled").boolValue)
                {
                    int resetStageIndex = m_flowManager.FindProperty("m_loopBackStage").intValue;
                    if (resetStageIndex == i)
                    {
                        GUILayout.Label(m_loopbackTex, GUILayout.Width(20), GUILayout.Height(20));
                    }
                }

                GUILayout.FlexibleSpace();

                // Stage title header
                GUIStyle titles = new GUIStyle();
                titles.normal.textColor = Color.white;
                titles.fontSize = 15;
                SerializedProperty stage = stagesArray.GetArrayElementAtIndex(i);
                GUILayout.Label(new GUIContent(stage.FindPropertyRelative("Name").stringValue), titles);

                GUILayout.FlexibleSpace();

                // Remove stage button
                if (GUILayout.Button(new GUIContent("x", "Remove stage")))
                {
                    m_flowStages.RemoveAt(i);

                    if (stagesArray.GetArrayElementAtIndex(i).objectReferenceValue != null)
                        stagesArray.DeleteArrayElementAtIndex(i);
                    stagesArray.DeleteArrayElementAtIndex(i);

                    m_flowManager.ApplyModifiedProperties();

                    // Update flowstage so that they point to the new array indes
                    for (int j = i; j < stagesArray.arraySize; j++)
                    {
                        m_flowStages[j].UpdateStageAndEvents(stagesArray.GetArrayElementAtIndex(j), stagesArray.GetArrayElementAtIndex(j).FindPropertyRelative("Events"), stagesArray.GetArrayElementAtIndex(j).FindPropertyRelative("ExitEvents"));
                    }

                    EditorGUI.EndChangeCheck();
                    EditorGUI.BeginChangeCheck();

                    break;
                }

                GUILayout.EndHorizontal();

                if (i < m_flowStages.Count)
                {
                    m_flowStages[i].Draw();
                }

                GUILayout.EndVertical();
            }

            // Add new stage button
            if (GUILayout.Button(new GUIContent("+\nNew stage", "Add a new stage."), GUILayout.ExpandHeight(true)))
            {
                stagesArray.arraySize++;
                SerializedProperty stage = stagesArray.GetArrayElementAtIndex(m_flowStages.Count);
                SerializedProperty events = stage.FindPropertyRelative("Events");
                SerializedProperty exitEvents = stage.FindPropertyRelative("ExitEvents");
                int index = m_flowStages.Count;
                m_flowStages.Add(new FlowStageEditor(m_flowManager, stage, events, exitEvents, index));
            }

            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_flowManager.targetObject);
                if (!m_flowManager.ApplyModifiedProperties())
                {
                    // This can be because of a desync
                    //EditorUtility.DisplayDialog("Failed to apply changes", "The changes made have not been applied.  Try selecting a new gameObject in the hierarchy to correct this.", "Ok");
                }
            }
        }

        private void Update()
        {
            if (EditorApplication.isPlaying)
            {
                m_updateTimer += Time.time - m_startTime;
                if (m_updateTimer > 1.0) // Update every second
                {
                    m_startTime = Time.time;
                    m_updateTimer = 0;

                    Repaint();
                }
                AutoScroll();
            }
        }

        private void AutoScroll()
        {
            float windowWidth = position.width;

            int numStages = m_flowStages.Count;

            // Each flow stage 700;
            float size = numStages * 700;

            if (size > windowWidth)
            {
                float delta = size - windowWidth;
                float targetPos = (float)m_flowManager.FindProperty("m_currentStage").intValue / (float)(numStages - 2) * delta;
                m_scrollPosition.x = Mathf.SmoothStep(m_scrollPosition.x, targetPos, 0.1f);
            }
        }
    }
}
