using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementObjectRim : MonoBehaviour
{
    private Material _material;
    private float _oldMultiplier;
    private static readonly int Multiplier = Shader.PropertyToID("_Multiplier");

    [Range(0, 1)] public float ActiveMultiplier = 1f; 
    
    private void Awake()
    {
        _material = GetComponent<MeshRenderer>().material; // instanced! not shared!
    }

    public void SetActive(bool active)
    {
        if (active)
        {
            _oldMultiplier = _material.GetFloat(Multiplier);
            _material.SetFloat(Multiplier, ActiveMultiplier);
        }
        else
        {
            _material.SetFloat(Multiplier, _oldMultiplier);
        }
    }
}
