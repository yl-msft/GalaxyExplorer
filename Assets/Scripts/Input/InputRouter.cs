// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using GalaxyExplorer.HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA.Input;

namespace GalaxyExplorer
{
    public class InputRouter : GE_Singleton<InputRouter>
    {
        public Vector3 fakeInput;
        public bool enableFakeInput = false;
        public bool FakeTapUpdate;

        public bool HandsVisible { get; private set; }

        [HideInInspector()]
        public Vector2 XamlMousePosition = new Vector2(0, 0);

        /// <summary>
        /// Inputs that were started and that are currently active
        /// </summary>
        public HashSet<InteractionSourceKind> PressedSources { get; private set; }

        public event Action<InteractionSourceKind, Vector3, HeadPose> InputStarted;
        public event Action<InteractionSourceKind, Vector3, HeadPose> InputUpdated;
        public event Action<InteractionSourceKind, Vector3, HeadPose> InputCompleted;
        public event Action<InteractionSourceKind, Vector3, HeadPose> InputCanceled;

        public event Action InputTapped;

        /// <summary>
        /// May be called several times if the event is handled by several objects
        /// </summary>
        public event Action Tapped;

        private GestureRecognizer gestureRecognizer;
        private bool eventsAreRegistered = false;

        private bool ctrlKeyIsDown = false;
        private bool lCtrlKeyIsDown = false;
        private bool rCtrlKeyIsDown = false;

