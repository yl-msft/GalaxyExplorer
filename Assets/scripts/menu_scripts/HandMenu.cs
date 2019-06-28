using UnityEngine;

public class HandMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject _menuParent;

    [SerializeField]
    private GameObject _resetButton;

    [SerializeField]
    private GameObject _backButton;

    [SerializeField]
    private float _minShowingAngle = 135f;

    private AttachToControllerSolver _attachToControllerSolver;
    private HandMenuManager _handMenuManager;
    private GlobalMenuManager _globalMenuManager;

    private float _currentAngle = 0f;

    private Transform _cameraTransform;

    public bool IsCurrentlyVisible { get; private set; } = false;

    private void Start()
    {
        _handMenuManager = FindObjectOfType<HandMenuManager>();
        _globalMenuManager = FindObjectOfType<GlobalMenuManager>();

        _menuParent.SetActive(false);
        IsCurrentlyVisible = false;

        _resetButton.SetActive(false);
        _backButton.SetActive(false);

        _attachToControllerSolver = GetComponent<AttachToControllerSolver>();
        _attachToControllerSolver.TrackingLost += OnTrackingLost;

        _cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (_attachToControllerSolver.IsTracking)
        {
            _currentAngle = CalculateAngle();

            if (_currentAngle > _minShowingAngle)
            {
                bool inManipulationState = (_globalMenuManager.ForceSolverFocusManager != null && _globalMenuManager.ForceSolverFocusManager.IsManipulatingPlanet);

                // Check if the menu is already showing on the other hand
                if (!_handMenuManager.IsHandMenuAlreadyVisible && _handMenuManager.MenuIsIsAvailable && !inManipulationState)
                {
                    UpdateMenuVisibility(true);
                }
            }
            else if (_currentAngle < _minShowingAngle && IsCurrentlyVisible)
            {
                UpdateMenuVisibility(false);
            }
        }
    }

    public void UpdateMenuVisibility(bool isVisible)
    {
        if (IsCurrentlyVisible == isVisible) { return; }

        _menuParent.SetActive(isVisible);

        if (IsCurrentlyVisible != isVisible)
        {
            if (isVisible)
            {
                _handMenuManager.PlayMenuAudio(_menuParent.transform.position, MenuStates.Appearing);
            }
            else
            {
                _handMenuManager.PlayMenuAudio(_menuParent.transform.position, MenuStates.Disappearing);
            }
        }

        IsCurrentlyVisible = isVisible;
    }

    public void UpdateButtonsActive(bool resetIsActive, bool backIsActive)
    {
        // When the POIPlanetFocusManager is present in the currently loaded scenes, this means we are in the solar system or galactic center, so activate the  reset button
        _resetButton?.SetActive(resetIsActive);

        // If there is previous scene then user should able to go back, so activate the back button
        _backButton?.SetActive(backIsActive);
    }

    private void OnTrackingLost()
    {
        UpdateMenuVisibility(false);
    }

    private float CalculateAngle()
    {
        float angleCos = Vector3.Dot(transform.forward, _cameraTransform.forward);

        float angle = Mathf.Acos(angleCos);
        angle = angle * Mathf.Rad2Deg;

        return angle;
    }
}