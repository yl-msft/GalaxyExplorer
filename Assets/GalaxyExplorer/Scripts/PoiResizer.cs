// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class PoiResizer : MonoBehaviour
    {
        public GameObject PoiCard;
        public GameObject PoiAlternateCard;
        public GameObject PoiIndicator;
        public bool movePoiStartingPosition = false;

        void Start()
        {
            if (PoiCard)
            {
                PoiCard.transform.localScale = new Vector3(
                    PoiCard.transform.localScale.x * GalaxyExplorerManager.PoiScaleFactor,
                    PoiCard.transform.localScale.y * GalaxyExplorerManager.PoiScaleFactor,
                    PoiCard.transform.localScale.z);
            }
            if (PoiAlternateCard)
            {
                PoiAlternateCard.transform.localScale = new Vector3(
                    PoiAlternateCard.transform.localScale.x * GalaxyExplorerManager.PoiScaleFactor,
                    PoiAlternateCard.transform.localScale.y * GalaxyExplorerManager.PoiScaleFactor,
                    PoiAlternateCard.transform.localScale.z);
            }
            if (PoiIndicator)
            {
                PoiIndicator.transform.localScale = new Vector3(
                    PoiIndicator.transform.localScale.x * GalaxyExplorerManager.PoiScaleFactor,
                    PoiIndicator.transform.localScale.y * GalaxyExplorerManager.PoiScaleFactor,
                    PoiIndicator.transform.localScale.z);

                //if (ViewLoader.CurrentView.Equals("SolarSystemView"))
                //{
                //    PointOfInterest poi = GetComponentInParent<PointOfInterest>();
                //    if (poi)
                //    {
                //        poi.IndicatorOffset *= GalaxyExplorerManager.PoiMoveFactor;
                //    }
                //}

                Transform transformToMove = PoiIndicator.transform;
                transformToMove.localPosition = new Vector3(
                    transformToMove.localPosition.x,
                    transformToMove.localPosition.y * GalaxyExplorerManager.PoiMoveFactor,
                    transformToMove.localPosition.z);
            }

            if (movePoiStartingPosition)
            {
                transform.localPosition = transform.localPosition * GalaxyExplorerManager.GalaxyScaleFactor;
            }
        }
    }
}