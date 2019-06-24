using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementObjectHalo : MonoBehaviour
{
    private Material _material;
    private float _oldDiameter;
    private static readonly int OuterDiameter = Shader.PropertyToID("_OuterDiameter");

    [Range(0, 1)] public float ActiveDiameter = 1f; 
    
    private void Awake()
    {
        _material = GetComponent<MeshRenderer>().material; // instanced! not shared!
    }

    public void SetActive(bool active)
    {
        if (active)
        {
            _oldDiameter = _material.GetFloat(OuterDiameter);
            _material.SetFloat(OuterDiameter, ActiveDiameter);
        }
        else
        {
            _material.SetFloat(OuterDiameter, _oldDiameter);
        }
    }
}
