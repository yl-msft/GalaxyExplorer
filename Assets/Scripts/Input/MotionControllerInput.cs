// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using GalaxyExplorer.HoloToolkit.Unity.InputModule;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA.Input;

namespace GalaxyExplorer
{
    public class MotionControllerInput : GE_Singleton<MotionControllerInput>
    {
        public delegate void RotateCameraPovDelegate(float rotationAmount);
        public event RotateCameraPovDelegate RotateCameraPov;

        Dictionary<InteractionSourceHandedness, float> intendedRotation = new Dictionary<InteractionSourceHandedness, float>();

        void Start()
        {
            InteractionManager.SourceDetected += InteractionManager_SourceDetected;
            InteractionManager.SourceLost += InteractionManager_SourceLost;
            InteractionManager.SourcePressed += InteractionManager_SourcePressed;
            InteractionManager.SourceReleased += InteractionManager_SourceReleased;
            InteractionManager.SourceUpdated += InteractionManager_SourceUpdated;

            intendedRotation[InteractionSourceHandedness.Left] = 0f;
            intendedRotation[InteractionSourceHandedness.Right] = 0f;
        }

        private InteractionSourceHandedness ValidateGraspStateTracking(InteractionManager.SourceEventArgs args)
        {
            if (args.state.source.handedness == graspedHand)
            {
                if (!args.state.grasped)
                {
                    Debug.LogFormat("grasp state mismatch for {0} hand", args.state.source.handedness.ToString());
                    UseAlternateGazeRay = false;
                    return InteractionSourceHandedness.Unspecified;
                }
            }
            return graspedHand;
        }

        private void InteractionManager_SourceUpdated(InteractionManager.SourceEventArgs obj)
        {
            if (obj.state.source.sourceKind != InteractionSourceKind.Controller ||
                obj.state.source.handedness == InteractionSourceHandedness.Unspecified)
            {
                return;
            }

            // If the controller is grasped and this event is for that controller,
            // verify the controller is still being grasped and then try to update
            // the AlternateGazeRay.
            graspedHand = ValidateGraspStateTracking(obj);
            if (graspedHand != InteractionSourceHandedness.Unspecified &&
                graspedHand == obj.state.source.handedness)
            {
                UseAlternateGazeRay = obj.state.sourcePose.TryGetPointerRay(out AlternateGazeRay);
            }

            // Check out the X value for the thumbstick to see if we are
            // trying to rotate the POV. Only do this if there isn't a
            // tool selected.
            if (ToolManager.Instance.SelectedTool == null)
            {
                float x = (float)obj.state.controllerProperties.thumbstickX;
                float irot = intendedRotation[obj.state.source.handedness];

                if (irot != 0f && x < 0.1f)
                {
                    RotateCameraPov(irot);
                    intendedRotation[obj.state.source.handedness] = 0f;
                }
                else if (Mathf.Abs(x) >= 0.9f)
                {
                    intendedRotation[obj.state.source.handedness] = 45f * Mathf.Sign(x);
                }
            }
            else
            {
                HandleNavigation(obj);
            }
        }

        private bool navigationStarted = false;
        private InteractionSourceHandedness navigatingHand = InteractionSourceHandedness.Unspecified;

        private void HandleNavigation(InteractionManager.SourceEventArgs obj)
        {
            float displacementAlongX = (float)obj.state.controllerProperties.thumbstickX;
            float displacementAlongY = (float)obj.state.controllerProperties.thumbstickY;

            if (Mathf.Abs(displacementAlongX) >= 0.1f || Mathf.Abs(displacementAlongY) >= 0.1f || navigationStarted)
            {
                if (!navigationStarted)
                {
                    navigationStarted = true;
                    navigatingHand = obj.state.source.handedness;

                    //Raise navigation started event.
                    var args = new NavigationStartedEventArgs(
                        InteractionSourceKind.Controller,
                        Vector3.zero,
                        new HeadPose(),
                        (int)obj.state.source.id);
                    InputRouter.Instance.OnNavigationStarted(args);
                }

                if (obj.state.source.handedness == navigatingHand)
                {
                    Vector3 thumbValues = new Vector3(
                        displacementAlongX,
                        displacementAlongY,
                        0f);

                    InputRouter.Instance.OnNavigationUpdated(
                        new NavigationUpdatedEventArgs(InteractionSourceKind.Controller,
                            thumbValues, new HeadPose(), (int)obj.state.source.id));
                }
            }
        }

