// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;

namespace GalaxyExplorer
{
    // Entities to enable in VR mode
    public class VREnabled : MonoBehaviour
    {
        [SerializeField]
        List<GameObject> ActiveInVR = new List<GameObject>();

        void Start()
        {
            if (GalaxyExplorerManager.IsImmersiveHMD)
            {
                foreach (var item in ActiveInVR)
                {
                    item?.SetActive(true);
                }
            }
        }
    }
}