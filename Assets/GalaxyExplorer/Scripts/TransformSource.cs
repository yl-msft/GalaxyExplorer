// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

/// <summary>
/// Works along TransformHandler
/// It's purpose is to keep track of a transform so TransformHandler can update it's self based on another transform
/// </summary>
namespace GalaxyExplorer
{
    public class TransformSource : MonoBehaviour
    {
        [SerializeField]
        private int TransformSourceId = 0;

        public int GetTransformSourceId
        {
            get { return TransformSourceId; }
        }
    }
}
