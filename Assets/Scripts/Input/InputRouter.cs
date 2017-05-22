// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using GalaxyExplorer.HoloToolkit.Unity.InputModule;
using System;
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

        public Button BackButton;

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

        private void TryToRegisterEvents()
        {
            if (!eventsAreRegistered && gestureRecognizer != null)
            {
                gestureRecognizer.NavigationStartedEvent += OnNavigationStarted;
                gestureRecognizer.NavigationUpdatedEvent += OnNavigationUpdated;
                gestureRecognizer.NavigationCompletedEvent += OnNavigationCompleted;
                gestureRecognizer.NavigationCanceledEvent += OnNavigationCanceled;
                gestureRecognizer.TappedEvent += OnTapped;

                InteractionManager.SourceDetected += SourceManager_SourceDetected;
                InteractionManager.SourceLost += SourceManager_SourceLost;

                InteractionManager.SourcePressed += SourceManager_SourcePressed;
                InteractionManager.SourceReleased += SourceManager_SourceReleased;

                eventsAreRegistered = true;
            }
        }

        private void TryToUnregisterEvents()
        {
            if (eventsAreRegistered && gestureRecognizer != null)
            {
                gestureRecognizer.NavigationStartedEvent -= OnNavigationStarted;
                gestureRecognizer.NavigationUpdatedEvent -= OnNavigationUpdated;
                gestureRecognizer.NavigationCompletedEvent -= OnNavigationCompleted;
                gestureRecognizer.NavigationCanceledEvent -= OnNavigationCanceled;
                gestureRecognizer.TappedEvent -= OnTapped;

                InteractionManager.SourceDetected -= SourceManager_SourceDetected;
                InteractionManager.SourceLost -= SourceManager_SourceLost;

                InteractionManager.SourcePressed -= SourceManager_SourcePressed;
                InteractionManager.SourceReleased -= SourceManager_SourceReleased;

                eventsAreRegistered = false;
            }
        }

        private void Awake()
        {
            PressedSources = new HashSet<InteractionSourceKind>();
        }

        private void Start()
        {
            if (KeyboardInput.Instance)
            {
                KeyboardInput.Instance.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Space, KeyboardInput.KeyEvent.KeyReleased), FakeTapKeyboardHandler);
                KeyboardInput.Instance.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Backspace, KeyboardInput.KeyEvent.KeyReleased), FakeBackKeyboardHandler);
            }

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

        private void FakeBackKeyboardHandler(KeyboardInput.KeyCodeEventPair keyCodeEvent)
        {
            if (BackButton != null)
            {
                BackButton.ButtonAction();
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
        }

        private void OnDestroy()
        {
            if (gestureRecognizer != null)
            {
                gestureRecognizer.StopCapturingGestures();
                TryToUnregisterEvents();
                gestureRecognizer.Dispose();
            }

            if (KeyboardInput.Instance)
            {
                KeyboardInput.Instance.UnregisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Space, KeyboardInput.KeyEvent.KeyReleased), FakeTapKeyboardHandler);
                KeyboardInput.Instance.UnregisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Backspace, KeyboardInput.KeyEvent.KeyReleased), FakeBackKeyboardHandler);
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