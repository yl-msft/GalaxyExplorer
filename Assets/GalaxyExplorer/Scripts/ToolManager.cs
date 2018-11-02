// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GalaxyExplorer
{

    public class ToolManager : MonoBehaviour
    {
        public GEInteractiveToggle SelectedTool = null;
        public GameObject BackButton;
        public GameObject ShowButton;
        public GameObject HideButton;
        public float TargetMinZoomSize = 0.15f;
        public float LargestZoom = 3.0f;

        [HideInInspector]
        public bool ToolsVisible = false;

        private bool locked = false;
        private ToolPanel panel;

        private List<GEInteractiveToggle> allButtons = new List<GEInteractiveToggle>();
        private ViewLoader loader = null;
        private TransitionManager transition = null;
        private BoundingBox boundingBox = null;
        private bool groupBoundinBoxEntities = false;

        public bool IsLocked
        {
            get { return locked; }
        }

        private float smallestZoom;

        public float SmallestZoom
        {
            get { return smallestZoom; }
        }

        private void Start()
        {
            if (GalaxyExplorerManager.IsDesktop)
            {
                gameObject.SetActive(false);
                return;
            }

            smallestZoom = TargetMinZoomSize;

            panel = GetComponentInChildren<ToolPanel>(true) as ToolPanel;
            if (panel == null)
            {
                Debug.LogError("ToolManager couldn't find ToolPanel. Hiding and showing of Tools unavailable.");
            }

            GEInteractiveToggle[] buttonsArray = GetComponentsInChildren<GEInteractiveToggle>(true);
            foreach (var button in buttonsArray)
            {
                allButtons.Add(button);
            }

            ShowButton?.SetActive(false);
            BackButton?.SetActive(false);

            transition = FindObjectOfType<TransitionManager>();
            boundingBox = FindObjectOfType<BoundingBox>();

            loader = FindObjectOfType<ViewLoader>();
            if (loader)
            {
                loader.OnSceneIsLoaded += OnSceneIsLoaded;
                loader.OnLoadNewScene += OnLoadNewScene;
            }

            StartCoroutine(OnBoundingBoxCreated());

            // if its unity editor and a non intro scene is active on start then make the menu visible
#if UNITY_EDITOR
            OnSceneIsLoaded();
#endif
        }

        public void OnSceneIsLoaded()
        {
            StartCoroutine(OnSceneIsLoadedCoroutine());
        }

        // Callback when a new scene is requested to be loaded
        public void OnLoadNewScene()
        {
            if (ToolsVisible)
            {
                HideTools();
            }
        }

        // Callback when a new scene is loaded
        private IEnumerator OnSceneIsLoadedCoroutine()
        {
            if (!ToolsVisible && !loader.IsIntro())
            {
                // If tools/menu is not visible and intro flow has finished then make menu visible
                while (transition.InTransition)
                {
                    yield return null;
                }

                ShowTools();

                // If there is previous scene then user is able to go back so activate the back button
                BackButton?.SetActive(loader.IsTherePreviousScene());

            }

            yield return null;
        }

        // prevents tools from being accessed
        public void LockTools()
        {
            if (!locked)
            {
                UnselectAllTools();
                locked = true;
            }
        }

        // re-enables tool access
        public void UnlockTools()
        {
            locked = false;
        }

        public void UnselectAllTools()
        {
            // Deselect any other button that might be selected
            foreach (var button in allButtons)
            {
                button.DeselectButton();
            }
        }

        public bool SelectTool(GEInteractiveToggle tool)
        {
            if (locked)
            {
                return false;
            }

            bool isAnyToolSelected = (SelectedTool != null);
            SelectedTool = tool;

            // if Any tool was selected before this one was, then need to deselect the previous one
            if (isAnyToolSelected)
            {
                UnselectAllTools();
            }

            // TODO set cursor to select tool state

            return true;
        }

        public bool DeselectTool(GEInteractiveToggle tool)
        {
            if (locked)
            {
                return false;
            }

            // TODO set cursor normal state

            if (SelectedTool == tool)
            {
                SelectedTool = null;
                return true;
            }

            return false;
        }

        public void LowerTools()
        {
            panel.IsLowered = true;

            if (ShowButton && HideButton)
            {
                ShowButton.SetActive(true);
                HideButton.SetActive(false);
            }
        }

        public void RaiseTools()
        {
            panel.IsLowered = false;

            if (ShowButton && HideButton)
            {
                ShowButton.SetActive(false);
                HideButton.SetActive(true);
            }
        }

        public void ToggleTools()
        {
            if (panel.IsLowered)
            {
                RaiseTools();
            }
            else
            {
                LowerTools();
            }
        }

        [ContextMenu("Hide Tools")]
        public void HideTools()
        {
            StartCoroutine(HideToolsAsync());
        }

        [ContextMenu("Show Tools")]
        public void ShowTools()
        {
            StartCoroutine(ShowToolsAsync());
        }

        public IEnumerator HideToolsAsync()
        {
            ToolsVisible = false;
            // TODO fade tools
            panel.gameObject.SetActive(false);

            yield return null;
        }

        public IEnumerator ShowToolsAsync()
        {
            if (GalaxyExplorerManager.IsHoloLens || GalaxyExplorerManager.IsImmersiveHMD)
            {
                panel.gameObject.SetActive(true);
                ToolsVisible = true;
                // TODO fade in tools
                yield return null;
            }
        }

        public void ShowBackButton()
        {
            if (BackButton)
            {
                BackButton.SetActive(true);
            }
        }

        public void HideBackButton()
        {
            if (BackButton)
            {
                BackButton.SetActive(false);
            }
        }

        // On manipulate scene button pressed from menu
        // Enable bounding box and scale it appropriately so in coves the whole scene
        public void OnManipulateButtonPressed(bool enable)
        {
            if (boundingBox)
            {
                boundingBox.Target.GetComponentInChildren<Collider>().enabled = enable;

                if (enable)
                {
                    Bounds totalBounds = new Bounds(Vector3.zero, Vector3.zero);
                    SceneTransition sceneTransition = FindObjectOfType<SceneTransition>();
                    Vector3 size = sceneTransition.ThisEntireSceneCollider.gameObject.transform.lossyScale;
                    Vector3 colliderScale = sceneTransition.ThisEntireSceneCollider.size; // bounds.size
                    Vector3 colliderCenter = sceneTransition.ThisEntireSceneCollider.bounds.center;
                    Debug.Log("Collider bounds " + colliderScale + " center " + colliderCenter);

                    size = new Vector3(size.x * colliderScale.x, size.y * colliderScale.y, size.z * colliderScale.z);

                    //boundingBox.Target.transform.parent.transform.rotation = sceneTransition.ThisEntireSceneCollider.transform.parent.localRotation;

                    Transform child = boundingBox.Target.transform.GetChild(0);
                    child.position = colliderCenter; //  sceneTransition.ThisEntireSceneCollider.transform.position;
                    child.localScale = new Vector3(size.x / boundingBox.Target.transform.lossyScale.x, 
                        size.y / boundingBox.Target.transform.lossyScale.y, 
                        size.z / boundingBox.Target.transform.lossyScale.z);

                    boundingBox.Target.GetComponent<BoundingBoxRig>().Activate();
                    StartCoroutine(OnBoundingBoxCreated());
                }
                else
                {
                    boundingBox.Target.GetComponent<BoundingBoxRig>().Deactivate();
                }
            }
        }

        #region BoundingBox
        // Bounding Box Rig creates many entities used by bounding box
        // All of them are created in the scene under no parent, and makes the scene editor meesy and difficult to find sth
        // Find all entities created by BoundingBoxRig and group them under one single parent
        private IEnumerator OnBoundingBoxCreated()
        {
            yield return new WaitForSeconds(1);

            if (groupBoundinBoxEntities)
            {
                yield break;
            }

            GameObject parent = GameObject.Find("BoundingBoxEntities");
            parent = (parent == null) ?  new GameObject() : parent;
            parent.name = "BoundingBoxEntities";
            parent.transform.parent = transform;

            GameObject center = GameObject.Find("center");
            if (center)
            {
                center.transform.parent = parent.transform;
            }

            GameObject bbb = GameObject.Find("BoundingBoxBasic(Clone)");
            if (bbb)
            {
                bbb.transform.parent = parent.transform;
            }

            List<GameObject> corners = GetObjectsWithName("Corner");
            if (corners != null && corners.Count > 0)
            {
                foreach (var entity in corners)
                {
                    entity.transform.parent = parent.transform;
                }
            }

            List<GameObject> middles = GetObjectsWithName("Middle");
            if (middles != null && middles.Count > 0)
            {
                foreach (var entity in middles)
                {
                    entity.transform.parent = parent.transform;
                }

                groupBoundinBoxEntities = true;
            }

            AppBar appBar = FindObjectOfType<AppBar>();
            if (appBar)
            {
                appBar.transform.parent = parent.transform;
                appBar.gameObject.SetActive(false);
            }

            yield return null;
        }

        // Returns all objects that contain in their name the given string
        private List<GameObject> GetObjectsWithName(string objectName)
        {
            GameObject[] allObjects = (GameObject[])FindObjectsOfType(typeof(GameObject));
            List<GameObject> allObjWithName = new List<GameObject>();
            for (int i = 0; i < allObjects.Length; i++)
            {
                if (allObjects[i].name.Contains(objectName))
                {
                    allObjWithName.Add(allObjects[i]);
                }
            }

            return allObjWithName;
        }
        #endregion
    }
}
