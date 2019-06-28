using UnityEngine;

public interface IAudioMixer
{
    bool TryTransitionMixerSnapshot(string name, float transitionTime);
}
