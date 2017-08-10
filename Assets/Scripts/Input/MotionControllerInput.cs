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
            // trying to rotate the POV. Only do this if the trigger isn't
            // pressed.
            if (!obj.state.pressed)
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
        }

        [HideInInspector]
        public bool UseAlternateGazeRay = false;
        [HideInInspector]
        public Ray AlternateGazeRay;

        // GE will only track a single grasped hand at a time;
        // first one in wins.
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
                            PlayerInputManager.Instance.TriggerTapRelease();
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
            }
        }

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