//-----------------------------------------------------------------------
// <copyright file="AuduiEventWrangler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using UnityEngine;

namespace MRS.Audui
{
    /// <summary>
    /// The Wrangler registers as a global input listener and focus observer,
    /// and translates relevent input events into 'UI Actions' that can be
    /// handled and consumed independently of the originating event.
    /// </summary>
    public class AuduiEventWrangler : MonoBehaviour, IMixedRealityPointerHandler//IInputHandler, IInputClickHandler
    {
        /// <summary>
        /// A bank of Inspector settings.
        /// </summary>
        public string DefaultFocusEvent = "Default_Focus";

        public string DefaultBlurEvent;
        public string DefaultActionStartedEvent;
        public string DefaultActionEndedEvent;
        public string DefaultPrimaryActionEvent = "Default_Primary";
        public string DefaultSecondaryActionEvent;

        [SerializeField] private AudioClip defaultFocus;
        [SerializeField] private AudioClip defaultPrimary;

        private IAudioService<AudioId> AudioManager;
        private GameObject FocusedObject = null;

        private void OnEnable()
        {
            StartCoroutine(PostEnableSetup());
        }

        private IEnumerator PostEnableSetup()
        {
            // allow one frame for required objects to initialize
            yield return null;

            // Audui requires an instantiated UAudioManager, FocusManager and InputManager;
            AudioManager = MixedRealityToolkit.Instance.GetService<IAudioService<AudioId>>();
            if (AudioManager != null)
            {
                // if we have all three, set up as required
                //                FocusManager.Instance.FocusEntered += OnFocusEnter;
                //                FocusManager.Instance.FocusExited += OnFocusExit;
            }
            else
            {
                Debug.LogWarning("AuduiEventWrangler: could not access all three required companion components; disabling.");
                enabled = false;
            }
        }

        private void OnDisable()
        {
            if (AudioManager != null)
            {
                //                FocusManager.Instance.FocusExited -= OnFocusExit;
                //                FocusManager.Instance.FocusEntered -= OnFocusEnter;
                AudioManager = null;
            }
        }

        private void HandleAuduiEvent(UiAction action)
        {
            if (!FocusedObject)
            {
                return;
            }

            //            Debug.Log("AuduiEventWrangler: HandleAuduiEvent " + action.ToString());

            var eventData = new AuduiEventData(action);

            // Pass the event to a responder on the focused object; bubble up if required
            Transform targetTfrm = FocusedObject.transform;
            while (targetTfrm)
            {
                IAuduiEventResponder[] responders = targetTfrm.GetComponents<IAuduiEventResponder>();
                if (responders.Length > 0)
                {
                    // Note: multiple responders all get a shot at the event; we don't consider
                    // the event used flag here due to the issue of script execution order
                    for (int i = 0; i < responders.Length; ++i)
                    {
                        responders[i].HandleAuduiEvent(eventData);
                    }
                    //                    Debug.Log("  passed to: " + targetTfrm.gameObject.name);
                    break;
                }
                targetTfrm = targetTfrm.parent;
            }

            if (eventData.used)
            {
                // Event was consumed, do not locally handle.
                //                Debug.Log("  event consumed by target");
                return;
            }

            switch (eventData.action)
            {
                case UiAction.None:
                    break;

                    //                case UiAction.Focus:
                    //                    if (DefaultFocusEvent.Length > 0)
                    //                    {
                    //                        AudioManager.PlayEvent(DefaultFocusEvent);
                    //                    }
                    //                    break;
                    //
                    //                case UiAction.Blur:
                    //                    if (DefaultBlurEvent.Length > 0)
                    //                    {
                    //                        AudioManager.PlayEvent(DefaultBlurEvent);
                    //                    }
                    //                    break;
                    //
                    //                case UiAction.ActionStarted:
                    //                    if (DefaultActionStartedEvent.Length > 0)
                    //                    {
                    //                        AudioManager.PlayEvent(DefaultActionStartedEvent);
                    //                    }
                    //                    break;
                    //
                    //                case UiAction.ActionEnded:
                    //                    if (DefaultActionEndedEvent.Length > 0)
                    //                    {
                    //                        AudioManager.PlayEvent(DefaultActionEndedEvent);
                    //                    }
                    //                    break;
                    //
                    //                case UiAction.PrimaryAction:
                    //                    if (DefaultPrimaryActionEvent.Length > 0)
                    //                    {
                    //                        AudioManager.PlayEvent(DefaultPrimaryActionEvent);
                    //                    }
                    //                    break;
                    //
                    //                case UiAction.SecondaryAction:
                    //                    if (DefaultSecondaryActionEvent.Length > 0)
                    //                    {
                    //                        AudioManager.PlayEvent(DefaultSecondaryActionEvent);
                    //                    }
                    //                    break;
            }
        }

        /// <summary>
        /// FocusManager delegate.
        /// Always set our FocusedObject; only handle Focus for UI elements.
        /// </summary>
        /// <param name="focusedObject"></param>
        public void OnFocusEnter(GameObject focusedObject)
        {
            FocusedObject = focusedObject.layer == LayerMask.NameToLayer("UI") ? focusedObject : null;
            // we only need to handle focus for UI objects
            if (FocusedObject)
            {
                HandleAuduiEvent(UiAction.Focus);
            }
        }

        /// <summary>
        /// FocusManager delegate.
        /// Only handle Blur for UI elements; always clear our FocusedObject.
        /// </summary>
        /// <param name="focusedObject"></param>
        public void OnFocusExit(GameObject focusedObject)
        {
            // we only need to handle focus for UI objects
            if (focusedObject.layer == LayerMask.NameToLayer("UI"))
            {
                HandleAuduiEvent(UiAction.Blur);
            }
            FocusedObject = null;
        }

        /// <summary>
        /// Specify a desired focused object, or clear the focused object, without raising OnFocusEnter/OnFocusExit audio events
        /// </summary>
        /// <param name="focusedObject">Object to focus on, or null</param>
        public void OverrideFocusedObject(GameObject focusedObject)
        {
            FocusedObject = (focusedObject && focusedObject.layer == LayerMask.NameToLayer("UI")) ? focusedObject : null;
        }

        /// <summary>
        /// IInputHandler implementation.
        /// Raise an 'ActionStarted' event.
        /// </summary>
        /// <param name="eventData"></param>
        //        public void OnInputDown(InputEventData eventData)
        //        {
        //            HandleAuduiEvent(UiAction.ActionStarted);
        //        }
        //
        //        /// <summary>
        //        /// IInputHandler implementation.
        //        /// Raise an 'ActionEnded' event.
        //        /// </summary>
        //        /// <param name="eventData"></param>
        //        public void OnInputUp(InputEventData eventData)
        //        {
        //            HandleAuduiEvent(UiAction.ActionEnded);
        //        }
        //
        //        /// <summary>
        //        /// IInputClickHandler implementation.
        //        /// Raise a 'PrimaryAction' event.
        //        /// </summary>
        //        /// <param name="eventData"></param>
        //        public void OnInputClicked(InputClickedEventData eventData)
        //        {
        //            HandleAuduiEvent(UiAction.PrimaryAction);
        //        }

        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {
            HandleAuduiEvent(UiAction.ActionEnded);
        }

        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            HandleAuduiEvent(UiAction.ActionStarted);
        }

        public void OnPointerClicked(MixedRealityPointerEventData eventData)
        {
            HandleAuduiEvent(UiAction.PrimaryAction);
        }
    }
}