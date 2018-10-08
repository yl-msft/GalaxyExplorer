using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class FlowStageEditor
{
    private SerializedObject m_flowManager;
    private SerializedProperty m_stage;

    private ReorderableList m_events;
    private ReorderableList m_exitEvents;

    private int m_stageIndex;
    private bool m_stageTriggered = false;

    private Texture m_delayBarBackground;
    private Texture m_delayBar;

    public FlowStageEditor(SerializedObject _flowManager, SerializedProperty _stage, SerializedProperty _events, SerializedProperty _exitEvents, int _stageIndex)
    {
        m_events = new ReorderableList(_flowManager, _events, true, true, true, true);
        m_exitEvents = new ReorderableList(_flowManager, _exitEvents, true, true, true, true);
        m_stage = _stage;
        m_stageIndex = _stageIndex;
        m_flowManager = _flowManager;

        m_events.drawHeaderCallback = rect => {
            EditorGUI.LabelField(rect, "Events");
        };

        m_events.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {

            SerializedProperty element = m_events.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;

            float nameDelayWidth = rect.width / 4.0f;
            // Event Name
            EditorGUI.LabelField(
                new Rect(rect.x + 2, rect.y + EditorGUIUtility.singleLineHeight, nameDelayWidth, EditorGUIUtility.singleLineHeight), "Event group"
                );

            EditorGUI.PropertyField(
                new Rect(rect.x + nameDelayWidth, rect.y + EditorGUIUtility.singleLineHeight, nameDelayWidth, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("Name"), GUIContent.none);

            // Delay
            EditorGUI.LabelField(
                    new Rect(rect.x + 2 * nameDelayWidth + 10, rect.y + EditorGUIUtility.singleLineHeight, nameDelayWidth + EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight), "Time delay"
                    );
            EditorGUI.PropertyField(
                new Rect(rect.x + 3 * nameDelayWidth + 10, rect.y + EditorGUIUtility.singleLineHeight, nameDelayWidth - 10, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("Delay"), GUIContent.none);

            // Event
            EditorGUI.PropertyField(
                new Rect(rect.x + 2, rect.y + (EditorGUIUtility.singleLineHeight + 30), rect.width - 2, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("Event"), GUIContent.none);
        };

        m_events.elementHeightCallback = (index) =>
        {
            SerializedProperty element = m_events.serializedProperty.GetArrayElementAtIndex(index);
            float height = 0;

            string[] properties = new string[3] { "Name", "Event", "Delay" };

            for (int i = 0; i < properties.Length; i++)
            {
                height += EditorGUI.GetPropertyHeight(element.FindPropertyRelative(properties[i]));
            }
            int spacingHeight = 20;

            return height + spacingHeight;
        };

        m_exitEvents.drawHeaderCallback = rect => {
            EditorGUI.LabelField(rect, "Exit events");
        };

        m_exitEvents.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {

            SerializedProperty element = m_exitEvents.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;

            float nameDelayWidth = rect.width / 4.0f;
            // Event Name
            EditorGUI.LabelField(
                new Rect(rect.x + 2, rect.y + EditorGUIUtility.singleLineHeight, nameDelayWidth, EditorGUIUtility.singleLineHeight), "Event group"
                );

            EditorGUI.PropertyField(
                new Rect(rect.x + nameDelayWidth, rect.y + EditorGUIUtility.singleLineHeight, nameDelayWidth, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("Name"), GUIContent.none);

            // Delay
            EditorGUI.LabelField(
                new Rect(rect.x + 2 * nameDelayWidth + 10, rect.y + EditorGUIUtility.singleLineHeight, nameDelayWidth + EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight), "Time delay"
                );
            EditorGUI.PropertyField(
                new Rect(rect.x + 3 * nameDelayWidth + 10, rect.y + EditorGUIUtility.singleLineHeight, nameDelayWidth - 10, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("Delay"), GUIContent.none);

            // Event
            EditorGUI.PropertyField(
                new Rect(rect.x + 2, rect.y + (EditorGUIUtility.singleLineHeight + 30), rect.width - 2, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("Event"), GUIContent.none);
        };

        m_exitEvents.elementHeightCallback = (index) =>
        {
            SerializedProperty element = m_exitEvents.serializedProperty.GetArrayElementAtIndex(index);
            float height = 0;

            string[] properties = new string[3] { "Name", "Event", "Delay" };

            for (int i = 0; i < properties.Length; i++)
            {
                height += EditorGUI.GetPropertyHeight(element.FindPropertyRelative(properties[i]));
            }
            int spacingHeight = 20;

            return height + spacingHeight;
        };
    }

    public void UpdateStageAndEvents(SerializedProperty _stage, SerializedProperty _events, SerializedProperty _exitEvents)
    {
        m_stage = _stage;
        m_events.serializedProperty = _events;
        m_exitEvents.serializedProperty = _exitEvents;
    }

    private void DrawPlayModeUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();

        GUIStyle eventNameStyle = new GUIStyle();
        eventNameStyle.fontSize = 15;
        eventNameStyle.normal.textColor = Color.white;

        if (m_delayBarBackground == null)
            m_delayBarBackground = Resources.Load("loadingBar_background") as Texture;

        if (m_delayBar == null)
            m_delayBar = Resources.Load("loadingBar_bar") as Texture;

        for (int i=0; i<m_events.count; i++)
        {
            GUILayout.Space(10);
            GUILayout.Label(m_events.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Name").stringValue, eventNameStyle);
            GUILayout.Space(10);

            GUILayout.Box("box", GUILayout.ExpandWidth(true), GUILayout.MaxWidth(350));
            Rect rect = GUILayoutUtility.GetLastRect();

            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), m_delayBarBackground, ScaleMode.StretchToFill);

            SerializedProperty element = m_events.serializedProperty.GetArrayElementAtIndex(i);
            if (m_stageIndex == m_flowManager.FindProperty("m_currentStage").intValue || element.FindPropertyRelative("Triggered").boolValue) // If we have transitioned to the next stage
            {
                float fillValue = rect.width;

                if (!element.FindPropertyRelative("Triggered").boolValue)
                {
                    float delay = element.FindPropertyRelative("Delay").floatValue;

                    if (delay > 0)
                    {
                        fillValue = m_flowManager.FindProperty("m_timeSinceTap").floatValue / delay * (rect.width);
                    }

                    fillValue = Mathf.Clamp(fillValue, 0, rect.width);
                }

                GUI.DrawTexture(new Rect(rect.x, rect.y, fillValue, EditorGUIUtility.singleLineHeight), m_delayBar, ScaleMode.StretchToFill);
            }
        }

        GUILayout.EndVertical();
        GUILayout.BeginVertical();

        for (int i = 0; i < m_exitEvents.count; i++)
        {
            GUILayout.Space(10);
            GUILayout.Label(m_exitEvents.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Name").stringValue, eventNameStyle);
            GUILayout.Space(10);

            GUILayout.Box("box", GUILayout.ExpandWidth(true), GUILayout.MaxWidth(350));
            Rect rect = GUILayoutUtility.GetLastRect();

            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), m_delayBarBackground, ScaleMode.StretchToFill);

            SerializedProperty element = m_exitEvents.serializedProperty.GetArrayElementAtIndex(i);
            if (m_stageIndex == m_flowManager.FindProperty("m_currentStage").intValue - 1 || element.FindPropertyRelative("Triggered").boolValue) // If we have transitioned to the next stage
            {
                float fillValue = rect.width;

                if (!element.FindPropertyRelative("Triggered").boolValue)
                {
                    float delay = element.FindPropertyRelative("Delay").floatValue;

                    if (delay > 0)
                    {
                        fillValue = m_flowManager.FindProperty("m_timeSinceTap").floatValue / delay * (rect.width);
                    }

                    fillValue = Mathf.Clamp(fillValue, 0, rect.width);
                }

                GUI.DrawTexture(new Rect(rect.x, rect.y, fillValue, EditorGUIUtility.singleLineHeight), m_delayBar, ScaleMode.StretchToFill);
            }
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    public void Draw()
    {
        if (m_delayBarBackground == null)
            m_delayBarBackground = Resources.Load("loadingBar_background") as Texture;

        if (m_delayBar == null)
            m_delayBar = Resources.Load("loadingBar_bar") as Texture;

        Color defaultBackgroundColor = GUI.backgroundColor;

        if (m_stageTriggered && EditorApplication.isPlaying)
        {
            GUI.backgroundColor = Color.yellow;
        }
        if (EditorApplication.isPlaying)
        {
            if (m_stageIndex == m_flowManager.FindProperty("m_currentStage").intValue)
            {
                m_stageTriggered = true;
                GUI.backgroundColor = Color.red;
            }
        }

        GUILayout.BeginVertical("box", GUILayout.MinWidth(700));
        GUI.backgroundColor = defaultBackgroundColor;

        // Autotransition loading bar
        if(EditorApplication.isPlaying || EditorApplication.isPaused)
        {
            float autoTransitionDelay = m_stage.FindPropertyRelative("autoTransitionDelay").floatValue;
            if (autoTransitionDelay > 0.0f)
            {
                GUILayout.Box("box", GUILayout.ExpandWidth(true));
                Rect rect = GUILayoutUtility.GetLastRect();
                GUI.DrawTexture(rect, m_delayBarBackground);

                float fillValue = rect.width;

                if(m_flowManager.FindProperty("m_currentStage").intValue < m_stageIndex)
                {
                    fillValue = 0;
                }
                else if(m_flowManager.FindProperty("m_currentStage").intValue == m_stageIndex)
                {
                    fillValue *= m_flowManager.FindProperty("m_timeSinceTap").floatValue / autoTransitionDelay;
                    fillValue = Mathf.Min(rect.width, fillValue);
                }

                GUI.DrawTexture(new Rect(rect.x, rect.y, fillValue, rect.height), m_delayBar);
            }
        }

        GUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(m_stage.FindPropertyRelative("Name"), GUIContent.none);
		GUILayout.FlexibleSpace();
		EditorGUILayout.PropertyField(m_stage.FindPropertyRelative("clickToAdvance"));
		GUILayout.FlexibleSpace();
		EditorGUILayout.PropertyField(m_stage.FindPropertyRelative("autoTransitionDelay"));
        GUILayout.EndHorizontal();

        if (!EditorApplication.isPlaying) // We don't use the reorderable list in play mode as its fairly expensive to draw
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            m_events.DoLayoutList();
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            m_exitEvents.DoLayoutList();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        else
        {
            DrawPlayModeUI(); // A simplified UI with only event names and delay bars
        }

        GUILayout.EndVertical();
    }

    void DestroyStage()
    {
        m_stage.DeleteArrayElementAtIndex(m_stageIndex);
        m_flowManager.ApplyModifiedProperties();
    }
}