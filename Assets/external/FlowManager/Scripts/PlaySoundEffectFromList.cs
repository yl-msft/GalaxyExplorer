using UnityEngine;
using System;
//using WWHS;
using System.Collections;

public class PlaySoundEffectFromList : MonoBehaviour
{
    public float Pitch = 1f;
    public float StartingVolume = 1f;
    public bool SpatialSound = true;
    public bool Loop = false;
    public bool PlayOnAwake = false;
    AudioSource audioSource = null;
    public AudioClip[] SoundEffectClip = null;

//    private Tweener fadeTween;
    private Coroutine savedPlaySequence;

    private void Awake()
    {
        // Add an AudioSource component and set up some defaults
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = Loop;
        audioSource.spatialize = true;
        audioSource.dopplerLevel = 0.1f;
        audioSource.pitch = Pitch;
        if (SpatialSound)
        {
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.spatialBlend = 1.0f;
        }
        else
        {
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.spatialBlend = 0f;
        }

        audioSource.volume = StartingVolume;

        if (PlayOnAwake)
            Play(0);
    }

    // Occurs when this object starts colliding with another object
    public void Play(int index)
    {
        Play(index, StartingVolume, Loop);
    }

    public void Play(int index, float volume)
    {
        Play(index, volume, false);
    }
    public void Play(int index, bool looping)
    {
        Play(index, StartingVolume, looping);
    }

    public void Play(int index, float volume, bool looping)
    {
        Play(index, volume, looping, 0);
    }

    public void Play(int index, float volume, bool looping, float delay)
    {
		if (SoundEffectClip.Length > index)
		{
//			Destroy(fadeTween);
			if (savedPlaySequence != null) StopCoroutine(savedPlaySequence);

			audioSource.volume = volume;
			audioSource.clip = SoundEffectClip[index];
			audioSource.loop = looping;
			savedPlaySequence = StartCoroutine(PlaySequence(delay));
		}
	}

    private IEnumerator PlaySequence(float delay)
    {
        yield return new WaitForSeconds(delay);

        audioSource.Play();
    }

    public void FadeOut(float duration)
    {
//        Destroy(fadeTween);
//        fadeTween = Tweener.TweenComponent(audioSource, "volume", 0f, duration: duration);
        audioSource.volume = 0f;
    }

    public void FadeIn(float duration)
    {
        audioSource.volume = StartingVolume; // 0f;
//        Destroy(fadeTween);
//        fadeTween = Tweener.TweenComponent(audioSource, "volume", StartingVolume, duration: duration);
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }
}