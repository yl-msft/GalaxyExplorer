// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GalaxyExplorer
{
    public class AboutSlate : MonoBehaviour//, IInputClickHandler, IControllerTouchpadHandler
    {
        public Material AboutMaterial;
        public GameObject Slate;
        public float TransitionDuration = 1.0f;
        public GameObject AboutDesktopButton = null;
        public GameObject AboutMenuButton = null;

        //        private bool isAboutButtonClicked = false;

        private void Awake()
        {
            DisableLinks();
            AboutMaterial.SetFloat("_TransitionAlpha", 0);

            //            isAboutButtonClicked = false;

            transform.localScale = transform.localScale * GalaxyExplorerManager.SlateScaleFactor;
        }

        private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
        {
            Hide();
        }

        private void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        private void Start()
        {
            MixedRealityToolkit.InputSystem.Register(gameObject);

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

        //        private void Update()
        //        {
        //            isAboutButtonClicked = false;
        //        }

        // Callback when Desktop About button is clicked/touched/selected
        public void ButtonClicked()
        {
            //            isAboutButtonClicked = true;
        }

        // Is user touching the About slate area
        public bool IsUserTouchingAboutSlate()
        {
            Collider[] allChildren = GetComponentsInChildren<Collider>();
            foreach (var entity in allChildren)
            {
                //                if (entity.gameObject == InputManager.Instance.OverrideFocusedObject)
                //                {
                //                    return true;
                //                }
            }

            return false;
        }

        // Has user clicked the About slate area
        public bool IsClickOnAboutSlate(GameObject hitObject)
        {
            // Check if clicked object is the menu button or desktop button
            if (AboutMenuButton && AboutMenuButton.gameObject == hitObject)
            {
                return true;
            }
            else if (AboutDesktopButton && AboutDesktopButton.gameObject == hitObject)
            {
                return true;
            }

            // Check if clicked object is any of the slate object
            Collider[] allChildren = GetComponentsInChildren<Collider>();
            foreach (var entity in allChildren)
            {
                if (entity.gameObject == hitObject)
                {
                    return true;
                }
            }

            return false;
        }

        // On every user's click, check if the click is outside the about area and if it is and About card is on then deactivate it
        //        public void OnInputClicked(InputClickedEventData eventData)
        //        {
        // Focused object is either the gazed one in hololens or the overriden my mouse click one for desktop
        //            GameObject focusedObject = (FocusManager.Instance) ? FocusManager.Instance.TryGetFocusedObject(eventData) : null;
        //            focusedObject = (focusedObject == null) ? InputManager.Instance.OverrideFocusedObject : focusedObject;

        //            ToggleAboutSlateLogic(IsClickOnAboutSlate(focusedObject));
        //        }

        //        private void ToggleAboutSlateLogic(bool isAboutSelected)
        //        {
        //            bool isButtonSelected = (AboutMenuButton && AboutMenuButton.IsSelected) || (AboutDesktopButton && AboutDesktopButton.IsSelected);
        //
        //            if (!isAboutSelected && !isAboutButtonClicked && isButtonSelected)
        //            {
        //                Debug.Log("User clicked outside About Slate so toggle its button state");
        //
        //                if (AboutMenuButton && AboutMenuButton.IsSelected)
        //                {
        //                    AboutMenuButton.ToggleLogic();
        //                }
        //                else if (AboutDesktopButton && AboutDesktopButton.IsSelected)
        //                {
        //                    AboutDesktopButton.ToggleLogic();
        //                }
        //            }
        //        }

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
            //            var links = GetComponentsInChildren<Hyperlink>(includeInactive: true);
            //            foreach (var link in links)
            //            {
            //                link.gameObject.SetActive(true);
            //            }
        }

        private void DisableLinks()
        {
            //            var links = GetComponentsInChildren<Hyperlink>(includeInactive: true);
            //            foreach (var link in links)
            //            {
            //                link.gameObject.SetActive(false);
            //            }
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

        //        public void OnTouchpadTouched(InputEventData eventData)
        //        {
        //
        //        }
        //
        //        public void OnTouchpadReleased(InputEventData eventData)
        //        {
        //            ToggleAboutSlateLogic(IsUserTouchingAboutSlate());
        //        }
        //
        //        public void OnInputPositionChanged(InputPositionEventData eventData)
        //        {
        //
        //        }
    }
}