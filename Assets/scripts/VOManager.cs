// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

namespace GalaxyExplorer
{
    public class VOManager : MonoBehaviour
    {
        [Serializable]
        public class QueuedAudioClip
        {
            public AudioClip clip;
            public float delay;
            public bool allowReplay;
            public bool blockProgress;

            public QueuedAudioClip(AudioClip clip, float delay, bool allowReplay, bool blockProgress)
            {
                this.clip = clip;
                this.delay = delay;
                this.allowReplay = allowReplay;
                this.blockProgress = blockProgress;
            }
        }

        [SerializeField]
        private float FadeOutTime = 2.0f;

        private bool VOEnabled = true;

        private AudioSource audioSource;
        private Queue<QueuedAudioClip> clipQueue = new Queue<QueuedAudioClip>();
        private List<string> playedClips = new List<string>();

        private AudioClip nextClip;
        private float nextClipDelay;
        private float defaultVolume;

        private DateTime playStartTime;
        private float clipLength;

        private IAudioService audioService;
        

        public bool ShouldAudioBlockProgress => DateTime.UtcNow < playStartTime.AddSeconds(clipLength);
        public bool IsPlaying => clipQueue.Count > 0 || nextClip != null || audioSource != null && audioSource.isPlaying;
        public AudioClip CurrentClip => IsPlaying && audioSource != null ? audioSource.clip : null;

        private void Start()
        {
            audioService = MixedRealityToolkit.Instance.GetService<IAudioService>();
        }

        private void Update()
        {
            if (AudioHelper.FadingOut)
            {
                // Don't process any of queue while fading out
                return;
            }

            if (nextClip)
            {
                nextClipDelay -= Time.deltaTime;

                if (nextClipDelay <= 0.0f)
                {
                    // Fading out sets volume to 0, ensure we're playing at the right
                    // volume every time
                    if (audioSource != null)
                    {
                        audioSource.volume = defaultVolume;
                    }
                    audioService.PlayClip(nextClip, out audioSource);
                    nextClip = null;
                    
                }
            }
            else if (clipQueue != null && clipQueue.Count > 0 && (audioSource == null || !audioSource.isPlaying))
            {
                QueuedAudioClip queuedClip = clipQueue.Dequeue();

                if (queuedClip.clip && (queuedClip.allowReplay || !playedClips.Contains(queuedClip.clip.name)))
                {
                    nextClip = queuedClip.clip;
                    nextClipDelay = queuedClip.delay;
                    if (queuedClip.blockProgress)
                    { 
                        playStartTime = DateTime.UtcNow;
                        clipLength = nextClip.length + queuedClip.delay;
                    }
                    else
                    {
                        playStartTime = DateTime.MinValue;
                        clipLength = 0;
                    }

                    playedClips.Add(nextClip.name);
                }
            }
        }

        // Play clip with no delay and dont replace in queue. This is hooked in the editor in FlowManager
        public void PlayClip(AudioClip clip)
        {
            PlayClip(clip, 0.0f, false);
        }

        public bool PlayClip(QueuedAudioClip clip, bool replaceQueue = false)
        {
            return PlayClip(clip.clip, clip.delay, clip.allowReplay, replaceQueue, clip.blockProgress);
        }

        public bool PlayClip(AudioClip clip, float delay = 0.0f, bool allowReplay = false, bool replaceQueue = false, bool audioBlocksProgress = false)
        {
            bool clipWillPlay = false;

            if (VOEnabled)
            {
                if (replaceQueue)
                {
                    clipQueue.Clear();
                }

                clipQueue.Enqueue(new QueuedAudioClip(clip, delay, allowReplay, audioBlocksProgress));

                clipWillPlay = true;
            }

            return clipWillPlay;
        }

        public void Stop(bool clearQueue = false)
        {
            if (clearQueue)
            {
                clipQueue.Clear();
            }

            nextClip = null;
            playStartTime = DateTime.MinValue;
            clipLength = 0;

            // Fade out the audio that's currently playing to stop it. Check here to
            // prevent coroutines from stacking up and calling Stop() on audioSource
            // at undesired times. Audio that would be faded out instead would just
            // be skipped over if the queue was cleared, which is what we want.
            if (!AudioHelper.FadingOut)
            {
                StartCoroutine(AudioHelper.FadeOutOverSeconds(audioSource, FadeOutTime));
            }
        }
    }
}