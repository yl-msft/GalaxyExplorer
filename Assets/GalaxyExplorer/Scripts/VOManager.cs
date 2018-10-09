// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using HoloToolkit.Unity.InputModule;
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

            public QueuedAudioClip(AudioClip clip, float delay)
            {
                this.clip = clip;
                this.delay = delay;
            }
        }

        [SerializeField]
        private float FadeOutTime = 2.0f;

        private bool VOEnabled = true;

        private AudioSource audioSource;
        private Queue<QueuedAudioClip> clipQueue;

        private AudioClip nextClip;
        private float nextClipDelay;
        private float defaultVolume;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            clipQueue = new Queue<QueuedAudioClip>();
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

                if (queuedClip.clip)
                {
                    nextClip = queuedClip.clip;
                    nextClipDelay = queuedClip.delay;
                }
            }
        }

        public bool PlayClip(QueuedAudioClip clip, bool replaceQueue = false)
        {
            return PlayClip(clip.clip, clip.delay, replaceQueue);
        }

        public bool PlayClip(AudioClip clip, float delay = 0.0f, bool replaceQueue = false)
        {
            bool clipWillPlay = false;

            if (VOEnabled)
            {
                if (replaceQueue)
                {
                    clipQueue.Clear();
                }

                clipQueue.Enqueue(new QueuedAudioClip(clip, delay));

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