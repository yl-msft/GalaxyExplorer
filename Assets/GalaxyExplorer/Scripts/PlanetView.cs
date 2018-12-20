// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using UnityEngine;

/// <summary>
/// Component that is used in a planet scene
/// </summary>
namespace GalaxyExplorer
{
    public class PlanetView : MonoBehaviour, IInputClickHandler
    {
        [SerializeField]
        private string SceneName = "";

        public string GetSceneName
        {
            get { return (SceneName.Length > 0) ? SceneName : gameObject.scene.name; }
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            if (GalaxyExplorerManager.Instance.TransitionManager && !GalaxyExplorerManager.Instance.TransitionManager.IsInIntroFlow)
            {
                GalaxyExplorerManager.Instance.TransitionManager.LoadPrevScene();
            }
        }
    }
}
