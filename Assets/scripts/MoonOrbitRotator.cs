using UnityEngine;
using Random = System.Random;


public class MoonOrbitRotator : MonoBehaviour
{
    public float OrbitalPeriodMultiplicator = 1f;

    public float StartingRotationAngle;

    public bool StartInRandomAngle = true;
    public int RandomSeed;

    private void Start()
    {
        if (StartInRandomAngle)
        {
            var r = new Random(RandomSeed);
            StartingRotationAngle = r.Next()/(float)int.MaxValue * 360f;
        }
        
        transform.Rotate(
            Vector3.down,
            StartingRotationAngle,
            Space.Self);
    }

    // Update is called once per frame
    private void Update()
    {
        transform.Rotate(
            Vector3.down,
            10 * (1f/OrbitalPeriodMultiplicator)*Time.deltaTime,
            Space.Self);
    }
}
