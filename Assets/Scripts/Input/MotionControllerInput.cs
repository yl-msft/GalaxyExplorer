// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using GalaxyExplorer.HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.VR.WSA.Input;

namespace GalaxyExplorer
{
    public class MotionControllerInput : GE_Singleton<MotionControllerInput>
    {
        // Use this for initialization
        void Start()
        {
            InteractionManager.SourceDetected += InteractionManager_SourceDetected;
            InteractionManager.SourceLost += InteractionManager_SourceLost;
            InteractionManager.SourcePressed += InteractionManager_SourcePressed;
            InteractionManager.SourceReleased += InteractionManager_SourceReleased;
            InteractionManager.SourceUpdated += InteractionManager_SourceUpdated;
        }

        private bool ValidateGraspStateTracking(bool trackedState, InteractionSourceHandedness hand, InteractionManager.SourceEventArgs args)
        {
            if (args.state.source.sourceKind == InteractionSourceKind.Controller &&
                args.state.source.handedness == hand)
            {
                if (trackedState != args.state.grasped)
                {
                    Debug.LogFormat("grasp state mismatch for {0} hand", args.state.source.handedness.ToString());
                    UseAlternateGazeRay = false;
                    return false;
                }
            }
            return trackedState;
        }

        private void InteractionManager_SourceUpdated(InteractionManager.SourceEventArgs obj)
        {
            leftGraspPressed = ValidateGraspStateTracking(leftGraspPressed, InteractionSourceHandedness.Left, obj);
            rightGraspPressed = ValidateGraspStateTracking(rightGraspPressed, InteractionSourceHandedness.Right, obj);

            if (obj.state.source.sourceKind == InteractionSourceKind.Controller &&
                ((leftGraspPressed && obj.state.source.handedness == InteractionSourceHandedness.Left) ||
                 (rightGraspPressed && obj.state.source.handedness == InteractionSourceHandedness.Right)))
            {
                UseAlternateGazeRay = obj.state.sourcePose.TryGetPointerRay(out AlternateGazeRay);
            }
        }

        [HideInInspector]
        public bool UseAlternateGazeRay = false;
        [HideInInspector]
        public Ray AlternateGazeRay;

        bool leftTriggerPressed = false;
        bool rightTriggerPressed = false;
        bool leftGraspPressed = false;
        bool rightGraspPressed = false;

        private void InteractionManager_SourceReleased(InteractionManager.SourceEventArgs obj)
        {
            switch (obj.pressKind)
            {
                case InteractionPressKind.Select:
                    switch (obj.state.source.handedness)
                    {
                        case InteractionSourceHandedness.Left:
                            leftTriggerPressed = false;
                            PlayerInputManager.Instance.TriggerTapRelease();
                            break;
                        case InteractionSourceHandedness.Right:
                            rightGraspPressed = false;
                            PlayerInputManager.Instance.TriggerTapRelease();
                            break;
                    }
                    break;
                case InteractionPressKind.Grasp:
                    switch (obj.state.source.handedness)
                    {
                        case InteractionSourceHandedness.Left:
                            if (leftGraspPressed)
                            {
                                UseAlternateGazeRay = false;
                                leftGraspPressed = false;
                            }
                            break;
                        case InteractionSourceHandedness.Right:
                            if (rightGraspPressed)
                            {
                                UseAlternateGazeRay = false;
                                rightGraspPressed = false;
                            }
                            break;
                    }
                    break;
            }
        }

        private void InteractionManager_SourcePressed(InteractionManager.SourceEventArgs obj)
        {
            switch (obj.pressKind)
            {
                case InteractionPressKind.Select:
                    switch (obj.state.source.handedness)
                    {
                        case InteractionSourceHandedness.Left:
                            leftTriggerPressed = true;
                            break;
                        case InteractionSourceHandedness.Right:
                            rightTriggerPressed = true;
                            break;
                    }
                    PlayerInputManager.Instance.TriggerTapPress();
                    break;
                case InteractionPressKind.Grasp:
                    switch (obj.state.source.handedness)
                    {
                        case InteractionSourceHandedness.Left:
                            if (!rightGraspPressed)
                            {
                                leftGraspPressed = true;
                            }
                            break;
                        case InteractionSourceHandedness.Right:
                            if (!leftGraspPressed)
                            {
                                rightGraspPressed = true;
                            }
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
                rightGraspPressed = false;
                rightTriggerPressed = false;
                leftGraspPressed = false;
                leftTriggerPressed = false;
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