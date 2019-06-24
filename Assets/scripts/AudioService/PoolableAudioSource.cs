using System.Collections;
using Pools;
using UnityEngine;

public class PoolableAudioSource : APoolable
{
    [SerializeField] public AudioSource audioSource;

    public bool IsPlaying
    {
        get { return audioSource.isPlaying; }
    }

    public override bool IsActive
    {
        get {  return audioSource.isPlaying; }
    }

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    public void PlayClip(
        AudioClip clip, 
        float volume = 1) 
    {
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.time = 0;
        audioSource.Play();
        StartCoroutine(DestroyWithDelay(clip.length));
    }

    private IEnumerator DestroyWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay + .1f);
        Destroy();
    }

    protected override void Reset()
    {
        audioSource.clip = null;
        audioSource.volume = 1;
        audioSource.outputAudioMixerGroup = null;
        base.Reset();
    }
}
