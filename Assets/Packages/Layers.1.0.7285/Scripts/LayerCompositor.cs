// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace MRS.Layers
{
    /// <summary>
    /// An easy to use interface for multi scene editing.
    /// </summary>
    [ExecuteInEditMode]
    public class LayerCompositor : MonoBehaviour
    {
        [Serializable]
        public class Layer
        {
            public string id = "NoId";
            public bool autoload = true;
            public bool loadactive = true;
            public SceneField scene;
        }

        [SerializeField]
        private Layer[] layers;

        public Layer[] Layers
        {
            get { return layers; }
        }

        private static Dictionary<string, int> LayerReferences = new Dictionary<string, int>();

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoadComplete;

            // Wait one frame because calling OpenScene from assembly reloading callbacks is not supported.
            StartCoroutine(RefreshLayersYield(null, layers, true, gameObject.scene));
        }

        private void OnDisable()
        {
            RefreshLayers(layers, null, false, gameObject.scene);

            SceneManager.sceneLoaded -= OnSceneLoadComplete;
        }

        public static void RefreshLayers(Layer[] previousLayers, Layer[] newLayers, 
                                         bool refreshBuildSettings, Scene currentScene)
        {
            if (previousLayers == null)
            {
                previousLayers = new Layer[0];
            }

            if (newLayers == null)
            {
                newLayers = new Layer[0];
            }

            Layer[] layersToUnLoad = previousLayers.Except(newLayers).ToArray();

            if (refreshBuildSettings)
            {
                RefreshBuildSettings(currentScene, layersToUnLoad, newLayers);
            }

            foreach (Layer layer in layersToUnLoad)
            {
                UnloadLayer(layer, currentScene);
            }

            foreach (Layer layer in newLayers)
            {
                if (layer.autoload)
                {
                    LoadLayer(layer);
                }
            }
        }

        private IEnumerator RefreshLayersYield(Layer[] previousLayers, Layer[] newLayers, 
                                               bool refreshBuildSettings, Scene currentScene)
        {
            yield return null;

            RefreshLayers(previousLayers, newLayers, refreshBuildSettings, currentScene);
        }

        private static void RefreshBuildSettings(Scene currentScene, Layer[] layersToRemove, Layer[] layersToAdd)
        {
#if UNITY_EDITOR
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            foreach (Layer layer in layersToRemove)
            {
                scenes.RemoveAll(x => x.path == layer.scene.Path);
            }

            if (!scenes.Exists(x => x.path == currentScene.path))
            {
                scenes.Add(new EditorBuildSettingsScene(currentScene.path, true));
            }

            foreach (Layer layer in layersToAdd)
            {
                if (scenes.Exists(x => x.path == layer.scene.Path))
                {
                    continue;
                }

                scenes.Add(new EditorBuildSettingsScene(layer.scene.Path, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
#endif
        }

        private static bool TrackLayerReference(Layer layer, bool isLoad)
        {
            int count = LayerReferences.ContainsKey(layer.scene) ? LayerReferences[layer.scene] : 0;
            count = (isLoad) ? count + 1 : count - 1;
            LayerReferences[layer.scene] = count;

            return (count > 0);
        }

        /// <summary>
        /// Tests if the layer's scene path matches one in the SceneManager's scene set.
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static bool IsLayerLoaded(Layer layer)
        {
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (layer.scene.Path == scene.path)
                {
                    return true;
                }
            }

            return false;
        }

        private static void LoadLayer(Layer layer)
        {
            if (string.IsNullOrEmpty(layer.scene.Path))
            {
                return;
            }

            TrackLayerReference(layer, true);

            if (IsLayerLoaded(layer))
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorSceneManager.OpenScene(layer.scene, OpenSceneMode.Additive);

                return;
            }
#endif

            SceneManager.LoadSceneAsync(layer.scene, LoadSceneMode.Additive);
        }

        private static void UnloadLayer(Layer layer, Scene currentScene)
        {
            if (string.IsNullOrEmpty(layer.scene.Path))
            {
                return;
            }

            if (!IsLayerLoaded(layer))
            {
                return;
            }

            if (TrackLayerReference(layer, false))
            {
                return;
            }

            // Avoid unloading the scene being edited.
            if (layer.scene.Path == currentScene.path)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Scene scene = SceneManager.GetSceneByPath(layer.scene);
                EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new Scene[] { scene });
                EditorSceneManager.CloseScene(scene, true);

                return;
            }
#endif

            SceneManager.UnloadSceneAsync(layer.scene);
        }

        void OnSceneLoadComplete(Scene scene, LoadSceneMode mode)
        {
            Layer layer;
            if (GetLayerByScenePath(scene.path, out layer))
            {
                if (layer.loadactive == false)
                {
                    SetLayerActiveState(layer, false);
                }
            }
        }

        private bool GetLayerById(string id, out Layer layer)
        {
            for (int i = 0; i < layers.Length; ++i)
            {
                if (layers[i].id == id)
                {
                    layer = layers[i];
                    return true;
                }
            }
            layer = null;
            return false;
        }

        private bool GetLayerByScenePath(string path, out Layer layer)
        {
            for (int i = 0; i < layers.Length; ++i)
            {
                if (layers[i].scene.Path == path)
                {
                    layer = layers[i];
                    return true;
                }
            }
            layer = null;
            return false;
        }

        private void SetLayerActiveState(Layer layer, bool active)
        {
            if (IsLayerLoaded(layer))
            {
                Scene scene = SceneManager.GetSceneByPath(layer.scene);
                GameObject[] rootObjects = scene.GetRootGameObjects();
                for (int i = 0; i < rootObjects.Length; ++i)
                {
                    // When called from OnSceneLoadComplete to set a layer inactive, this executes
                    // before the layer is initialized. If the layer contains another LayerCompositor
                    // then this breaks the reference counting. To alleviate this, we do not alter
                    // the active state of any top-level gameobject containing a LayerCompositor.
                    if (rootObjects[i].GetComponent<LayerCompositor>() == null)
                    {
                        rootObjects[i].SetActive(active);
                    }
                }
            }
        }

        #region public API

        /// <summary>
        /// Signal the compositor to load a layer.
        /// </summary>
        /// <param name="id">The layer Id.</param>
        public void LoadLayer(string id)
        {
            Layer layer;
            if (GetLayerById(id, out layer))
            {
                LoadLayer(layer);
            }
        }

        /// <summary>
        /// Signal the compositor to unload a layer.
        /// </summary>
        /// <param name="id">The layer Id.</param>
        public void UnloadLayer(string id)
        {
            Layer layer;
            if (GetLayerById(id, out layer))
            {
                UnloadLayer(layer, gameObject.scene);
            }
        }

        /// <summary>
        /// Signal the compositor to set a layer's objects active.
        /// Note: single-argument signature to ease API-use via in-editor Unity Events
        /// </summary>
        /// <param name="id">The layer Id.</param>
        public void SetLayerActive(string id)
        {
            Layer layer;
            if (GetLayerById(id, out layer))
            {
                SetLayerActiveState(layer, true);
            }
        }

        /// <summary>
        /// Signal the compositor to set a layer's objects inactive.
        /// Note: single-argument signature to ease API-use via in-editor Unity Events
        /// </summary>
        /// <param name="id">The layer Id.</param>
        public void SetLayerInactive(string id)
        {
            Layer layer;
            if (GetLayerById(id, out layer))
            {
                SetLayerActiveState(layer, false);
            }
        }
        
        #endregion
    }
}
