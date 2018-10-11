// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

/// <summary>
/// Scene transition used when loaded and unloaded
/// Each scene needs to have one on the top parent entity
/// In order to specify the entities for the transitions
/// </summary>
namespace GalaxyExplorer
{
    public class SceneTransition : MonoBehaviour {

        [SerializeField]
        [Tooltip("Parent entity of all scene content. NOT a gameobject with a TransformHandler")]
        protected GameObject SceneObject = null;

        [SerializeField]
        [Tooltip("The collider that its the .")]
        protected SphereCollider SceneFocusCollider = null;

        [SerializeField]
        [Tooltip("True if this scene is a single planet scene.")]
        protected bool IsSinglePlanet = false;

        public GameObject ThisSceneObject
        {
            get { return SceneObject; }
            set { SceneObject = value; }
        }

        public SphereCollider ThisSceneFocusCollider
        {
            get { return SceneFocusCollider; }
            set { SceneFocusCollider = value; }
        }

        public bool IsSinglePlanetTransition
        {
            get { return IsSinglePlanet; }
            private set { }
        }
  
    }
}
