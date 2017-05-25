// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using UnityEngine;

namespace GalaxyExplorer
{
    public class ToolManager : GE_Singleton<ToolManager>
    {
        public Tool SelectedTool = null;
        public GameObject BackButton;
        public GameObject ShowButton;
        public GameObject HideButton;
        public float TargetMinZoomSize = 0.15f;
        public float LargestZoom = 3.0f;

        public event Action ContentZoomChanged;

        private bool locked = false;
        private ToolPanel panel;
        private ToolSounds toolSounds;

        public bool IsLocked
        {
            get { return locked; }
        }

        private float smallestZoom;

        public float SmallestZoom
        {
            get { return smallestZoom; }
        }

        private void Awake()
        {
            if (TransitionManager.Instance == null)
            {
                Debug.LogWarning("ToolManager: No TransitionManager was found, so the zoom tool will not properly size content - transition manager is needed to identify when new content has loaded.");
            }

            smallestZoom = TargetMinZoomSize;
            panel = GetComponent<ToolPanel>();

            if (panel == null)
            {
                Debug.LogError("ToolManager couldn't find ToolPanel. Hiding and showing of Tools unavailable.");
            }

            toolSounds = GetComponentInChildren<ToolSounds>();

            if (panel == null)
            {
                Debug.LogError("ToolManager couldn't find ToolSounds.");
            }

            KeyboardInput kbd = KeyboardInput.Instance;
            KeyboardInput.KeyEvent keyEvent = KeyboardInput.KeyEvent.KeyDown;
            kbd.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Equals,      keyEvent), HandleKeyboardZoomIn);
            kbd.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Plus,        keyEvent), HandleKeyboardZoomIn);
            kbd.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.KeypadPlus,  keyEvent), HandleKeyboardZoomIn);
            kbd.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Minus,       keyEvent), HandleKeyboardZoomOut);
            kbd.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.KeypadMinus, keyEvent), HandleKeyboardZoomOut);

        }

        private bool ctrlKeyIsDown = false;
        private bool lCtrlKeyIsDown = false;
        private bool rCtrlKeyIsDown = false;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                lCtrlKeyIsDown = true;
            }
            if (Input.GetKeyDown(KeyCode.RightControl))
            {
                rCtrlKeyIsDown = true;
            }
            if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                lCtrlKeyIsDown = false;
            }
            if (Input.GetKeyUp(KeyCode.RightControl))
            {
                rCtrlKeyIsDown = false;
            }
            ctrlKeyIsDown = lCtrlKeyIsDown || rCtrlKeyIsDown;
        }

        private void HandleKeyboardZoomOut(KeyboardInput.KeyCodeEventPair keyCodeEvent)
        {
            HandleKeyboardZoom(-1);
        }
        private void HandleKeyboardZoomIn(KeyboardInput.KeyCodeEventPair keyCodeEvent)
        {
            HandleKeyboardZoom(1);
        }
        private void HandleKeyboardZoom(int direction)
        {
            if (ctrlKeyIsDown)
            {
                InputRouter.Instance.UpdateZoomFromXaml(1 + (direction * 0.03f));
            }
        }

        private void Start()
        {
            if (TransitionManager.Instance)
            {
                TransitionManager.Instance.ContentLoaded += ViewContentLoaded;
            }

            // It would be nice if we had callbacks for registering voice
            // commands, but it requires finding game objects that are not
            // active; this bypasses that issue by getting all of the children
            // gaze selection targets and manually registering them in the tool
            // panel which is active
            GazeSelectionTarget[] selectionTargets = GetComponentsInChildren<GazeSelectionTarget>(true);
            foreach (GazeSelectionTarget selectionTarget in selectionTargets)
            {
                selectionTarget.RegisterVoiceCommands();
            }

            ShowButton.SetActive(false);
            BackButton.SetActive(false);
            toolSounds.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (TransitionManager.Instance)
            {
                TransitionManager.Instance.ContentLoaded -= ViewContentLoaded;
            }
        }

        private void ViewContentLoaded()
        {
            if (ViewLoader.Instance != null)
            {
                GameObject content = ViewLoader.Instance.GetCurrentContent();
                SceneSizer contentSizer = content.GetComponent<SceneSizer>();
                smallestZoom = Mathf.Max(content.transform.localScale.x, content.transform.localScale.y, content.transform.localScale.z);

                // make sure all content is to the same scale by removing the fill percentage, so this is the scale that fits to view volume
                if (contentSizer != null)
                {
                    smallestZoom /= contentSizer.FullScreenFillPercentage;
                }

                // adjust the smallest zoom from the content's loaded state to our target min zoom size (currently fitted to view volume)
                if (TransitionManager.Instance != null)
                {
                    smallestZoom *= TargetMinZoomSize / Mathf.Max(TransitionManager.Instance.ViewVolume.transform.lossyScale.x, TransitionManager.Instance.ViewVolume.transform.lossyScale.y, TransitionManager.Instance.ViewVolume.transform.lossyScale.z);
                }
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

            Tool[] tools = GetComponentsInChildren<Tool>();
            foreach (Tool tool in tools)
            {
                if (removeHighlight)
                {
                    tool.RemoveHighlight();
                }

                tool.Unselect();
            }

            Button[] buttons = GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
            {
                button.RemoveHighlight();
            }
        }

        public bool SelectTool(Tool tool)
        {
            if (locked)
            {
                return false;
            }

            UnselectAllTools(removeHighlight: false);
            SelectedTool = tool;

            if (Cursor.Instance)
            {
                Cursor.Instance.ApplyToolState(tool.type);
            }

            return true;
        }

        public bool DeselectTool(Tool tool)
        {
            if (locked)
            {
                return false;
            }

            if (Cursor.Instance)
            {
                Cursor.Instance.ClearToolState();
            }

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
                ToolSounds.Instance.PlayMoveToolsDownSound();
                ShowButton.SetActive(true);
                HideButton.SetActive(false);
            }
        }

        public void RaiseTools()
        {
            panel.IsLowered = false;

            if (ShowButton && HideButton)
            {
                ToolSounds.Instance.PlayMoveToolsUpSound();
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
            StartCoroutine(HideToolsAsync(instant));
        }

        [ContextMenu("Show Tools")]
        public void ShowTools()
        {
            StartCoroutine(ShowToolsAsync());
        }

        public IEnumerator HideToolsAsync(bool instant)
        {
            yield return StartCoroutine(panel.FadeOut(instant));
        }

        public IEnumerator ShowToolsAsync()
        {
            yield return StartCoroutine(panel.FadeIn());
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

        public void UpdateZoomFromXaml(float delta)
        {
            if (!IsLocked)
            {

                // TODO: Normalize this zoom code with what is in Tool.HandleUpdatedInput
                float smallestScale = ToolManager.Instance.TargetMinZoomSize;

                Bounds currentBounds = Tool.GetContentBounds();

                float contentXSize = currentBounds.extents.x == 0 ? smallestScale : currentBounds.extents.x;
                float zoomContentSizeFactor = contentXSize / smallestScale;
                float contentScalar = ToolManager.Instance.SmallestZoom / ToolManager.Instance.TargetMinZoomSize;
                float newScale = ViewLoader.Instance.GetCurrentContent().transform.localScale.x * delta;

                if (newScale < ToolManager.Instance.SmallestZoom)
                {
                    newScale = ToolManager.Instance.SmallestZoom;
                }

                if (newScale > ToolManager.Instance.LargestZoom)
                {
                    newScale = ToolManager.Instance.LargestZoom;
                }

                ViewLoader.Instance.GetCurrentContent().transform.localScale = new Vector3(newScale, newScale, newScale);
                ToolManager.Instance.RaiseContentZoomChanged();
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