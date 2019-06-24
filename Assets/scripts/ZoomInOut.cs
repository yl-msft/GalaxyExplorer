// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zoom in and out functionality
/// Affects position, rotation and scale of previous and new scene
/// </summary>
namespace GalaxyExplorer
{
    public class ZoomInOut : MonoBehaviour
    {
        private Transform PreviousScene;
        private SphereCollider PreviousSceneFocusCollider;

        private Transform NextScene;
        private SphereCollider NextSceneFocusCollider;

        [Range(0f, 1f)]
        private float transitionAmount = 0f;

        private float previousFocusInitialScale = 0f;
        private float previousSceneInitialScale = 0f;
        private float nextFocusInitialScale = 0f;
        private float nextSceneInitialScale = 0f;
        private float scalar = 0.0f;

        private Vector3 nextSceneInitialPosition = Vector3.zero; // where focus object was before transition
        private Vector3 previousSceneInitialPosition = Vector3.zero;
        private Vector3 nextSceneDisplacement = Vector3.zero;

        private Quaternion previousSceneRotation = Quaternion.identity;
        private Quaternion previousSceneDiffRotation = Quaternion.identity;
        private Quaternion nextSceneInitialRotation = Quaternion.identity;
        private Quaternion nextInitQuaternion = Quaternion.identity;

        public bool ZoomOutIsDone
        {
            get; set;
        }

        public bool ZoomInIsDone
        {
            get; set;
        }

        public Transform GetNextScene
        {
            get { return NextScene; }
        }


        public IEnumerator ZoomInOutInitialization(GameObject newScene, GameObject prevSceneLoaded)
        {
            if (prevSceneLoaded == null)
            {
                yield break;
            }
            else
            {
                SceneTransition previousTransition = prevSceneLoaded.GetComponentInChildren<SceneTransition>();
                PreviousScene = previousTransition.ThisSceneObject.transform;
                PreviousSceneFocusCollider = previousTransition.ThisSceneFocusCollider;

                SceneTransition newTransition = newScene.GetComponentInChildren<SceneTransition>();
                NextScene = newTransition.ThisSceneObject.transform;
                NextSceneFocusCollider = newTransition.ThisSceneFocusCollider;

                // if new scene is a single planet, select the planet in previous scene that loaded the next scene
                if (newTransition.IsSinglePlanetTransition)
                {
                    GameObject singlePlanet = null;
                    GameObject relatedPlanet = null;
                    GetRelatedPlanets(out relatedPlanet, out singlePlanet);

                    PreviousSceneFocusCollider = relatedPlanet.GetComponentInChildren<SphereCollider>();
                }
                // if previous scene is a single planet then select the planet in next scene that loades this single planet scene
                else if (previousTransition.IsSinglePlanetTransition)
                {
                    GameObject singlePlanet = null;
                    GameObject relatedPlanet = null;
                    GetRelatedPlanets(out relatedPlanet, out singlePlanet);

                    NextSceneFocusCollider = relatedPlanet.GetComponentInChildren<SphereCollider>();
                }
                // if transition is from galaxy to solar system, galaxy to galactiv center and back
                else
                {
                    GameObject previousPlanetPOI = null;
                    GameObject nextPlanetPOI = null;
                    GetRelatedScenes(out previousPlanetPOI, out nextPlanetPOI, previousTransition, newTransition);

                    PreviousSceneFocusCollider = (previousPlanetPOI) ? previousPlanetPOI.GetComponentInChildren<SphereCollider>() : PreviousSceneFocusCollider;
                    NextSceneFocusCollider = (nextPlanetPOI) ? nextPlanetPOI.GetComponentInChildren<SphereCollider>() : NextSceneFocusCollider;
                }

                previousFocusInitialScale = PreviousSceneFocusCollider.radius * PreviousSceneFocusCollider.transform.lossyScale.x;
                previousSceneInitialScale = PreviousScene.localScale.x;
                nextFocusInitialScale = NextSceneFocusCollider.radius * NextSceneFocusCollider.transform.lossyScale.x;
                nextSceneInitialScale = NextScene.localScale.x;

                nextSceneInitialPosition = PreviousSceneFocusCollider.transform.position;
                previousSceneInitialPosition = PreviousScene.transform.position;

                previousSceneRotation = PreviousScene.transform.rotation;
                nextSceneInitialRotation = NextScene.rotation;

                // Should take into account any other parent with rotation
                // previous scene final rotation should be equal to x, and that x makes the previous focus collider rotatio ssame as the initial next focus collider rotation
                // so from the rotation of next focus collider need to remove the rotation of the previous focus collider but without taking nto accont the top parent that we manipulate
                previousSceneDiffRotation = NextSceneFocusCollider.transform.rotation * Quaternion.Inverse(GetRotation(PreviousSceneFocusCollider.gameObject, PreviousScene.gameObject));

                // we want the previous focus collider rotation to much the next focus collider rotation
                // the parent of the next scene should have a rotation that results to the desired next focus collider rotation
                // the next scene's rotations that matter are all parents and focus except the top parent which is the one that we manipulate. 
                nextInitQuaternion = PreviousSceneFocusCollider.transform.rotation * Quaternion.Inverse(GetRotation(NextSceneFocusCollider.gameObject, NextScene.gameObject));

                // next scene focus displacement 
                nextSceneDisplacement = NextSceneFocusCollider.transform.position - NextScene.position;
                nextSceneDisplacement = Quaternion.Inverse(NextSceneFocusCollider.transform.parent.rotation) * nextSceneDisplacement;

                scalar = nextFocusInitialScale / previousFocusInitialScale;
            }

            yield return null;
        }

