// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

namespace MRS.FlowManager
{
    /// <summary>
    /// FlowManager is a sequencing tool, designed to aid development of smaller-scale or story-led app experiences.
    /// </summary>
    [System.Serializable]
    public class FlowManager : MonoBehaviour
    {
        public int m_currentStage = 0;
        public bool m_restartEnabled;
        public int m_loopBackStage = 0;
        public string m_currentStageName = "none";
        public float m_fastestTapTime = 2f;
        public FlowStage[] m_stages;

        private float m_timeAtTap = -1000f;

        [SerializeField] // So we can access this from the editor
        private float m_timeSinceTap = 10000f;

        public delegate void TransitionEventDelegate(int timedstage);

        public TransitionEventDelegate OnAutoTransition;
        public TransitionEventDelegate OnManualTransition;
        public TransitionEventDelegate OnStageTransition;
        public TransitionEventDelegate OnLoopbackTransition;

        private List<IEnumerator> m_currentEntryEvents = new List<IEnumerator>();

        private T GetComponentInNamedChild<T>(GameObject root, string name)
        {
            foreach (Transform child in root.transform)
            {
                if (child.gameObject.name == name)
                {
                    return child.gameObject.GetComponent<T>();
                }
            }
            T nullComponent = default(T);
            return nullComponent;
        }

        /// <summary>
        /// Initialize and transition to first stage
        /// </summary>
        private void Start()
        {
            m_timeAtTap = Time.time;

            if (m_stages.Length > 0)
            {
                StartCoroutine(WaitForPrerequisitesThenEnter());
            }
        }

        private IEnumerator WaitForPrerequisitesThenEnter()
        {
            while (MixedRealityToolkit.InputSystem == null)
            {
                yield return null;
            }
            EnterStage(0);
        }

        /// <summary>
        /// Handle timers for tap-ignore and autotransition.
        /// </summary>
        private void Update()
        {
            m_timeSinceTap = Time.time - m_timeAtTap;

            // passed final stage? nothing else to do
            if (m_currentStage >= m_stages.Length)
                return;

            // handle autotransition
            if (m_stages[m_currentStage].autoTransitionDelay > 0.0f)
            {
                m_stages[m_currentStage].autoTransitionCounter += Time.deltaTime;
                if (m_stages[m_currentStage].autoTransitionCounter >= m_stages[m_currentStage].autoTransitionDelay)
                {
                    m_stages[m_currentStage].autoTransitionCounter = 0.0f;

                    if (OnAutoTransition != null)
                    {
                        OnAutoTransition.Invoke(m_currentStage + 1);
                    }

                    AdvanceStage();
                }
            }
        }

        /// <summary>
        /// Initiate a user-selected stage transition.
        /// </summary>
        private void RequestTransition()
        {
            // Test for tap-ignore
            m_timeSinceTap = Time.time - m_timeAtTap;
            if (m_timeSinceTap > Mathf.Max(m_fastestTapTime, m_stages[m_currentStage].disableTapTime))
            {
                if (OnManualTransition != null)
                {
                    OnManualTransition.Invoke(m_currentStage + 1);
                }

                AdvanceStage();
            }
        }

        public void AdvanceStage()
        {
            m_timeAtTap = Time.time;

            ExitStage();

            // Evaluate end-of-stage flow
            if (m_currentStage < m_stages.Length - 1)
            {
                // Another stage to go to
                EnterStage(m_currentStage + 1);
            }
            else if (m_currentStage == m_stages.Length - 1)
            {
                // Currently in the final stage

                // If we don't need to restart then we can early-out
                if (!m_restartEnabled)
                {
                    // ensure we don't process this stage again
                    m_currentStage++;
                    return;
                }

                // If we have no exit-events we can restart immediately
                if (m_stages[m_currentStage].ExitEvents == null)
                {
                    Restart();
                    return;
                }

                // Otherwise calculate the time required to fire all the exit-events, then restart
                float combinedDelay = 0;
                foreach (DelayedEvent eventWithDelay in m_stages[m_currentStage].ExitEvents)
                {
                    if (eventWithDelay.Delay > combinedDelay)
                        combinedDelay = eventWithDelay.Delay;
                }
                Invoke("Restart", combinedDelay);
            }
        }

