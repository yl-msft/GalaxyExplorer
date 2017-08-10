// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA;

namespace GalaxyExplorer.HoloToolkit.Unity
{
    public class PlayspaceManager : GE_Singleton<PlayspaceManager>
    {
        public GameObject FloorQuad;
        public AnimationCurve FloorFadeCurve;
        public float FadeInOutTime = 1.0f;
        public GameObject SpaceBackground;

        [Tooltip("Drag the CameraRigs game object from CoreSystems here.")]
        public GameObject CameraRigs;

        public Material PlayspaceBoundsMaterial;

        [Tooltip("If true, the floor grid is rendered, even if the device isn't an Opaque HMD; Useful for screenshots.")]
        public bool useFakeFloor = false;
        private StageRoot StageRoot;
        private bool floorVisible = false;
        private bool recalculateFloor = false;
        private Vector3[] playspaceBounds;

        // Use this for initialization
        private IEnumerator Start()
        {
            // Check to see if we are on an occluded HMD
            floorVisible = MyAppPlatformManager.Instance.Platform == MyAppPlatformManager.PlatformId.ImmersiveHMD;
            if (!floorVisible)
            {
                floorVisible = useFakeFloor;
            }
            else
            {
                useFakeFloor = false;
            }

            if (!floorVisible)
            {
                // If not, disable the playspace manager
                gameObject.SetActive(false);
                GetComponent<KeywordManager>().StopKeywordRecognizer();
                yield break;
            }

            // Get our StageRoot component if it is missing
            StageRoot = GetComponent<StageRoot>();
            if (!StageRoot)
            {
                StageRoot = gameObject.AddComponent<StageRoot>();
            }

            yield return WaitForWorldManagerAndStageRootLocated();

            // Move the starfield out of the hierarchy
            SpaceBackground.transform.SetParent(null);

            // parent the FloorQuad to the Camera rigs so it stays "locked" to the real world
            FloorQuad.transform.SetParent(CameraRigs.transform, true);
            FloorQuad.SetActive(true);
            FadeInOut(floorVisible);
            recalculateFloor = true;

            // Hook up the controller to rotate the camera
            if (InputModule.GamepadInput.Instance)
            {
                InputModule.GamepadInput.Instance.RotateCameraPov += Controller_RotateCameraPov;
            }
            // Hook up the motion controller to rotate the camera
            if (MotionControllerInput.Instance)
            {
                MotionControllerInput.Instance.RotateCameraPov += Controller_RotateCameraPov;
            }
        }

        private bool isRotating = false;
        private float yRotationDelta = 0f;
        private int itemsFadedSoFar = 0;
        private List<GameObject> itemsToFade = new List<GameObject>();
        private float rotationFadeInOutTime = 0.3f;
        private void Controller_RotateCameraPov(float rotationAmount)
        {
            if (isRotating)
            {
                return;
            }
            // initiate fade-out prior to rotation
            yRotationDelta = rotationAmount;
            TransitionManager.Instance.FadeComplete += FadeoutComplete;
            itemsToFade.Clear();
            // add the floor and the stars to the items to fade
            itemsToFade.Add(FloorQuad);
            itemsToFade.Add(StarBackgroundManager.Instance.Stars);
            // If there is current content, fade it.
            GameObject currentContent = ViewLoader.Instance.GetCurrentContent();
            if (currentContent)
            {
                itemsToFade.Add(currentContent);
                // Deal with points of interest
                PointOfInterest[] pois = currentContent.GetComponentsInChildren<PointOfInterest>();
                foreach (PointOfInterest poi in pois)
                {
                    itemsToFade.Add(poi.gameObject);
                    poi.OnGazeDeselect();
                }
            }

            itemsFadedSoFar = 0;
            foreach (GameObject go in itemsToFade)
            {
                StartCoroutine(TransitionManager.Instance.FadeContent(go, TransitionManager.FadeType.FadeOut, rotationFadeInOutTime, FloorFadeCurve, deactivateOnFadeout: false));
            }
            isRotating = itemsToFade.Count > 0;
            if (isRotating)
            {
                CardPOIManager.Instance.HideAllCards();
            }
        }

        private void FadeoutComplete()
        {
            if (++itemsFadedSoFar < itemsToFade.Count)
            {
                return;
            }
            TransitionManager.Instance.FadeComplete -= FadeoutComplete;

            // rotate the camera
            CameraRigs.transform.RotateAround(Camera.main.transform.position, Vector3.up, yRotationDelta);

            TransitionManager.Instance.FadeComplete += FadeinComplete;
            itemsFadedSoFar = 0;
            foreach (GameObject go in itemsToFade)
            {
                StartCoroutine(TransitionManager.Instance.FadeContent(go, TransitionManager.FadeType.FadeIn, rotationFadeInOutTime, FloorFadeCurve));
            }
        }

        private void FadeinComplete()
        {
            if (++itemsFadedSoFar < itemsToFade.Count)
            {
                return;
            }
            TransitionManager.Instance.FadeComplete -= FadeinComplete;
            isRotating = false;
        }

