// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
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
        private PlacementRing _placementRing;
        
        [SerializeField]
        private float DesktopDuration = 2.0f;

        [SerializeField]
        private Animator IntroEarthPlacementAnimator;

        public delegate void ContentPlacedCallback(Vector3 position);

        public ContentPlacedCallback OnContentPlaced;
        public Interactable PlacementConfirmationButton;
        public Transform ConfirmationButtonOffsetTransform;

        private void Awake()
        {
            _confirmationButton = GetComponentInChildren<PlacementConfirmationButton>();
            _forceSolver = GetComponent<ForceSolver>();
            _placementRing = GetComponentInChildren<PlacementRing>();
        }

        private void Start()
        {
            _cameraMain = Camera.main;
            
            IntroEarthPlacementAnimator.SetTrigger("Intro");

            var buttonOffset = ConfirmationButtonOffsetTransform.localPosition;

            switch (GalaxyExplorerManager.Platform)
            {
                case GalaxyExplorerManager.PlatformId.HoloLensGen1:
                    _placementRing.gameObject.SetActive(false);
                    buttonOffset.z = -.2f;
                    break;
                
                case GalaxyExplorerManager.PlatformId.Desktop:
                    _placementRing.gameObject.SetActive(false);
                    StartCoroutine(ReleaseContent(DesktopDuration));
                    isPlaced = true;
                    StartCoroutine(StartOnboarding(true));
                    return;
                
                case GalaxyExplorerManager.PlatformId.ArticulatedHandsPlatform:
                case GalaxyExplorerManager.PlatformId.ImmersiveHMD:
                    buttonOffset.z = -_placementRing.Diameter * .5f;
                    break;
                
                case GalaxyExplorerManager.PlatformId.Phone:
                    //Should never get here
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ConfirmationButtonOffsetTransform.localPosition = buttonOffset;
            
            // if platform is desktop then bypass placement
            if (GalaxyExplorerManager.IsDesktop)
            {
                _placementRing.gameObject.SetActive(false);
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

            GalaxyExplorerManager.Instance.OnboardingManager.OnPlacementConfirmed();
            // have to make sure finished onboarding if vo manager is still fading it out
            while (GalaxyExplorerManager.Instance.VoManager.IsPlaying)
            {
                yield return null;
            }
            yield return new WaitForSeconds(waitingTime);


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
