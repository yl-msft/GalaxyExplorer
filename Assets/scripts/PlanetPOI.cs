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

            Collider[] allPlanetCollders = (Planet) ? Planet.GetComponentsInChildren<Collider>() : new Collider[]{};
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
    }
}