// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

//using HoloToolkit.Unity.InputModule;

using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

/// <summary>
/// Planet script is attached to every planet gameobject, the actual sphere of the planet so user is able to aitap, mouse click or touch the planet
/// </summary>
namespace GalaxyExplorer
{
    public class Planet : MonoBehaviour, IMixedRealityPointerHandler, IMixedRealityFocusHandler//, IInputClickHandler, IFocusable, IControllerTouchpadHandler
    {
        [SerializeField]
        private PointOfInterest POI = null;

        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {
            if (POI != null && POI.isActiveAndEnabled)
            {
                POI.OnPointerUp(eventData);
            }
        }

        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            if (POI != null && POI.isActiveAndEnabled)
            {
                POI.OnPointerDown(eventData);
            }
        }

        public void OnPointerClicked(MixedRealityPointerEventData eventData)
        {
            if (POI != null && POI.isActiveAndEnabled)
            {
                POI.OnPointerClicked(eventData);
            }
        }

        public void OnBeforeFocusChange(FocusEventData eventData)
        {
            if (POI != null && POI.isActiveAndEnabled)
            {
                POI.OnBeforeFocusChange(eventData);
            }
        }

        public void OnFocusChanged(FocusEventData eventData)
        {
            if (POI != null && POI.isActiveAndEnabled)
            {
                POI.OnFocusChanged(eventData);
            }
        }

        public void OnFocusEnter(FocusEventData eventData)
        {
            if (POI != null && POI.isActiveAndEnabled)
            {
                POI.OnFocusEnter(eventData);
            }
        }

        public void OnFocusExit(FocusEventData eventData)
        {
            if (POI != null && POI.isActiveAndEnabled)
            {
                POI.OnFocusExit(eventData);
            }
        }
    }
}