using UnityEngine;

public class HandMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject _menuParent;

    [SerializeField]
    private float _minShowingAngle = 135f;

    private AttachToControllerSolver _attachToControllerSolver;
    private HandMenuManager _handMenuManager;
    private float _currentAngle = 0f;
    private Transform _cameraTransform;

    public bool IsVisible { get; private set; } = false;

    private void Start()
    {
        SetMenuVisibility(false);

        _handMenuManager = FindObjectOfType<HandMenuManager>();

        _attachToControllerSolver = GetComponent<AttachToControllerSolver>();
        _attachToControllerSolver.TrackingLost += OnTrackingLost;

        _cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
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

        Debug.Log("angle = " + angle);

        return angle;
    }
}