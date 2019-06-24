using System;
using System.Collections;
using System.Linq;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

[RequireComponent(typeof(InputSystemGlobalListener))]
public class ControllerTransformTracker : MonoBehaviour, IMixedRealitySourceStateHandler
{
    public event Action<TrackedObjectType, Transform> NewControllerTrackingStarted;

    public event Action<TrackedObjectType> ControllerTrackingEnded;

    public event Action AllTrackingLost, TrackingUpdated, LeftTrackingUpdated, RightTrackingUpdated, LeftTrackingLost, RightTrackingLost, AnyTrackingStarted, LeftTrackingStarted, RightTrackingStarted;

    [Flags]
    public enum Controllers
    {
        LeftHand = 1,
        RightHand = 2,
        LeftController = 4,
        RightController = 8,
        Left = LeftHand | LeftController,
        Right = RightHand | RightController,
        //        Hand = LeftHand|RightHand,
        //        Controller = LeftController|RightController
    }

    private static readonly InputSourceType[] TypesToCheckAgainst =
        {InputSourceType.Other, InputSourceType.Controller, InputSourceType.Hand};

    private Transform
        _leftHandTr,
        _rightHandTr,
        _leftControllerTr,
        _rightControllerTr;

    private Controllers _trackedControllers;
    private IMixedRealityController _leftController, _rightController, _leftHandController, _rightHandController;

    [SerializeField]
    private Vector3 handOffsetRotation;

    [SerializeField]
    private Vector3 controllerOffsetRotation;

    private Quaternion _handOffsetRotationQuaternion, _controllerOffsetRotationQuaternion;

    private IMixedRealityHandJointService HandJointService => _handJointService ??
                                                              (_handJointService = MixedRealityToolkit.Instance.GetService<IMixedRealityHandJointService>());

    private IMixedRealityHandJointService _handJointService;

    private Vector3 _twoHandRotationVector = Vector3.zero;

    #region public unity fields

    public TrackedHandJoint handJointToTrack = TrackedHandJoint.Palm;
    public DeviceInputType controllerInputActionType = DeviceInputType.SpatialPointer;

    #endregion public unity fields

    #region public accessors

    public bool IsTracking => _trackedControllers != 0;
    public bool BothSides => LeftSide && RightSide; 
    public bool RightSide => (_trackedControllers & Controllers.Right) > 0;
    public bool LeftSide => (_trackedControllers & Controllers.Left) > 0;
    public Controllers TrackedControlles => _trackedControllers;

    public Transform LeftTransform =>
        _trackedControllers.HasFlag(Controllers.LeftController) ? _leftControllerTr : _leftHandTr;

    public Transform RightTransform =>
        _trackedControllers.HasFlag(Controllers.RightController) ? _rightControllerTr : _rightHandTr;

    public Vector3 HandOffsetRotation
    {
        get => _handOffsetRotationQuaternion.eulerAngles;
        set => _handOffsetRotationQuaternion = Quaternion.Euler(value);
    }

    public Vector3 ControllerOffsetRotation
    {
        get => _controllerOffsetRotationQuaternion.eulerAngles;
        set => _controllerOffsetRotationQuaternion = Quaternion.Euler(value);
    }

    public Vector3 LeftSidePosition =>
        _trackedControllers.HasFlag(Controllers.LeftController) ? _leftControllerTr.position : _leftHandTr.position;

    public Vector3 RightSidePosition =>
        _trackedControllers.HasFlag(Controllers.RightController) ? _rightControllerTr.position : _rightHandTr.position;

    public Quaternion LeftSideRotation =>
        _trackedControllers.HasFlag(Controllers.LeftController) ?
            _leftControllerTr.rotation * _controllerOffsetRotationQuaternion : _leftHandTr.rotation * _handOffsetRotationQuaternion;