        public IEnumerator ZoomInOutCoroutine(float duration, AnimationCurve positionCurve, AnimationCurve rotationCurve, AnimationCurve scaleCurve, AnimationCurve extraPositionCurve = null)
        {
            if (PreviousScene == null || NextScene == null || PreviousSceneFocusCollider == null || NextSceneFocusCollider == null)
            {
                ZoomOutIsDone = true;
                ZoomInIsDone = true;
                yield break;
            }

            transitionAmount = 0.0f;

            while (transitionAmount <= 1.0f)
            {
                transitionAmount += Time.deltaTime / duration;

                // Rotate scenes. Previous scene need to rotate around a pivot point which is the previous scene's focus point
                Vector3 noRotPivot = Quaternion.Inverse(PreviousScene.transform.rotation) * (PreviousSceneFocusCollider.transform.position - PreviousScene.position);
                PreviousScene.position += (PreviousScene.rotation * noRotPivot);
                NextScene.transform.rotation = Quaternion.Slerp(nextInitQuaternion, nextSceneInitialRotation, Mathf.Clamp01(rotationCurve.Evaluate(transitionAmount)));
                PreviousScene.transform.rotation = Quaternion.Slerp(previousSceneRotation, previousSceneDiffRotation, Mathf.Clamp01(rotationCurve.Evaluate(transitionAmount)));
                PreviousScene.position -= (PreviousScene.rotation * noRotPivot);

                // Scale scenes. Previous scene's focus point should not move because of scale so need to position the scene back to where it was 
                // before scale in order for its focus point to remain at the same position
                Vector3 posBeforeScale = PreviousSceneFocusCollider.transform.position;
                PreviousScene.localScale = Vector3.one * Mathf.Lerp(previousSceneInitialScale, scalar, Mathf.Clamp01(scaleCurve.Evaluate(transitionAmount)));
                NextScene.localScale = Vector3.one * Mathf.Lerp(nextSceneInitialScale * (1f / scalar), nextSceneInitialScale, Mathf.Clamp01(scaleCurve.Evaluate(transitionAmount)));
                Vector3 newDisplacement = PreviousSceneFocusCollider.transform.position - PreviousScene.position;
                PreviousScene.position = posBeforeScale - newDisplacement;

                // Position scenes. FOr next scene take into account the focus collider pivot as well
                if (extraPositionCurve != null)
                {
                    NextScene.transform.position = Vector3.Lerp(nextSceneInitialPosition - (NextSceneFocusCollider.transform.position - NextScene.position), previousSceneInitialPosition, Mathf.Clamp01(extraPositionCurve.Evaluate(positionCurve.Evaluate(transitionAmount))));
                }
                else
                {
                    NextScene.transform.position = Vector3.Lerp(nextSceneInitialPosition - (NextSceneFocusCollider.transform.position - NextScene.position), previousSceneInitialPosition, Mathf.Clamp01(positionCurve.Evaluate(transitionAmount)));
                }
                
                PreviousScene.transform.position = NextSceneFocusCollider.transform.position - newDisplacement;

                yield return null;
            }

            transitionAmount = 0.0f;
            ZoomOutIsDone = true;
            ZoomInIsDone = true;

            yield return null;
        }

        private void GetRelatedPlanets(out GameObject relatedPlanetObject, out GameObject singlePlanetObject)
        {
            PlanetView planetView = FindObjectOfType<PlanetView>();

            singlePlanetObject = (planetView) ? planetView.gameObject : null;
            relatedPlanetObject = null;

            PlanetPOI[] allPlanets = FindObjectsOfType<PlanetPOI>();
            foreach (var planet in allPlanets)
            {
                // if this scene is loaded from this planet
                if (planetView && (planetView.GetSceneName == planet.GetSceneToLoad))
                {
                    relatedPlanetObject = planet.PlanetObject;
                    break;
                }
            }
        }

        // During transition that doesnt involve single planet so during transition from galaxy to solar or to galactic center
        // we need to identify the poi that spawned the next scene or if going backwards the poi that we are going into
        private void GetRelatedScenes(out GameObject previousRelatedPlanetObjectt, out GameObject nextRelatedPlanetObjectt, SceneTransition previousScene, SceneTransition newScene)
        {
            previousRelatedPlanetObjectt = null;
            nextRelatedPlanetObjectt = null;

            // If a planet of the previous scene loads the new scene then this is the previous focus collider 
            PlanetPOI[] allPreviousPlanets = previousScene.GetComponentsInChildren<PlanetPOI>();
            foreach (var planet in allPreviousPlanets)
            {
                if (planet && planet.GetSceneToLoad == newScene.gameObject.scene.name)
                {
                    previousRelatedPlanetObjectt = planet.PlanetObject;
                    break;
                }
            }

            // If a planet of the previous scene loads the new scene then this is the previous focus collider 
            PlanetPOI[] allNewPlanets = newScene.GetComponentsInChildren<PlanetPOI>();
            foreach (var planet in allNewPlanets)
            {
                if (planet && planet.GetSceneToLoad == previousScene.gameObject.scene.name)
                {
                    nextRelatedPlanetObjectt = planet.PlanetObject;
                    break;
                }
            }
        }

        // Get rotation of child object until the parent object
        private Quaternion GetRotation(GameObject child, GameObject parent)
        {
            Quaternion rotation = Quaternion.identity;
            List<GameObject> allParents = new List<GameObject>();
            allParents.Add(child);
            GameObject thisParent = child.transform.parent.gameObject;

            // Create list of all parents from top to bottom hierarchy
            while (thisParent != parent)
            {
                allParents.Add(thisParent);
                thisParent = thisParent.transform.parent.gameObject;
            }

            for (int i = allParents.Count - 1; i >= 0; --i)
            {
                rotation *= allParents[i].transform.localRotation;
            }

            return rotation;
        }
    }
}

