// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

// Handles functionality for the Logo of the app that appears during introduction flow
namespace GalaxyExplorer
{
    public class Logo : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Distance of Logo in MR")]
        private float LogoDistanceMR = 2.0f;

        private void Start()
        {
            // position the logo and orient it towards the user in MR devices
            if (GalaxyExplorerManager.IsHoloLensGen1 || GalaxyExplorerManager.IsHoloLens2 || GalaxyExplorerManager.IsImmersiveHMD)
            {
                gameObject.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * LogoDistanceMR);

                Vector3 forwardDirection = gameObject.transform.position - Camera.main.transform.position;
                gameObject.transform.rotation = Quaternion.LookRotation(forwardDirection.normalized);
            }
        }
    }
}