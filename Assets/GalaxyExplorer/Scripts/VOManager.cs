// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
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

            public QueuedAudioClip(AudioClip clip, float delay, bool allowReplay)
            {
                this.clip = clip;
                this.delay = delay;
                this.allowReplay = allowReplay;
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

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            defaultVolume = audioSource.volume;
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
                    audioSource.volume = defaultVolume;
                    audioSource.PlayOneShot(nextClip);
                    nextClip = null;
                }
            }
            else if (clipQueue != null && clipQueue.Count > 0 && !audioSource.isPlaying)
            {
                QueuedAudioClip queuedClip = clipQueue.Dequeue();

                if (queuedClip.clip && (queuedClip.allowReplay || !playedClips.Contains(queuedClip.clip.name)))
                {
                    nextClip = queuedClip.clip;
                    nextClipDelay = queuedClip.delay;

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
            return PlayClip(clip.clip, clip.delay, replaceQueue);
        }

        public bool PlayClip(AudioClip clip, float delay = 0.0f, bool allowReplay = false, bool replaceQueue = false)
        {
            bool clipWillPlay = false;

            if (VOEnabled)
            {
                if (replaceQueue)
                {
                    clipQueue.Clear();
                }

                clipQueue.Enqueue(new QueuedAudioClip(clip, delay, allowReplay));

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