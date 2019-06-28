using UnityEngine;

public class PlacementForceSolver : ForceSolver
{
    private PlacementRing _placementRing;

    protected override void Awake()
    {
        base.Awake();
        _placementRing = GetComponentInChildren<PlacementRing>();
    }

    protected override Vector3 GetOffsetPositionFromController()
    {
        var ringPosition = _placementRing.transform.position;
        return ringPosition + _placementRing.VectorToDiameterCircle(GoalPosition) - GoalPosition;
//        var diameter = _placementRing.Diameter + _placementRing.Thickness;
//        var controllerPosition = GoalPosition;
//        var placementRingTransform = _placementRing.transform;
//        var ringPosition = placementRingTransform.position;
//        var direction = (controllerPosition - ringPosition).normalized;
//        var up = placementRingTransform.up;
//        var point = ringPosition + diameter * .5f * Vector3.Cross(up, Vector3.Cross(direction, up));
//        return ringPosition - point;
    }
}
