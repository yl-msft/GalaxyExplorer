// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class LocationView : MonoBehaviour
    {
        [Header("Background Music")]
        [SerializeField]
        private string MusicEvent = "";

        [SerializeField]
        private float MusicDelayInSeconds = 1.0f;

        [Header("Voice Over")]
        [SerializeField]
        private VOManager.QueuedAudioClip VoiceOver;

        private bool playMusic = true;
        private float delayTimer = 0.0f;

        private MusicManager musicManager = null;
        private TransitionManager transitionManager = null;

        void Start()
        {
            delayTimer = MusicDelayInSeconds;
            musicManager = FindObjectOfType<MusicManager>();
            transitionManager = FindObjectOfType<TransitionManager>();

            VOManager voManager = FindObjectOfType<VOManager>();
            if (voManager && !transitionManager.IsInIntroFlow)
            {
                voManager.Stop(true);
                voManager.PlayClip(VoiceOver);
            }
        }

        void Update()
        {
            if (playMusic && !transitionManager.IsInIntroFlow)
            {
                delayTimer -= Time.deltaTime;
                if (delayTimer <= 0.0f)
                {
                    musicManager.FindSnapshotAndTransition(MusicEvent);
                    playMusic = false;
                }
            }
        }
    }
}
