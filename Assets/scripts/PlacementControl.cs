// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Physics;
using System.Collections;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

namespace GalaxyExplorer
{
    public class PlacementControl : MonoBehaviour
    {
        private bool isPlaced;
        private Camera _cameraMain;
        private PlacementConfirmationButton _confirmationButton;
        
        [SerializeField]
        private float DesktopDuration = 2.0f;

        [SerializeField]
        private Animator IntroEarthPlacementAnimator;

        public delegate void ContentPlacedCallback(Vector3 position);

        public ContentPlacedCallback OnContentPlaced;
        public Interactable PlacementConfirmationButton;

        private void Awake()
        {
            _confirmationButton = GetComponentInChildren<PlacementConfirmationButton>();
        }

        private void Start()
        {
            _cameraMain = Camera.main;
            
            IntroEarthPlacementAnimator.SetTrigger("Intro");
            
            // if platform is desktop then bypass placement
            if (GalaxyExplorerManager.IsDesktop)
            {
                StartCoroutine(ReleaseContent(DesktopDuration));
                isPlaced = true;
                return;
            }

            // Position earth pin in front of camera and a bit lower in VR
            var offset = GalaxyExplorerManager.IsImmersiveHMD ? Vector3.down * .5f : Vector3.zero;

            gameObject.transform.position =
                _cameraMain.transform.position + _cameraMain.transform.forward * 2f + offset;
            
            PlacementConfirmationButton.OnClick.AddListener(ConfirmPlacement);
        }

        private IEnumerator ReleaseContent(float waitingTime)
        {
            // Wait for 1 sec so previous transition finishes
            yield return new WaitForSeconds(waitingTime);

            IntroEarthPlacementAnimator.SetTrigger("Place");

            OnContentPlaced?.Invoke(transform.position);

            yield return null;
        }

        public void ConfirmPlacement()
        {
            if (!isPlaced)
            {
                StartCoroutine(ReleaseContent(0));
            }
            isPlaced = true;
            _confirmationButton.Hide();
        }
    }
}