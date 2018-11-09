using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityFade : MonoBehaviour
{
    public Renderer[] RenderersToFade;
    public float UserClippingDistance = 1f;
    public float FadeStart = .5f;
    public float FadeEnd = 0.3f;

    private float scaledFadeEnd = 0f;
    private float scaledFadeStart = 0f;

    void LateUpdate()
    {
        Vector3 displacement = this.transform.position - Camera.main.transform.position;

        float distanceFromUserClip = displacement.magnitude - UserClippingDistance;

        scaledFadeStart = FadeStart * this.transform.lossyScale.x;
        scaledFadeEnd = FadeEnd * this.transform.lossyScale.x;

        float fadeAmount = 1f - Mathf.Clamp01((distanceFromUserClip - scaledFadeEnd) / (scaledFadeStart - scaledFadeEnd));

        foreach (Renderer rend in RenderersToFade)
        {
            rend.sharedMaterial.SetFloat("_Fade", fadeAmount);
        }
    }
}
