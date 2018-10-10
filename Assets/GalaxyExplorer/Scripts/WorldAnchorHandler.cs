// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using UnityEngine;

namespace GalaxyExplorer
{
    public class WorldAnchorHandler : SingleInstance<WorldAnchorHandler>
    {
        private UnityEngine.XR.WSA.WorldAnchor anchor;
        private bool anchorActivelyTracking = true;

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
            GameObject sourceObject = FindObjectOfType<TransformSource>().gameObject;

            anchor = sourceObject.AddComponent<UnityEngine.XR.WSA.WorldAnchor>();

            if (anchor)
            {
                anchor.OnTrackingChanged += GalaxyWorldAnchor_OnTrackingChanged;
            }
        }

        public void DestroyWorldAnchor()
        {
            if (anchor != null)
            {
                anchor.OnTrackingChanged -= GalaxyWorldAnchor_OnTrackingChanged;
                DestroyImmediate(anchor);
            }
        }

        #region Callbacks
    
        private void GalaxyWorldAnchor_OnTrackingChanged(UnityEngine.XR.WSA.WorldAnchor self, bool located)
        {
            anchorActivelyTracking = located;
        }

        private void ResetStarted()
        {
            if (anchor != null)
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