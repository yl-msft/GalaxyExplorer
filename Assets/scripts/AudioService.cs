using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;

#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

public class AudioService : BaseExtensionService, IAudioService<AudioId>
{
    private List<AudioSource> audioSources;
    private Dictionary<AudioId, AudioClip> audioClipCache;

    public AudioService(IMixedRealityServiceRegistrar registrar, string name, uint priority, BaseMixedRealityProfile profile) : base(registrar, name, priority, profile)
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            return;
        }

#endif
        var audioProfile = ConfigurationProfile as AudioServiceProfile;
        if (audioProfile != null)
        {
            audioSources = new List<AudioSource>();
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                for (int i = 0; i < audioProfile.numberOfAudioSources; i++)
                {
                    audioSources.Add(mainCamera.gameObject.AddComponent<AudioSource>());
                }
            }
            audioClipCache = new Dictionary<AudioId, AudioClip>();
            foreach (var audioTuple in audioProfile.audioClips)
            {
                if (!audioClipCache.ContainsKey(audioTuple.audioId))
                {
                    audioClipCache.Add(audioTuple.audioId, audioTuple.clip);
                }
            }
        }
    }

    public void PlayClip(AudioId audioId)
    {
        if (audioClipCache != null && audioClipCache.ContainsKey(audioId) && audioSources != null)
        {
            foreach (var audioSource in audioSources)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.clip = audioClipCache[audioId];
                    audioSource.time = 0;
                    audioSource.Play();
                    break;
                }
            }
        }
    }
}