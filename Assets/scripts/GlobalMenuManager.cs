using UnityEngine;
using GalaxyExplorer;
using System.Collections;

public class GlobalMenuManager : MonoBehaviour
{
    public ForceSolverFocusManager ForceSolverFocusManager
    {
        get
        {
            if (_pOIPlanetFocusManager == null)
            {
                _pOIPlanetFocusManager = FindObjectOfType<ForceSolverFocusManager>();
            }

            return _pOIPlanetFocusManager;
        }
    }

    public bool ResetButtonNeedsShowing { get; private set; } = false;
    public bool BackButtonNeedsShowing { get; private set; } = false;

    public bool MenuIsVisible { get; private set; } = false;

    public delegate void AboutSlateOnDelegate(bool enable);

    public AboutSlateOnDelegate OnAboutSlateOnDelegate;

    private DesktopMenuManager _desktopButtonsManager;
    private GGVMenuManager _ggvMenuManager;
    private HandMenuManager _handMenuManager;
    private ForceSolverFocusManager _pOIPlanetFocusManager;

    private AboutSlate _aboutSlate;

    private void Start()
    {
        _desktopButtonsManager = GetComponent<DesktopMenuManager>();
        _ggvMenuManager = GetComponent<GGVMenuManager>();
        _handMenuManager = GetComponent<HandMenuManager>();

        _aboutSlate = FindObjectOfType<AboutSlate>();

        BackButtonNeedsShowing = false;
        ResetButtonNeedsShowing = false;
        UpdateMenuState(false);

        if (GalaxyExplorerManager.Instance.ViewLoaderScript)
        {
            GalaxyExplorerManager.Instance.ViewLoaderScript.OnSceneIsLoaded += OnSceneIsLoaded;
            GalaxyExplorerManager.Instance.ViewLoaderScript.OnLoadNewScene += OnLoadNewScene;
        }
    }

    private void OnDestroy()
    {
        if (GalaxyExplorerManager.Instance != null && GalaxyExplorerManager.Instance.ViewLoaderScript)
        {
            GalaxyExplorerManager.Instance.ViewLoaderScript.OnSceneIsLoaded -= OnSceneIsLoaded;
            GalaxyExplorerManager.Instance.ViewLoaderScript.OnLoadNewScene -= OnLoadNewScene;
        }
    }

    public void OnSceneIsLoaded()
    {
        StartCoroutine(OnSceneIsLoadedCoroutine());
    }

    // Callback when a new scene is requested to be loaded
    public void OnLoadNewScene()
    {
        UpdateMenuState(false);
    }

    // Callback when a new scene is loaded
    private IEnumerator OnSceneIsLoadedCoroutine()
    {
        // Waiting necessary for events in flow manager to be called and
        // stage of intro flow to be correct when executing following code
        yield return new WaitForSeconds(1);

        if (!MenuIsVisible && !GalaxyExplorerManager.Instance.TransitionManager.IsInIntroFlow)
        {
            // If menu is not visible and intro flow has finished then make menu visible
            while (GalaxyExplorerManager.Instance.TransitionManager.InTransition)
            {
                yield return null;
            }
            yield return null;

            UpdateMenuState(true);
        }
    }

    private void UpdateMenuState(bool show)
    {
        if (ForceSolverFocusManager != null)
        {
            // When the POIPlanetFocusManager is present in the currently loaded scenes, this means we are in the solar system and the reset button should be visible
            ResetButtonNeedsShowing = true;
        }
        else
        {
            // When the POIPlanetFocusManager isn't present in the currently loaded scenes, this means we're not in the solar system and the reset button shouldn't show up
            ResetButtonNeedsShowing = false;
        }

        BackButtonNeedsShowing = GalaxyExplorerManager.Instance.ViewLoaderScript.IsTherePreviousScene();
        MenuIsVisible = show;

        switch (GalaxyExplorerManager.Platform)
        {
            case GalaxyExplorerManager.PlatformId.HoloLensGen1:
            case GalaxyExplorerManager.PlatformId.ImmersiveHMD:
                _ggvMenuManager.SetMenuVisibility(MenuIsVisible, ResetButtonNeedsShowing, BackButtonNeedsShowing);
                break;

            case GalaxyExplorerManager.PlatformId.ArticulatedHandsPlatform:
                _handMenuManager.SetMenuVisibility(MenuIsVisible, ResetButtonNeedsShowing, BackButtonNeedsShowing);
                break;

            case GalaxyExplorerManager.PlatformId.Desktop:
                _desktopButtonsManager.SetMenuVisibility(MenuIsVisible, ResetButtonNeedsShowing, BackButtonNeedsShowing);
                break;

            default:
                Debug.Log("Unsupported platform");
                break;
        }
    }

    public void OnAboutButtonPressed()
    {
        _aboutSlate.ButtonClicked();
    }

    public void OnResetButtonPressed()
    {
        if (ForceSolverFocusManager)
        {
            _pOIPlanetFocusManager.ResetAllForseSolvers();
        }
        else
        {
            Debug.Log("No POIPlanetFocusManager found in currently loaded scenes");
        }
    }

    public void OnBackButtonPressed()
    {
        GalaxyExplorerManager.Instance.TransitionManager.LoadPrevScene();
    }
}