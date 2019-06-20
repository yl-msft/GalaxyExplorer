// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class GECursor : MonoBehaviour
    {
        void Start()
        {
            if (GalaxyExplorerManager.IsDesktop)
            {
                gameObject.SetActive(false);
                Debug.Log("Desktop platform detected so gaze cursor is deactivated");
            }
        }
    }
}
