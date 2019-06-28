using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Moon : MonoBehaviour
{
    private Animator _animator;
    private HashSet<MeshRenderer> _renderers = new HashSet<MeshRenderer>();
    private float _oldBlend = -1f;
    private bool _isOpaque = true;

    [Range(0,1)]
    public float Blend;
    
    private static readonly int Srcblend = Shader.PropertyToID("_SRCBLEND");
    private static readonly int Dstblend = Shader.PropertyToID("_DSTBLEND");
    private static readonly int TransitionAlpha = Shader.PropertyToID("_TransitionAlpha");

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        var renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            var material = renderer.sharedMaterial;
            if (material.HasProperty("_SRCBLEND") && 
                material.HasProperty("_DSTBLEND") &&
                material.HasProperty("_TransitionAlpha"))
            {
                _renderers.Add(renderer);
            }
        }
    }

    private void Update()
    {
        if (Mathf.Abs(_oldBlend - Blend) >= float.Epsilon)
        {
            var canBeOpaque = Mathf.Abs(Mathf.Abs(Blend-.5f) -.5f) < float.Epsilon;
            foreach (var renderer in _renderers)
            {
                var material = renderer.material; //create material instance
                if (_isOpaque && !canBeOpaque)
                {
                    material.SetInt(Srcblend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);  
                    material.SetInt(Dstblend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Transparent;
                }
                else if (!_isOpaque && canBeOpaque)
                {
                    material.SetInt(Srcblend, (int)UnityEngine.Rendering.BlendMode.One);  
                    material.SetInt(Dstblend, (int)UnityEngine.Rendering.BlendMode.Zero);  
                    material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Geometry;
                }
                material.SetFloat(TransitionAlpha, Blend);
                renderer.enabled = Blend > float.Epsilon;
            }
            _oldBlend = Blend;
            _isOpaque = canBeOpaque;
        }
    }

    public virtual void Show()
    {
        _animator.SetBool("Visible", true);
    }

    public virtual void Hide()
    {
        _animator.SetBool("Visible", false);
    }
}
