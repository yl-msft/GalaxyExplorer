// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
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

        private TransitionManager Transition = null;

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
            Transition = FindObjectOfType<TransitionManager>();
        }

        public override void OnInputDown(InputEventData eventData)
        {

        }

        public override void OnInputUp(InputEventData eventData)
        {
            // Fade out card description material
            if (CardDescription)
            {
                StartCoroutine(geFadeManager.FadeMaterial(CardDescriptionMaterial, GEFadeManager.FadeType.FadeOut, cardPoiManager.POIFadeOutTime, cardPoiManager.POIOpacityCurve));
            }

            if (Transition)
            {
                Transition.LoadNextScene(SceneToLoad, true);
            }
        }

        public override void OnFocusEnter()
        {
            base.OnFocusEnter();
        }

        public override void OnFocusExit()
        {
            base.OnFocusExit();
        }
    }
}
