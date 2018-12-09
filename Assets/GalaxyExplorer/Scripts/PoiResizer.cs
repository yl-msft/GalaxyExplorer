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

        void Start()
        {
            if (PoiCard)
            {
                PoiCard.transform.localScale = new Vector3(
                    PoiCard.transform.localScale.x * GalaxyExplorerManager.Instance.GetPoiScaleFactor,
                    PoiCard.transform.localScale.y * GalaxyExplorerManager.Instance.GetPoiScaleFactor,
                    PoiCard.transform.localScale.z);
            }
            if (PoiAlternateCard)
            {
                PoiAlternateCard.transform.localScale = new Vector3(
                    PoiAlternateCard.transform.localScale.x * GalaxyExplorerManager.Instance.GetPoiScaleFactor,
                    PoiAlternateCard.transform.localScale.y * GalaxyExplorerManager.Instance.GetPoiScaleFactor,
                    PoiAlternateCard.transform.localScale.z);
            }
            if (PoiIndicator)
            {
                PoiIndicator.transform.localScale = new Vector3(
                    PoiIndicator.transform.localScale.x * GalaxyExplorerManager.Instance.GetPoiScaleFactor,
                    PoiIndicator.transform.localScale.y * GalaxyExplorerManager.Instance.GetPoiScaleFactor,
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
                    transformToMove.localPosition.y * GalaxyExplorerManager.Instance.PoiMoveFactor,
                    transformToMove.localPosition.z);
            }

            if (movePoiStartingPosition)
            {
                transform.localPosition = transform.localPosition * GalaxyExplorerManager.Instance.GetGalaxyScaleFactor;
            }
        }
    }
}