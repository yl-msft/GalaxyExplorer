// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

//using HoloToolkit.Unity.InputModule;

using System.Collections;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

/// <summary>
/// Its attached to a poi if the poi is supposed to load a new planet scene when selected
/// </summary>
namespace GalaxyExplorer
{
    public class PlanetPOI : PointOfInterest
    {
        [SerializeField]
        private string SceneToLoad = "";

        [SerializeField]
        private GameObject Planet = null;

        public string GetSceneToLoad
        {
            get { return SceneToLoad; }
        }

        public GameObject PlanetObject
        {
            get { return Planet; }
        }

        protected override void Start()
        {
            base.Start();

            Collider[] allPlanetCollders = (Planet) ? Planet.GetComponentsInChildren<Collider>() : null;
            foreach (var item in allPlanetCollders)
            {
                allPoiColliders.Add(item);
            }
        }

        public override void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            StartCoroutine(OnPointerDownRoutine());
        }

        private IEnumerator OnPointerDownRoutine()
        {
            isCardActive = true;
            yield return StartCoroutine(GalaxyExplorerManager.Instance.CardPoiManager.UpdateActivationOfPOIColliders(false));

            
            // Fade out card description material
            if (CardDescription)
            {
                StartCoroutine(GalaxyExplorerManager.Instance.GeFadeManager.FadeMaterial(CardDescriptionMaterial, GEFadeManager.FadeType.FadeOut, GalaxyExplorerManager.Instance.CardPoiManager.POIFadeOutTime, GalaxyExplorerManager.Instance.CardPoiManager.POIOpacityCurve));
            }

            yield return new WaitForSeconds(.3f);
            GalaxyExplorerManager.Instance.TransitionManager.LoadNextScene(SceneToLoad);
            var poiBehaviors = FindObjectsOfType<POIBehavior>();
            if (poiBehaviors != null)
            {
                foreach (var poiBehavior in poiBehaviors)
                {
                    poiBehavior.enabled = false;
                }
            }
        }

        public override void OnFocusEnter(FocusEventData eventData)
        {
            base.OnFocusEnter(eventData);
        }

        public override void OnFocusExit(FocusEventData eventData)
        {
            base.OnFocusExit(eventData);
        }
    }
}