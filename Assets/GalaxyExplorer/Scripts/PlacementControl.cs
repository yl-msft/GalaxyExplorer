// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using UnityEngine;

namespace GalaxyExplorer
{
    public class PlacementControl : MonoBehaviour, IInputClickHandler
    {
        [SerializeField]
        private float DesktopDuration = 2.0f;

        public delegate void ContentPlacedCallback(Vector3 position);
        public ContentPlacedCallback OnContentPlaced;

        private Tagalong volumeTagalong;
        private Interpolator volumeInterpolator;

        private bool isPlaced = false;

        private void Start()
        {
            volumeTagalong = gameObject.GetComponent<Tagalong>();
            volumeInterpolator = gameObject.GetComponent<Interpolator>();

            // if platform is desktop or immersive headset then disable tag along
            if (GalaxyExplorerManager.IsDesktop)
            {
                StartCoroutine(ReleaseContent(DesktopDuration));
                isPlaced = true;
            }

            Animator wireframe = GetComponentInChildren<Animator>();
            wireframe?.SetTrigger("Intro");

            // Position earth pin in front of camera and a bit lower in VR
            if (GalaxyExplorerManager.IsImmersiveHMD)
            {
                gameObject.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * volumeTagalong.TagalongDistance) + Vector3.down * 0.5f;
            }
            // Position earthpin exactly in front of camera in Hololens
            else if (GalaxyExplorerManager.IsHoloLens)
            {
                gameObject.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * volumeTagalong.TagalongDistance);
            }
        }

        private IEnumerator ReleaseContent(float waitingTime)
        {
            // Wait for 1 sec so previous transition finishes
            yield return new WaitForSeconds(waitingTime);

            Animator wireframe = GetComponentInChildren<Animator>();
            wireframe?.SetTrigger("Place");

            // Disable Tagalong and interpolator
            volumeTagalong.enabled = false;
            volumeInterpolator.enabled = false;

            if (OnContentPlaced != null)
            {
                OnContentPlaced.Invoke(transform.position);
            }

            yield return null;
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            if (!isPlaced)
            {
                StartCoroutine(ReleaseContent(1));
            }

            isPlaced = true;
        }
    }
}