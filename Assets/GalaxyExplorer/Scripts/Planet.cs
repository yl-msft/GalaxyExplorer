// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using UnityEngine;

/// <summary>
/// Planet script is attached to every planet gameobject, the actual sphere of the planet so user is able to aitap, mouse click or touch the planet 
/// </summary>
namespace GalaxyExplorer
{
    public class Planet : MonoBehaviour, IInputClickHandler, IFocusable, IControllerTouchpadHandler
    {
        [SerializeField]
        private PointOfInterest POI = null;

        public void OnFocusEnter()
        {
            POI?.OnFocusEnter();
        }

        public void OnFocusExit()
        {
            POI?.OnFocusExit();
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            POI?.OnInputClicked(eventData);
        }

        public void OnInputPositionChanged(InputPositionEventData eventData)
        {
            POI?.OnInputPositionChanged(eventData);
        }

        public void OnTouchpadReleased(InputEventData eventData)
        {
            POI?.OnTouchpadReleased(eventData);
        }

        public void OnTouchpadTouched(InputEventData eventData)
        {
            POI?.OnTouchpadTouched(eventData);
        }
    }
}