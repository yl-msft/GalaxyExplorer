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

    private float _currentAngle = 0f;
    private float _interButtonDistance = 0.04f;
    private Vector3 _originalBackButtonLocalPosition;
    private Transform _cameraTransform;

    public bool IsCurrentlyVisible { get; private set; } = false;

    private void Start()
    {
        _handMenuManager = FindObjectOfType<HandMenuManager>();
        _menuParent.SetActive(false);
        IsCurrentlyVisible = false;

        _originalBackButtonLocalPosition = _backButton.transform.localPosition;

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
                // Check if the menu is already showing on the other hand
                if (!_handMenuManager.IsAMenuVisible && _handMenuManager.MenuIsInActiveState)
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
        if (resetIsActive && !_resetButton.activeSelf)
        {
            // When the POIPlanetFocusManager is present in the currently loaded scenes, this means we are in the solar system and the reset button should be visible
            _resetButton.SetActive(true);
            _backButton.transform.localPosition = new Vector3(0f, _originalBackButtonLocalPosition.y + _interButtonDistance, 0f);
        }
        else if (!resetIsActive && _resetButton.activeSelf)
        {
            // When the POIPlanetFocusManager isn't present in the currently loaded scenes, this means we're not in the solar system and the reset button shouldn't show up
            _resetButton.SetActive(false);
            _backButton.transform.localPosition = _originalBackButtonLocalPosition;
        }

        // If there is previous scene then user is able to go back so activate the back button
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