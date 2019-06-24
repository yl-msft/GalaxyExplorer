//// Copyright Microsoft Corporation. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.
//
////using HoloToolkit.Unity.InputModule;
//
//using Microsoft.MixedReality.Toolkit.SDK.Input.Handlers;
//using MRS.Audui;
//using UnityEngine;
//
//namespace GalaxyExplorer
//{
//    public class GEKeyboardInputSource : BaseInputSource
//    {
//        public delegate void KeyboardDelegate(GameObject selectedObject);
//        public KeyboardDelegate OnKeyboardSpaceTapDownDelegate;
//        public KeyboardDelegate OnKeyboardSpaceTapUpDelegate;
//
//        private GameObject focusedObject = null;
//        private uint keyboardInputId = 70000;
//
//        public GameObject FocusedObject
//        {
//            get { return focusedObject; }
//        }
//
//        #region Unity methods
//
//        protected virtual void Start()
//        {
//            if (!GalaxyExplorerManager.IsDesktop || !GalaxyExplorerManager.IsImmersiveHMD)
//            {
//                this.enabled = false;
//            }
//
//            // Register key events
//            KeyboardManager.KeyEvent keyDownEvent = KeyboardManager.KeyEvent.KeyDown;
//            KeyboardManager.Instance.RegisterKeyEvent(new KeyboardManager.KeyCodeEventPair(KeyCode.Space, keyDownEvent), SpaceTapKeyboardHandler);
//
//            KeyboardManager.KeyEvent keyUpEvent = KeyboardManager.KeyEvent.KeyUp;
//            KeyboardManager.Instance.RegisterKeyEvent(new KeyboardManager.KeyCodeEventPair(KeyCode.Space, keyUpEvent), SpaceTapKeyboardHandler);
//
//        }
//
//        #endregion // Unity methods
//
//        private void SpaceTapKeyboardHandler(KeyboardManager.KeyCodeEventPair keyCodeEvent)
//        {
//            if (keyCodeEvent.KeyEvent == KeyboardManager.KeyEvent.KeyDown)
//            {
//                // In desktop use the pointed mouse object to override the input manager's focused object
//                if (GalaxyExplorerManager.IsDesktop)
//                {
//                    RaycastHit hit;
//                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//
//                    if (Physics.Raycast(ray, out hit))
//                    {
//                        focusedObject = hit.collider.gameObject;
////                        InputManager.Instance.OverrideFocusedObject = focusedObject;
//                    }
//                }
//                
//                OnKeyboardSpaceTapDownDelegate?.Invoke(focusedObject);
////                InputManager.Instance.RaiseInputClicked(this, keyboardInputId, InteractionSourcePressInfo.Select, 0);
//            }
//            else if (keyCodeEvent.KeyEvent == KeyboardManager.KeyEvent.KeyUp)
//            {
//                OnKeyboardSpaceTapUpDelegate?.Invoke(focusedObject);
////                InputManager.Instance.OverrideFocusedObject = null;
//                focusedObject = null;
//            }
//        }
//
//        #region Base Input Source Methods
//
//        public override bool TryGetSourceKind(uint sourceId, out InteractionSourceInfo sourceKind)
//        {
//            sourceKind = InteractionSourceInfo.Hand;
//            return true;
//        }
//
//        public override bool TryGetPointerPosition(uint sourceId, out Vector3 position)
//        {
//            position = Vector3.zero;
//            return false;
//        }
//
//        public override bool TryGetPointerRotation(uint sourceId, out Quaternion rotation)
//        {
//            rotation = Quaternion.identity;
//            return false;
//        }
//
//        public override bool TryGetPointingRay(uint sourceId, out Ray pointingRay)
//        {
//            pointingRay = default(Ray);
//            return false;
//        }
//
//        public override bool TryGetGripPosition(uint sourceId, out Vector3 position)
//        {
//            position = Vector3.zero;
//            return false;
//        }
//
//        public override bool TryGetGripRotation(uint sourceId, out Quaternion rotation)
//        {
//            rotation = Quaternion.identity;
//            return false;
//        }
//
//        public override SupportedInputInfo GetSupportedInputInfo(uint sourceId)
//        {
//            return SupportedInputInfo.PointerPosition | SupportedInputInfo.Pointing;
//        }
//
//        public override bool TryGetThumbstick(uint sourceId, out bool isPressed, out Vector2 position)
//        {
//            isPressed = false;
//            position = Vector2.zero;
//            return false;
//        }
//
//        public override bool TryGetTouchpad(uint sourceId, out bool isPressed, out bool isTouched, out Vector2 position)
//        {
//            isPressed = false;
//            isTouched = false;
//            position = Vector2.zero;
//            return false;
//        }
//
//        public override bool TryGetSelect(uint sourceId, out bool isPressed, out double pressedAmount)
//        {
//            isPressed = false;
//            pressedAmount = 0.0;
//            return false;
//        }
//
//        public override bool TryGetGrasp(uint sourceId, out bool isPressed)
//        {
//            isPressed = false;
//            return false;
//        }
//
//        public override bool TryGetMenu(uint sourceId, out bool isPressed)
//        {
//            isPressed = false;
//            return false;
//        }
//
//        #endregion // Base Input Source Methods
//    }
//}
