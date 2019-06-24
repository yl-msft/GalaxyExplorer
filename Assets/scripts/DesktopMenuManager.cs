using UnityEngine;

public class DesktopMenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _menuParent;

    [SerializeField]
    private GameObject _buttonParent;

    [SerializeField]
    private GameObject _resetButton;

    [SerializeField]
    private GameObject _backButton;

    private Vector3 _defaultBackButtonLocalPosition;
    private Vector3 _fullMenuVisibilityBackButtonPos;
    private Transform _cameraTransform;

    public bool IsVisible { get; private set; } = false;

    private void Start()
    {
        SetMenuVisibility(false, false, false);

        // Store the x value of the local position for the back button when all menu buttons are visible
        _fullMenuVisibilityBackButtonPos = _backButton.transform.localPosition;

        // Since reset is not visible during most of the app states, regard its local position as the default back button local position
        _defaultBackButtonLocalPosition = _resetButton.transform.localPosition;

        // Since the app starts with reset button not visible, move the back button to its spot instead
        _backButton.transform.localPosition = _defaultBackButtonLocalPosition;

        _backButton.SetActive(false);
        _resetButton.SetActive(false);
        _buttonParent.SetActive(false);

        _cameraTransform = Camera.main.transform;
    }

    public void SetMenuVisibility(bool isVisible, bool resetIsActive, bool backIsActive)
    {
        //if (IsVisible == isVisible) { return; }

        if (isVisible)
        {
            UpdateButtonsActive(resetIsActive, backIsActive);
        }

        _menuParent.SetActive(isVisible);

        IsVisible = isVisible;
    }

    private void UpdateButtonsActive(bool resetIsActive, bool backIsActive)
    {
        if (resetIsActive && !_resetButton.activeSelf)
        {
            // When the POIPlanetFocusManager is present in the currently loaded scenes, this means we are in the solar system and the reset button should be visible
            _resetButton.SetActive(true);
            _backButton.transform.localPosition = _fullMenuVisibilityBackButtonPos;
        }
        else if (!resetIsActive && _resetButton.activeSelf)
        {
            // When the POIPlanetFocusManager isn't present in the currently loaded scenes, this means we're not in the solar system and the reset button shouldn't show up
            _resetButton.SetActive(false);
            _backButton.transform.localPosition = _defaultBackButtonLocalPosition;
        }

        // If there is previous scene then user is able to go back so activate the back button
        _backButton?.SetActive(backIsActive);
    }

    public void OnToggleDesktopButtonVisibility()
    {
        _buttonParent.SetActive(!_buttonParent.activeSelf);
    }
}