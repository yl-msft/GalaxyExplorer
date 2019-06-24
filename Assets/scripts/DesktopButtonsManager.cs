using GalaxyExplorer;
using UnityEngine;

public class DesktopButtonsManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _menuParent;

    [SerializeField]
    private GameObject _backButton;

    [SerializeField]
    private GameObject _resetButton;

    private POIPlanetFocusManager POIPlanetFocusManager
    {
        get
        {
            if (_pOIPlanetFocusManager == null)
            {
                _pOIPlanetFocusManager = FindObjectOfType<POIPlanetFocusManager>();
            }

            return _pOIPlanetFocusManager;
        }
    }

    private POIPlanetFocusManager _pOIPlanetFocusManager;
    private AboutSlate _aboutSlate;
    private Vector3 _defaultBackButtonLocalPosition;
    private float _fullMenuVisibleBackButtonX;
    private Transform _cameraTransform;

    public bool IsVisible { get; private set; } = false;

    private void Start()
    {
        SetMenuVisibility(false);

        _aboutSlate = FindObjectOfType<AboutSlate>();

        // Store the x value of the local position for the back button when all menu buttons are visible
        _fullMenuVisibleBackButtonX = _backButton.transform.localPosition.x;

        // Since reset is not visible during most of the app states, regard its local position as the default back button local position
        _defaultBackButtonLocalPosition = _resetButton.transform.localPosition;

        // Since the app starts with reset button not visible, move the back button to its spot instead
        _backButton.transform.localPosition = _defaultBackButtonLocalPosition;

        GalaxyExplorerManager.Instance.ToolsManager.BackButtonNeedsShowing += OnBackButtonNeedsToShow;
        _backButton.SetActive(false);

        _cameraTransform = Camera.main.transform;
    }

    private void OnBackButtonNeedsToShow(bool show)
    {
        _backButton.SetActive(show);
    }

    private void Update()
    {
        if (GalaxyExplorerManager.Platform != GalaxyExplorerManager.PlatformId.Desktop) { return; }

        if (GalaxyExplorerManager.Instance.TransitionManager.IsInIntroFlow || GalaxyExplorerManager.Instance.TransitionManager.InTransition) { return; }

        SetMenuVisibility(true);

        if (POIPlanetFocusManager != null && !_resetButton.activeInHierarchy)
        {
            // When the POIPlanetFocusManager is present in the currently loaded scenes, this means we are in the solar system and the reset button should be visible
            _resetButton.SetActive(true);
            _backButton.transform.localPosition = new Vector3(_fullMenuVisibleBackButtonX, 0f, 0f);
        }
        else if (POIPlanetFocusManager == null && _resetButton.activeInHierarchy)
        {
            // When the POIPlanetFocusManager isn't present in the currently loaded scenes, this means we're not in the solar system and the reset button shouldn't show up
            _resetButton.SetActive(false);
            _backButton.transform.localPosition = _defaultBackButtonLocalPosition;
        }
    }

    public void OnAboutButtonPressed()
    {
        _aboutSlate.ButtonClicked();
    }

    public void OnBackButtonPressed()
    {
        GalaxyExplorerManager.Instance.TransitionManager.LoadPrevScene();
    }

    public void OnResetButtonPressed()
    {
        if (POIPlanetFocusManager)
        {
            _pOIPlanetFocusManager.ResetAllForseSolvers();
        }
        else
        {
            Debug.Log("No POIPlanetFocusManager found in currently loaded scenes");
        }
    }

    private void SetMenuVisibility(bool isVisible)
    {
        _menuParent.SetActive(isVisible);
        IsVisible = isVisible;
    }
}