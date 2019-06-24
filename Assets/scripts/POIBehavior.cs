using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using MRS.Layers;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class POIBehavior : MonoBehaviour//, IMixedRealityPointerHandler
{
    private const float INITIAL_DELAY = 5f; 
    
    [SerializeField] private List<Renderer> objectsToFade;
    [SerializeField] private List<TextMeshPro> textsToFade;
    [SerializeField] private float alphaColor = .2f;
    [SerializeField] private float scale = 1f;
    [SerializeField] private Vector2 offset = Vector2.zero;
    [SerializeField] private GameObject pressableButton;
    [SerializeField] private Renderer windowRenderer;
    [SerializeField] private Material windowMaterial;
    [SerializeField] private Material occlusionMaterial;
    [SerializeField] private Texture2D windowImage;
    [SerializeField] private float windowImageScale;
    [SerializeField] private Vector2 windowImageOffset;
    
    
    private BoxCollider boxCollider;
    private GameObject camera;
    private Transform[] corners;
    private bool fading;
    private List<Material> materialsToFade;
    private Vector3 colliderSize;
    private Color currentColor;
    
    RaycastHit[] raycastResults = new RaycastHit[1];

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        colliderSize = boxCollider.size;
        windowMaterial = Instantiate(windowMaterial);
        windowMaterial.SetFloat("_Scale", windowImageScale);
        windowMaterial.SetTexture("_MainTex", windowImage);
        windowMaterial.SetTextureOffset("_MainTex", windowImageOffset);
        windowRenderer.materials = new[] {windowMaterial, occlusionMaterial};
        
        camera = Camera.main.gameObject;
        corners = new Transform[5];
        Vector3[] verts = new Vector3[5]; 
        verts[0] = transform.TransformPoint(boxCollider.center);
        verts[1] = transform.TransformPoint(boxCollider.center + (new Vector3(boxCollider.size.x, boxCollider.size.y, 0) * 0.4f));
        verts[2] = transform.TransformPoint(boxCollider.center + (new Vector3(boxCollider.size.x, -boxCollider.size.y, 0) * 0.4f));
        verts[3] = transform.TransformPoint(boxCollider.center + (new Vector3(-boxCollider.size.x, boxCollider.size.y, 0) * 0.4f));
        verts[4] = transform.TransformPoint(boxCollider.center + (new Vector3(-boxCollider.size.x, -boxCollider.size.y, 0) * 0.4f));

        for (var i = 0; i < verts.Length; i++)
        {
            var go = new GameObject($"corner {i}");
            go.transform.position = verts[i];
            go.transform.SetParent(transform, true);
            corners[i] = go.transform;
        }
        
        var anchor = new GameObject("Anchor");
        anchor.transform.SetParent(transform.parent, false);
        anchor.transform.localPosition = transform.localPosition;
        
        materialsToFade = new List<Material>();

        foreach (var fadingObject in objectsToFade)
        {
            materialsToFade.Add(fadingObject.material);
        }
        
        foreach (var fadingText in textsToFade)
        {
            fadingText.color = Color.clear;
        }
        Fade(false, 0, 0);
        Fade(false, INITIAL_DELAY, 0);
    }

    void Update()
    {
        if (fading)
        {
            return;
        }

        boxCollider.size = colliderSize;
        var allPointsVisible = IsPointVisible(corners[0].position);;
        allPointsVisible = allPointsVisible && IsPointVisible(corners[1].position);
        allPointsVisible = allPointsVisible && IsPointVisible(corners[2].position);
        allPointsVisible = allPointsVisible && IsPointVisible(corners[3].position);
        allPointsVisible = allPointsVisible && IsPointVisible(corners[4].position);
        Fade(allPointsVisible);
        transform.localPosition = offset;
        transform.localScale = scale * Vector3.one;
    }

    private bool IsPointVisible(Vector3 position)
    {
        var layerMask = 1 << LayerMask.NameToLayer("POI");
        var direction = (position - camera.transform.position).normalized;
        Debug.DrawRay(camera.transform.position, (position - camera.transform.position)*2, Color.green);
        RaycastHit hit;
        var isVisible = false;
        if(Physics.Raycast(camera.transform.position, direction, out hit, float.PositiveInfinity, layerMask ))
        {
            if (hit.collider != null)
            {
                if (hit.collider.gameObject == gameObject)
                {
                    return true;
                }
            }
        }
       return false;
    }


    public void Fade(bool fadeIn, float overTime = .3f, float alpha = -1)
    {
        StartCoroutine(FadeRoutine(fadeIn, overTime, alpha));
        pressableButton.SetActive(fadeIn);

    }

    private IEnumerator FadeRoutine(bool fadeIn, float overTime = .3f, float alpha = -1f)
    {
        fading = true;
        alpha = alpha != -1 ? alpha : alphaColor;
        var fadingColor = fadeIn ? Color.white:  new Color(1,1,1,alpha);
        var startColor = currentColor;
      
        boxCollider.size = fadeIn ? colliderSize : Vector3.zero;
        var timeSoFar = 0f;
        while (timeSoFar < overTime)
        {
            foreach (var material in materialsToFade)
            {
                material.color = Color.Lerp(startColor, fadingColor, timeSoFar / overTime);
            }
        
            foreach (var fadingText in textsToFade)
            {
                fadingText.color = Color.Lerp(startColor, fadingColor, timeSoFar / overTime);
            }
            yield return null;
            timeSoFar += Time.deltaTime;
        }
        
        foreach (var material in materialsToFade)
        {
            material.color =  fadingColor;
        }
        
        foreach (var fadingText in textsToFade)
        {
            fadingText.color = fadingColor;
        }

        currentColor = fadingColor;

        fading = false;
    }
}
