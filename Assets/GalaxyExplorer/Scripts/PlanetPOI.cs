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

        private TransitionManager Transition = null;
        private GameObject planetObject = null;

        public string GetSceneToLoad
        {
            get { return SceneToLoad; }
        }

        public GameObject PlanetObject
        {
            get { return planetObject; }
        }

        protected override void Start()
        {
            base.Start();
            Transition = FindObjectOfType<TransitionManager>();

            // Find planet
            PlanetTransform planet = transform.parent.GetComponentInChildren<PlanetTransform>();
            planetObject = (planet) ? planet.gameObject : null;
        }

        public override void OnInputDown(InputEventData eventData)
        {

        }

        public override void OnInputUp(InputEventData eventData)
        {
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
