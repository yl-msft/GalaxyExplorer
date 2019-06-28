// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

//using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace GalaxyExplorer
{
    public class POICardDescription : MonoBehaviour//, IInputClickHandler, IFocusable
    {
        [SerializeField]
        private PointOfInterest POI = null;

        public void OnFocusEnter()
        {
            POI?.UpdateCardDescription(true);
        }

        public void OnFocusExit()
        {
            POI?.UpdateCardDescription(false);
        }

//        public void OnInputClicked(InputClickedEventData eventData)
//        {
//            POI?.OnInputClicked(eventData);
//        }
    }
}