    public Quaternion RightSideRotation =>
        _trackedControllers.HasFlag(Controllers.RightController) ?
            _rightControllerTr.rotation * _controllerOffsetRotationQuaternion : _rightHandTr.rotation * _handOffsetRotationQuaternion;

    public Vector3 ResolvedPosition => transform.position;
    public Quaternion ResolvedRotation => transform.rotation;

    public Transform ResolvedTransform => transform;

    #endregion public accessors

    private void Awake()
    {
        _handOffsetRotationQuaternion = Quaternion.Euler(handOffsetRotation);
        _controllerOffsetRotationQuaternion = Quaternion.Euler(controllerOffsetRotation);
    }

    private void Start()
    {
        _leftHandTr = HandJointService?.RequestJointTransform(handJointToTrack, Handedness.Left);
        _rightHandTr = HandJointService?.RequestJointTransform(handJointToTrack, Handedness.Right);

        StartCoroutine(CheckForWMRControllers());
    }

    private IEnumerator CheckForWMRControllers()
    {
        while (MixedRealityToolkit.InputSystem.DetectedControllers == null || MixedRealityToolkit.InputSystem.DetectedControllers.Count == 0)
        {
            yield return null;
        }

        foreach (var detectedController in MixedRealityToolkit.InputSystem.DetectedControllers)
        {
            // hands are present any way, we only have to monitor controllers
            if (detectedController != null && !(detectedController is IMixedRealityHand))
            {
                if (CheckController(detectedController))
                {
                    AttachController(detectedController);
                }
            }
        }
    }

    private void Update()
    {
        CalculateTrackingTransform();
    }

    private void CalculateTrackingTransform()
    {
        if (_trackedControllers == 0) return;

        if (!BothSides)
        {
            if ((_trackedControllers & Controllers.Left) > 0)
            {
                transform.SetPositionAndRotation(LeftSidePosition, LeftSideRotation);
            }
            else
            {
                transform.SetPositionAndRotation(RightSidePosition, RightSideRotation);
            }
            _twoHandRotationVector = Vector3.zero;
        }
        else
        {
            transform.position = (LeftSidePosition + RightSidePosition) * .5f;
            var newRotationVector = LeftSidePosition - RightSidePosition;
            if (_twoHandRotationVector != Vector3.zero)
            {
                transform.rotation = Quaternion.FromToRotation(_twoHandRotationVector, newRotationVector) *
                                     transform.rotation;
            }
            _twoHandRotationVector = newRotationVector;
        }
    }

    private static bool CheckController(IMixedRealityController controller)
    {
        return controller != null && TypesToCheckAgainst.Contains(controller.InputSource.SourceType);
    }

    private static bool TryGetControllerVisualizerTransform(IMixedRealityController controller, out Transform transform)
    {
        if (controller?.Visualizer != null && controller.Visualizer.GameObjectProxy != null)
        {
            transform = controller.Visualizer.GameObjectProxy.transform;
            return true;
        }

        transform = null;
        return false;
    }

    private void DetachController(IMixedRealityController controller)
    {
        var before = _trackedControllers;

        if (controller == _leftController)
        {
            _leftController = null;
            _leftControllerTr = null;
            _trackedControllers &= ~Controllers.LeftController;
            ControllerTrackingEnded?.Invoke(TrackedObjectType.MotionControllerLeft);
        }
        else if (controller == _rightController)
        {
            _rightController = null;
            _rightControllerTr = null;
            _trackedControllers &= ~Controllers.RightController;
            ControllerTrackingEnded?.Invoke(TrackedObjectType.MotionControllerRight);
        }
        else if (controller == _leftHandController)
        {
            _trackedControllers &= ~Controllers.LeftHand;
            _leftHandController = null;
            ControllerTrackingEnded?.Invoke(TrackedObjectType.HandJointLeft);
        }
        else if (controller == _rightHandController)
        {
            _trackedControllers &= ~Controllers.RightHand;
            _rightHandController = null;
            ControllerTrackingEnded?.Invoke(TrackedObjectType.HandJointRight);
        }

        var after = _trackedControllers;
        CheckBeforeAfter(before, after);
    }

