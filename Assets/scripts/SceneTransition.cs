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

        [SerializeField]
        [Tooltip("The percentage of space within a boundary that the target collider will fill.")]
        [Range(0,1)]
        protected float FillVolumePercentage = 0.75f;

        private Vector3 defaultSize;


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

        // Get the value of scale for this scene in order to fill given percentage of the targetSize
        public float GetScalar(float targetSize)
        {
            return targetSize * FillVolumePercentage / Mathf.Max(defaultSize.x, defaultSize.y, defaultSize.z);
        }

        public float GetScalar()
        {
            return Mathf.Max(transform.lossyScale.x * defaultSize.x, transform.lossyScale.y * defaultSize.y, transform.lossyScale.z * defaultSize.z);
        }

        public void Awake()
        {
            // if the scene starts out hidden, the collider bounds size may not be calculated, so search for the renderer and use that instead
            // If there is a collider that surrounds the whole scene then uses this, if not, use the focus collider
            defaultSize = (EntireSceneCollider) ? EntireSceneCollider.bounds.size : (SceneFocusCollider) ? SceneFocusCollider.bounds.size : Vector3.zero;
            if (defaultSize == Vector3.zero)
            {
                Renderer targetRenderer = SceneFocusCollider.gameObject.GetComponent<Renderer>();
                
                if (targetRenderer != null)
                {
                    defaultSize = targetRenderer.bounds.size;
                    Debug.Log("Default size bounds was calculated by renderer as collider returned zero");
                }
            }
        }

    }
}
