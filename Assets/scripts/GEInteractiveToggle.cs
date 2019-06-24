//// Copyright Microsoft Corporation. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.
//
//using HoloToolkit.Examples.InteractiveElements;
//using Microsoft.MixedReality.Toolkit.SDK.UX.Interactable.Events;
//using UnityEngine;
//using UnityEngine.Events;
//
//// Galaxy Explorer button based on MRTK button
//namespace GalaxyExplorer
//{
//    public class GEInteractiveToggle : InteractiveToggle
//    {
//        [Header("GEInteractiveToggle members")]
//        [SerializeField]
//        [Tooltip("Is button that stays active even if another button is selected. Its deactivated only if user explicitly selects it again")]
//        private bool isPrimaryButton = false;
//
//        public bool IsPrimaryButton
//        {
//            get { return isPrimaryButton; }
//        }
//
//        public UnityEvent OnGazeSelect;
//        public UnityEvent OnGazeDeselect;
//
//        protected override void Start()
//        {
//            base.Start();
//        }
//
//        // On button click toggle logic, set this as the selected one or if it was select it then unselect it
//        public override void ToggleLogic()
//        {
//            if (IsSelected)
//            {
//                GalaxyExplorerManager.Instance.ToolsManager.SelectedTool = null;
//            }
//            else
//            {
//                GalaxyExplorerManager.Instance.ToolsManager.SelectTool(this);
//            }
//
//            base.ToggleLogic();
//        }
//
//        public override void OnFocusEnter()
//        {
//            base.OnFocusEnter();
//
//            if ((AllowDeselect && !IsSelected) && !PassiveMode)
//            {
//                OnGazeSelect?.Invoke();
//            }
//        }
//
//        public override void OnFocusExit()
//        {
//            base.OnFocusExit();
//
//            if ((AllowDeselect && !IsSelected) && !PassiveMode)
//            {
//                OnGazeDeselect?.Invoke();
//            }
//        }
//
//        // Deselect Button ONLY if its not the one currently selected
//        // So deselect it if another button is selected now
//        // Dont deselect that way any primary buttons
//        public void DeselectButton()
//        {
//            if (IsSelected &&
//                !PassiveMode
//                && GalaxyExplorerManager.Instance.ToolsManager.SelectedTool != this
//                && !IsPrimaryButton)
//            {
//                if (OnDeselection != null)
//                {
//                    OnDeselection.Invoke();
//                }
//
//                if (OnGazeDeselect != null)
//                {
//                    OnGazeDeselect.Invoke();
//                }
//
//                IsSelected = false;
//                HasGaze = false;
//                HasSelection = false;
//
//                Debug.Log("Button " + gameObject.name + " was deselected because it was selected while another button got selected");
//            }
//            IsSelected = false;
//        }
//
//        // Reset is needed in buttons that the moment they are selected they are deselected as well, so they dont stay active
//        // Such buttons are the controls ones, show and hide
//        // This function is hooked up in editor events
//        public void ResetButton()
//        {
//            if (IsSelected && GalaxyExplorerManager.Instance.ToolsManager.SelectedTool == this)
//            {
//                GalaxyExplorerManager.Instance.ToolsManager.SelectedTool = null;
//            }
//
//            IsSelected = false;
//            HasGaze = false;
//            HasSelection = false;
//        }
//    }
//}
