// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using UnityEngine;

namespace GalaxyExplorer
{
    public class WorldAnchorHandler : SingleInstance<WorldAnchorHandler>
    {
        private UnityEngine.XR.WSA.WorldAnchor viewLoaderAnchor;
        private bool viewLoaderAnchorActivelyTracking = true;

        private void Start()
        {
            //placementControl = TransitionManager.Instance.ViewVolume.GetComponentInChildren<PlacementControl>();
            //
            //if (placementControl != null)
            //{
            //    placementControl.ContentHeld += PlacementControl_ContentHeld;
            //    placementControl.ContentPlaced += PlacementControl_ContentPlaced;
            //}
            //
            //if (TransitionManager.Instance != null)
            //{
            //    TransitionManager.Instance.ResetStarted += ResetStarted;
            //}
        }

   
        public void CreateWorldAnchor()
        {
            GameObject sourceObject = FindObjectOfType<ViewLoader>().gameObject;

            viewLoaderAnchor = sourceObject.AddComponent<UnityEngine.XR.WSA.WorldAnchor>();

            if (viewLoaderAnchor)
            {
                viewLoaderAnchor.OnTrackingChanged += GalaxyWorldAnchor_OnTrackingChanged;
            }
        }

        public void DestroyWorldAnchor()
        {
            if (viewLoaderAnchor != null)
            {
                viewLoaderAnchor.OnTrackingChanged -= GalaxyWorldAnchor_OnTrackingChanged;
                DestroyImmediate(viewLoaderAnchor);
            }
        }

        #region Callbacks
    
        private void GalaxyWorldAnchor_OnTrackingChanged(UnityEngine.XR.WSA.WorldAnchor self, bool located)
        {
            viewLoaderAnchorActivelyTracking = located;
        }

        private void ResetStarted()
        {
            if (viewLoaderAnchor != null)
            {
                DestroyWorldAnchor();
            }
        }

        private void ResetFinished()
        {
            CreateWorldAnchor();
        }
        #endregion
    }
}