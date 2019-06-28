// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

/// <summary>
/// Works along TransformSource
/// Its purpose is to keep a transform at the same position and rotation as the source transform
/// </summary>
namespace GalaxyExplorer
{
    public class TransformHandler : MonoBehaviour
    {
        [SerializeField]
        private int TransformSourceId = 0;

        private Transform transformSource = null;

        void Start()
        {
            TransformSource[] alltrasformSources = FindObjectsOfType<TransformSource>();
            foreach (var currentSource in alltrasformSources)
            {
                if (currentSource.GetTransformSourceId == TransformSourceId)
                {
                    transformSource = currentSource.transform;
                    break;
                }
            }
        }


        void Update()
        {
            if (transformSource)
            {
                transform.position = transformSource.position;
                transform.rotation = transformSource.rotation;
                transform.localScale = transformSource.lossyScale;
            }
        }
    }
}
