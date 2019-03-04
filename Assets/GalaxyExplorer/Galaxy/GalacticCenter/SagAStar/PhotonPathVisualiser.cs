using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonPathVisualiser : MonoBehaviour
{
    public bool Visualise = true;
    public Material BlackHoleMaterial;

    private Vector3 currentRayPosition;
    private Vector3 lastRayPosition;

    private void OnDrawGizmos()
    {
        if (BlackHoleMaterial == null || !Visualise)
            return;

        GravityRayMarchGizmo();
    }

    private void GravityRayMarchGizmo()
    {
        float _Scale = BlackHoleMaterial.GetFloat("_Scale");
        float _ActualScale = _Scale * this.transform.lossyScale.x;
        Vector3 orientation = this.transform.up.normalized;
        float _BlackHoleMass = BlackHoleMaterial.GetFloat("_BlackHoleMass");
        float _DiscOuterDistance = BlackHoleMaterial.GetFloat("_DiscOuterDistance");
        float _MaxStepCount = BlackHoleMaterial.GetFloat("_MaxStepCount");
        float _EventHorizonDistance = BlackHoleMaterial.GetFloat("_EventHorizonDistance");
        float _StepSizeExtension = BlackHoleMaterial.GetFloat("_StepSizeExtension");
        float _FrontStepExtension = BlackHoleMaterial.GetFloat("_FrontStepExtension");

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, _ActualScale * _EventHorizonDistance * _BlackHoleMass);

        Vector3 massCentre = this.transform.position;
        Vector3 rayOrigin = Camera.main.transform.position;

        float squareDistance = 0f;

        Vector3 currentRayDirection = Camera.main.transform.forward;
        currentRayPosition = Camera.main.transform.position;
        lastRayPosition = currentRayPosition;

        // Find out which side of the accretin disc we are on initially
        int isAboveCentre = 0;
        Vector3 initialDisplacement = massCentre - currentRayPosition;
        Vector3 displacementNormalized = initialDisplacement.normalized;
        float initialPlaneDistance = Vector3.Dot(orientation, initialDisplacement);
        if (initialPlaneDistance <= 0)
        {
            isAboveCentre = 1;
        }
        else
        {
            isAboveCentre = 0;
        }
        int wasAboveCentre = isAboveCentre;

        float signedDistance = sdSphere(rayOrigin, massCentre, (_DiscOuterDistance + _FrontStepExtension) * _ActualScale);
        currentRayPosition += currentRayDirection.normalized * signedDistance;

        int hasCrossedEventHorizon = 0;

        DrawGizmoLine(hasCrossedEventHorizon);

        float stepSize = (((_DiscOuterDistance * 2) + _FrontStepExtension) / _MaxStepCount) * (1 +_StepSizeExtension);

        /*[unroll(10)] */
        for (int i = 0; i < _MaxStepCount; ++i)
        //for (int i = 0; i < 5; ++i)
        {

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(currentRayPosition, currentRayPosition + orientation * .1f);

            Vector3 displacement = massCentre - currentRayPosition;

            float planeDistance = Vector3.Dot(orientation, displacement);

            if (planeDistance <= 0)
            {
                isAboveCentre = 1;
                Gizmos.color = Color.red;
            }
            else
            {
                isAboveCentre = 0;
                Gizmos.color = Color.cyan;
            }

            Gizmos.DrawLine(currentRayPosition, currentRayPosition + displacement.normalized * .1f);

            squareDistance = Vector3.Dot(displacement, displacement);

            if (squareDistance < (_EventHorizonDistance * _EventHorizonDistance) * (_BlackHoleMass * _BlackHoleMass))
            {
                hasCrossedEventHorizon = 1;
            }

            if (hasCrossedEventHorizon == 0 && isAboveCentre != wasAboveCentre)
            {
                float rayDirectionScalar = 1 / Vector3.Dot(orientation, currentRayDirection);
                Vector3 intersectionPoint = currentRayPosition + currentRayDirection * rayDirectionScalar * planeDistance;
                DrawIntersectionPoint(intersectionPoint);

                // Vector3 intersectionDisplacement = massCentre - intersectionPoint;
                // float intersectionSquareDistance = Vector3.Dot(intersectionDisplacement, intersectionDisplacement);

                wasAboveCentre = isAboveCentre;
            }

            float forceMagnitude = _BlackHoleMass / squareDistance;

            float extendedStepSize = stepSize * (1 + _StepSizeExtension);

            currentRayDirection += forceMagnitude * displacement * extendedStepSize * _ActualScale;

            currentRayPosition += currentRayDirection.normalized * extendedStepSize * _ActualScale;

            DrawGizmoLine(hasCrossedEventHorizon);
        }

        if (hasCrossedEventHorizon == 0)
        {

        }
    }

    private void DrawIntersectionPoint(Vector3 intersectionPoint)
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(intersectionPoint, .01f);
    }

    void DrawGizmoLine(int hasCrossedEventHorizon)
    {
        if (hasCrossedEventHorizon == 0)
        {
            Gizmos.color = Color.white;
        }
        else
        {
            Gizmos.color = Color.red;
        }

        Gizmos.DrawLine(lastRayPosition, currentRayPosition);
        Gizmos.DrawWireSphere(currentRayPosition, .01f);
        lastRayPosition = currentRayPosition;
    }

    float sdSphere(Vector3 position, Vector3 centre, float radius)
    {
        return (centre - position).magnitude - radius;
    }
}
