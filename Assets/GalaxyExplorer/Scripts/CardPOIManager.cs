// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GalaxyExplorer
{
    public class CardPOIManager : MonoBehaviour, IInputClickHandler
    {
        [Header("Galaxy Card POI Fading")]
        [Tooltip("The time it takes for all points of interest to completely fade out when a card point of interest is selected.")]
        public float POIFadeOutTime = 1.0f;
        [Tooltip("How the opacity changes when all points of interest fade out when a card is selected.")]
        public AnimationCurve POIOpacityCurve;

        [Header("Galaxy Card Text Sliding")]
        [Tooltip("The time it takes for the card description to move from its unselected position to its selected position.")]
        public float DescriptionSlideOutTime = 1.0f;
        [Tooltip("The time it takes for the card description to move from its selected position to its unselected/highlight position.")]
        public float DescriptionSlideInTime = 0.5f;
        [Tooltip("The vector (local space) that descripts where the description card moves to when selected.")]
        public Vector3 DescriptionSlideDirection;
        [Tooltip("How the description card moves when it slides to selected and unselected positions.")]
        public AnimationCurve DescriptionSlideCurve;

        private List<PointOfInterest> allPOIs = new List<PointOfInterest>();
        private ToolManager toolsManager = null;


        private void Start()
        {
            InputManager.Instance.AddGlobalListener(gameObject);

            toolsManager = FindObjectOfType<ToolManager>();
            if (toolsManager)
            {
                toolsManager.OnAboutSlateOnDelegate += OnAboutSlateOnDelegate;
            }
        }

        public void RegisterPOI(PointOfInterest poi)
        {
            allPOIs.Add(poi);
        }

        public void UnRegisterPOI(PointOfInterest poi)
        {
            allPOIs.Remove(poi);
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            StartCoroutine(NotifyPOIs());
        }

        private IEnumerator NotifyPOIs()
        {
            yield return new WaitForEndOfFrame();

            // Find if a card POI is activa and its card is on/visible
            bool isAnyCardActive = false;
            foreach (var poi in allPOIs)
            {
                if (poi.IsCardActive)
                {
                    isAnyCardActive = true;
                    break;
                }
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
            }
            else
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

        // When a card poi is on, if About Slate gets activated through menu or desktop button then card poi need to be deactivated
        public void OnAboutSlateOnDelegate(bool enable)
        {
            if (enable)
            {
                // Find if a card POI is activa and its card is on/visible
                bool isAnyCardActive = false;
                foreach (var poi in allPOIs)
                {
                    if (poi.IsCardActive)
                    {
                        isAnyCardActive = true;
                        poi.OnInputUp(null);
                        break;
                    }
                }

                if (isAnyCardActive)
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
        }
    }
}