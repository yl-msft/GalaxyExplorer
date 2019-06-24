using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveGameObjectLinker : MonoBehaviour
{
    [SerializeField] private GameObject linkedGameObject;
    private void OnEnable()
    {
        if (linkedGameObject != null)
        {
            linkedGameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (linkedGameObject != null)
        {
            linkedGameObject.SetActive(false);
        }
    }
}
