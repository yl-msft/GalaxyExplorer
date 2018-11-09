// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using MRS.FlowManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GalaxyExplorer
{
    public class IntroFlow : MonoBehaviour
    {
        [System.Serializable]
        public class IntroStage
        {
            [SerializeField]
            private AudioClip Audio;

            [SerializeField]
            private float AudioDelay = 0.0f;

            [SerializeField]
            private IntroFlowState Stage;

            public AudioClip GetAudio { get { return Audio; } }

            public float GetAudioDelay { get { return AudioDelay; } }

            public IntroFlowState GetStage { get { return Stage; } }
        }

  
        [SerializeField]
        private List<IntroStage> IntroStages = new List<IntroStage>();

        [SerializeField]
        private VOManager VOManagerScript = null;

        private IntroFlowState currentState = IntroFlowState.kLogo;

        private MusicManager musicManagerScript = null;
        private FlowManager flowManagerScript = null;
        private ViewLoader viewLoaderScript = null;
        private Transform sourceTransform = null;

        public delegate void IntroFinishedCallback();
        public IntroFinishedCallback OnIntroFinished;


        public enum IntroFlowState
        {
            kLogo,
            kEarthPin,
            kSolarView,
            kGalaxyView
        }

        public void OnStageTransition(int timedstage)
        {
            if (timedstage > 0 && timedstage - 1 < IntroStages.Count)
            {
                if (VOManagerScript)
                {
                    VOManagerScript.PlayClip(IntroStages[timedstage - 1].GetAudio, IntroStages[timedstage - 1].GetAudioDelay);
                }
                
                currentState = IntroStages[timedstage - 1].GetStage;
            }

            // Intro has finished
            if (timedstage + 1 == IntroStages.Count && OnIntroFinished != null)
            {
                OnIntroFinished.Invoke();
            }
        }

        public void OnSceneIsLoaded()
        {
            StartCoroutine(Initialization());
        }

        // Position and rotate transform along the WorldAnchor which is the transform that ViewLoader lives
        // After modifying transform, create world anchor
        public void OnPlacementFinished(Vector3 position)
        {
            flowManagerScript.AdvanceStage();

            sourceTransform.position = position;

            // rotate to face camera
            var lookPos = Camera.main.transform.position - position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(-lookPos);
            sourceTransform.rotation = rotation;

            FindObjectOfType<WorldAnchorHandler>().CreateWorldAnchor();
        }

        void Start()
        {
            StartCoroutine(Initialization());
        }

        private IEnumerator Initialization()
        {
            // ViewLoader of CoreSystems scene needs to be loaded and then continue
            yield return new WaitUntil(() => FindObjectOfType<ViewLoader>() != null);

            // need to wait otherwise the viewloader subscription to callback becomes null in holoLens
            //yield return new WaitForSeconds(1);
            yield return new WaitForEndOfFrame();

            PlacementControl placement = FindObjectOfType<PlacementControl>();
            if (placement)
            {
                placement.OnContentPlaced += OnPlacementFinished;
            }

            if (viewLoaderScript == null)
            {
                viewLoaderScript = FindObjectOfType<ViewLoader>();
                if (viewLoaderScript)
                    viewLoaderScript.OnSceneIsLoaded += OnSceneIsLoaded;
            }

            if (flowManagerScript == null)
            {
                FlowManager[] allFlowManagers = Resources.FindObjectsOfTypeAll<FlowManager>();
                flowManagerScript = (allFlowManagers != null && allFlowManagers.Length >= 1) ? allFlowManagers[0] : null;
                if (flowManagerScript)
                {
                    flowManagerScript.enabled = true;
                    flowManagerScript.OnStageTransition += OnStageTransition;
                }
            }

            // World anchor is being created along the gameobject that has ViewLoader
            // so thats the transform that we need to modify when user chooses position of hologram
            if (sourceTransform == null)
            {
                sourceTransform = viewLoaderScript ? viewLoaderScript.transform : null;
            }

            if (musicManagerScript == null)
            {
                musicManagerScript = FindObjectOfType<MusicManager>();

                if (musicManagerScript)
                {
                    StartCoroutine(PlayWelcomeMusic());
                }
            }

            yield return null;
        }

        private IEnumerator PlayWelcomeMusic()
        {
            yield return new WaitForEndOfFrame();

            if (musicManagerScript)
            {
                musicManagerScript.FindSnapshotAndTransition(musicManagerScript.WelcomeTrack);
            }

            yield return null;
        }
    }
}
