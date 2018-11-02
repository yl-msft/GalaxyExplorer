// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
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
        private Quaternion nextSceneFocusInitialRotation = Quaternion.identity;
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

                previousFocusInitialScale = PreviousSceneFocusCollider.radius * PreviousSceneFocusCollider.transform.lossyScale.x;
                previousSceneInitialScale = PreviousScene.localScale.x;
                nextFocusInitialScale = NextSceneFocusCollider.radius * NextSceneFocusCollider.transform.lossyScale.x;
                nextSceneInitialScale = NextScene.localScale.x;

                nextSceneInitialPosition = PreviousSceneFocusCollider.transform.position;
                previousSceneInitialPosition = PreviousScene.transform.position;

                previousSceneRotation = PreviousScene.transform.rotation;
                nextSceneFocusInitialRotation = NextSceneFocusCollider.transform.rotation;
                // Should take into account any other parent with rotation
                // I manually calculate the rotation angles as the PreviousSceneFocusCollider.transform.rotation was returning wrong value for some reason
                Vector3 previousSceneDiffAngles = GetRotation(PreviousSceneFocusCollider.gameObject, PreviousScene.gameObject) - PreviousScene.rotation.eulerAngles;
                // Quaternion previousSceneDiffAngles = PreviousSceneFocusCollider.transform.rotation * Quaternion.Inverse(PreviousScene.rotation);
                previousSceneDiffRotation = nextSceneFocusInitialRotation * Quaternion.Inverse(Quaternion.Euler(previousSceneDiffAngles));
                nextSceneInitialRotation = NextScene.rotation;
                Quaternion temp = NextScene.rotation * Quaternion.Inverse(NextSceneFocusCollider.transform.rotation) * NextSceneFocusCollider.transform.localRotation;
                nextInitQuaternion = PreviousSceneFocusCollider.transform.rotation * Quaternion.Inverse(NextSceneFocusCollider.transform.localRotation) * temp;

                // next scene focus displacement 
                nextSceneDisplacement = NextSceneFocusCollider.transform.position - NextScene.position;
                nextSceneDisplacement = Quaternion.Inverse(NextSceneFocusCollider.transform.parent.rotation) * nextSceneDisplacement;

                scalar = nextFocusInitialScale / previousFocusInitialScale;
            }

            yield return null;
        }

        public IEnumerator ZoomOutCoroutine(float duration, AnimationCurve rotationCurve, AnimationCurve scaleCurve)
        {
            if (PreviousScene == null || PreviousSceneFocusCollider == null)
            {
                ZoomOutIsDone = true;
                yield break;
            }

            transitionAmount = 0.0f;

            while (transitionAmount <= 1.0f)
            {
                transitionAmount += Time.deltaTime / duration;

                // Rotate scenes. Previous scene need to rotate around a pivot point which is the previous scene's focus point
                //Vector3 noRotPivot = Quaternion.Inverse(PreviousScene.transform.rotation) * (PreviousSceneFocusCollider.transform.position - PreviousScene.position);
                //PreviousScene.position += (PreviousScene.rotation * noRotPivot);
                PreviousScene.transform.rotation = Quaternion.Slerp(previousSceneRotation, previousSceneDiffRotation, Mathf.Clamp01(rotationCurve.Evaluate(transitionAmount)));
                //PreviousScene.position -= (PreviousScene.rotation * noRotPivot);

                // Scale scenes. Previous scene's focus point should not move because of scale so need to position the scene back to where it was 
                // before scale in order for its focus point to remain at the same position
                //Vector3 posBeforeScale = PreviousSceneFocusCollider.transform.position;
                PreviousScene.localScale = Vector3.one * Mathf.Lerp(previousSceneInitialScale, scalar, Mathf.Clamp01(scaleCurve.Evaluate(transitionAmount)));

                //Vector3 newDisplacement = PreviousSceneFocusCollider.transform.position - PreviousScene.position;
                //PreviousScene.position = posBeforeScale - newDisplacement;

                //PreviousScene.transform.position = Vector3.Lerp(previousSceneInitialPosition, -nextSceneInitialPosition * scalar, TransitionAmount);

                yield return null;
            }

            transitionAmount = 0.0f;
            ZoomOutIsDone = true;

            yield return null;
        }

        public IEnumerator ZoomInCoroutine(float duration, AnimationCurve positionCurve, AnimationCurve rotationCurve, AnimationCurve scaleCurve)
        {
            if (NextScene == null || NextSceneFocusCollider == null)
            {
                ZoomInIsDone = true;
                yield break;
            }

            transitionAmount = 0.0f;

            while (transitionAmount <= 1.0f)
            {
                transitionAmount += Time.deltaTime / duration;

                // Rotate scenes. Previous scene need to rotate around a pivot point which is the previous scene's focus point
                NextScene.transform.rotation = Quaternion.Slerp(nextInitQuaternion, nextSceneInitialRotation, Mathf.Clamp01(rotationCurve.Evaluate(transitionAmount)));

                // Scale scenes. Previous scene's focus point should not move because of scale so need to position the scene back to where it was 
                // before scale in order for its focus point to remain at the same position
                NextScene.localScale = Vector3.one * Mathf.Lerp(nextSceneInitialScale, scalar, Mathf.Clamp01(scaleCurve.Evaluate(transitionAmount))) * (1f / scalar);

                // Position scenes. FOr next scene take into account the focus collider pivot as well
                NextScene.transform.position = Vector3.Lerp(nextSceneInitialPosition, previousSceneInitialPosition, Mathf.Clamp01(positionCurve.Evaluate(transitionAmount)));

                yield return null;
            }

            transitionAmount = 0.0f;
            ZoomInIsDone = true;

            yield return null;
        }

        public IEnumerator ZoomInOutCoroutine(float duration, AnimationCurve positionCurve, AnimationCurve rotationCurve, AnimationCurve scaleCurve)
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
                PreviousScene.localScale = Vector3.one * Mathf.Lerp(1f, scalar, Mathf.Clamp01(scaleCurve.Evaluate(transitionAmount)));
                NextScene.localScale = Vector3.one * Mathf.Lerp(1f, scalar, Mathf.Clamp01(scaleCurve.Evaluate(transitionAmount))) * (1f / scalar);
                Vector3 newDisplacement = PreviousSceneFocusCollider.transform.position - PreviousScene.position;
                PreviousScene.position = posBeforeScale - newDisplacement;

                // Position scenes. FOr next scene take into account the focus collider pivot as well
                //Vector3 nextSceneRotatedDispl = (PreviousSceneFocusCollider.transform.rotation * nextSceneDisplacement) * (1f / scalar);
                NextScene.transform.position = Vector3.Lerp(nextSceneInitialPosition - (NextSceneFocusCollider.transform.position - NextScene.position), previousSceneInitialPosition, Mathf.Clamp01(positionCurve.Evaluate(transitionAmount)));
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

        private Vector3 GetRotation(GameObject child, GameObject parent)
        {
            Vector3 rotation = child.transform.localRotation.eulerAngles + parent.transform.localRotation.eulerAngles;
            GameObject thisParent = child.transform.parent.gameObject;
            while (thisParent != parent)
            {
                rotation += thisParent.transform.localRotation.eulerAngles;
                thisParent = thisParent.transform.parent.gameObject;
            }

            return rotation;
        }
    }
}

