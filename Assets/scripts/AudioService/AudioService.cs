using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Pools;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Audio;

public class AudioService : BaseExtensionService, IAudioService
{
    private static float SameClipCoolDownTime = .05f;
    
    private Dictionary<AudioId, AudioInfo> audioClipCache;
    private Dictionary<Transform, List<PoolableAudioSource>> playingCache;
    private Dictionary<string, DateTime> lastPlayedTimes;
    private ObjectPooler objectPooler;
    private Transform mainCameraTransform;
    private AudioServiceProfile audioProfile;
    
    public AudioService(IMixedRealityServiceRegistrar registrar, string name, uint priority, BaseMixedRealityProfile profile) : base(registrar, name, priority, profile)
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            return;
        }
        
#endif

        audioProfile = ConfigurationProfile as AudioServiceProfile;
        if (audioProfile != null)
        {
            audioClipCache = new Dictionary<AudioId, AudioInfo>();
            playingCache = new Dictionary<Transform, List<PoolableAudioSource>>();
            lastPlayedTimes = new Dictionary<string, DateTime>();
            foreach (var audioInfo in audioProfile.audioClips)
            {
                if (!audioClipCache.ContainsKey(audioInfo.audioId))
                {
                    audioClipCache.Add(audioInfo.audioId,audioInfo);
                }
            }
        }

        objectPooler = ObjectPooler.CreateObjectPool<PoolableAudioSource>(8);

        GetTarget(null);

    }

    public void PlayClip(AudioId audioId, Transform target, float volume)
    {
        if (audioClipCache != null && audioClipCache.ContainsKey(audioId))
        {
            var audioInfo = audioClipCache[audioId];
            PlayClip(audioInfo.clip, target, volume == -1 ? audioInfo.volume : volume);
        }
    }

    public void PlayClip(AudioId audioId, out AudioSource playedSource, Transform target, float volume)
    {
        playedSource = null;
        if (audioClipCache != null && audioClipCache.ContainsKey(audioId))
        {
            var audioInfo = audioClipCache[audioId];
            PlayClip(audioInfo.clip, out playedSource, target, volume == -1 ? audioInfo.volume : volume);
        }
    }

    public void PlayClip(AudioClip clip, Transform target, float volume)
    {
        AudioSource source;
        PlayClip(clip, out source, target, volume);
    }

    public void PlayClip(AudioClip clip, out AudioSource playedSource, Transform target, float volume)
    {
        if (lastPlayedTimes.ContainsKey(clip.name))
        {
            var lastPlayTime = lastPlayedTimes[clip.name];
            if ((DateTime.UtcNow - lastPlayTime).TotalSeconds < SameClipCoolDownTime)
            {
                playedSource = null;
                return;
            }
            
        }
        var source = GetTargetSource(GetTarget(target));
        playedSource = source.AudioSource;
        source.PlayClip(clip);
        lastPlayedTimes[clip.name] = DateTime.UtcNow;
    }

    public bool TryTransitionMixerSnapshot(string name, float transitionTime)
    {
        bool transitioned = false;

        if (audioProfile.musicAudioMixer)
        {
            AudioMixerSnapshot snapshot = audioProfile.musicAudioMixer.FindSnapshot(name);

            if (snapshot)
            {
                snapshot.TransitionTo(transitionTime);
                transitioned = true;
            }
            else
            {
                Debug.LogWarning("Couldn't find AudioMixer Snapshot with name " + name);
            }
        }

        return transitioned;
    }

    private Transform GetTarget(Transform target)
    {
        if (mainCameraTransform == null)
        {
            if (Camera.main != null)
            {
                mainCameraTransform = Camera.main.transform;
            }
        }
        return target == null ? mainCameraTransform : target;
    }

    private PoolableAudioSource GetTargetSource(Transform target)
    {
        PoolableAudioSource source = null;
        List<PoolableAudioSource> sources = null;
        if (playingCache.ContainsKey(target))
        {
            sources = playingCache[target];
            foreach (var poolableAudioSource in sources)
            {
                if (!poolableAudioSource.IsPlaying)
                {
                    source = poolableAudioSource;
                    break;
                }
            }
        }
        else
        {
            sources = new List<PoolableAudioSource>();
        }
        // NULL operator is overriden! will return true if audio source pool is not active!
        if (source == null)
        {
            source = objectPooler.GetNextObject<PoolableAudioSource>(parent:target);
            sources.Add(source);
            source.OnPoolableDestroyed += OnPoolableAudioSourceDestroyed;
        }
        playingCache[target] = sources;
        return source;
    }

    private void OnPoolableAudioSourceDestroyed(APoolable source, Transform parent)
    {
        var poolableAudioSource = source as PoolableAudioSource;
        if (poolableAudioSource == null)
        {
            return;
        }
        if(playingCache.TryGetValue(parent, out var sources))
        {
            sources.Remove(poolableAudioSource);
        }
    }
}
