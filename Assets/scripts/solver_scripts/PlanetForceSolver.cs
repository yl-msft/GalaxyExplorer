using System;
using System.Collections.Generic;
using System.Linq;
using GalaxyExplorer;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

public class PlanetForceSolver : ForceSolver
{
    private PlanetOffsetScaleController _scaleController;
    private Vector3 _editScaleTarget = Vector3.one;
    private float _oldBlend;
    private PlanetHighlighter _planetHighlighter;
    private IAudioService _audioService;
    private AudioSource _voAudioSource, _ambientAudioSource;
    private List<Moon> _moons = new List<Moon>();

    [SerializeField]
    private AudioClip planetAudioClip;
    [SerializeField]
    private AudioClip planetAmbiantClip;

    protected override void Awake()
    {
        base.Awake();
        
        _scaleController = GetComponentInChildren<PlanetOffsetScaleController>();
        if (_scaleController != null)
        {
            _editScaleTarget = Vector3.one *
                               (PlanetOffsetScaleController.TargetEditScaleCm /
                                _scaleController.transform.localScale.x);

        }

        _planetHighlighter = GetComponentInChildren<PlanetHighlighter>();
        _audioService = MixedRealityToolkit.Instance.GetService<IAudioService>();

        _moons = GetComponentsInChildren<Moon>().ToList();
    }

    private void StopAudio()
    {
        if (_voAudioSource != null)
        {
            _voAudioSource.Stop();
        }

        if (_ambientAudioSource != null)
        {
            _ambientAudioSource.Stop();
        }
    }

    private void StartAudio()
    {
        GalaxyExplorerManager.Instance.VoManager.Stop(true);
        GalaxyExplorerManager.Instance.VoManager.PlayClip(planetAudioClip);
        _audioService.PlayClip(planetAmbiantClip, out _ambientAudioSource, transform, playOptions:PlayOptions.Loop);
    }

    private void HideMoons()
    {
        foreach (var moon in _moons)
        {
            moon.Hide();
        }
    }

    private void ShowMoons()
    {
        foreach (var moon in _moons)
        {
            moon.Show();
        }
    }

    protected override void OnStartRoot()
    {
        base.OnStartRoot();
        _planetHighlighter.gameObject.SetActive(true);
        StopAudio();
        HideMoons();
    }

    protected override void OnStartAttraction()
    {
        base.OnStartAttraction();
        _planetHighlighter.gameObject.SetActive(false);
        StartAudio();
        HideMoons();
    }


    protected override void OnStartManipulation()
    {
        base.OnStartManipulation();
        ShowMoons();
    }

    protected override void OnStartFree()
    {
        base.OnStartFree();
        ShowMoons();
    }

    public override void SolverUpdate()
    {
        base.SolverUpdate();
        switch (ForceState)
        {
            case State.Root:
                GoalScale = Vector3.one;
                UpdateWorkingScaleToGoal();
                break;
                
            case State.Attraction:
                if (_scaleController == null) break;
                GoalScale = _editScaleTarget;
                UpdateWorkingScaleToGoal();
                break;
            
            case State.Dwell:
            case State.Free:
            case State.Manipulation:
            case State.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override void OnFocusExit(FocusEventData eventData)
    {
        base.OnFocusExit(eventData);
        _planetHighlighter.SetFocused(false);
    }

    public override void OnFocusEnter(FocusEventData eventData)
    {
        base.OnFocusEnter(eventData);
        _planetHighlighter.SetFocused(true);
    }
}
