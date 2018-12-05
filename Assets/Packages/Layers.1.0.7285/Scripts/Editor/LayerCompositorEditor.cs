// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace MRS.Layers
{
    /// <summary>
    /// Implementation of a custom inspector for the LayerCompositor component.
    /// </summary>
    [CustomEditor(typeof(LayerCompositor))]
    public class LayerCompositorEditor : Editor
    {
        private readonly Color greenTint = new Color(0.3f, 0.4f, 0.3f, 0.5f);
        private readonly Color redTint = new Color(0.4f, 0.3f, 0.3f, 0.5f);
        private readonly Color highlightTint = new Color(0.3f, 0.3f, 0.3f, 0.0f);

        private const string layersListName = "layers";
        private ReorderableList layersList;
        private Rect rect1, rect2, rect3, rect4, rect5;

        private void BuildRowOfColumns(Rect rect, bool isHeader)
        {
            // Here we build rendering fields for our row data; both the list header and the data rows.
            // The header row is wider than the others since it does not have a drag handle, so we subtract from it
            // to help align the headings with the data fields.
            float consideredWidth = isHeader ? rect.width - 10.0f : rect.width;
            float colWidth1 = (consideredWidth * 0.05f); // idx
            float colWidth2 = (consideredWidth * 0.33f); // string field
            float colWidth3 = (consideredWidth * 0.10f); // checkbox
            float colWidth4 = (consideredWidth * 0.07f); // checkbox
            float colWidth5 = (consideredWidth * 0.45f); // reference field

            // Checkboxes are left-aligned; set up modifiers here so we can display them more centrally
            // TODO: they are also click-sensitive across the whole rectangle, not just the rendered box, so there's
            //  an adjustment on rect3's width, below, that prevents clicks over rect4 being intercepted by rect3.
            //  Need to do this better!
            float c3Adj = (colWidth3 * 0.30f);
            float c4Adj = (colWidth4 * 0.25f);

            float x = isHeader ? rect.x + 10.0f : rect.x;
            float y = isHeader ? rect.y : rect.y + EditorGUIUtility.standardVerticalSpacing;
            rect1 = new Rect(x, y, colWidth1, EditorGUIUtility.singleLineHeight);
            x += (colWidth1);
            rect2 = new Rect(x, y, colWidth2, EditorGUIUtility.singleLineHeight);
            x += (colWidth2);
            rect3 = new Rect(x + (isHeader ? 0.0f : c3Adj), y, colWidth3-c3Adj, EditorGUIUtility.singleLineHeight);
            x += (colWidth3);
            rect4 = new Rect(x + (isHeader ? 0.0f : c4Adj), y, colWidth4, EditorGUIUtility.singleLineHeight);
            x += (colWidth4);
            rect5 = new Rect(x, y, colWidth5, EditorGUIUtility.singleLineHeight);
        }

        private void OnEnable()
        {
            LayerCompositor layerCompositor = (LayerCompositor)target;

            layersList = new ReorderableList(serializedObject, serializedObject.FindProperty(layersListName), true, true, true, true);

            layersList.drawHeaderCallback = (Rect rect) =>
            {
                BuildRowOfColumns(rect, true);

                EditorGUI.LabelField(rect, new GUIContent("Layer index and Identifier", "Layer data. The Id is used at runtime to selectively Load/Unload a layer. Autoload specifies if the layer autoloads at runtime. Active constrols if the layer root objects are active after loading. Finally, provide a reference to a Unity scene."));
                EditorGUI.LabelField(rect3, new GUIContent("Autoload"));
                EditorGUI.LabelField(rect4, new GUIContent("Active"));
                EditorGUI.LabelField(rect5, new GUIContent("Scene"));
            };

            layersList.drawElementBackgroundCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if ((layerCompositor.Layers == null) || (layerCompositor.Layers.Length == 0))
                {
                    return;
                }
                Texture2D bgTex = new Texture2D(1, 1);
                Color bgCol;
                LayerCompositor.Layer selectedLayer = layerCompositor.Layers[index];
                bgCol = (LayerCompositor.IsLayerLoaded(selectedLayer) ? greenTint : redTint);
                if (isActive)
                {
                    bgCol += highlightTint;
                }
                bgTex.SetPixel(0, 0, bgCol);
                bgTex.Apply();
                GUI.DrawTexture(rect, bgTex as Texture);
            };

            layersList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                EditorGUI.BeginChangeCheck();

                SerializedProperty element = layersList.serializedProperty.GetArrayElementAtIndex(index);
                {
                    BuildRowOfColumns(rect, false);

                    GUIContent layerIdx = new GUIContent(index.ToString(), "A layer ID.");
                    EditorGUI.LabelField(rect1, layerIdx);
                    EditorGUI.PropertyField(rect2, element.FindPropertyRelative(nameof(LayerCompositor.Layer.id)), GUIContent.none);
                    EditorGUI.PropertyField(rect3, element.FindPropertyRelative(nameof(LayerCompositor.Layer.autoload)), GUIContent.none);
                    EditorGUI.PropertyField(rect4, element.FindPropertyRelative(nameof(LayerCompositor.Layer.loadactive)), GUIContent.none);
                    EditorGUI.PropertyField(rect5, element.FindPropertyRelative(nameof(LayerCompositor.Layer.scene)), GUIContent.none);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    LayerCompositor.Layer[] previousLayers = layerCompositor.Layers.Clone() as LayerCompositor.Layer[];
                    serializedObject.ApplyModifiedProperties();
                    LayerCompositor.RefreshLayers(previousLayers, layerCompositor.Layers, true, layerCompositor.gameObject.scene);
                }
            };

            layersList.onAddCallback = (ReorderableList list) =>
            {
                ReorderableList.defaultBehaviours.DoAddButton(list);
                list.serializedProperty.serializedObject.ApplyModifiedProperties();
                LayerCompositor.Layer newLayer = layerCompositor.Layers[layerCompositor.Layers.Length - 1];
                newLayer.id = "NoID";
                newLayer.autoload = true;
                newLayer.loadactive = true;
            };

            layersList.onRemoveCallback = (ReorderableList list) =>
            {
                LayerCompositor.Layer[] previousLayers = layerCompositor.Layers.Clone() as LayerCompositor.Layer[];
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                list.serializedProperty.serializedObject.ApplyModifiedProperties();
                LayerCompositor.RefreshLayers(previousLayers, layerCompositor.Layers, true, layerCompositor.gameObject.scene);
                Repaint(); // removes Refresh when no layers remain
            };
        }

        private static bool LayerCompositorHasDuplicates(LayerCompositor layerCompositor)
        {
            foreach (LayerCompositor.Layer layer in layerCompositor.Layers)
            {
                if (Array.FindAll(layerCompositor.Layers, x => x.scene == layer.scene).Length > 1)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool LayerCompositorContainsParentScene(LayerCompositor layerCompositor)
        {
            return Array.Exists(layerCompositor.Layers, x => x.scene == layerCompositor.gameObject.scene.path);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, new string[] { layersListName });
            layersList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            LayerCompositor layerCompositor = (LayerCompositor)target;

            // protect against empty Layers set, and hide the buttons when not needed
            if ((layerCompositor.Layers != null) && (layerCompositor.Layers.Length > 0))
            {
                if (LayerCompositorHasDuplicates(layerCompositor))
                {
                    EditorGUILayout.HelpBox("Duplicate scenes are listed in the layers.", MessageType.Warning);
                }

                if (LayerCompositorContainsParentScene(layerCompositor))
                {
                    EditorGUILayout.HelpBox("The scene of this gameobject is listed as a layer.", MessageType.Warning);
                }

                if (GUILayout.Button("Load/unload selected layer"))
                {
                    if (layersList.index != -1)
                    {
                        LayerCompositor.Layer selectedLayer = layerCompositor.Layers[layersList.index];
                        if (LayerCompositor.IsLayerLoaded(selectedLayer))
                        {
                            layerCompositor.UnloadLayer(selectedLayer.id);
                        }
                        else
                        {
                            layerCompositor.LoadLayer(selectedLayer.id);
                        }
                    }
                }

                if (GUILayout.Button("Refresh"))
                {
                    LayerCompositor.RefreshLayers(null, layerCompositor.Layers, true, layerCompositor.gameObject.scene);
                }
            }
        }
    }
}
