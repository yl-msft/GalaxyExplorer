// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using TMPro;
using UnityEngine;
using static GalaxyExplorer.GEFadeManager;

namespace GalaxyExplorer
{
    public class GGVMenuManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _resetButton;

        [SerializeField]
        private GameObject _backButton;

        [SerializeField]
        public GameObject RaiseButton;

        [SerializeField]
        public GameObject LowerButton;

        public float MinZoom = 0.15f;
        public float LargestZoom = 3.0f;

        [SerializeField]
        private AnimationCurve toolsOpacityChange = null;

        [SerializeField]
        private float FadeToolsDuration = 1.0f;

        [HideInInspector]
        public bool ToolsVisible = false;

        private ToolPanel _menuParent;
        private TextMeshPro[] GGVMenuTextComponents;
        private Vector3 _defaultBackButtonLocalPosition;
        private float _fullMenuVisibleBackButtonX;

        public void SetMenuVisibility(bool show, bool resetIsActive, bool backIsActive)
        {
            if (show)
            {
                UpdateButtonsActive(resetIsActive, backIsActive);
                ShowMenu();
            }
            else
            {
                HideMenu();
            }
        }

        private void ShowMenu()
        {
            _menuParent.gameObject.SetActive(true);
            ToolsVisible = true;

            Fader[] allToolFaders = GetComponentsInChildren<Fader>();
            GalaxyExplorerManager.Instance.GeFadeManager.Fade(allToolFaders, GEFadeManager.FadeType.FadeIn, FadeToolsDuration, toolsOpacityChange);
        }

        private void HideMenu()
        {
            ToolsVisible = false;
            SetVisibleTextLabels(false);

            Fader[] allToolFaders = GetComponentsInChildren<Fader>();
            GalaxyExplorerManager.Instance.GeFadeManager.Fade(allToolFaders, GEFadeManager.FadeType.FadeOut, FadeToolsDuration, toolsOpacityChange);

            _menuParent.gameObject.SetActive(false);
        }

        private void UpdateButtonsActive(bool resetIsActive, bool backIsActive)
        {
            if (resetIsActive && !_resetButton.activeSelf)
            {
                // When the POIPlanetFocusManager is present in the currently loaded scenes, this means we are in the solar system and the reset button should be visible
                _resetButton.SetActive(true);
                _backButton.transform.localPosition = new Vector3(_fullMenuVisibleBackButtonX, 0f, 0f);
            }
            else if (!resetIsActive && _resetButton.activeSelf)
            {
                // When the POIPlanetFocusManager isn't present in the currently loaded scenes, this means we're not in the solar system and the reset button shouldn't show up
                _resetButton.SetActive(false);
                _backButton.transform.localPosition = _defaultBackButtonLocalPosition;
            }

            // If there is previous scene then user is able to go back so activate the back button
            _backButton?.SetActive(backIsActive);
        }

        private void OnFadeComplete(FadeType type)
        {
            if (ToolsVisible)
            {
                SetVisibleTextLabels(true);
            }
        }

        private void SetVisibleTextLabels(bool isVisible)
        {
            if (GGVMenuTextComponents == null)
            {
                _menuParent = GetComponentInChildren<ToolPanel>(true) as ToolPanel;

                GGVMenuTextComponents = _menuParent.GetComponentsInChildren<TextMeshPro>(true);

                if (GGVMenuTextComponents == null)
                {
                    Debug.LogWarning("GGVMenuTextComponents not found");
                    return;
                }
            }

            foreach (TextMeshPro text in GGVMenuTextComponents)
            {
                text.enabled = isVisible;
            }
        }

        private void Start()
        {
            _menuParent = GetComponentInChildren<ToolPanel>(true) as ToolPanel;

            GGVMenuTextComponents = _menuParent.GetComponentsInChildren<TextMeshPro>(true);

            SetVisibleTextLabels(false);
            GalaxyExplorerManager.Instance.GeFadeManager.OnFadeComplete += OnFadeComplete;

            RaiseButton.SetActive(false);
            _resetButton.SetActive(false);
            _backButton.SetActive(false);

            SetMenuVisibility(false, false, false);
            ToolsVisible = false;

            // Store the x value of the local position for the back button when all menu buttons are visible
            _fullMenuVisibleBackButtonX = _backButton.transform.localPosition.x;

            // Since reset is not visible during most of the app states, regard its local position as the default back button local position
            _defaultBackButtonLocalPosition = _resetButton.transform.localPosition;

            // Since the app starts with reset button not visible, move the back button to its spot instead
            _backButton.transform.localPosition = _defaultBackButtonLocalPosition;
        }

        private void OnDestroy()
        {
            if (GalaxyExplorerManager.Instance && GalaxyExplorerManager.Instance.GeFadeManager != null)
            {
                GalaxyExplorerManager.Instance.GeFadeManager.OnFadeComplete -= OnFadeComplete;
            }
        }

        public void LowerTools()
        {
            _menuParent.IsLowered = true;

            if (RaiseButton && LowerButton)
            {
                RaiseButton.SetActive(true);
                LowerButton.SetActive(false);
            }
        }

        public void RaiseTools()
        {
            _menuParent.IsLowered = false;

            if (RaiseButton && LowerButton)
            {
                RaiseButton.SetActive(false);
                LowerButton.SetActive(true);
            }
        }
    }
}