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
        private ForceSolver _forceSolver;
        
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
            _forceSolver = GetComponent<ForceSolver>();
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
                StartCoroutine(StartOnboarding(true));
                return;
            }

            // Position earth pin in front of camera and a bit lower in VR
            var offset = GalaxyExplorerManager.IsImmersiveHMD ? Vector3.down * .5f : Vector3.zero;

            gameObject.transform.position =
                _cameraMain.transform.position + _cameraMain.transform.forward * 2f + offset;
            
            PlacementConfirmationButton.OnClick.AddListener(ConfirmPlacement);

            StartCoroutine(StartOnboarding());
        }

        private IEnumerator StartOnboarding(bool skipPlacement = false)
        {
            while (!GalaxyExplorerManager.IsInitialized)
            {
                yield return null;
            }
            GalaxyExplorerManager.Instance.OnboardingManager.StartIntro(_forceSolver, skipPlacement);
        }

        private IEnumerator ReleaseContent(float waitingTime)
        {
            IntroEarthPlacementAnimator.SetTrigger("Place");
            
            // Wait for 1 sec so previous transition finishes
            yield return new WaitForSeconds(waitingTime);


            OnContentPlaced?.Invoke(transform.position);
            GalaxyExplorerManager.Instance.OnboardingManager.OnPlacementConfirmed();

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
