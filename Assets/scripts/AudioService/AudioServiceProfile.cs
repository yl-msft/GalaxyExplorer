using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Mixed Reality Toolkit/Audio Service Profile", fileName = "AudioServiceProfile", order = (int)CreateProfileMenuItemIndices.RegisteredServiceProviders)]
public class AudioServiceProfile : BaseMixedRealityProfile
{
    [SerializeField] public AudioMixer musicAudioMixer;
    [SerializeField] public List<AudioInfo> audioClips;
}

[Serializable]
public class AudioInfo
{
    public AudioId audioId;
    public AudioClip clip;
    [Range(0, 2)] public float volume = 1;
}

[Serializable]
public enum AudioId
{
    // Don't reorder or insert new items into enum, always add them to the end.
    // Unity Serializes enums as ints which would result in a 
    // different sound being played if order is changed.
    None = 0,
    Focus,
    Select,
    CardSelect,
    CardDeselect,
    ToolboxShow,
    ToolBoxHide,
    ForcePull,
    ForceDwell,
    ManipulationStart,
    ManipulationEnd,
}

[Serializable]
public enum PlayOptions
{
    None = 0,
    PlayOnce,
    Loop
}
