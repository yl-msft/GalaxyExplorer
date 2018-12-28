// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine;

namespace GalaxyExplorer
{
    public class Hyperlink : MonoBehaviour, IInputClickHandler, IControllerTouchpadHandler
    {
        public string URL;

        public event Action Clicked;

        void OnMouseDown()
        {
            OnInputUp(null);
        }

        public void OnHoldCanceled()
        {
        }

        public void OnHoldCompleted()
        {
            OnInputUp(null);
        }

        public void OnHoldStarted()
        {
        }

        public void OnInputPositionChanged(InputPositionEventData eventData)
        {
 
        }

        public void OnInputUp(InputEventData eventData)
        {
            
        }

        public void OnTouchpadReleased(InputEventData eventData)
        {
            OnInputClicked(null);

            eventData.Use();
        }

        public void OnTouchpadTouched(InputEventData eventData)
        {

        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            if (Clicked != null)
            {
                Clicked();
            }

            if (!string.IsNullOrEmpty(URL))
            {
#if NETFX_CORE
                UnityEngine.WSA.Application.InvokeOnUIThread(() =>
                {
                    var uri = new System.Uri(URL);
                    var unused = Windows.System.Launcher.LaunchUriAsync(uri);
                }, false);
#else
                Application.OpenURL(URL);
#endif
            }
        }
    }
}