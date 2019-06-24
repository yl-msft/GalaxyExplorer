using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[Serializable]
public class UnityIMixedRealityControllerEvent : UnityEvent<IMixedRealityController>{}

[RequireComponent(typeof(SolverHandler))]
public class AttachToControllerSolver : Solver
{

    public event Action TrackingStarted, TrackingLost;
    
    private SolverHandler _handler;
    
    [SerializeField]
    private ControllerTransformTracker _controllerTracker;

    [SerializeField]
    private Handedness _handedness = Handedness.Both;
    
    public bool IsTracking { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        _handler = GetComponent<SolverHandler>();
        Debug.Assert(_handler != null);
        if (_controllerTracker == null)
        {
            Debug.LogWarning("AttachToControllerSolver missing controller tracker, will create own instance");
            _controllerTracker = gameObject.AddComponent<ControllerTransformTracker>();
        }
        
        switch (_handedness)
        {
            case Handedness.None:
            case Handedness.Other:
                break;
            case Handedness.Left:
                _controllerTracker.LeftTrackingUpdated += UpdateTrackedTransform;
                _controllerTracker.LeftTrackingLost += LostTracking;
                break;
            case Handedness.Right:
                _controllerTracker.RightTrackingUpdated += UpdateTrackedTransform;
                _controllerTracker.RightTrackingLost += LostTracking;
                break;
            case Handedness.Both:
            case Handedness.Any:
                _controllerTracker.TrackingUpdated += UpdateTrackedTransform;
                _controllerTracker.AllTrackingLost += LostTracking;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateTrackedTransform()
    {
        switch (_handedness)
        {
            case Handedness.None:
            case Handedness.Other:
                break;
            
            case Handedness.Left:
                _handler.TransformTarget = _controllerTracker.LeftTransform;
                break;
            case Handedness.Right:
                _handler.TransformTarget = _controllerTracker.RightTransform;
                break;
            case Handedness.Both:
            case Handedness.Any:
                _handler.TransformTarget = _controllerTracker.transform;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (IsTracking) return;
        TrackingStarted?.Invoke();
        IsTracking = true;
    }

    private void LostTracking()
    {
        if(!IsTracking) return;
        IsTracking = false;
        TrackingLost?.Invoke();
    }

    public override void SolverUpdate()
    {
        if(!IsTracking) return;
        switch (_handedness)
        {
            case Handedness.None:
            case Handedness.Other:
                break;
            case Handedness.Left:
                GoalPosition = _controllerTracker.LeftSidePosition;
                GoalRotation = _controllerTracker.LeftSideRotation;
                break;
            case Handedness.Right:
                GoalPosition = _controllerTracker.RightSidePosition;
                GoalRotation = _controllerTracker.RightSideRotation;
                break;
            case Handedness.Both:
            case Handedness.Any:
                GoalPosition = _controllerTracker.ResolvedPosition;
                GoalRotation = _controllerTracker.ResolvedRotation;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        UpdateWorkingPositionToGoal();
        UpdateWorkingRotationToGoal();
    }
}
