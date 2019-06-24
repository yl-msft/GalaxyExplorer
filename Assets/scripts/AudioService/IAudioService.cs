using System;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

public interface IAudioService<IdType, PlayOptionsType> : IMixedRealityExtensionService 
    where IdType : Enum
    where PlayOptionsType : Enum
{
    void PlayClip(IdType audioId, Transform target = null, float volume = -1);
    
    void PlayClip(IdType audioId, out AudioSource playedSource,  Transform target = null, float volume = -1);

    void PlayClip(AudioClip clip, Transform target = null, float volume = -1);

    void PlayClip(AudioClip clip, out AudioSource playedSource, Transform target = null, float volume = -1, PlayOptionsType playOptions = default);
}

// This is a convenience interface that defines the id type used in this application
// so that the code that uses the generic interface is not required to continually
// define the generic type.  
// IAudioService<AudioId, PlayOptions> becomes IAudioService
public interface IAudioService : IAudioService<AudioId, PlayOptions>, IAudioMixer
{
    
}
