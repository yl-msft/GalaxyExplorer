// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using System;
using System.Collections;
using UnityEngine;

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

        public event Action ContentZoomChanged;

        private bool locked = false;
        private ToolPanel panel;

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
            panel = GetComponent<ToolPanel>();

            if (panel == null)
            {
                Debug.LogError("ToolManager couldn't find ToolPanel. Hiding and showing of Tools unavailable.");
            }
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

        public void UnselectAllTools(bool removeHighlight = true)
        {
            SelectedTool = null;

            // TODO remove button highlight
        }

        public bool SelectTool(GEInteractiveToggle tool)
        {
            if (locked)
            {
                return false;
            }

            UnselectAllTools(removeHighlight: false);
            SelectedTool = tool;

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
            HideTools(false);
        }

        public void HideTools(bool instant)
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(HideToolsAsync(instant));
            }
        }

        [ContextMenu("Show Tools")]
        public void ShowTools()
        {
            gameObject.SetActive(true);
            StartCoroutine(ShowToolsAsync());
        }

        public IEnumerator HideToolsAsync(bool instant)
        {
            ToolsVisible = false;
            // TODO fade tools
            gameObject.SetActive(false);

            yield return null;
        }

        public IEnumerator ShowToolsAsync()
        {
            if (GalaxyExplorerManager.IsHoloLens || GalaxyExplorerManager.IsImmersiveHMD)
            {
                ToolsVisible = true;
                // TODO fade in tools
                yield return null;
            }
        }

        public void ShowBackButton()
        {
            if (ToolManager.BackButtonVisibilityChangeRequested != null)
            {
                ToolManager.BackButtonVisibilityChangeRequested(visible: true);
            }
            else if (BackButton)
            {
                BackButton.SetActive(true);
            }
        }

        public void HideBackButton()
        {
            if (ToolManager.BackButtonVisibilityChangeRequested != null)
            {
                ToolManager.BackButtonVisibilityChangeRequested(visible: false);
            }
            else if (BackButton)
            {
                BackButton.SetActive(false);
            }
        }

        public void RaiseContentZoomChanged()
        {
            if (ContentZoomChanged != null)
            {
                ContentZoomChanged();
            }
        }

        public delegate void ButtonVisibilityRequest(bool visible);
        public static event ButtonVisibilityRequest BackButtonVisibilityChangeRequested;
    }
}
