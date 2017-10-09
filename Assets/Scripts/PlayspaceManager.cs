// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.XR;
using UnityEngine.XR;

namespace GalaxyExplorer
{
    public class PlayspaceManager : SingleInstance<PlayspaceManager>
    {
        public AnimationCurve FloorFadeCurve;
        public float FadeInOutTime = 1.0f;
        public GameObject SpaceBackground;

        [Tooltip("Drag the CameraRigs game object from CoreSystems here.")]
        public GameObject CameraRigs;

        public Material PlayspaceBoundsMaterial;

        [Tooltip("If true, the floor grid is rendered, even if the device isn't an Opaque HMD; Useful for screenshots.")]
        public bool useFakeFloor = false;
        public GameObject FloorQuad;
        private bool floorVisible = false;
        private bool recalculateFloor = false;

        private KeywordManager keywordManager = null;

        private void Awake()
        {
            keywordManager = GetComponent<KeywordManager>();
            keywordManager.enabled = MyAppPlatformManager.SpeechEnabled;
        }

        // Use this for initialization
        private IEnumerator Start()
        {
            // Check to see if we are on an occluded HMD
            if (MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.ImmersiveHMD)
            {
                floorVisible = XRDevice.SetTrackingSpaceType(TrackingSpaceType.RoomScale);
                if (floorVisible)
                {
                    // Position our floor at (0,0,0) which should be where the
                    // shell says it is supposed to be from OOBE calibration
                    FloorQuad.transform.position = Vector3.zero;
                    useFakeFloor = false;
                }
                else
                {
                    // Theoretically, Unity does this automatically...
                    XRDevice.SetTrackingSpaceType(TrackingSpaceType.Stationary);
                    InputTracking.Recenter();
                    floorVisible = useFakeFloor = true;
                }
            }

            if (!floorVisible)
            {
                // If not, disable the playspace manager
                gameObject.SetActive(false);
                if (keywordManager.enabled)
                {
                    keywordManager.StopKeywordRecognizer();
                }
                yield break;
            }

            // Move the starfield out of the hierarchy
            SpaceBackground.transform.SetParent(null);

            // parent the FloorQuad to the Camera rigs so it stays "locked" to the real world
            FloorQuad.transform.SetParent(CameraRigs.transform, true);
            FloorQuad.SetActive(true);
            FadeInOut(floorVisible);
            recalculateFloor = true;

            // Hook up the controller to rotate the camera
            if (GamepadInput.Instance)
            {
                GamepadInput.Instance.RotateCameraPov += Controller_RotateCameraPov;
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
                    var floorPosition = FloorQuad.transform.position;
                    floorPosition.y = -0.5f;
                    FloorQuad.transform.position = floorPosition;
                    FloorQuad.transform.localScale = Vector3.one * 10f;
                    FloorQuad.GetComponent<Renderer>().sharedMaterial.SetInt("_LinesPerMeter", 10);
                    FloorQuad.GetComponent<Renderer>().sharedMaterial.SetFloat("_LineScale", 0.00075f);
                    recalculateFloor = false;
                }
                else
                {
                    Vector3 newScale = FloorQuad.transform.localScale;
                    // TODO: TryGetDimensions always returns false on Unity 2017.2.0b9
                    if (Boundary.TryGetDimensions(out newScale) || true)
                    {
                        // inflate bounds by 1 meter all around
                        newScale.x += 2.0f;
                        newScale.y += 2.0f;
                        FloorQuad.transform.localScale = newScale;
                        recalculateFloor = false;
                    }
                    Debug.Log(string.Format("FloorQuad.localScale  is: {0}", FloorQuad.transform.localScale.ToString()));

                    Vector3 lossyScale = FloorQuad.transform.lossyScale;
                    FloorQuad.GetComponent<Renderer>().sharedMaterial.SetVector("_WorldScale", new Vector4(lossyScale.x, lossyScale.y, lossyScale.z, 0));
                }
            }
        }

        protected override void OnDestroy()
        {
            if (GamepadInput.Instance)
            {
                GamepadInput.Instance.RotateCameraPov -= Controller_RotateCameraPov;
            }
            base.OnDestroy();
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