        private void TryToRegisterEvents()
        {
            if (!eventsAreRegistered)
            {
                if (gestureRecognizer != null)
                {
                    gestureRecognizer.NavigationStartedEvent += OnNavigationStarted;
                    gestureRecognizer.NavigationUpdatedEvent += OnNavigationUpdated;
                    gestureRecognizer.NavigationCompletedEvent += OnNavigationCompleted;
                    gestureRecognizer.NavigationCanceledEvent += OnNavigationCanceled;
                    gestureRecognizer.TappedEvent += OnTapped;
                }

                InteractionManager.SourceDetected += SourceManager_SourceDetected;
                InteractionManager.SourceLost += SourceManager_SourceLost;
                InteractionManager.SourcePressed += SourceManager_SourcePressed;
                InteractionManager.SourceReleased += SourceManager_SourceReleased;

                KeyboardInput kbd = KeyboardInput.Instance;
                if (kbd != null)
                {
                    KeyboardInput.KeyEvent keyEvent = KeyboardInput.KeyEvent.KeyDown;
                    kbd.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Equals, keyEvent), HandleKeyboardZoomIn);
                    kbd.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Plus, keyEvent), HandleKeyboardZoomIn);
                    kbd.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.KeypadPlus, keyEvent), HandleKeyboardZoomIn);
                    kbd.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Minus, keyEvent), HandleKeyboardZoomOut);
                    kbd.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.KeypadMinus, keyEvent), HandleKeyboardZoomOut);

                    keyEvent = KeyboardInput.KeyEvent.KeyReleased;
                    kbd.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Space, keyEvent), FakeTapKeyboardHandler);
                    kbd.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Backspace, keyEvent), HandleBackButtonFromKeyboard);
                }

                eventsAreRegistered = true;
            }
        }

        private void TryToUnregisterEvents()
        {
            if (eventsAreRegistered)
            {
                if (gestureRecognizer != null)
                {
                    gestureRecognizer.NavigationStartedEvent -= OnNavigationStarted;
                    gestureRecognizer.NavigationUpdatedEvent -= OnNavigationUpdated;
                    gestureRecognizer.NavigationCompletedEvent -= OnNavigationCompleted;
                    gestureRecognizer.NavigationCanceledEvent -= OnNavigationCanceled;
                    gestureRecognizer.TappedEvent -= OnTapped;
                }

                InteractionManager.SourceDetected -= SourceManager_SourceDetected;
                InteractionManager.SourceLost -= SourceManager_SourceLost;
                InteractionManager.SourcePressed -= SourceManager_SourcePressed;
                InteractionManager.SourceReleased -= SourceManager_SourceReleased;

                KeyboardInput kbd = KeyboardInput.Instance;
                if (kbd != null)
                {
                    KeyboardInput.KeyEvent keyEvent = KeyboardInput.KeyEvent.KeyDown;
                    kbd.UnregisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Equals, keyEvent), HandleKeyboardZoomIn);
                    kbd.UnregisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Plus, keyEvent), HandleKeyboardZoomIn);
                    kbd.UnregisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.KeypadPlus, keyEvent), HandleKeyboardZoomIn);
                    kbd.UnregisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Minus, keyEvent), HandleKeyboardZoomOut);
                    kbd.UnregisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.KeypadMinus, keyEvent), HandleKeyboardZoomOut);

                    keyEvent = KeyboardInput.KeyEvent.KeyReleased;
                    kbd.UnregisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Space, keyEvent), FakeTapKeyboardHandler);
                    kbd.UnregisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Backspace, keyEvent), HandleBackButtonFromKeyboard);
                }
                eventsAreRegistered = false;
            }
        }

        private void Awake()
        {
            PressedSources = new HashSet<InteractionSourceKind>();
        }

        private void Start()
        {
            gestureRecognizer = new GestureRecognizer();
            gestureRecognizer.SetRecognizableGestures(GestureSettings.Hold | GestureSettings.Tap |
                                                      GestureSettings.NavigationY | GestureSettings.NavigationX);

            gestureRecognizer.StartCapturingGestures();

            if (MyAppPlatformManager.Instance.Platform == MyAppPlatformManager.PlatformId.ImmersiveHMD)
            {
                gameObject.AddComponent<GamepadInput>();
            }

            TryToRegisterEvents();
        }

        private void FakeTapKeyboardHandler(KeyboardInput.KeyCodeEventPair keyCodeEvent)
        {
            SendFakeTap();
        }

        private void HandleBackButtonFromKeyboard(KeyboardInput.KeyCodeEventPair keyCodeEvent)
        {
            var backButton = ToolManager.Instance.FindButtonByType(ButtonType.Back);
            if (backButton != null)
            {
                backButton.ButtonAction();
            }
        }

        public void SendFakeTap()
        {
            OnTapped(new TappedEventArgs(
                InteractionSourceKind.Other,
                0,
                new HeadPose(),
                new InteractionSourcePose(),
                0));
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
                Instance.HandleZoomFromXaml(1 + (direction * 0.03f));
            }
        }

        private bool ReadyForXamlInput
        {
            get
            {
                // Ignore input fromXaml until the introduction flow has
                // gotten us to GalaxyView.
                return IntroductionFlow.Instance == null || (
                    IntroductionFlow.Instance != null &&
                    IntroductionFlow.Instance.currentState == IntroductionFlow.IntroductionState.IntroductionStateComplete);
            }
        }

        public void HandleZoomFromXaml(float delta)
        {
            if (ReadyForXamlInput)
            {
                ToolManager.Instance.UpdateZoomFromXaml(delta);
            }
        }

        public void HandleRotationFromXaml(float delta)
        {
            if (ReadyForXamlInput)
            {
                ToolManager.Instance.UpdateRotationFromXaml(Math.Sign(delta));
            }
        }

        public void HandleTranslateFromXaml(Vector2 delta)
        {
            if (ReadyForXamlInput)
            {
                if (ctrlKeyIsDown)
                {
                    // if a control key is down, perform a rotation instead of translation
                    HandleRotationFromXaml(delta.y);
                }
                else
                {
                    delta *= 0.001f;
                    Camera.main.transform.parent.position += new Vector3(delta.x, delta.y, 0);
                }
            }
        }

        public void HandleResetFromXaml()
        {
            Button resetButton = ToolManager.Instance.FindButtonByType(ButtonType.Reset);
            if (resetButton &&
                TransitionManager.Instance &&
                !TransitionManager.Instance.InTransition)
            {
                // tell the camera to go back to (0,0,0)
                StartCoroutine(ResetCameraToOrigin());

                // reset everything else
                resetButton.ButtonAction();
            }
        }

        public void HandleAboutFromXaml()
        {
            Tool aboutTool = ToolManager.Instance.FindToolByType(ToolType.About);
            if (aboutTool &&
                TransitionManager.Instance &&
                !TransitionManager.Instance.InTransition)
            {
                aboutTool.Select();
            }
        }

        private IEnumerator ResetCameraToOrigin()
        {
            // TODO: Consider moving this code into TransitionManager
            Vector3 startPosition = Camera.main.transform.parent.position;

            float time = 0.0f;
            float timeFraction = 0.0f;
            do
            {
                time += Time.deltaTime;
                timeFraction = Mathf.Clamp01(time / TransitionManager.Instance.TransitionTimeCube);

                Vector3 newPosition = Vector3.Lerp(
                    startPosition,
                    Vector3.zero,
                    Mathf.Clamp01(TransitionManager.Instance.TransitionCurveCube.Evaluate(timeFraction)));

                Camera.main.transform.parent.position = newPosition;
                yield return null;

            } while (timeFraction < 1f);

            Camera.main.transform.parent.position = Vector3.zero;

            // Resetting the view changes the content's lookRotation which might
            // be confused if the camera was moving at the same time.
            // Since the camera is now done moving, re-reset the content to
            // get the final lookRotation just right.
            ToolManager.Instance.FindButtonByType(ButtonType.Reset).ButtonAction();
        }

        private void Update()
        {
            if (enableFakeInput)
            {
                if (fakeInput == Vector3.zero)
                {
                    OnNavigationCompleted(new NavigationCompletedEventArgs(
                        InteractionSourceKind.Controller,
                        fakeInput,
                        new HeadPose(),
                        0));
                }
                else
                {
                    OnNavigationUpdated(new NavigationUpdatedEventArgs(
                        InteractionSourceKind.Controller,
                        fakeInput,
                        new HeadPose(),
                        0));
                }

                if (FakeTapUpdate)
                {
                    OnTapped(new TappedEventArgs(
                        InteractionSourceKind.Other,
                        0,
                        new HeadPose(),
                        new InteractionSourcePose(),
                        0));
                    FakeTapUpdate = false;
                }
            }

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

        private void OnDestroy()
        {
            if (gestureRecognizer != null)
            {
                gestureRecognizer.StopCapturingGestures();
                gestureRecognizer.Dispose();
            }
            if (eventsAreRegistered)
            {
                TryToUnregisterEvents();
            }
        }

        #region EventCallbacks

        private void SourceManager_SourceLost(InteractionManager.SourceEventArgs args)
        {
            if (args.state.source.sourceKind == InteractionSourceKind.Hand)
            {
                HandsVisible = false;
            }
        }

        private void SourceManager_SourceDetected(InteractionManager.SourceEventArgs args)
        {
            if (args.state.source.sourceKind == InteractionSourceKind.Hand)
            {
                HandsVisible = true;
            }
        }

        private void SourceManager_SourcePressed(InteractionManager.SourceEventArgs args)
        {
            PressedSources.Add(args.state.source.sourceKind);
        }

        private void SourceManager_SourceReleased(InteractionManager.SourceEventArgs args)
        {
            PressedSources.Remove(args.state.source.sourceKind);
        }

        public void OnNavigationStarted(NavigationStartedEventArgs args)
        {
            bool handled = false;
            if (GazeSelectionManager.Instance && GazeSelectionManager.Instance.SelectedTarget)
            {
                handled = GazeSelectionManager.Instance.SelectedTarget.OnNavigationStarted(args.sourceKind, args.normalizedOffset, args.headPose);
            }

            if (!handled && InputStarted != null)
            {
                InputStarted(args.sourceKind, args.normalizedOffset, args.headPose);
            }
        }

        public void OnNavigationUpdated(NavigationUpdatedEventArgs args)
        {
            bool handled = false;
            if (GazeSelectionManager.Instance && GazeSelectionManager.Instance.SelectedTarget)
            {
                handled = GazeSelectionManager.Instance.SelectedTarget.OnNavigationUpdated(args.sourceKind, args.normalizedOffset, args.headPose);
            }

            if (!handled && InputUpdated != null)
            {
                InputUpdated(args.sourceKind, args.normalizedOffset, args.headPose);
            }
        }

        public void OnNavigationCompleted(NavigationCompletedEventArgs args)
        {
            bool handled = false;
            if (GazeSelectionManager.Instance && GazeSelectionManager.Instance.SelectedTarget)
            {
                handled = GazeSelectionManager.Instance.SelectedTarget.OnNavigationCompleted(args.sourceKind, args.normalizedOffset, args.headPose);
            }

            if (!handled && InputCompleted != null)
            {
                InputCompleted(args.sourceKind, args.normalizedOffset, args.headPose);
            }
        }

        public void OnNavigationCanceled(NavigationCanceledEventArgs args)
        {
            bool handled = false;
            if (GazeSelectionManager.Instance && GazeSelectionManager.Instance.SelectedTarget)
            {
                handled = GazeSelectionManager.Instance.SelectedTarget.OnNavigationCanceled(args.sourceKind, args.normalizedOffset, args.headPose);
            }

            if (!handled && InputCanceled != null)
            {
                InputCanceled(args.sourceKind, args.normalizedOffset, args.headPose);
            }
        }

        private void OnTapped(TappedEventArgs args)
        {
            InternalHandleOnTapped();
        }

        public void InternalHandleOnTapped()
        {
            if (TransitionManager.Instance != null && !TransitionManager.Instance.InTransition)
            {
                bool handled = false;
                if (GazeSelectionManager.Instance && GazeSelectionManager.Instance.SelectedTarget)
                {
                    handled = GazeSelectionManager.Instance.SelectedTarget.OnTapped();
                }
                else
                {
                    PlacementControl placementControl = TransitionManager.Instance.ViewVolume.GetComponentInChildren<PlacementControl>();

                    if (placementControl != null && placementControl.IsHolding)
                    {
                        handled = placementControl.OnTapped();
                        if (ToolSounds.Instance)
                        {
                            ToolSounds.Instance.PlaySelectSound();
                        }
                    }
                }

                if (!handled && InputTapped != null)
                {
                    InputTapped();
                }

                if (Tapped != null)
                {
                    Tapped();
                }
            }
        }

        #endregion
    }
}