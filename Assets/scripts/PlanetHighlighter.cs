using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

public class PlanetHighlighter : MonoBehaviour
{
    private Animator _animator;
    
    public MeshRenderer RingRenderer;
    private static readonly int Visible = Animator.StringToHash("Visible");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void SetFocused(bool focused)
    {
        _animator.SetBool(Visible, focused);
    }
}
