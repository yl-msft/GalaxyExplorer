// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using UnityEngine;

namespace GalaxyExplorer
{
    public class PoiResizer : MonoBehaviour
    {
        public GameObject PoiCard;
        public GameObject PoiAlternateCard;
        public GameObject PoiIndicator;
        public bool movePoiStartingPosition = false;

        private GalaxyExplorerManager geManager = null;

        void Start()
        {
            geManager = FindObjectOfType<GalaxyExplorerManager>();

            if (PoiCard)
            {
                PoiCard.transform.localScale = new Vector3(
                    PoiCard.transform.localScale.x * geManager.GetPoiScaleFactor,
                    PoiCard.transform.localScale.y * geManager.GetPoiScaleFactor,
                    PoiCard.transform.localScale.z);
            }
            if (PoiAlternateCard)
            {
                PoiAlternateCard.transform.localScale = new Vector3(
                    PoiAlternateCard.transform.localScale.x * geManager.GetPoiScaleFactor,
                    PoiAlternateCard.transform.localScale.y * geManager.GetPoiScaleFactor,
                    PoiAlternateCard.transform.localScale.z);
            }
            if (PoiIndicator)
            {
                PoiIndicator.transform.localScale = new Vector3(
                    PoiIndicator.transform.localScale.x * geManager.GetPoiScaleFactor,
                    PoiIndicator.transform.localScale.y * geManager.GetPoiScaleFactor,
                    PoiIndicator.transform.localScale.z);

                //if (ViewLoader.Instance.CurrentView.Equals("SolarSystemView"))
                //{
                //    PointOfInterest poi = GetComponentInParent<PointOfInterest>();
                //    if (poi)
                //    {
                //        poi.IndicatorOffset *= geManager.PoiMoveFactor;
                //    }
                //}

                Transform transformToMove = PoiIndicator.transform;
                transformToMove.localPosition = new Vector3(
                    transformToMove.localPosition.x,
                    transformToMove.localPosition.y * geManager.PoiMoveFactor,
                    transformToMove.localPosition.z);
            }

            if (movePoiStartingPosition)
            {
                transform.localPosition = transform.localPosition * geManager.GetGalaxyScaleFactor;
            }
        }
    }
}