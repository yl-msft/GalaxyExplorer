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
        [Header("Scene Transitions")]
        [Tooltip("The first time the galaxy appears, this defines how the scene moves into position.")]
        public AnimationCurve IntroTransitionCurveContentChange;
        [Tooltip("The curve that defines how content moves when transitioning from the galaxy to the solar system scene.")]
        public AnimationCurve GalaxyToSSTransitionCurveContentChange;
        [Tooltip("The curve that defines how content moves when transitioning from the solar system to the galaxy.")]
        public AnimationCurve SSToGalaxyTransitionCurveContentChange;
        [Tooltip("The curve that defines how content moves when transitioning from the solar system to a planet or the sun.")]
        public AnimationCurve SSToPlanetTransitionCurveContentChange;
        [Tooltip("The curve that defines how content moves (position and scale only) when transitioning from a planet or the sun to the solar system.")]
        public AnimationCurve PlanetToSSPositionScaleCurveContentChange;
        [Tooltip("The curve that defines how content moves (rotation only) when transitioning from a planet or the sun to the solar system.")]
        public AnimationCurve PlanetToSSRotationCurveContentChange;

        [Header("OpeningScene")]
        [Tooltip("The time it takes to fully transition from one scene opening and getting into position at the center of the cube or room.")]
        public float TransitionTimeOpeningScene = 3.0f;
        [Tooltip("Drives the opacity of the new scene that was loaded in when transitioning backwards.")]
        public AnimationCurve BackTransitionOpacityCurveContentChange;
        [Tooltip("Drives the opacity of the new scene that was loaded when transitioning from planet to solar system view.")]
        public AnimationCurve PlanetToSSTransitionOpacityCurveContentChange;

        [Header("Closing Scene")]
        [Tooltip("How long it takes to completely fade the galaxy scene when transitioning from this scene.")]
        public float GalaxyVisibilityTimeClosingScene = 1.0f;
        [Tooltip("How long it takes to completely fade the solar system scene when transitioning from this scene.")]
        public float SolarSystemVisibilityTimeClosingScene = 1.0f;
        [Tooltip("How long it takes to completely fade a planet or sun scene when transitioning from this scene.")]
        public float PlanetVisibilityTimeClosingScene = 1.0f;
        [Tooltip("Drives the opacity animation for the scene that is closing.")]
        public AnimationCurve OpacityCurveClosingScene;

        [Header("Start Transition")]
        public float StartTransitionTime = 1.0f;
        [Tooltip("Drives the POI opacity animation for the closing scene before content is loaded and starts moving into position.")]
        public AnimationCurve POIOpacityCurveStartTransition;

        [Header("End Transition")]
        [Tooltip("This offset is applied to the time it takes to completely transition, so the end transition can start slightly before content has completely moved into place.")]
        public float EndTransitionTimeOffset = -1.0f;
        [Tooltip("The time it takes for one point of interest to completely fade out and the end of a transition.")]
        public float POIOpacityChangeTimeEndTransition = 1.0f;
        [Tooltip("The time between the previous and next points of interest fading out at the end of a transition.")]
        public float POIOpacityTimeOffsetEndTransition = 0.5f;
        [Tooltip("Drives the POI opacity animation for the opening scene after it has completely moved into place.")]
        public AnimationCurve POIOpacityCurveEndTransition;

        private GameObject prevSceneLoaded;     // tracks the last scene loaded for transitions when loading new scenes

        private bool inTransition = false;

        private ViewLoader ViewLoaderScript = null;
        private GEFadeManager FadeManager = null;
        private ZoomInOut ZoomInOutBehaviour = null;

        private bool isIntro = true;

        public bool IsIntro
        {
            get { return isIntro; }

            set { isIntro = value; }
        }

        public bool InTransition
        {
            get { return inTransition; }
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

            FadeManager = FindObjectOfType<GEFadeManager>();
            FadeManager.OnFadeComplete += OnFadeComplete;

            ZoomInOutBehaviour = FindObjectOfType<ZoomInOut>();
        }

        private void Update()
        {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.Backspace))
            {
                LoadPrevScene();
            }
        }

        // Called when fade is complete
        private void OnFadeComplete(GEFadeManager.FadeType fadeType)
        {

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
                TransformHandler[] parentContent = FindObjectsOfType<TransformHandler>();
                foreach (var parent in parentContent)
                {
                    if (parent.gameObject != prevSceneLoaded)
                    {
                        return parent.gameObject;
                    }
                }
            }

            if (content == null)
            {
                PlacementControl placement = FindObjectOfType<PlacementControl>();
                return (placement) ? placement.gameObject : content;
            }

            return content;
        }

        /// <summary>
        /// When a scene has finished all the functionality to unload this is called from scene transition script
        /// </summary>
        private void OnUnloadComplete()
        {
            Debug.Log("All unload functionality has ended");
        }

        private IEnumerator NextSceneLoadedCoroutine()
        {
            GameObject content = FindContent();
            ZoomInOutBehaviour.ZoomInIsDone = false;
            ZoomInOutBehaviour.ZoomOutIsDone = false;

            // Initialize zoom in and out transition properties
            StartCoroutine(ZoomInOutBehaviour.ZoomInOutInitialization(content, prevSceneLoaded));

            SceneTransition previousTransition = (prevSceneLoaded) ? prevSceneLoaded.GetComponentInChildren<SceneTransition>() : null;
            SceneTransition newTransition = content.GetComponentInChildren<SceneTransition>();

            // In order for the next scene not being visible while the previous is fading, set scale to zero
            if (ZoomInOutBehaviour.NextScene)
            {
                ZoomInOutBehaviour.NextScene.transform.localScale = Vector3.zero;
            }

            bool zoomInOutSimultaneously = newTransition.IsSinglePlanetTransition || (previousTransition && previousTransition.IsSinglePlanetTransition);

            // Zoom in and out simultaneously in case of single planet involved in transition
            if (zoomInOutSimultaneously)
            {
                StartCoroutine(ZoomInOutSimultaneouslyFlow(previousTransition, newTransition));
            }
            // else zoom out the previous scene 
            else
            {
                StartCoroutine(ZoomInOutSeparetlyFlow(previousTransition, newTransition));
            }

            // wait until prev scene transition finishes
            while (!ZoomInOutBehaviour.ZoomOutIsDone)
            {
                yield return null;
            }

            // Unload previous scene
            if (prevSceneLoaded != null)
            {
                UnloadScene(ViewLoader.PreviousView, true);
            }

            // Wait until transition is done
            while (!ZoomInOutBehaviour.ZoomInIsDone)
            {
                yield return null;
            }

            // Fade in pois of next scene
            newTransition.Fade(newTransition.GetComponentInChildren<POIMaterialsFader>(), GEFadeManager.FadeType.FadeIn, POIOpacityChangeTimeEndTransition, POIOpacityCurveEndTransition);

            while (newTransition.IsFading)
            {
                yield return null;
            }

            inTransition = false;

            yield return null;
        }

        // Flow of transition when previous and new scenes zoom in and out at the same time e.g when going to a planet or when leaving a planet
        private IEnumerator ZoomInOutSimultaneouslyFlow(SceneTransition previousTransition, SceneTransition newTransition)
        {
            GameObject singlePlanet = null;
            GameObject relatedPlanet = null;
            GetRelatedPlanets(out relatedPlanet, out singlePlanet);

            // In previous scene, fade out pois and then fade the rest of the scene
            if (previousTransition)
            {
                previousTransition.Fade(previousTransition.GetComponentInChildren<POIMaterialsFader>(), GEFadeManager.FadeType.FadeOut, StartTransitionTime, POIOpacityCurveStartTransition);
                while (previousTransition.IsFading)
                {
                    yield return null;
                }

                float fadeTime = GetClosingSceneVisibilityTime();
                previousTransition.FadeExcept(previousTransition.GetComponentsInChildren<Fader>(), typeof(POIMaterialsFader), null, GEFadeManager.FadeType.FadeOut, fadeTime, OpacityCurveClosingScene);
            }

            // Make invisible one of the two planets that represent the same entity in both scenes
            SetRenderersVisibility(newTransition.IsSinglePlanetTransition ? relatedPlanet : singlePlanet, false);

            // make alpha of pois of next scene equal to zero so they arent visible
            FadeManager.SetAlphaOnFader(newTransition.GetComponentInChildren<POIMaterialsFader>(), 0.0f);

            // if going back to solar system from a planet then fade in solar system
            // Dont fade the material of the selected/related planet in the next scene or any poi
            if (previousTransition && previousTransition.IsSinglePlanetTransition)
            {
                Fader[] allFaders = newTransition.GetComponentsInChildren<Fader>();
                FadeManager.SetAlphaOnFaderExcept(allFaders, typeof(POIMaterialsFader), 0.0f);
                FadeManager.SetAlphaOnFader(relatedPlanet.GetComponent<Fader>(), 1.0f);

                AnimationCurve opacityCurve = newTransition.gameObject.name.Contains("SolarSystemView") ? PlanetToSSTransitionOpacityCurveContentChange : BackTransitionOpacityCurveContentChange;
                newTransition.FadeExcept(allFaders, typeof(POIMaterialsFader), relatedPlanet, GEFadeManager.FadeType.FadeIn, TransitionTimeOpeningScene, opacityCurve);
            }

            StartCoroutine(ZoomInOutBehaviour.ZoomInOutCoroutine(GetContentTransitionCurve(newTransition.gameObject.scene.name), GetContentRotationCurve(newTransition.gameObject.scene.name), GetContentTransitionCurve(newTransition.gameObject.scene.name)));

            yield return null;
        }

        // Flow of transition when previous and new scenes zoom in and out separetly, e.g galaxy to solar scene
        private IEnumerator ZoomInOutSeparetlyFlow(SceneTransition previousTransition, SceneTransition newTransition)
        {
            // Zoom out previous scene. First fade out  pois and when that ends then zoom out previous scene
            if (previousTransition)
            {
                previousTransition.Fade(previousTransition.GetComponentInChildren<POIMaterialsFader>(), GEFadeManager.FadeType.FadeOut, StartTransitionTime, POIOpacityCurveStartTransition);

                while (previousTransition.IsFading)
                {
                    yield return null;
                }

                StartCoroutine(ZoomInOutBehaviour.ZoomOutCoroutine(GetContentRotationCurve(previousTransition.gameObject.scene.name), GetContentTransitionCurve(previousTransition.gameObject.scene.name)));

                // wait until prev scene transition finishes
                while (!ZoomInOutBehaviour.ZoomOutIsDone)
                {
                    yield return null;
                }
            }
            else
            {
                // There is no previous scene so set the flag to true about zooming out previous scene
                ZoomInOutBehaviour.ZoomOutIsDone = true;
            }

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Zoom in new scene and make sure that pois wont be visible just yet. Also fade in spiral galaxy 
            FadeManager.SetAlphaOnFader(newTransition.GetComponentInChildren<POIMaterialsFader>(), 0.0f);
            FadeManager.SetAlphaOnFader(newTransition.GetComponentsInChildren<SpiralGalaxy.SpiralGalaxyFader>(), 0.0f);
            AnimationCurve opacityCurve = newTransition.gameObject.name.Contains("SolarSystemView") ? PlanetToSSTransitionOpacityCurveContentChange : BackTransitionOpacityCurveContentChange;
            newTransition.Fade(newTransition.GetComponentsInChildren<SpiralGalaxy.SpiralGalaxyFader>(), GEFadeManager.FadeType.FadeIn, TransitionTimeOpeningScene, opacityCurve);

            StartCoroutine(ZoomInOutBehaviour.ZoomInCoroutine(GetContentTransitionCurve(newTransition.gameObject.scene.name), GetContentRotationCurve(newTransition.gameObject.scene.name), GetContentTransitionCurve(newTransition.gameObject.scene.name)));

            yield return null;
        }

        private void SetRenderersVisibility(GameObject source, bool isVisible)
        {
            MeshRenderer[] allRenderers = source.GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in allRenderers)
            {
                renderer.enabled = isVisible;
            }
        }

        private void GetRelatedPlanets(out GameObject relatedPlanetObject, out GameObject singlePlanetObject)
        {
            singlePlanetObject = FindObjectOfType<PlanetView>().gameObject;
            relatedPlanetObject = null;

            PlanetPOI[] allPlanets = FindObjectsOfType<PlanetPOI>();
            foreach (var planet in allPlanets)
            {
                // if this scene is loaded from this planet
                if (singlePlanetObject.gameObject.scene.name == planet.GetSceneToLoad)
                {
                    relatedPlanetObject = planet.PlanetObject;
                    break;
                }
            }
        }

        private AnimationCurve GetContentTransitionCurve(string loadedSceneName)
        {
            if (prevSceneLoaded == null)
            {
                return IntroTransitionCurveContentChange;
            }

            if (loadedSceneName.Contains("GalaxyView"))
            {
                return SSToGalaxyTransitionCurveContentChange;
            }

            if (loadedSceneName.Contains("SolarSystemView"))
            {
                if (prevSceneLoaded.name.Contains("GalaxyView"))
                {
                    return GalaxyToSSTransitionCurveContentChange;
                }
                else
                {
                    return PlanetToSSPositionScaleCurveContentChange;
                }
            }

            return SSToPlanetTransitionCurveContentChange;
        }

        private AnimationCurve GetContentRotationCurve(string loadedSceneName)
        {
            if (prevSceneLoaded == null)
            {
                return IntroTransitionCurveContentChange;
            }

            if (loadedSceneName.Contains("GalaxyView"))
            {
                return SSToGalaxyTransitionCurveContentChange;
            }

            if (loadedSceneName.Contains("SolarSystemView"))
            {
                if (prevSceneLoaded.name.Contains("GalaxyView"))
                {
                    return GalaxyToSSTransitionCurveContentChange;
                }
                else
                {
                    return PlanetToSSRotationCurveContentChange;
                }
            }

            return SSToPlanetTransitionCurveContentChange;
        }

        private float GetClosingSceneVisibilityTime()
        {
            if (prevSceneLoaded == null)
            {
                Debug.LogError("TransitionManager: Unable to find the time it takes to fade the last loaded scene because no previous loaded scene was found.");
                return 0.0f;
            }

            if (prevSceneLoaded.gameObject.scene.name.Contains("GalaxyView"))
            {
                return GalaxyVisibilityTimeClosingScene;
            }

            if (prevSceneLoaded.gameObject.scene.name.Contains("SolarSystemView"))
            {
                return SolarSystemVisibilityTimeClosingScene;
            }

            return PlanetVisibilityTimeClosingScene;
        }

    }
}