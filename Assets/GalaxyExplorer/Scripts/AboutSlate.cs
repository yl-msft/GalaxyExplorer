// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GalaxyExplorer
{
    public class AboutSlate : MonoBehaviour, IInputClickHandler
    {
        public Material AboutMaterial;
        public GameObject Slate;
        public float TransitionDuration = 1.0f;
        public GEInteractiveToggle AboutDesktopButton = null;
        public GEInteractiveToggle AboutMenuButton = null;

        private bool isAboutButtonClicked = false;
        private GETouchScreenInputSource touchScreenInputSource = null;

        private void Awake()
        {
            DisableLinks();
            AboutMaterial.SetFloat("_TransitionAlpha", 0);

            isAboutButtonClicked = false;
        }

        private void Start()
        {
            InputManager.Instance.AddGlobalListener(gameObject);

            FindObjectOfType<InputRouter>().OnKeyboadSelection += OnKeyboadSelection;

            touchScreenInputSource = FindObjectOfType<GETouchScreenInputSource>();
            touchScreenInputSource.OnTouchStartedDelegate += OnTouchStartedDelegate;

            if (AboutDesktopButton == null)
            {
                Debug.LogWarning("AboutSlate.cs is missing AboutDesktopButton");
            }

            if (AboutDesktopButton)
            {
                Button button = AboutDesktopButton.GetComponent<Button>();
                button.onClick.AddListener(ButtonClicked);
            }
        }

        private void Update()
        {
            isAboutButtonClicked = false;
        }

        // Callback when Desktop About button is clicked/touched/selected
        public void ButtonClicked()
        {
            isAboutButtonClicked = true;
        }

        // AboutSlate needs to receive every screen touch in order to decide to act if AboutSlate is active
        private void OnTouchStartedDelegate(GameObject touchedObject)
        {
            OnInputClicked(null);
        }

        // Is user touching the About slate area
        public bool IsUserTouchingAboutSlate()
        {
            Collider[] allChildren = GetComponentsInChildren<Collider>();
            foreach (var entity in allChildren)
            {
                if (entity.gameObject == touchScreenInputSource.TouchedObject)
                {
                    return true;
                }
            }

            return false;
        }

        // On action triggered by keyboard
        public void OnKeyboadSelection()
        {
            if (AboutDesktopButton && AboutDesktopButton.IsSelected)
            {
                AboutDesktopButton.ToggleLogic();
            }
        }

        // On every user's click, check if the click is outside the about area and if it is and About card is on then deactivate it
        public void OnInputClicked(InputClickedEventData eventData)
        {
            // Check if clicked object is any of the slate object
            bool isAboutSlateSelected = false;
            Collider[] allChildren = GetComponentsInChildren<Collider>();
            foreach (var entity in allChildren)
            {
                if (entity.gameObject == GazeManager.Instance.HitObject)
                {
                    isAboutSlateSelected = true;
                    break;
                }
            }

            bool isTouch = IsUserTouchingAboutSlate();
            bool isButtonSelected = (AboutMenuButton && AboutMenuButton.IsSelected) || (AboutDesktopButton && AboutDesktopButton.IsSelected);

            if (!isAboutSlateSelected && !isTouch && !isAboutButtonClicked && isButtonSelected)
            {
                Debug.Log("User clicked outside About Slate so toggle its button state");

                if (AboutMenuButton && AboutMenuButton.IsSelected)
                {
                    AboutMenuButton.ToggleLogic();
                }
                else if (AboutDesktopButton && AboutDesktopButton.IsSelected)
                {
                    AboutDesktopButton.ToggleLogic();
                }
            }
        }

        private IEnumerator AnimateToOpacity(float target)
        {
            var timeLeft = TransitionDuration;

            DisableLinks();
            Slate.SetActive(true);

            if (TransitionDuration > 0)
            {
                while (timeLeft > 0)
                {
                    Slate.SetActive(true);
                    AboutMaterial.SetFloat("_TransitionAlpha", Mathf.Lerp(target, 1 - target, timeLeft / TransitionDuration));
                    yield return null;

                    timeLeft -= Time.deltaTime;
                }
            }

            AboutMaterial.SetFloat("_TransitionAlpha", target);

            if (target > 0)
            {
                EnableLinks();
                Slate.SetActive(true);
                gameObject.SetActive(true);
            }
            else
            {
                DisableLinks();
                Slate.SetActive(false);
                gameObject.SetActive(false);
            }
        }

        private void EnableLinks()
        {
            var links = GetComponentsInChildren<Hyperlink>(includeInactive: true);
            foreach (var link in links)
            {
                link.gameObject.SetActive(true);
            }
        }

        private void DisableLinks()
        {
            var links = GetComponentsInChildren<Hyperlink>(includeInactive: true);
            foreach (var link in links)
            {
                link.gameObject.SetActive(false);
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);

            EnableLinks();

            StartCoroutine(AnimateToOpacity(1));
        }

        public void Hide()
        {
            if (gameObject.activeSelf)
            {
                StartCoroutine(AnimateToOpacity(0));
            }
        }
    }
}