    private void AttachController(IMixedRealityController controller)
    {
        var before = _trackedControllers;
        // if hand controller we already have the transforms from the service
        if (controller is IMixedRealityHand handController)
        {
            switch (handController.ControllerHandedness)
            {
                case Handedness.Left:
                    Debug.Assert(!_trackedControllers.HasFlag(Controllers.LeftHand) && _leftHandController == null);
                    _leftHandController = controller;
                    _trackedControllers |= Controllers.LeftHand;
                    NewControllerTrackingStarted?.Invoke(TrackedObjectType.HandJointLeft, _leftHandTr);
                    break;

                case Handedness.Right:
                    Debug.Assert(!_trackedControllers.HasFlag(Controllers.RightHand) && _rightHandController == null);
                    _rightHandController = controller;
                    _trackedControllers |= Controllers.RightHand;
                    NewControllerTrackingStarted?.Invoke(TrackedObjectType.HandJointRight, _rightHandTr);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else if (TryGetControllerVisualizerTransform(controller, out var transform))
        {
            // if regular controller then get the transform
            switch (controller.ControllerHandedness)
            {
                case Handedness.Left:
                    Debug.Assert(!_trackedControllers.HasFlag(Controllers.LeftController) && _leftController == null);
                    _leftController = controller;
                    _leftControllerTr = transform;
                    _trackedControllers |= Controllers.LeftController;
                    NewControllerTrackingStarted?.Invoke(TrackedObjectType.MotionControllerLeft, _leftControllerTr);
                    break;

                case Handedness.Right:
                    Debug.Assert(!_trackedControllers.HasFlag(Controllers.RightController) && _rightController == null);
                    _rightController = controller;
                    _rightControllerTr = transform;
                    _trackedControllers |= Controllers.RightController;
                    NewControllerTrackingStarted?.Invoke(TrackedObjectType.MotionControllerRight, _rightControllerTr);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        var after = _trackedControllers;
        CheckBeforeAfter(before, after);
    }

    private void CheckBeforeAfter(Controllers before, Controllers after)
    {
        if (before == after) return;
        // check for top level
        if (before == 0 && after > 0)
        {
            AnyTrackingStarted?.Invoke();
            TrackingUpdated?.Invoke();
        }
        else if (before > 0 && after == 0)
        {
            AllTrackingLost?.Invoke();
        }
        if (before > 0 && after > 0)
        {
            TrackingUpdated?.Invoke();
        }

        // check for left side
        if ((before & Controllers.Left) == 0 && (after & Controllers.Left) > 0)
        {
            LeftTrackingStarted?.Invoke();
            LeftTrackingUpdated?.Invoke();
        }
        else if ((before & Controllers.Left) > 0 && (after & Controllers.Left) == 0)
        {
            LeftTrackingLost?.Invoke();
        }
        else if ((before & Controllers.Left) != (after & Controllers.Left))
        {
            LeftTrackingUpdated?.Invoke();
        }

        // check for right side
        if ((before & Controllers.Right) == 0 && (after & Controllers.Right) > 0)
        {
            RightTrackingStarted?.Invoke();
            RightTrackingUpdated?.Invoke();
        }
        else if ((before & Controllers.Right) > 0 && (after & Controllers.Right) == 0)
        {
            RightTrackingLost?.Invoke();
        }
        else if ((before & Controllers.Right) != (after & Controllers.Right))
        {
            RightTrackingUpdated?.Invoke();
        }
    }

    public void OnSourceDetected(SourceStateEventData eventData)
    {
        var controller = eventData.Controller;
        if (!CheckController(controller)) return;

        AttachController(controller);
    }

    public void OnSourceLost(SourceStateEventData eventData)
    {
        var controller = eventData.Controller;
        if (!CheckController(controller)) return;

        DetachController(controller);
    }
}