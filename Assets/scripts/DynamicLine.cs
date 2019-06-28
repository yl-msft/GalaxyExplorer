using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[ExecuteInEditMode]
public class DynamicLine : MonoBehaviour
{
    [SerializeField] private GameObject objectA;
    [SerializeField] private GameObject objectB;
    [SerializeField] private int numLinePoints = 20;
    
    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Start()
    {
        lineRenderer.positionCount = numLinePoints;
    }

    void Update()
    {
        if (lineRenderer.positionCount != numLinePoints)
        {
            lineRenderer.positionCount = numLinePoints;
        }

        if (objectA != null && objectB != null)
        {
            if (numLinePoints > 0)
            {
                lineRenderer.SetPosition(0,objectA.transform.position);
                for (int i = 1; i < numLinePoints -1; i++)
                {
                    lineRenderer.SetPosition(i, Vector3.Lerp(objectA.transform.position, objectB.transform.position, i+1/(float)numLinePoints));
                }
                lineRenderer.SetPosition(numLinePoints-1, objectB.transform.position);
            }
        }
    }
}