        private void OnDrawGizmos()
        {
            Vector3 lossyScale = FloorQuad.transform.lossyScale;
            FloorQuad.GetComponent<Renderer>().sharedMaterial.SetVector("_WorldScale", new Vector4(lossyScale.x, lossyScale.y, lossyScale.z, 0));
        }

        private void Update()
        {
            if (recalculateFloor)
            {
                if (useFakeFloor)
                {
                    floorPosition.y = -0.5f;
                    FloorQuad.transform.position = floorPosition;
                    FloorQuad.transform.localScale = Vector3.one * 10f;
                    FloorQuad.GetComponent<Renderer>().sharedMaterial.SetInt("_LinesPerMeter", 10);
                    FloorQuad.GetComponent<Renderer>().sharedMaterial.SetFloat("_LineScale", 0.00075f);
                    recalculateFloor = false;
                    return;
                }
                // Get the stage bounds from the WorldManager and calculate the floor's dimensions
                playspaceBounds = null;
                bool getStageBoundsSucceeded = false;
                if (StageRoot)
                {
                    getStageBoundsSucceeded = StageRoot.TryGetBounds(out playspaceBounds);
                }
                if (getStageBoundsSucceeded && playspaceBounds != null)
                {
                    CalculateFloorDimensions();
                    floorPosition.y = StageRoot.transform.position.y;
                    FloorQuad.transform.position = floorPosition;

                    recalculateFloor = false;
                }
            }
        }

        private void OnDestroy()
        {
            if (StageRoot)
            {
                StageRoot.OnTrackingChanged -= StageRoot_OnTrackingChanged;
            }
            if (InputModule.GamepadInput.Instance)
            {
                InputModule.GamepadInput.Instance.RotateCameraPov -= Controller_RotateCameraPov;
            }
            if (MotionControllerInput.Instance)
            {
                MotionControllerInput.Instance.RotateCameraPov -= Controller_RotateCameraPov;
            }
        }

        private void StageRoot_OnTrackingChanged(StageRoot self, bool located)
        {
            recalculateFloor = located;
        }

        private IEnumerator WaitForWorldManagerAndStageRootLocated()
        {
            if (useFakeFloor)
            {
                yield break;
            }

            // Default the floor to inactive
            FloorQuad.SetActive(false);

            // Wait until we acquire an Active tracking state
            while (WorldManager.state != PositionalLocatorState.Active)
            {
                yield return null;
            }

            // Make sure our stage root is located
            while (!StageRoot.isLocated)
            {
                yield return null;
            }

            StageRoot.OnTrackingChanged += StageRoot_OnTrackingChanged;
        }

        private void CalculateFloorDimensions()
        {
            Vector3 newScale = FloorQuad.transform.localScale;
            CalculateFloorDimensionsFromBounds(ref newScale);
            FloorQuad.transform.localScale = newScale;
            Debug.Log(string.Format("FloorQuad.localScale  is: {0}", FloorQuad.transform.localScale.ToString()));

            Vector3 lossyScale = FloorQuad.transform.lossyScale;
            FloorQuad.GetComponent<Renderer>().sharedMaterial.SetVector("_WorldScale", new Vector4(lossyScale.x, lossyScale.y, lossyScale.z, 0));
        }

        Vector3 floorPosition = Vector3.zero;

        private void CalculateFloorDimensionsFromBounds(ref Vector3 dimensions)
        {
            if (this.playspaceBounds != null && this.playspaceBounds.Length > 0)
            {
                float maxX, minX, maxY, minY;
                maxY = maxX = float.MinValue;
                minY = minX = float.MaxValue;

                for (int i = 0; i < playspaceBounds.Length; i++)
                {
                    // Note, The extents of the bounds are in X and Z, but our Quad's dimensions are X and Y.
                    maxY = Mathf.Max(maxY, playspaceBounds[i].z);
                    maxX = Mathf.Max(maxX, playspaceBounds[i].x);
                    minY = Mathf.Min(minY, playspaceBounds[i].z);
                    minX = Mathf.Min(minX, playspaceBounds[i].x);
                }

                dimensions.x = (maxX - minX) + 2f; // extend the floor quad by 1m on each side
                dimensions.y = (maxY - minY) + 2f;
                //dimensions.z is unchanged
                floorPosition = new Vector3((maxX + minX) / 2f, floorPosition.y, (maxY + minY) / 2f);
                Debug.LogFormat("FloorPosition: {0}", floorPosition.ToString());
            }
            else
            {
                Debug.Log("bounds was null or an empty array");
            }
        }

        private void FadeInOut(bool fadeIn)
        {
            if (fadeIn)
            {
                FloorQuad.SetActive(true);
            }
            StartCoroutine(TransitionManager.Instance.FadeContent(
                FloorQuad,
                fadeIn ? TransitionManager.FadeType.FadeIn : TransitionManager.FadeType.FadeOut,
                Instance.FadeInOutTime,
                Instance.FloorFadeCurve));
        }

        public void ToggleFloor()
        {
            floorVisible = !floorVisible;
            FadeInOut(floorVisible);
        }
    }
}