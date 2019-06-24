// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace GalaxyExplorer
{
    public class ForceGrabPOI : PointOfInterest
    {
        [SerializeField] private GameObject Planet;

        [SerializeField] private ForceSolver PlanetForceSolver;

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
            PlanetForceSolver.ResetToRoot();
        }
    }
}