        public void JumpToStage(int targetStage)
        {
            ExitStage();
            if (targetStage < m_stages.Length)
            {
                EnterStage(targetStage);
            }
        }

        /// <summary>
        /// Sets up the new stage.
        /// </summary>
        /// <param name="newStageIdx">The new stage.</param>
        private void EnterStage(int newStageIdx)
        {
            FlowStage newStage = m_stages[newStageIdx];
            m_currentStage = newStageIdx;
            m_currentStageName = newStage.Name;

            if (newStage.clickToAdvance)
            {
                ReceiveTaps(true);
            }
            m_timeAtTap = Time.time;

            newStage.autoTransitionCounter = 0.0f;

            TriggerEntryEvents(m_currentStage);

            if (OnStageTransition != null)
            {
                OnStageTransition.Invoke(m_currentStage);
            }
        }

        /// <summary>
        /// Queue up the entry events for the stage.
        /// </summary>
        /// <param name="_currentStage"></param>
        private void TriggerEntryEvents(int _currentStage)
        {
            if (m_stages[_currentStage].ExitEvents == null)
                return;

            m_currentEntryEvents.Clear();
            foreach (DelayedEvent eventWithDelay in m_stages[_currentStage].Events)
            {
                IEnumerator cr = TriggerEvent(eventWithDelay);
                m_currentEntryEvents.Add(cr);
                StartCoroutine(cr);
            }
        }

        private void ExitStage()
        {
            // Execute any stage events that haven't yet fired;
            // 1. cancel all cued coroutines
            foreach (IEnumerator coroutine in m_currentEntryEvents)
            {
                StopCoroutine(coroutine);
            }
            // 2. manually trigger unfired events
            foreach (DelayedEvent e in m_stages[m_currentStage].Events)
            {
                if (!e.Triggered)
                {
                    e.Triggered = true;
                    e.Event.Invoke();
                }
            }

            // Clear input reception if we used it
            if (m_stages[m_currentStage].clickToAdvance)
            {
                ReceiveTaps(false);
            }

            // Trigger exit events for the stage we are leaving
            TriggerExitEvents(m_currentStage);
        }

        /// <summary>
        /// Queue up the stage exit-events.
        /// </summary>
        /// <param name="currentStage"></param>
        private void TriggerExitEvents(int currentStage)
        {
            if (m_stages[currentStage].ExitEvents == null)
                return;

            foreach (DelayedEvent eventWithDelay in m_stages[currentStage].ExitEvents)
                StartCoroutine(TriggerEvent(eventWithDelay));
        }

        // Invoke an event after a delay.
        private IEnumerator TriggerEvent(DelayedEvent _event)
        {
            yield return new WaitForSeconds(_event.Delay);
            _event.Triggered = true;
            _event.Event.Invoke();
        }

        private void ReceiveTaps(bool receive)
        {
            if (receive)
            {
                MixedRealityToolkit.InputSystem.PushModalInputHandler(gameObject);
            }
            else
            {
                MixedRealityToolkit.InputSystem.PopModalInputHandler();
            }
        }

        /// <summary>
        /// Return to designated starting stage; app is reponsible for its own cleanup and reinit.
        /// </summary>
        public void Restart()
        {
            if (OnLoopbackTransition != null)
            {
                OnLoopbackTransition.Invoke(m_currentStage + 1);
            }

            EnterStage(m_loopBackStage);

            foreach (FlowStage stage in m_stages)
            {
                if (stage.Events != null)
                {
                    foreach (DelayedEvent eventDelayed in stage.Events)
                    {
                        eventDelayed.Triggered = false;
                    }
                }
                if (stage.ExitEvents != null)
                {
                    foreach (DelayedEvent exitEvent in stage.ExitEvents)
                    {
                        exitEvent.Triggered = false;
                    }
                }
            }
        }
    }
}