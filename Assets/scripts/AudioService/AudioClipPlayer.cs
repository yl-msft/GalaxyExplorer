using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

public class AudioClipPlayer : MonoBehaviour, IMixedRealityPointerHandler, IMixedRealityFocusHandler
{
    [SerializeField] private AudioId onFocus;
    [SerializeField] private AudioId onClick;

    private IAudioService audioService;
    
    void Awake()
    {
        audioService = MixedRealityToolkit.Instance.GetService<IAudioService>();
        audioService = MixedRealityToolkit.Instance.GetService<IAudioService>();
    }

    #region IMixedRealityPointerHandlerFunctions
    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        audioService.PlayClip(onClick);
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
    }
    #endregion

    #region IMixedRealityFocusHandlerFunctions
    public void OnBeforeFocusChange(FocusEventData eventData)
    {
    }

    public void OnFocusChanged(FocusEventData eventData)
    {
    }

    public void OnFocusEnter(FocusEventData eventData)
    {
        audioService.PlayClip(onFocus);
    }

    public void OnFocusExit(FocusEventData eventData)
    {
    }
    #endregion
}
