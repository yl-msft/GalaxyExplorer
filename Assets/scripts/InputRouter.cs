//// Copyright Microsoft Corporation. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.
//
////using HoloToolkit.Unity.InputModule;
//using UnityEngine;
//
//namespace GalaxyExplorer
//{
//    public class InputRouter : MonoBehaviour
//    {
//        [SerializeField]
//        [Tooltip("Factor that affects portion of movement during zoom in and out in Desktop")]
//        private float ZoomDesktopFactor = 1.0f;
//
//        [SerializeField]
//        [Tooltip("Min zoom in Desktop")]
//        private float MinZoomDesktop = 0.0f;
//
//        [SerializeField]
//        [Tooltip("Max zoom in Desktop")]
//        private float MaxZoomDesktop = 4.0f;
//
//        public delegate void KeyboadSelectionDelegate();
//        public KeyboadSelectionDelegate OnKeyboadSelection;
//
//        private bool isCtrlHeld = false;
//
//        private void Start()
//        {
//            // Register key events
//            KeyboardManager.KeyEvent keyDownEvent = KeyboardManager.KeyEvent.KeyDown;
//            KeyboardManager.Instance.RegisterKeyEvent(new KeyboardManager.KeyCodeEventPair(KeyCode.Backspace, keyDownEvent), BackSpaceKeyboardHandler);
//
//            KeyboardManager.Instance.RegisterKeyEvent(new KeyboardManager.KeyCodeEventPair(KeyCode.Alpha0, keyDownEvent), ResetCameraKeyboardHandler);
//
//            KeyboardManager.KeyEvent keyHeldEvent = KeyboardManager.KeyEvent.KeyHeld;
//            KeyboardManager.Instance.RegisterKeyEvent(new KeyboardManager.KeyCodeEventPair(KeyCode.LeftControl, keyHeldEvent), CtrlKeyboardHandler);
//            KeyboardManager.Instance.RegisterKeyEvent(new KeyboardManager.KeyCodeEventPair(KeyCode.RightControl, keyHeldEvent), CtrlKeyboardHandler);
//
//            KeyboardManager.Instance.RegisterKeyEvent(new KeyboardManager.KeyCodeEventPair(KeyCode.Equals, keyHeldEvent), PlusMinusKeyboardHandler);
//            KeyboardManager.Instance.RegisterKeyEvent(new KeyboardManager.KeyCodeEventPair(KeyCode.Minus, keyHeldEvent), PlusMinusKeyboardHandler);
//
//            KeyboardManager.KeyEvent keyUpEvent = KeyboardManager.KeyEvent.KeyUp;
//            KeyboardManager.Instance.RegisterKeyEvent(new KeyboardManager.KeyCodeEventPair(KeyCode.LeftControl, keyUpEvent), CtrlKeyboardHandler);
//            KeyboardManager.Instance.RegisterKeyEvent(new KeyboardManager.KeyCodeEventPair(KeyCode.RightControl, keyUpEvent), CtrlKeyboardHandler);
//
//            RegisterForKeyboardAndMouseGoBackButtons();
//        }
//
//        private void RegisterForKeyboardAndMouseGoBackButtons()
//        {
//#if WINDOWS_UWP
//            UnityEngine.WSA.Application.InvokeOnUIThread(() =>
//            {
//                var coreWindow = Windows.UI.Core.CoreWindow.GetForCurrentThread();
//                if (coreWindow != null)
//                {
//                    coreWindow.KeyDown += (sender, args) =>
//                    {
//                        // check for VK_BROWSER_BACK (available on some keyboards)
//                        if (args.VirtualKey == Windows.System.VirtualKey.GoBack)
//                        {
//                            KeyboardManager.Instance.InjectKeyboardEvent(new KeyboardManager.KeyCodeEventPair(KeyCode.Backspace, KeyboardManager.KeyEvent.KeyDown));
//                        }
//                    };
//                    coreWindow.PointerPressed += (sender, args) =>
//                    {
//                        // check for VK_XBUTTON1
//                        if (args.CurrentPoint.Properties.IsXButton1Pressed)
//                        {
//                            KeyboardManager.Instance.InjectKeyboardEvent(new KeyboardManager.KeyCodeEventPair(KeyCode.Backspace, KeyboardManager.KeyEvent.KeyDown));
//                        }
//                    };
//                }
//            }, false);
//#endif
//        }
//
//        private void Update()
//        {
//            // If ctrl + mouse wheel then zoom in and out
//            if (isCtrlHeld && !Mathf.Approximately(Input.mouseScrollDelta.y, 0.0f))
//            {
//                ZoomInOut(Input.mouseScrollDelta.y * ZoomDesktopFactor);
//            }
//        }
//
//        private void BackSpaceKeyboardHandler(KeyboardManager.KeyCodeEventPair keyCodeEvent)
//        {
//            GalaxyExplorerManager.Instance.TransitionManager.LoadPrevScene();
//            OnKeyboadSelection?.Invoke();
//        }
//
//        private void CtrlKeyboardHandler(KeyboardManager.KeyCodeEventPair keyCodeEvent)
//        {
//            if (keyCodeEvent.KeyEvent == KeyboardManager.KeyEvent.KeyHeld)
//            {
//                isCtrlHeld = true;
//            }
//            else if (keyCodeEvent.KeyEvent == KeyboardManager.KeyEvent.KeyUp)
//            {
//                isCtrlHeld = false;
//            }
//        }
//
//        private void PlusMinusKeyboardHandler(KeyboardManager.KeyCodeEventPair keyCodeEvent)
//        {
//            if (isCtrlHeld)
//            {
//                ZoomInOut(((keyCodeEvent.KeyCode == KeyCode.Minus) ? -1.0f : 1.0f) * ZoomDesktopFactor);
//            }
//        }
//
//
//        // Zoom in out in Desktop. Clamp position to max min
//        private void ZoomInOut(float amount)
//        {
//            float sign = (!Mathf.Approximately(amount, 0.0f)) ? Mathf.Abs(amount) / amount : 0.0f;
//            Vector3 cameraPosition = Camera.main.transform.position;
//            Vector3 cameraToScene = (cameraPosition - GalaxyExplorerManager.Instance.CameraControllerHandler.transform.position);
//
//            if (sign > 0.0f && cameraToScene.z < 0.0f ? Mathf.Abs(cameraToScene.z) > MinZoomDesktop : cameraToScene.magnitude < MaxZoomDesktop)
//            {
//                Vector3 direction = -GalaxyExplorerManager.Instance.CameraControllerHandler.transform.forward;
//                GalaxyExplorerManager.Instance.CameraControllerHandler.transform.localPosition += sign * direction * ZoomDesktopFactor;
//                Vector3 currentPosition = GalaxyExplorerManager.Instance.CameraControllerHandler.transform.position;
//
//                if (Mathf.Abs((cameraPosition - currentPosition).z) > MaxZoomDesktop)
//                {
//                    Vector3 clampedPosition = cameraPosition - direction * MaxZoomDesktop;
//                    GalaxyExplorerManager.Instance.CameraControllerHandler.transform.position = new Vector3(currentPosition.x, currentPosition.y, clampedPosition.z);
//                }
//                else if (Mathf.Abs((cameraPosition - currentPosition).z) < MinZoomDesktop)
//                {
//                    Vector3 clampedPosition = cameraPosition - direction * MinZoomDesktop;
//                    GalaxyExplorerManager.Instance.CameraControllerHandler.transform.position = new Vector3(currentPosition.x, currentPosition.y, clampedPosition.z);
//                }
//            }
//        }
//
//        private void ResetCameraKeyboardHandler(KeyboardManager.KeyCodeEventPair keyCodeEvent)
//        {
//            if (isCtrlHeld)
//            {
//                GalaxyExplorerManager.Instance.TransitionManager.ResetDesktopCameraToOrigin();
//            }
//        }
//    }
//}
