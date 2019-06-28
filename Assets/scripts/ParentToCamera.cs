using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentToCamera : MonoBehaviour
{
    [SerializeField] private Vector3 offsetPosition;

    private void Start()
    {
        var camera = Camera.main;
        if (camera != null)
        {
            transform.parent = camera.transform;
            transform.localRotation = Quaternion.identity;
            transform.localPosition = offsetPosition;
        }
    }
}
