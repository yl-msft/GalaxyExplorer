// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the transition between scenes. Has the flow that is followed during transitions
/// Triggers fade, zoom in and out. load and unload.
/// </summary>
namespace GalaxyExplorer
{
    public class TransitionManager : MonoBehaviour
    {
        private GameObject prevSceneLoaded;     // tracks the last scene loaded for transitions when loading new scenes

        private bool inTransition = false;
       
        private ViewLoader ViewLoaderScript = null;

        private bool isIntro = true;

        public bool IsIntro
        {
            get { return isIntro; }

            set { isIntro = value; }
        }

        private void Start()
        {
            ViewLoaderScript = FindObjectOfType<ViewLoader>();

            if (ViewLoaderScript == null)
            {
                Debug.LogError("TransitionManager: No ViewLoader found - unable to process transitions.");
                return;
            }

            IntroFlow intro = FindObjectOfType<IntroFlow>();
            intro.OnIntroFinished += OnIntroFInished;
        }

        // Called when intro flow has finished
        private void OnIntroFInished()
        {
            IsIntro = false;
        }

        public void UnloadScene(string scene, bool keepItOnStack)
        {
            ViewLoaderScript.UnLoadView(scene, keepItOnStack);
        }

        public void LoadPrevScene() 
        {
            if (inTransition)
            {
                Debug.LogWarning("TransitionManager: Currently in a transition and cannot change view to new scene until current transition completes.");
                return;
            }

            inTransition = true;
            prevSceneLoaded = FindContent();
  
            ViewLoaderScript.PopSceneFromStack();
            ViewLoaderScript.LoadPreviousScene(PrevSceneLoaded); 
        }

        private void PrevSceneLoaded(string oldSceneName)
        {
            StartCoroutine(NextSceneLoadedCoroutine());
        }

        public void LoadNextScene(string sceneName)
        {
            LoadNextScene(sceneName, true);
        }

        public void LoadNextScene(string sceneName, bool keepOnStack)
        {
            if (inTransition)
            {
                Debug.LogWarning("TransitionManager: Currently in a transition and cannot change view to '" + sceneName + "' until current transition completes.");
                return;
            }

            if (!keepOnStack)
            {
                ViewLoaderScript.PopSceneFromStack();
            }

            inTransition = true;
            prevSceneLoaded = FindContent();
      
            ViewLoaderScript.LoadViewAsync(sceneName, NextSceneLoaded); 
        }

        private void NextSceneLoaded(string oldSceneName)
        {
            StartCoroutine(NextSceneLoadedCoroutine());
        }

        // Find top parent entity of new scene that is loaded
        private GameObject FindContent()
        {
            GameObject content = null;

            if (content == null)
            {
                PlacementControl placement = FindObjectOfType<PlacementControl>();
                return (placement) ? placement.gameObject : content;
            }

            return content;
        }

        private IEnumerator NextSceneLoadedCoroutine()
        {
            // Unload previous scene
            if (prevSceneLoaded != null)
            {
                UnloadScene(ViewLoader.PreviousView, true);
            }

            inTransition = false;

            yield return null;
        }

    }
}