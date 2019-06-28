using UnityEngine;

public class UiPreviewTarget : MonoBehaviour
{
    public int slotId;
    public string displayName;
    public Vector3 initialPosition = new Vector3(0,0,-.01f);
    public Quaternion initialRotation;
    public float initialFov = 60;
    public ForceSolver forceSolver;
}
