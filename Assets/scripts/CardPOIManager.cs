// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GalaxyExplorer
{
    public class CardPOIManager : MonoBehaviour, IMixedRealityPointerHandler
    {
        [Header("Galaxy Card POI Fading")]
        [Tooltip("The time it takes for all points of interest to completely fade out when a card point of interest is selected.")]
        public float POIFadeOutTime = 1.0f;

        [Tooltip("How the opacity changes when all points of interest fade out when a card is selected.")]
        public AnimationCurve POIOpacityCurve;

        private List<PointOfInterest> allPOIs = new List<PointOfInterest>();

        private SpiralGalaxy[] spiralGalaxies;
        private PoiAnimator poiAnimator;

        public void RegisterPOI(PointOfInterest poi)
        {
            if (!allPOIs.Contains(poi))
            {
                allPOIs.Add(poi);
            }
        }

        public void UnRegisterPOI(PointOfInterest poi)
        {
            allPOIs.Remove(poi);
        }

        // Find if a card POI is activa and its card is on/visible
        public bool IsAnyCardActive()
        {
            foreach (var poi in allPOIs)
            {
                if (poi.IsCardActive)
                {
                    return true;
                }
            }

            return false;
        }

        // If a poi card is active then deactivate all poi colliders so user cant activate another one during card presentation
        // This needs to happen in every airtap, mouse click, controller click, keyboard tap so any open magic window card will close
        public IEnumerator UpdateActivationOfPOIColliders(bool waitForEndOfFrame = true)
        {
            if (waitForEndOfFrame)
            {
                yield return new WaitForEndOfFrame();
            }

            if (GalaxyExplorerManager.Instance.TransitionManager.InTransition)
            {
                yield break;
            }

            bool isAnyCardActive = IsAnyCardActive();
            if (spiralGalaxies == null || spiralGalaxies.Length < 1)
            {
                spiralGalaxies = FindObjectsOfType<SpiralGalaxy>();
            }

            if (poiAnimator == null)
            {
                poiAnimator = FindObjectOfType<PoiAnimator>();
            }

            if (isAnyCardActive)
            {
                foreach (var poi in allPOIs)
                {
                    if (poi.IndicatorCollider)
                    {
                        poi.IndicatorCollider.enabled = false;
                    }
                }
                if (spiralGalaxies != null)
                {
                    foreach (var spiralGalaxy in spiralGalaxies)
                    {
                        spiralGalaxy.IsSpinning = false;
                    }

                    if (poiAnimator != null)
                    {
                        poiAnimator.animator.speed = 0;
                    }
                }
            }
            else
            {
                if (spiralGalaxies != null)
                {
                    foreach (var spiralGalaxy in spiralGalaxies)
                    {
                        spiralGalaxy.IsSpinning = true;
                    }
                    if (poiAnimator != null)
                    {
                        poiAnimator.animator.speed = 1;
                    }
                }
            }
        }

        // Find if a card POI is active and its card is on/visible, close the card and trigger audio
        public void CloseAnyOpenCard()
        {
            bool isCardActive = false;

            foreach (var poi in allPOIs)
            {
                if (poi.IsCardActive)
                {
                    isCardActive = true;

                    poi.OnPointerDown(null);
                    break;
                }
            }

            // If any magic window card was active then activate all indicator colliders
            if (isCardActive)
            {
                foreach (var poi in allPOIs)
                {
                    if (poi.IndicatorCollider)
                    {
                        poi.IndicatorCollider.enabled = true;
                    }
                }
            }
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {
        }

        public virtual void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            StartCoroutine(UpdateActivationOfPOIColliders());
        }

        public void OnPointerClicked(MixedRealityPointerEventData eventData)
        {
        }
    }
}