        [HideInInspector]
        public bool UseAlternateGazeRay = false;
        [HideInInspector]
        public Ray AlternateGazeRay;

        // Using the grasp button will cause GE to replace the gaze cursor with
        // the pointer ray from the grasped controller. Since GE (currently)
        // only can handle input from a single source, we will only track one
        // controller at a time. The first one in wins.
        InteractionSourceHandedness graspedHand = InteractionSourceHandedness.Unspecified;

        private void InteractionManager_SourceReleased(InteractionManager.SourceEventArgs obj)
        {
            if (obj.state.source.sourceKind != InteractionSourceKind.Controller)
            {
                return;
            }

            switch (obj.pressKind)
            {
                case InteractionPressKind.Select:
                    switch (obj.state.source.handedness)
                    {
                        case InteractionSourceHandedness.Left:
                        case InteractionSourceHandedness.Right:
                            if (navigationStarted &&
                                obj.state.source.handedness == navigatingHand)
                            {
                                navigationStarted = false;
                                navigatingHand = InteractionSourceHandedness.Unspecified;

                                var args = new NavigationCompletedEventArgs(
                                    InteractionSourceKind.Controller,
                                    Vector3.zero,
                                    new HeadPose(),
                                    (int)obj.state.source.id);
                                InputRouter.Instance.OnNavigationCompleted(args);
                                Debug.Log("SourceReleased -> OnNavigationCompleted");
                            }
                            else
                            {
                                PlayerInputManager.Instance.TriggerTapRelease();
                            }
                            break;
                    }
                    break;
                case InteractionPressKind.Grasp:
                    if (graspedHand == obj.state.source.handedness)
                    {
                        UseAlternateGazeRay = false;
                        graspedHand = InteractionSourceHandedness.Unspecified;
                    }
                    break;
                case InteractionPressKind.Menu:
                    if (ToolManager.Instance.ToolsVisible)
                    {
                        ToolManager.Instance.UnselectAllTools();
                        ToolManager.Instance.HideTools(false);
                    }
                    else
                    {
                        ToolManager.Instance.ShowTools();
                    }
                    break;
            }
        }

        private void InteractionManager_SourcePressed(InteractionManager.SourceEventArgs obj)
        {
            if (obj.state.source.sourceKind != InteractionSourceKind.Controller)
            {
                return;
            }

            switch (obj.pressKind)
            {
                case InteractionPressKind.Select:
                    switch (obj.state.source.handedness)
                    {
                        case InteractionSourceHandedness.Left:
                        case InteractionSourceHandedness.Right:
                            PlayerInputManager.Instance.TriggerTapPress();
                            break;
                    }
                    break;
                case InteractionPressKind.Grasp:
                    switch (obj.state.source.handedness)
                    {
                        case InteractionSourceHandedness.Left:
                        case InteractionSourceHandedness.Right:
                            graspedHand = obj.state.source.handedness;
                            break;
                    }
                    break;
            }
        }

        #region Source_Lost_Detected
        private void InteractionManager_SourceLost(InteractionManager.SourceEventArgs obj)
        {
            // if we lost all (Motion)Controllers, enable the GamePad script
            if (obj.state.source.sourceKind == InteractionSourceKind.Controller && 
                GamepadInput.Instance && 
                InteractionManager.numSourceStates == 0)
            {
                Debug.Log("Enabling GamepadInput instance");
                UseAlternateGazeRay = false;
                graspedHand = InteractionSourceHandedness.Unspecified;
                GamepadInput.Instance.enabled = true;
                InputRouter.Instance.SetGestureRecognitionState(true);
            }
        }

        private void InteractionManager_SourceDetected(InteractionManager.SourceEventArgs obj)
        {
            // if we detected a (Motion)Controller, disable the GamePad script
            if (obj.state.source.sourceKind == InteractionSourceKind.Controller &&
                GamepadInput.Instance)
            {
                Debug.Log("Disabling GamepadInput instance");
                GamepadInput.Instance.enabled = false;
                InputRouter.Instance.SetGestureRecognitionState(false);
            }
        }
        #endregion // Source_Lost_Detected

        private void OnDestroy()
        {
            InteractionManager.SourceDetected -= InteractionManager_SourceDetected;
            InteractionManager.SourceLost -= InteractionManager_SourceLost;
            InteractionManager.SourcePressed -= InteractionManager_SourcePressed;
            InteractionManager.SourceReleased -= InteractionManager_SourceReleased;
            InteractionManager.SourceUpdated -= InteractionManager_SourceUpdated;
        }
    }
}