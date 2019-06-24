using System;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

public interface IAudioService<IdType> : IMixedRealityExtensionService where IdType : Enum 
{
    void PlayClip(IdType audioId, Transform target = null, float volume = -1);
    
    void PlayClip(IdType audioId, out AudioSource playedSource,  Transform target = null, float volume = -1);

    void PlayClip(AudioClip clip, Transform target = null, float volume = -1);

    void PlayClip(AudioClip clip, out AudioSource playedSource, Transform target = null, float volume = -1);
}

// This is a convenience interface that defines the id type used in this application
// so that the code that uses the generic interface is not required to continually
// define the generic type.  
// IAudioService<AudioId> becomes IAudioService
public interface IAudioService : IAudioService<AudioId>, IAudioMixer
{
    
}
