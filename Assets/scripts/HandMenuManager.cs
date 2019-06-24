using UnityEngine;

public enum MenuStates { Appearing, Disappearing };

public class HandMenuManager : MonoBehaviour
{
    [SerializeField]
    private HandMenu _handMenuLeft;

    [SerializeField]
    private HandMenu _handMenuRight;

    [SerializeField]
    private AudioSource _movableAudioSource;

    [SerializeField]
    private AudioClip _menuAppearAudioClip;

    [SerializeField]
    private AudioClip _menuDisappearAudioClip;

    public bool IsAMenuVisible
    {
        get { return _handMenuLeft.IsCurrentlyVisible || _handMenuRight.IsCurrentlyVisible; }
    }

    public bool MenuIsInActiveState { get; private set; } = false;

    public void SetMenuVisibility(bool isInActiveState, bool resetIsActive, bool backIsActive)
    {
        MenuIsInActiveState = isInActiveState;

        if (MenuIsInActiveState)
        {
            _handMenuLeft.UpdateButtonsActive(resetIsActive, backIsActive);
            _handMenuRight.UpdateButtonsActive(resetIsActive, backIsActive);
        }
        else
        {
            _handMenuLeft.UpdateMenuVisibility(false);
            _handMenuRight.UpdateMenuVisibility(false);
        }
    }

    public void PlayMenuAudio(Vector3 position, MenuStates menuState)
    {
        switch (menuState)
        {
            case MenuStates.Appearing:
                _movableAudioSource.clip = _menuAppearAudioClip;
                break;

            case MenuStates.Disappearing:
                _movableAudioSource.clip = _menuDisappearAudioClip;
                break;

            default:
                break;
        }

        _movableAudioSource.transform.position = position;
        _movableAudioSource.Play();
    }
}