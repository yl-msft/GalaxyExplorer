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
        [SerializeField]
        [Tooltip("Duration of Logo stage")]
        private float LogoDuration = 5.0f;


        private IntroFlowState currentState = IntroFlowState.kLogo;
        private float timer = 0.0f;

        private MusicManager musicManagerScript = null;
        private FlowManager flowManagerScript = null;
        private ViewLoader viewLoaderScript = null;
        private Transform sourceTransform = null;
        private VOManager VOManagerScript = null;

        public delegate void IntroFinishedCallback();
        public IntroFinishedCallback OnIntroFinished;


        public enum IntroFlowState
        {
            kLogo,              // Logo
            kEarthPinMR,        // Earth pin stage in Desktop platform
            kEarthPinDesktop,   // Earth pin stage in MR platform
            kSolarView,         // Spawn solar system
            kGalaxyView         // Spawn galaxy view
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
            // if its not Desktop platform then skip the next stage and go directly to solar system stage
            if (GalaxyExplorerManager.IsDesktop)
            {
                flowManagerScript.AdvanceStage();
            }
            else
            {
                flowManagerScript.JumpToStage(3);
            }

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

        private void Update()
        {
            switch (currentState)
            {
                case IntroFlowState.kLogo:
                    timer += Time.deltaTime;

                    if(timer >= LogoDuration)
                    {
                        // if its Desktop platform then jump to earth pin desktop stage
                        if (GalaxyExplorerManager.IsDesktop)
                        {
                            flowManagerScript.JumpToStage(2);
                        }
                        else
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

            if (VOManagerScript == null)
            {
                VOManagerScript = FindObjectOfType<VOManager>();
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
