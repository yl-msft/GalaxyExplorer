using System;
using System.Collections;
using GalaxyExplorer;
using UnityEngine;
using UnityEngine.Playables;

public class OnboardingManager : MonoBehaviour
{
    public enum Stage
    {
        None,
        Intro,
        AfterDedicatedIntro,
        AfterForcePull,
        AfterConfirmation,
        Done
    }
    
    private GalaxyExplorerManager.PlatformId _platformId;
    private ForceSolver _placementForceSolver;
    
    public VOManager VoManager;
    
    
    [Header("All devices")] public AudioClip Intro_01, Intro_02, Intro_04;

    [Header("HoloLens (gen1)")] public AudioClip Intro_0301_hl1, Intro_0302_hl1;

    [Header("HoloLens2")] public AudioClip Intro_0301_hl2, Intro_0302_hl2;
    
    [Header("VR")] public AudioClip Intro_03_vr;
    
    [Header("Timelines")] public PlayableDirector ForceDirector, PlacementDirector;

    public Stage OnboardingStage { get; private set; }

    private void Awake()
    {
        OnboardingStage = Stage.None;
        ForceDirector.gameObject.SetActive(false);
        PlacementDirector.gameObject.SetActive(false);
    }

    private void AdvanceStateMachine()
    {
        switch (OnboardingStage)
        {
            case Stage.Intro:
                switch (_platformId)
                {
                    case GalaxyExplorerManager.PlatformId.HoloLensGen1:
                        VoManager.PlayClip(Intro_0301_hl1);
                        break;
                    
                    case GalaxyExplorerManager.PlatformId.ArticulatedHandsPlatform:
                        VoManager.PlayClip(Intro_0301_hl2);
                        ForceDirector.gameObject.SetActive(true);
                        ForceDirector.Play();
                        break;
                    
                    case GalaxyExplorerManager.PlatformId.ImmersiveHMD:
                        VoManager.PlayClip(Intro_03_vr);
                        break;
                    case GalaxyExplorerManager.PlatformId.Desktop:
                        break;
                    case GalaxyExplorerManager.PlatformId.Phone:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                OnboardingStage = Stage.AfterDedicatedIntro;
                EvaluateOnEndOfVoiceOver();
                break;

            case Stage.AfterDedicatedIntro:
                switch (_platformId)
                {
                    case GalaxyExplorerManager.PlatformId.HoloLensGen1:
                    case GalaxyExplorerManager.PlatformId.ImmersiveHMD:
                    case GalaxyExplorerManager.PlatformId.ArticulatedHandsPlatform:
                        EnableForcePull();
                        break;
                    
                    case GalaxyExplorerManager.PlatformId.Desktop:
                        break;
                    case GalaxyExplorerManager.PlatformId.Phone:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                OnboardingStage = Stage.AfterForcePull;
                break;
            
            case Stage.AfterForcePull:
                switch (_platformId)
                {
                    case GalaxyExplorerManager.PlatformId.HoloLensGen1:
                        VoManager.PlayClip(Intro_0302_hl1, 0f, false, true);
                        break;
                    
                    case GalaxyExplorerManager.PlatformId.ArticulatedHandsPlatform:
                        VoManager.PlayClip(Intro_0302_hl2, 0f, false, true);
                        ForceDirector.Stop();
                        ForceDirector.gameObject.SetActive(false);
                        PlacementDirector.gameObject.SetActive(true);
                        PlacementDirector.Play();
                        break;
                    
                    case GalaxyExplorerManager.PlatformId.ImmersiveHMD:
                        break;
                    case GalaxyExplorerManager.PlatformId.Desktop:
                        break;
                    case GalaxyExplorerManager.PlatformId.Phone:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                // always chain intro 4
                VoManager.PlayClip(Intro_04);
                OnboardingStage = Stage.AfterConfirmation;
                break;
            
            case Stage.AfterConfirmation:
                DisableForcePull();
                switch (_platformId)
                {
                    case GalaxyExplorerManager.PlatformId.HoloLensGen1:
                        break;
                    case GalaxyExplorerManager.PlatformId.ArticulatedHandsPlatform:
                        PlacementDirector.Stop();
                        PlacementDirector.gameObject.SetActive(false);
                        break;
                    case GalaxyExplorerManager.PlatformId.ImmersiveHMD:
                        break;
                    case GalaxyExplorerManager.PlatformId.Desktop:
                        break;
                    case GalaxyExplorerManager.PlatformId.Phone:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                OnboardingStage = Stage.Done;
                break;
            
            case Stage.None:
                break;
            
            case Stage.Done:
                return;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
        
    }

    private void EnableForcePull()
    {
        _placementForceSolver.EnableForce = true;
    }

    private void DisableForcePull()
    {
        _placementForceSolver.EnableForce = false;
    }

    private void EvaluateOnEndOfVoiceOver()
    {
        StartCoroutine(WaitTillEndOfVoiceOverAndEvaluate());
    }

    private void OnForcePullFree(ForceSolver _)
    {
        OnboardingStage = Stage.AfterForcePull;
        AdvanceStateMachine();
    }

    private IEnumerator WaitTillEndOfVoiceOverAndEvaluate()
    {
        while (VoManager.IsPlaying)
        {
            yield return null;
        }

        AdvanceStateMachine();
    }

    public void StartIntro(ForceSolver placementForceSolver, bool skipPlacement = false)
    {
        _placementForceSolver = placementForceSolver;
        DisableForcePull();
        _placementForceSolver.SetToFree.AddListener(OnForcePullFree);
        _platformId = GalaxyExplorerManager.Platform;
        VoManager.PlayClip(Intro_01);
        if (!skipPlacement)
        {
            VoManager.PlayClip(Intro_02);
        }
        OnboardingStage = skipPlacement ? Stage.AfterConfirmation : Stage.Intro;
        EvaluateOnEndOfVoiceOver();
    }

    public void OnPlacementConfirmed()
    {
        OnboardingStage = Stage.AfterConfirmation;
        EvaluateOnEndOfVoiceOver();
    }
}
    
    
