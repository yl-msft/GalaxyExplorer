// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine;

namespace GalaxyExplorer
{
    public class Hyperlink : MonoBehaviour, IInputHandler, ITouchHandler
    {
        public string URL;

        public event Action Clicked;

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

        public void OnInputDown(InputEventData eventData)
        {
  
        }

        public void OnInputUp(InputEventData eventData)
        {
            if (Clicked != null)
            {
                Clicked();
            }

            if (!string.IsNullOrEmpty(URL))
            {
#if NETFX_CORE
            var uri = new System.Uri(URL);
            var unused = Windows.System.Launcher.LaunchUriAsync(uri);
#else
                Application.OpenURL(URL);
#endif
            }
        }
    }
}