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

        [Header("Points Of Interest")]
        [Tooltip("The time it takes for one point of interest to completely fade out.")]
        public float PoiFadeOutDuration = 1.0f;
        [Tooltip("Drives the POI opacity animation for the closing scene.")]
        public AnimationCurve POIOpacityCurveStartTransition;
        [Tooltip("The time it takes for one point of interest to completely fade in.")]
        public float PoiFadeInDuration = 2.0f;
        [Tooltip("Drives the POI opacity animation for the opening scene after it has completely moved into place.")]
        public AnimationCurve POIOpacityCurveEndTransition;


        private ViewLoader ViewLoaderScript = null;
        private GEFadeManager FadeManager = null;
        private ZoomInOut ZoomInOutBehaviour = null;
        private GameObject prevSceneLoaded;     // tracks the last scene loaded for transitions when loading new scenes
        private string prevSceneLoadedName = "";

        private bool inTransition = false;
        private bool isFading = false;
        private IntroStage introStage = IntroStage.kInactiveIntro;

        private enum IntroStage
        {
            kActiveIntro,
            kLastStageIntro,
            kInactiveIntro
        }

        public bool InTransition
        {
            get { return inTransition; }
        }

        public bool IsInIntroFlow
        {
            get { return (introStage != IntroStage.kInactiveIntro && introStage != IntroStage.kLastStageIntro); }
        }

        private void Start()
        {
            ViewLoaderScript = FindObjectOfType<ViewLoader>();

            if (ViewLoaderScript == null)
            {
                Debug.LogError("TransitionManager: No ViewLoader found - unable to process transitions.");
                return;
            }

            FadeManager = FindObjectOfType<GEFadeManager>();
            FadeManager.OnFadeComplete += OnFadeComplete;

            ZoomInOutBehaviour = FindObjectOfType<ZoomInOut>();
        }

        // Callback when introduction flow starts. This is hooked up in FlowManager in editor
        public void OnIntroStarted()
        {
            introStage = IntroStage.kActiveIntro;
        }

        // Callback when introduction flow is completed. This is hooked up in FlowManager in editor
        public void OnIntroFinished()
        {
            introStage = IntroStage.kLastStageIntro;
        }

        // Called when fade is complete
        private void OnFadeComplete(GEFadeManager.FadeType fadeType)
        {
            isFading = false;
        }

        public void UnloadScene(string scene, bool keepItOnStack)
        {
            ViewLoaderScript.UnLoadView(scene, keepItOnStack);
        }

        public void LoadPrevScene()
        {
            // Check if there is previous scene to go back to
            if (!ViewLoaderScript.IsTherePreviousScene())
            {
                Debug.LogWarning("TransitionManager: There is NO previous scene to go back to.");
                return;
            }

            if (inTransition)
            {
                Debug.LogWarning("TransitionManager: Currently in a transition and cannot change view to new scene until current transition completes.");
                return;
            }

            inTransition = true;
            prevSceneLoaded = FindContent();
            prevSceneLoadedName = (prevSceneLoaded) ? prevSceneLoaded.name : "";

            ViewLoaderScript.PopSceneFromStack();
            ViewLoaderScript.LoadPreviousScene(PrevSceneLoaded);
        }

        private void PrevSceneLoaded()
        {
            StartCoroutine(NextSceneLoadedCoroutine());
        }

        public void LoadNextScene(string sceneName)
        {
            LoadNextScene(sceneName, true);
        }

        // Load scene that is part of the intro flow so dont keep it in stack
        public void LoadNextIntroScene(string sceneName)
        {
            LoadNextScene(sceneName, false);
        }

        public void LoadNextScene(string sceneName, bool keepOnStack)
        {
            if (inTransition)
            {
                Debug.LogWarning("TransitionManager: Currently in a transition and cannot change view to '" + sceneName + "' until current transition completes.");
                return;
            }

            inTransition = true;
            prevSceneLoaded = FindContent();
            prevSceneLoadedName = (prevSceneLoaded) ? prevSceneLoaded.name : "";

            ViewLoaderScript.LoadViewAsync(sceneName, NextSceneLoaded);

            if (!keepOnStack)
            {
                ViewLoaderScript.PopSceneFromStack();
            }
        }

        private void NextSceneLoaded()
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

        /// <summary>
        /// Callback when next scene is loaded
        /// Has the logic of the flow related to previous and next scene
        /// </summary>
        private IEnumerator NextSceneLoadedCoroutine()
        {
            GameObject nextSceneContent = FindContent();
            ZoomInOutBehaviour.ZoomInIsDone = false;
            ZoomInOutBehaviour.ZoomOutIsDone = false;

            SceneTransition previousTransition = (prevSceneLoaded) ? prevSceneLoaded.GetComponentInChildren<SceneTransition>() : null;
            SceneTransition newTransition = nextSceneContent.GetComponentInChildren<SceneTransition>();

            DeactivateOrbitUpdater(newTransition);

            // Scale new scene to fit inside the volume
            float scaleToFill = FindObjectOfType<TransformSource>().transform.lossyScale.x;
            float targetSize = newTransition.GetScalar(scaleToFill);
            newTransition.transform.GetChild(0).localScale = Vector3.one * targetSize;

            // Initialize zoom in and out transition properties
            StartCoroutine(ZoomInOutBehaviour.ZoomInOutInitialization(nextSceneContent, prevSceneLoaded));

            // In order for the next scene not being visible while the previous is fading, set scale to zero and deactivate all its colliders
            if (ZoomInOutBehaviour.GetNextScene)
            {
                ZoomInOutBehaviour.GetNextScene.transform.localScale = Vector3.zero;
                SetCollidersActivation(ZoomInOutBehaviour.GetNextScene.GetComponentsInChildren<Collider>(), false);
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
                UnloadScene(prevSceneLoaded.scene.name, true);
            }

            // Wait until next scene transition is done
            while (!ZoomInOutBehaviour.ZoomInIsDone)
            {
                yield return null;
            }

            // Fade in pois of next scene
            if (introStage != IntroStage.kActiveIntro)
            {
                isFading = true;
                FadeManager.Fade(newTransition.GetComponentInChildren<POIMaterialsFader>(), GEFadeManager.FadeType.FadeIn, PoiFadeInDuration, POIOpacityCurveEndTransition);
            }

            while (isFading)
            {
                yield return null;
            }

            // Activate colliders of next scene
            if (ZoomInOutBehaviour.GetNextScene)
            {
                SetCollidersActivation(ZoomInOutBehaviour.GetNextScene.GetComponentsInChildren<Collider>(), true);
            }

            inTransition = false;
            introStage = (introStage == IntroStage.kLastStageIntro) ? IntroStage.kInactiveIntro : introStage;

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
                isFading = true;
                FadeManager.Fade(previousTransition.GetComponentInChildren<POIMaterialsFader>(), GEFadeManager.FadeType.FadeOut, PoiFadeOutDuration, POIOpacityCurveStartTransition);
                while (isFading)
                {
                    yield return null;
                }

                isFading = true;
                float fadeTime = GetClosingSceneVisibilityTime();
                FadeManager.FadeExcept(previousTransition.GetComponentsInChildren<Fader>(), typeof(POIMaterialsFader), null, GEFadeManager.FadeType.FadeOut, fadeTime, OpacityCurveClosingScene);
            }

            // Make invisible one of the two planets that represent the same entity in both scenes
            SetRenderersVisibility(newTransition.IsSinglePlanetTransition ? relatedPlanet.transform.parent.gameObject : singlePlanet.transform.parent.gameObject, false);

            // make alpha of pois of next scene equal to zero so they arent visible
            FadeManager.SetAlphaOnFader(newTransition.GetComponentInChildren<POIMaterialsFader>(), 0.0f);

            // if going back to solar system from a planet then fade in solar system
            // Dont fade the material of the selected/related planet in the next scene or any poi
            if (previousTransition && previousTransition.IsSinglePlanetTransition)
            {
                Fader[] allFaders = newTransition.GetComponentsInChildren<Fader>();
                FadeManager.SetAlphaOnFaderExcept(allFaders, typeof(POIMaterialsFader), 0.0f);
                FadeManager.SetAlphaOnFader(relatedPlanet.GetComponent<Fader>(), 1.0f);

                isFading = true;
                AnimationCurve opacityCurve = newTransition.gameObject.name.Contains("SolarSystem") ? PlanetToSSTransitionOpacityCurveContentChange : BackTransitionOpacityCurveContentChange;
                FadeManager.FadeExcept(allFaders, typeof(POIMaterialsFader), relatedPlanet, GEFadeManager.FadeType.FadeIn, TransitionTimeOpeningScene, opacityCurve);
            }

            StartCoroutine(ZoomInOutBehaviour.ZoomInOutCoroutine(TransitionTimeOpeningScene, GetContentTransitionCurve(newTransition.gameObject.scene.name), GetContentRotationCurve(newTransition.gameObject.scene.name), GetContentTransitionCurve(newTransition.gameObject.scene.name)));
            StartCoroutine(LightTransitions(previousTransition, newTransition));

            yield return null;
        }

        // Light transition is necessary in case of Simultaneous transitions in order for sun light position to be the same and transition to look good
        private IEnumerator LightTransitions(SceneTransition previousTransition, SceneTransition newTransition)
        {
            GameObject singlePlanet = null;
            GameObject relatedPlanet = null;
            GetRelatedPlanets(out relatedPlanet, out singlePlanet);

            // if next scene is a planet then position its light to where sun of previous scene is and move it towards its initial position
            if (newTransition && newTransition.IsSinglePlanetTransition)
            {
                SunLightReceiver sunLight = newTransition.GetComponentInChildren<SunLightReceiver>();
                Transform previousSun = FindByName(previousTransition.transform, "Sun");
                //previousTransition.transform.Find("Sun");
                Vector3 initialSunPosition = Vector3.zero;
                if (sunLight && sunLight.Sun && previousSun)
                {
                    initialSunPosition = sunLight.Sun.transform.localPosition;
                    sunLight.Sun.transform.position = singlePlanet.transform.position - (relatedPlanet.transform.position - previousSun.position);

                    float delta = 0.0f;
                    do
                    {
                        delta += Time.deltaTime / TransitionTimeOpeningScene;
                        delta = Mathf.Clamp(delta, 0.0f, 1.0f);
                        sunLight.Sun.transform.localPosition = Vector3.Lerp(sunLight.Sun.transform.localPosition, initialSunPosition, delta);
                        yield return null;
                    } while (delta < 1.0f);
                }
            }
            // in case of previous scene is a single planet scene, find the new scene related planet's SunLightReceivers
            // and replace their sun to a new gameobject pretending to be a sun. This sun;s starting position is 
            // same as the old scene's sun position and transition to new sun's position
            else if (previousTransition && previousTransition.IsSinglePlanetTransition)
            {
                SunLightReceiver[] allLightReceivers = relatedPlanet.GetComponentsInChildren<SunLightReceiver>();
                Transform newSun = FindByName(newTransition.transform, "Sun");
                if (allLightReceivers.Length > 0 && newSun)
                {
                    GameObject lightPlaceholder = new GameObject();
                    lightPlaceholder.transform.parent = newTransition.ThisSceneObject.transform;

                    // Old scene's light position
                    SunLightReceiver oldLight = previousTransition.GetComponentInChildren<SunLightReceiver>();
                    lightPlaceholder.transform.position = (oldLight && oldLight.Sun) ? oldLight.Sun.transform.position : Vector3.zero;

                    // Replace sun to the placeholder object
                    foreach (var light in allLightReceivers)
                    {
                        light.Sun = lightPlaceholder.transform;
                    }

                    // Move placeholder light position towards sun's position
                    float delta = 0.0f;
                    do
                    {
                        delta += Time.deltaTime / TransitionTimeOpeningScene;
                        delta = Mathf.Clamp(delta, 0.0f, 1.0f);
                        lightPlaceholder.transform.position = Vector3.Lerp(lightPlaceholder.transform.position, newSun.transform.position, delta);
                        yield return null;
                    } while (delta < 1.0f);

                    Destroy(lightPlaceholder);
                }
            }
        }

        // Return transform with specific name that lives under a specific entity
        private Transform FindByName(Transform parent, string name)
        {
            Transform child = null;

            for (int i = 0; i < parent.childCount; ++i)
            {
                if (parent.GetChild(i).name.Contains(name))
                {
                    return parent.GetChild(i);
                }
                else if (parent.GetChild(i).childCount > 0)
                {
                    child = FindByName(parent.GetChild(i), name);
                    if (child)
                    {
                        return child;
                    }
                }
            }
            
            return child;
        }

        // Flow of transition when previous and new scenes zoom in and out separetly, e.g galaxy to solar scene
        private IEnumerator ZoomInOutSeparetlyFlow(SceneTransition previousTransition, SceneTransition newTransition)
        {
            // Zoom out previous scene. First fade out  pois and when that ends then zoom out previous scene
            if (previousTransition)
            {
                if (introStage == IntroStage.kInactiveIntro)
                {
                    isFading = true;
                    FadeManager.Fade(previousTransition.GetComponentInChildren<POIMaterialsFader>(), GEFadeManager.FadeType.FadeOut, PoiFadeOutDuration, POIOpacityCurveStartTransition);
                }

                while (isFading)
                {
                    yield return null;
                }

                StartCoroutine(ZoomInOutBehaviour.ZoomOutCoroutine(TransitionTimeOpeningScene * 0.5f, GetContentRotationCurve(newTransition.gameObject.scene.name), GetContentTransitionCurve(newTransition.gameObject.scene.name)));
                FadeManager.FadeExcept(previousTransition.GetComponentsInChildren<Fader>(), typeof(POIMaterialsFader), null, GEFadeManager.FadeType.FadeOut, TransitionTimeOpeningScene * 0.5f, POIOpacityCurveStartTransition);

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

            // Make sure that new scene wont be visible just yet. 
            FadeManager.SetAlphaOnFader(newTransition.GetComponentsInChildren<Fader>(), 0.0f);

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Make sure that new scene pois wont be visible just yet. For some reason, for pois to become invisible it is needed the above 2 frames waiting time
            FadeManager.SetAlphaOnFader(newTransition.GetComponentsInChildren<POIMaterialsFader>(), 0.0f);

            // Fade in scene except pois, and zoom in new scene
            isFading = true;
            AnimationCurve opacityCurve = newTransition.gameObject.name.Contains("SolarSystem") ? PlanetToSSTransitionOpacityCurveContentChange : BackTransitionOpacityCurveContentChange;
            FadeManager.FadeExcept(newTransition.GetComponentsInChildren<Fader>(), typeof(POIMaterialsFader), null, GEFadeManager.FadeType.FadeIn, TransitionTimeOpeningScene * 0.5f, opacityCurve);

            StartCoroutine(ZoomInOutBehaviour.ZoomInCoroutine(TransitionTimeOpeningScene * 0.5f, GetContentTransitionCurve(newTransition.gameObject.scene.name), GetContentRotationCurve(newTransition.gameObject.scene.name), GetContentTransitionCurve(newTransition.gameObject.scene.name)));

            yield return null;
        }

        private void DeactivateOrbitUpdater(SceneTransition newTransition)
        {
            // If going into a single planet then deactivate the previous scene's planet rotation script
            // in order to stop the previous planet moving
            if (newTransition && newTransition.IsSinglePlanetTransition)
            {
                GameObject singlePlanet = null;
                GameObject relatedPlanet = null;
                GetRelatedPlanets(out relatedPlanet, out singlePlanet);

                if (relatedPlanet && relatedPlanet.GetComponentInChildren<OrbitUpdater>())
                {
                    relatedPlanet.GetComponentInChildren<OrbitUpdater>().enabled = false;
                }
                else if (relatedPlanet && relatedPlanet.transform.parent.GetComponent<OrbitUpdater>())
                {
                    relatedPlanet.transform.parent.GetComponent<OrbitUpdater>().enabled = false;
                }
            }
        }

        private void SetCollidersActivation(Collider[] allColliders, bool areActive)
        {
            foreach (var collider in allColliders)
            {
                collider.enabled = areActive;
            }
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
            if (prevSceneLoadedName.CompareTo("") == 0)
            {
                return IntroTransitionCurveContentChange;
            }

            if (loadedSceneName.Contains("GalaxyView"))
            {
                return SSToGalaxyTransitionCurveContentChange;
            }

            if (loadedSceneName.Contains("SolarSystem"))
            {
                if (prevSceneLoadedName.Contains("Galaxy"))
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
            if (prevSceneLoadedName.CompareTo("") == 0)
            {
                return IntroTransitionCurveContentChange;
            }

            if (loadedSceneName.Contains("Galaxy"))
            {
                return SSToGalaxyTransitionCurveContentChange;
            }

            if (loadedSceneName.Contains("SolarSystem"))
            {
                if (prevSceneLoadedName.Contains("Galaxy"))
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
            if (prevSceneLoadedName.CompareTo("") == 0)
            {
                return 0.0f;
            }

            if (prevSceneLoadedName.Contains("Galaxy"))
            {
                return GalaxyVisibilityTimeClosingScene;
            }

            if (prevSceneLoadedName.Contains("SolarSystem"))
            {
                return SolarSystemVisibilityTimeClosingScene;
            }

            return PlanetVisibilityTimeClosingScene;
        }

    }
}