using UnityEngine;

[ExecuteInEditMode]
public class PlanetOffsetScaleController : MonoBehaviour
{
    public static readonly int sunDiamterInKm = 1392000;
    public static readonly float TargetEditScaleCm = 30f;
    public static readonly float inAppSunDiameterInCm = 40f;
    public static readonly float TargetOrbitScaleToCm = inAppSunDiameterInCm/sunDiamterInKm*1000;
    public static readonly float GlobalPlanetScaleFactor = 10f;
    
    public float PlanetDiameterInKilometer = 1000f;
    public bool UseGlobalPlanetScaleFactor = true;
    public float CustomPerPlanetScaleFactor = 1f;

    private void Update()
    {
        transform.localScale =
            Vector3.one * PlanetDiameterInKilometer * .001f * TargetOrbitScaleToCm *
            (UseGlobalPlanetScaleFactor
                ? GlobalPlanetScaleFactor
                : CustomPerPlanetScaleFactor);
    }
}
