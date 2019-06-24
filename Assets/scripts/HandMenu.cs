using UnityEngine;
using GalaxyExplorer;

public class HandMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject _menuParent;

    [SerializeField]
    private GameObject _backButton;

    [SerializeField]
    private GameObject _resetButton;

    [SerializeField]
    private float _minShowingAngle = 135f;

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

    private AttachToControllerSolver _attachToControllerSolver;
    private HandMenuManager _handMenuManager;
    private POIPlanetFocusManager _pOIPlanetFocusManager;
    private AboutSlate _aboutSlate;

    private float _currentAngle = 0f;
    private float _interButtonDistance = 0.04f;
    private Vector3 _originalBackButtonLocalPosition;
    private Transform _cameraTransform;

    public bool IsVisible { get; private set; } = false;

    private void Start()
    {
        SetMenuVisibility(false);

        _handMenuManager = FindObjectOfType<HandMenuManager>();
        _aboutSlate = FindObjectOfType<AboutSlate>();

        _originalBackButtonLocalPosition = _backButton.transform.localPosition;

        GalaxyExplorerManager.Instance.ToolsManager.BackButtonNeedsShowing += OnBackButtonNeedsToShow;
        _backButton.SetActive(false);

        _attachToControllerSolver = GetComponent<AttachToControllerSolver>();
        _attachToControllerSolver.TrackingLost += OnTrackingLost;

        _cameraTransform = Camera.main.transform;
    }

    private void OnBackButtonNeedsToShow(bool show)
    {
        _backButton.SetActive(show);
    }

    private void Update()
    {
        // if (GalaxyExplorerManager.Platform != GalaxyExplorerManager.PlatformId.ArticulatedHandsPlatform) { return; }

        if (GalaxyExplorerManager.Instance.TransitionManager.IsInIntroFlow || GalaxyExplorerManager.Instance.TransitionManager.InTransition) { return; }

        if (_attachToControllerSolver.IsTracking)
        {
            _currentAngle = CalculateAngle();

            if (_currentAngle > _minShowingAngle && !IsVisible)
            {
                // Check if the menu is already showing on the other hand
                if (!_handMenuManager.IsAMenuVisible)
                {
                    SetMenuVisibility(true);
                    _handMenuManager.PlayMenuAudio(_menuParent.transform.position, MenuStates.Appearing);
                }
            }
            else if (_currentAngle < _minShowingAngle && IsVisible)
            {
                SetMenuVisibility(false);
                _handMenuManager.PlayMenuAudio(_menuParent.transform.position, MenuStates.Disappearing);
            }
        }

        if (POIPlanetFocusManager != null && !_resetButton.activeInHierarchy)
        {
            // When the POIPlanetFocusManager is present in the currently loaded scenes, this means we are in the solar system and the reset button should be visible
            _resetButton.SetActive(true);
            _backButton.transform.localPosition = new Vector3(0f, _originalBackButtonLocalPosition.y + _interButtonDistance, 0f);
        }
        else if (POIPlanetFocusManager == null && _resetButton.activeInHierarchy)
        {
            // When the POIPlanetFocusManager isn't present in the currently loaded scenes, this means we're not in the solar system and the reset button shouldn't show up
            _resetButton.SetActive(false);
            _backButton.transform.localPosition = _originalBackButtonLocalPosition;
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

    private void OnTrackingLost()
    {
        if (IsVisible)
        {
            _handMenuManager.PlayMenuAudio(_menuParent.transform.position, MenuStates.Disappearing);
            SetMenuVisibility(false);
        }
    }

    private void SetMenuVisibility(bool isVisible)
    {
        _menuParent.SetActive(isVisible);
        IsVisible = isVisible;
    }

    private float CalculateAngle()
    {
        float angleCos = Vector3.Dot(transform.forward, _cameraTransform.forward);

        float angle = Mathf.Acos(angleCos);
        angle = angle * Mathf.Rad2Deg;

        return angle;
    }
}