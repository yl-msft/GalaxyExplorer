// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using GalaxyExplorer.HoloToolkit.Unity.InputModule;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;


namespace GalaxyExplorer
{
    public class MotionControllerInput : GE_Singleton<MotionControllerInput>
    {
        public delegate void RotateCameraPovDelegate(float rotationAmount);
        public event RotateCameraPovDelegate RotateCameraPov;

        [HideInInspector]
        public bool UseAlternateGazeRay = false;
        [HideInInspector]
        public Ray AlternateGazeRay;

        private Dictionary<InteractionSourceHandedness, float> intendedRotation = new Dictionary<InteractionSourceHandedness, float>();

        void Awake()
        {
            InteractionManager.InteractionSourceDetected += InteractionManager_OnInteractionSourceDetected;
            InteractionManager.InteractionSourceLost += InteractionManager_OnInteractionSourceLost;
            InteractionManager.InteractionSourcePressed += InteractionManager_OnInteractionSourcePressed;
            InteractionManager.InteractionSourceReleased += InteractionManager_OnInteractionSourceReleased;
            InteractionManager.InteractionSourceUpdated += InteractionManager_OnInteractionSourceUpdated;

            intendedRotation[InteractionSourceHandedness.Left] = 0f;
            intendedRotation[InteractionSourceHandedness.Right] = 0f;
        }

        private InteractionSourceHandedness ValidateGraspStateTracking(InteractionSourceUpdatedEventArgs args)
        {
            if (args.state.source.handedness == graspedHand)
            {
                if (!args.state.grasped)
                {
                    Debug.LogFormat("grasp state mismatch for {0} hand", args.state.source.handedness.ToString());
                    UseAlternateGazeRay = false;
                    return InteractionSourceHandedness.Unknown;
                }
            }
            return graspedHand;
        }

