//-----------------------------------------------------------------------
// <copyright file="AuduiCustomSound.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections;
using UnityEngine;

namespace MRS.Audui
{
    class AuduiCustomSound : MonoBehaviour, IAuduiEventResponder
    {
        [SerializeField]
        private string CustomFocusEvent;
        [SerializeField]
        private string CustomBlurEvent;
        [SerializeField]
        private string CustomActionStartedEvent;
        [SerializeField]
        private string CustomActionEndedEvent;
        [SerializeField]
        private string CustomPrimaryActionEvent;
        [SerializeField]
        private string CustomSecondaryActionEvent;

        [Tooltip("If true, replace the fallback sound, otherwise complement it.")]
        [SerializeField]
        private bool OverrideDefaultSound = true;

        private HoloToolkit.Unity.UAudioManager AudioManager;

        private void OnEnable()
        {
            StartCoroutine(PostEnableSetup());
        }

        private IEnumerator PostEnableSetup()
        {
            // allow one frame for required object to initialize
            yield return null;

            // This component requires an instantiated UAudioManager
            AudioManager = HoloToolkit.Unity.UAudioManager.Instance;
            if (!AudioManager)
            {
                enabled = false;
            }
        }

        private void PlayEvent(AuduiEventData eventData, string eventName)
        {
            // Protect against an event firing on the object before this script gets enabled
            if (!AudioManager)
            {
                return;
            }

            AudioManager.PlayEvent(eventName);

            if (OverrideDefaultSound)
            {
                eventData.Use();
            }
        }

        /// <summary>
        /// IAuduiEventResponder implementation.
        /// </summary>
        /// <param name="eventData"></param>
        public void HandleAuduiEvent(AuduiEventData eventData)
        {
            switch (eventData.action)
            {
                case UiAction.None:
                    break;

                case UiAction.Focus:
                    if (CustomFocusEvent.Length > 0)
                    {
                        PlayEvent(eventData, CustomFocusEvent);
                    }
                    break;

                case UiAction.Blur:
                    if (CustomBlurEvent.Length > 0)
                    {
                        PlayEvent(eventData, CustomBlurEvent);
                    }
                    break;

                case UiAction.ActionStarted:
                    if (CustomActionStartedEvent.Length > 0)
                    {
                        PlayEvent(eventData, CustomActionStartedEvent);
                    }
                    break;

                case UiAction.ActionEnded:
                    if (CustomActionEndedEvent.Length > 0)
                    {
                        PlayEvent(eventData, CustomActionEndedEvent);
                    }
                    break;

                case UiAction.PrimaryAction:
                    if (CustomPrimaryActionEvent.Length > 0)
                    {
                        PlayEvent(eventData, CustomPrimaryActionEvent);
                    }
                    break;

                case UiAction.SecondaryAction:
                    if (CustomSecondaryActionEvent.Length > 0)
                    {
                        PlayEvent(eventData, CustomSecondaryActionEvent);
                    }
                    break;
            }
        }
    }
}
