// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using UnityEngine;

namespace GalaxyExplorer
{
    public class Hyperlink : MonoBehaviour, IMixedRealityInputHandler
    {
        [SerializeField]
        private string URL;

        private float _coolDownDuration = 1f;
        private bool _inCoolDown = false;

        public void OpenURL()
        {
            if (!string.IsNullOrEmpty(URL) && !_inCoolDown)
            {
                Application.OpenURL(URL);

                // Since events are currently fired twice, enforce a cooldown before another link can be clicked
                StartCoroutine(CoolDown());
            }
        }

        public void OnInputDown(InputEventData eventData = null)
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

            eventData.Use();
        }

        private IEnumerator CoolDown()
        {
            _inCoolDown = true;

            yield return new WaitForSeconds(_coolDownDuration);

            _inCoolDown = false;
        }

        private void OnDisable()
        {
            _inCoolDown = false;
        }

        public void OnInputUp(InputEventData eventData)
        {
        }
    }
}