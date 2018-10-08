// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.Events;

namespace MRS.FlowManager
{
    /// <summary>
    /// A wrapped UnityEvent that can trigger after a delay.
    /// </summary>
    [System.Serializable]
    public class DelayedEvent
    {
        public string Name;
        public float Delay = 0f;
        [SerializeField]
        public UnityEvent Event;
        public bool Triggered = false;

        public DelayedEvent()
        {
        }

        public DelayedEvent(string _name, float _delay, UnityEvent _event)
        {
            Name = _name;
            Delay = _delay;
            Event = _event;
        }
    }

    /// <summary>
    /// A Flowstage defines a discrete phase of the app flow. Primary properties are a pair of event sets for triggering
    ///  a) upon entry and during a stage, and
    ///  b) upon transition to the next stage.
    /// </summary>
    [System.Serializable]
    public class FlowStage
    {
        public string Name;
        public bool clickToAdvance = true;
        public float disableTapTime = 0f;
        public float autoTransitionDelay = 0.0f;
        public float audioDelay = 0f;

        public DelayedEvent[] Events;
        public DelayedEvent[] ExitEvents;

        public float autoTransitionCounter { get; set; }

        public FlowStage()
        {
        }

        public FlowStage(string _name)
        {
            Name = _name;
        }
    }
}
