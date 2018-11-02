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

        // This is the object that the zoom in and out transitions will move
        // so it should NOT have a TransformHandler component
        [SerializeField]
        [Tooltip("Parent entity of all scene content. NOT a gameobject with a TransformHandler")]
        protected GameObject SceneObject = null;

        // Its the focus collider of the scene so zoom in and ot transitions focs on this collider/object/position
        [SerializeField]
        [Tooltip("The collider that its the focus collider of the scene.")]
        protected SphereCollider SceneFocusCollider = null;

        // This is used for the bounding box in order to cover the whole scene
        [SerializeField]
        [Tooltip("Collider that covers the entire scene")]
        protected BoxCollider EntireSceneCollider = null;

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

        public BoxCollider ThisEntireSceneCollider
        {
            get { return EntireSceneCollider; }
            set { EntireSceneCollider = value; }
        }

        public bool IsSinglePlanetTransition
        {
            get { return IsSinglePlanet; }
            private set { }
        }
  
    }
}
