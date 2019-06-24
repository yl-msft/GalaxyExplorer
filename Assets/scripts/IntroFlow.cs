// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using MRS.FlowManager;
using System.Collections;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

namespace GalaxyExplorer
{
    public class IntroFlow : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Duration of Logo stage")]
        private float LogoDuration = 5.0f;


        private IntroFlowState currentState = IntroFlowState.kNone;
        private float timer = 0.0f;

        private FlowManager flowManagerScript = null;
        private ViewLoader viewLoaderScript = null;
        private IAudioService audioService;

        private const string WelcomeSnapShot = "01_Welcome";
        private const float TransitionTime = 3.8f;

        public delegate void IntroFinishedCallback();
        public IntroFinishedCallback OnIntroFinished;


        public enum IntroFlowState
        {
            kLogo,              // Logo
            kEarthPinMR,        // Earth pin stage in Desktop platform
            kEarthPinDesktop,   // Earth pin stage in MR platform
            kSolarView,         // Spawn solar system
            kGalaxyView,        // Spawn galaxy view
            kNone
        }

        public void OnStageTransition(int timedstage)
        {
            currentState = (IntroFlowState)timedstage;

            // Intro has finished
            if (currentState == IntroFlowState.kGalaxyView && OnIntroFinished != null)
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
            // Anchor the content in place
            FindObjectOfType<WorldAnchorHandler>().CreateWorldAnchor(position);

            // if its not Desktop platform then skip the next stage and go directly to solar system stage
            if (GalaxyExplorerManager.IsDesktop && flowManagerScript)
            {
                flowManagerScript.AdvanceStage();
            }
            else if (!GalaxyExplorerManager.IsDesktop && flowManagerScript)
            {
                flowManagerScript.JumpToStage(3);
            }
        }

        void Start()
        {
            StartCoroutine(Initialization());
        }

        private void Update()
        {
            switch (currentState)
            {
                case IntroFlowState.kLogo:
                    timer += Time.deltaTime;

                    if (timer >= LogoDuration)
                    {
                        // if its Desktop platform then jump to earth pin desktop stage
                        if (GalaxyExplorerManager.IsDesktop && flowManagerScript)
                        {
                            flowManagerScript.JumpToStage(2);
                        }
                        else if (!GalaxyExplorerManager.IsDesktop && flowManagerScript)
                        {
                            flowManagerScript.AdvanceStage();
                        }
                    }

                    break;
            }
        }

        private IEnumerator Initialization()
        {
            // ViewLoader of CoreSystems scene needs to be loaded and then continue
            yield return new WaitUntil(() => FindObjectOfType<ViewLoader>() != null);

            // need to wait otherwise the viewloader subscription to callback becomes null in holoLens
            //yield return new WaitForSeconds(1);
            yield return new WaitForEndOfFrame();
            
            audioService = MixedRealityToolkit.Instance.GetService<IAudioService>();

            PlacementControl placement = FindObjectOfType<PlacementControl>();
            if (placement)
            {
                placement.OnContentPlaced += OnPlacementFinished;
            }

            if (viewLoaderScript == null)
            {
                viewLoaderScript = GalaxyExplorerManager.Instance.ViewLoaderScript;
                if (viewLoaderScript)
                {
                    viewLoaderScript.OnSceneIsLoaded += OnSceneIsLoaded;
                }
            }

            if (flowManagerScript == null)
            {
                flowManagerScript = GalaxyExplorerManager.Instance.FlowManagerHandler;
                if (flowManagerScript)
                {
                    flowManagerScript.enabled = true;
                    flowManagerScript.OnStageTransition += OnStageTransition;
                }
            }

            StartCoroutine(PlayWelcomeMusic());

            yield return null;
        }

        private IEnumerator PlayWelcomeMusic()
        {
            yield return new WaitForEndOfFrame();

            audioService.TryTransitionMixerSnapshot(WelcomeSnapShot, TransitionTime);

            yield return null;
        }
    }
}
