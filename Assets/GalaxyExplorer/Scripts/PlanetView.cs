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
        [SerializeField]
        private string SceneName = "";

        public string GetSceneName
        {
            get { return (SceneName.Length > 0) ? SceneName : gameObject.scene.name; }
        }

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
            if (Transition && !Transition.IsInIntroFlow)
            {
                Transition.LoadPrevScene();
            }
        }
    }
}
