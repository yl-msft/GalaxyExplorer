using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mixed Reality Toolkit/Audio Service", fileName = "AudioServiceProfile", order = (int)CreateProfileMenuItemIndices.RegisteredServiceProviders)]
public class AudioServiceProfile : BaseMixedRealityProfile
{
    [SerializeField] public List<AudioTuple> audioClips;
    [SerializeField] public int numberOfAudioSources = 2;
}

[Serializable]
public class AudioTuple
{
    public AudioId audioId;
    public AudioClip clip;
}

[Serializable]
public enum AudioId
{
    None = 0,
    Focus,
    Select,
    CardSelect,
    CardDeselect,
    ToolboxShow,
    ToolBoxHide
}