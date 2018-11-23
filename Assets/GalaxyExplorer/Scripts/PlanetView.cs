// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using UnityEngine;

/// <summary>
/// Component that is used in a planet scene
/// </summary>
namespace GalaxyExplorer
{
    public class PlanetView : MonoBehaviour, IInputHandler
    {
        private TransitionManager Transition = null;

        void Start()
        {
            Transition = FindObjectOfType<TransitionManager>();
        }

        public virtual void OnInputDown(InputEventData eventData)
        {

        }

        public virtual void OnInputUp(InputEventData eventData)
        {
            if (Transition)
            {
                Transition.LoadPrevScene();
            }
        }
    }
}