        private void InteractionManager_OnInteractionSourceUpdated(InteractionSourceUpdatedEventArgs obj)
        {
            if (obj.state.source.kind != InteractionSourceKind.Controller ||
                obj.state.source.handedness == InteractionSourceHandedness.Unknown)
            {
                return;
            }

            // If the controller is grasped and this event is for that controller,
            // verify the controller is still being grasped and then try to update
            // the AlternateGazeRay.
            graspedHand = ValidateGraspStateTracking(obj);
            if (graspedHand != InteractionSourceHandedness.Unknown &&
                graspedHand == obj.state.source.handedness)
            {
                Vector3 origin;
                Vector3 direction;
                if (obj.state.sourcePose.TryGetPosition(out origin) &&
                    obj.state.sourcePose.TryGetForward(out direction))
                {
                    // TODO: shouldn't need to do this; results aren't perfect either.
                    origin += Camera.main.transform.position;
                    AlternateGazeRay.origin = origin;
                    AlternateGazeRay.direction = direction;
                    UseAlternateGazeRay = true;
                }
                else
                {
                    UseAlternateGazeRay = false;
                }
            }

            // Check out the X value for the thumbstick to see if we are
            // trying to rotate the POV. Only do this if there isn't a
            // tool selected.
            if (ToolManager.Instance.SelectedTool == null &&
                obj.state.source.handedness != InteractionSourceHandedness.Unknown)
            {
                float x = obj.state.thumbstickPosition.x;
                float irot = intendedRotation[obj.state.source.handedness];

                if (irot != 0f && x < 0.1f)
                {
                    if (RotateCameraPov != null)
                    {
                        RotateCameraPov(irot);
                    }
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
        private InteractionSourceHandedness navigatingHand = InteractionSourceHandedness.Unknown;

        private void HandleNavigation(InteractionSourceUpdatedEventArgs obj)
        {
            float displacementAlongX = obj.state.thumbstickPosition.x;
            float displacementAlongY = obj.state.thumbstickPosition.y;

            if (Mathf.Abs(displacementAlongX) >= 0.1f ||
                Mathf.Abs(displacementAlongY) >= 0.1f ||
                navigationStarted)
            {
                if (!navigationStarted)
                {
                    navigationStarted = true;
                    navigatingHand = obj.state.source.handedness;

                    //Raise navigation started event.
                    InputRouter.Instance.OnNavigationStartedWorker(InteractionSourceKind.Controller, Vector3.zero, new Ray());
                }

                if (obj.state.source.handedness == navigatingHand)
                {
                    Vector3 thumbValues = new Vector3(
                        displacementAlongX,
                        displacementAlongY,
                        0f);

                    InputRouter.Instance.OnNavigationUpdatedWorker(InteractionSourceKind.Controller, thumbValues, new Ray());
                }
            }
        }

        // Using the grasp button will cause GE to replace the gaze cursor with
        // the pointer ray from the grasped controller. Since GE (currently)
        // only can handle input from a single source, we will only track one
        // controller at a time. The first one in wins.
        InteractionSourceHandedness graspedHand = InteractionSourceHandedness.Unknown;

        private void InteractionManager_OnInteractionSourceReleased(InteractionSourceReleasedEventArgs obj)
        {
            if (obj.state.source.kind != InteractionSourceKind.Controller)
            {
                return;
            }

            switch (obj.pressType)
            {
                case InteractionSourcePressType.Select:
                    switch (obj.state.source.handedness)
                    {
                        case InteractionSourceHandedness.Left:
                        case InteractionSourceHandedness.Right:
                            if (navigationStarted &&
                                obj.state.source.handedness == navigatingHand)
                            {
                                navigationStarted = false;
                                navigatingHand = InteractionSourceHandedness.Unknown;
                                InputRouter.Instance.OnNavigationCompletedWorker(InteractionSourceKind.Controller, Vector3.zero, new Ray());
                                Debug.Log("SourceReleased -> OnNavigationCompleted");
                            }
                            else
                            {
                                PlayerInputManager.Instance.TriggerTapRelease();
                            }
                            break;
                    }
                    break;
                case InteractionSourcePressType.Grasp:
                    if (graspedHand == obj.state.source.handedness)
                    {
                        UseAlternateGazeRay = false;
                        graspedHand = InteractionSourceHandedness.Unknown;
                    }
                    break;
                case InteractionSourcePressType.Menu:
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

        private void InteractionManager_OnInteractionSourcePressed(InteractionSourcePressedEventArgs obj)
        {
            if (obj.state.source.kind != InteractionSourceKind.Controller)
            {
                return;
            }

            switch (obj.pressType)
            {
                case InteractionSourcePressType.Select:
                    switch (obj.state.source.handedness)
                    {
                        case InteractionSourceHandedness.Left:
                        case InteractionSourceHandedness.Right:
                            if (PlayerInputManager.Instance)
                            {
                                PlayerInputManager.Instance.TriggerTapPress();
                            }
                            break;
                    }
                    break;
                case InteractionSourcePressType.Grasp:
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
        private void InteractionManager_OnInteractionSourceLost(InteractionSourceLostEventArgs obj)
        {
            // if we lost all (Motion)Controllers, enable the GamePad script
            if (obj.state.source.kind == InteractionSourceKind.Controller && 
                GamepadInput.Instance && 
                InteractionManager.numSourceStates == 0)
            {
                Debug.Log("Enabling GamepadInput instance");
                UseAlternateGazeRay = false;
                graspedHand = InteractionSourceHandedness.Unknown;
                GamepadInput.Instance.enabled = true;
                InputRouter.Instance.SetGestureRecognitionState(true);
            }
        }

        private void InteractionManager_OnInteractionSourceDetected(InteractionSourceDetectedEventArgs obj)
        {
            // if we detected a (Motion)Controller, disable the GamePad script
            if (obj.state.source.kind == InteractionSourceKind.Controller &&
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
            InteractionManager.InteractionSourceDetected -= InteractionManager_OnInteractionSourceDetected;
            InteractionManager.InteractionSourceLost -= InteractionManager_OnInteractionSourceLost;
            InteractionManager.InteractionSourcePressed -= InteractionManager_OnInteractionSourcePressed;
            InteractionManager.InteractionSourceReleased -= InteractionManager_OnInteractionSourceReleased;
            InteractionManager.InteractionSourceUpdated -= InteractionManager_OnInteractionSourceUpdated;
        }
    }
}