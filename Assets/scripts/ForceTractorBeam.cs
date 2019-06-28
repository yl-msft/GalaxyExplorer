using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ForceTractorBeam : MonoBehaviour
{
    public event Action<ForceTractorBeam> Destroyed;

    private const float s_divider = (float) 1.0d / int.MaxValue;

    private bool _wasActive;

    private BaseMixedRealityLineDataProvider _lineDataProvider;
    private LineRenderer _lineRenderer, _handRayLineRenderer;
    private MaterialPropertyBlock _tractorBeamMaterialPropertyBlock;
    private IMixedRealityPointer _handRayPointer;
    
    private static readonly Dictionary<IMixedRealityPointer, ForceTractorBeam> _staticPointersToTractorBeams =
        new Dictionary<IMixedRealityPointer, ForceTractorBeam>();

    [Range(0,1)]
    public float Coverage;
    public float TractorBeamWidth = .02f;


    private static readonly int LineLength = Shader.PropertyToID("_LineLength");
    private static readonly int LineWidth = Shader.PropertyToID("_LineWidth");
    private static readonly int Active = Shader.PropertyToID("_Active");
    private static readonly int CoverageProperty = Shader.PropertyToID("_Coverage");

    public static ForceTractorBeam AttachToHandRayPointer(ShellHandRayPointer pointer, GameObject tractorBeamPrefab)
    {
        if (!_staticPointersToTractorBeams.TryGetValue(pointer, out var tractorBeam))
        {
            tractorBeam = Instantiate(tractorBeamPrefab, pointer.transform).GetComponent<ForceTractorBeam>();
            tractorBeam._handRayPointer = pointer;
            _staticPointersToTractorBeams.Add(pointer, tractorBeam);
        }
        return tractorBeam;
    }

    public static ForceTractorBeam GetTractorBeamFromPointer(ShellHandRayPointer pointer)
    {
        return _staticPointersToTractorBeams.TryGetValue(pointer, out var tb) ? tb : null;
    }

    private void Awake()
    {
        _tractorBeamMaterialPropertyBlock = new MaterialPropertyBlock();
        var parent = transform.parent;
        _lineRenderer = GetComponent<LineRenderer>();
        _handRayLineRenderer = parent.GetComponent<LineRenderer>();
        _lineDataProvider = parent.GetComponent<BaseMixedRealityLineDataProvider>();
        
        UpdateLine();
        Dissipate();
    }

    private void CopyLinePositions()
    {
        if (_lineRenderer.positionCount != _handRayLineRenderer.positionCount)
        {
            _lineRenderer.positionCount = _handRayLineRenderer.positionCount;
        }
        for (var i = 0; i < _handRayLineRenderer.positionCount; i++)
        {
            _lineRenderer.SetPosition(i,_handRayLineRenderer.GetPosition(i));
        }
    }

    private void OnDestroy()
    {
        _staticPointersToTractorBeams.Remove(_handRayPointer);
        Destroyed?.Invoke(this);
    }

    private void Update()
    {
        var isTargetingForceSolver = _handRayPointer.FocusTarget is ForceSolver;
        if (!_wasActive && !_handRayPointer.IsActive)
        {
            return;
        }
        if (!_handRayPointer.IsActive || !isTargetingForceSolver || Math.Abs(Coverage) < float.Epsilon)
        {
            Dissipate();
        }
        else
        {
            _lineRenderer.enabled = true;
            UpdateLine();
            _wasActive = true;
        }
        _tractorBeamMaterialPropertyBlock.SetFloat(CoverageProperty, Coverage);
        _lineRenderer.SetPropertyBlock(_tractorBeamMaterialPropertyBlock);
    }

    private void UpdateLine()
    {
        CopyLinePositions();
        _lineRenderer.widthMultiplier = TractorBeamWidth;
        var lineLength = _lineDataProvider.UnClampedWorldLength;
        _tractorBeamMaterialPropertyBlock.SetFloat(Active, 1f);
        _tractorBeamMaterialPropertyBlock.SetFloat(LineLength, lineLength);
        _tractorBeamMaterialPropertyBlock.SetFloat(LineWidth, TractorBeamWidth);
    }

    public void Dissipate()
    {
        Coverage = 0f;
        _tractorBeamMaterialPropertyBlock.SetFloat(Active, 0f);
        
        // have to check for null in case the pointer object has been destroyed while having focus
        if (!_lineRenderer.Equals(null))
        {
            _lineRenderer.enabled = false;
        }
        
        _wasActive = false;
    }
}
