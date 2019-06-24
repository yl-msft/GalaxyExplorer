// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

//using HoloToolkit.Unity.InputModule;
//using HoloToolkit.Unity;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GalaxyExplorer
{
    public class CardPOIManager : MonoBehaviour, IMixedRealityPointerHandler//, IInputClickHandler, IControllerTouchpadHandler
    {
        [Header("Galaxy Card POI Fading")]
        [Tooltip("The time it takes for all points of interest to completely fade out when a card point of interest is selected.")]
        public float POIFadeOutTime = 1.0f;

        [Tooltip("How the opacity changes when all points of interest fade out when a card is selected.")]
        public AnimationCurve POIOpacityCurve;

        [Header("Galaxy Card Text Sliding")]
        [Tooltip("The time it takes for the card description to move from its unselected position to its selected position.")]
        public float DescriptionSlideOutTime = 1.0f;

        [Tooltip("The time it takes for the card description to move from its selected position to its unselected/highlight position.")]
        public float DescriptionSlideInTime = 0.5f;

        [Tooltip("The vector (local space) that descripts where the description card moves to when selected.")]
        public Vector3 DescriptionSlideDirection;

        [Tooltip("How the description card moves when it slides to selected and unselected positions.")]
        public AnimationCurve DescriptionSlideCurve;

        private List<PointOfInterest> allPOIs = new List<PointOfInterest>();

        private void Start()
        {
            //            InputManager.Instance.AddGlobalListener(gameObject);
            //            MixedRealityToolkit.InputSystem.Register(gameObject);

            //            if (GalaxyExplorerManager.Instance.MouseInput)
            //            {
            //                GalaxyExplorerManager.Instance.MouseInput.OnMouseClickDelegate += OnMouseClickDelegate;
            //                GalaxyExplorerManager.Instance.MouseInput.OnMouseClickUpDelegate += OnMouseClickUpDelegate;
            //                GalaxyExplorerManager.Instance.MouseInput.OnMouseOnHoverDelegate += OnMouseOnHoverDelegate;
            //                GalaxyExplorerManager.Instance.MouseInput.OnMouseOnUnHoverDelegate += OnMouseOnUnHoverDelegate;
            //            }

            if (GalaxyExplorerManager.Instance.ToolsManager)
            {
                GalaxyExplorerManager.Instance.ToolsManager.OnBoundingBoxDelegate += OnBoundingBoxDelegate;
            }
        }

        // Callback when bounding box is on/off in HoloLens and MR devices
        // Update pois activation
        private void OnBoundingBoxDelegate(bool isBBenabled)
        {
            StartCoroutine(OnBoundingBoxDelegateCoroutine(isBBenabled));
        }

        private IEnumerator OnBoundingBoxDelegateCoroutine(bool isBBenabled)
        {
            // Wait in order for OnInputClicked to be executed and then continue with this functionality
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Update poi collider activation
            foreach (var poi in allPOIs)
            {
                poi?.UpdateCollidersActivation(!isBBenabled);
            }
        }

        private void OnMouseOnUnHoverDelegate(GameObject selectedObject)
        {
            GalaxyExplorerManager.Instance.AudioEventWrangler.OverrideFocusedObject(null);
        }

        private void OnMouseOnHoverDelegate(GameObject selectedObject)
        {
            if (selectedObject)
            {
                GalaxyExplorerManager.Instance.AudioEventWrangler.OnFocusEnter(selectedObject);
            }
        }

        private void OnMouseClickUpDelegate(GameObject selectedObject)
        {
            GalaxyExplorerManager.Instance.AudioEventWrangler.OverrideFocusedObject(null);
        }

        private void OnMouseClickDelegate(GameObject selectedObject)
        {
            if (selectedObject)
            {
                GalaxyExplorerManager.Instance.AudioEventWrangler.OverrideFocusedObject(selectedObject);
            }
        }

        public void RegisterPOI(PointOfInterest poi)
        {
            if (!allPOIs.Contains(poi))
            {
                allPOIs.Add(poi);
            }
        }

        public void UnRegisterPOI(PointOfInterest poi)
        {
            allPOIs.Remove(poi);
        }

        // Find if a card POI is activa and its card is on/visible
        private bool IsAnyCardActive()
        {
            foreach (var poi in allPOIs)
            {
                if (poi.IsCardActive)
                {
                    return true;
                }
            }

            return false;
        }

        // If a poi card is active then deactivate all poi colliders so user cant activate another one during card presentation
        // This needs to happen in every airtap, mouse click, controller click, keyboard tap so any open magic window card will close
        private IEnumerator UpdateActivationOfPOIColliders()
        {
            yield return new WaitForEndOfFrame();

            if (GalaxyExplorerManager.Instance.TransitionManager.InTransition)
            {
                yield break;
            }

            bool isAnyCardActive = IsAnyCardActive();
            if (isAnyCardActive)
            {
                foreach (var poi in allPOIs)
                {
                    if (poi.IndicatorCollider)
                    {
                        poi.IndicatorCollider.enabled = false;
                    }
                }
            }
        }

        // Find if a card POI is active and its card is on/visible, close the card and trigger audio
        private IEnumerator CloseAnyOpenCard(MixedRealityPointerEventData eventData)
        {
            bool isCardActive = false;

            foreach (var poi in allPOIs)
            {
                if (poi.IsCardActive)
                {
                    // eventData needs to be used in case that we are clocing the card because we dont want this click to propagate into the focused handler
                    eventData?.Use();
                    isCardActive = true;

                    CardPOI cardPoi = (CardPOI)poi;
                    GalaxyExplorerManager.Instance.AudioEventWrangler.OverrideFocusedObject((cardPoi) ? cardPoi.GetCardObject.GetComponentInChildren<Collider>().gameObject : poi.IndicatorCollider.gameObject);

                    poi.OnPointerDown(null);

                    Debug.Log("Close card because of input");
                    break;
                }
            }

            // If any magic window card was active then activate all indicator colliders
            if (isCardActive)
            {
                foreach (var poi in allPOIs)
                {
                    if (poi.IndicatorCollider)
                    {
                        poi.IndicatorCollider.enabled = true;
                    }
                }
            }

            yield return null;
        }

        // Deactivate all pois that might have active card description except the one that is currently focused/touched
        // Note that the focused/touched object could be a planet and not its poi indicator
        private void DeactivateAllDescriptionsHandlers(GameObject focusedObject)
        {
            foreach (var poi in allPOIs)
            {
                if (poi.IndicatorCollider.gameObject != focusedObject)
                {
                    PlanetPOI planetPOI = poi as PlanetPOI;
                    if (planetPOI)
                    {
                        // If planet sphere object is the focused object then dont unfocus from it
                        Planet planet = planetPOI.PlanetObject.GetComponentInChildren<Planet>();
                        if (planet && planet.gameObject == focusedObject)
                        {
                            continue;
                        }
                    }

                    poi.OnFocusExit(null);
                }
            }
        }

        //        public void OnTouchpadTouched(InputEventData eventData)
        //        {
        //
        //        }

        //        public void OnTouchpadReleased(InputEventData eventData)
        //        {
        //            // GETouchScreenInputSource sets InputManager.Instance.OverrideFocusedObject on collider touch
        ////            GameObject focusedObj = InputManager.Instance.OverrideFocusedObject;
        ////            DeactivateAllDescriptionsHandlers(focusedObj);
        //
        //            bool isAnyCardActive = IsAnyCardActive();
        //            StartCoroutine(CloseAnyOpenCard(eventData));
        //            StartCoroutine(UpdateActivationOfPOIColliders());
        //
        //            if (isAnyCardActive)
        //            {
        //                GalaxyExplorerManager.Instance.AudioEventWrangler.OnInputClicked(null);
        //            }
        //        }

        //        public void OnInputPositionChanged(InputPositionEventData eventData)
        //        {
        //
        //        }
        //

        // Called by poi if any poi is focused in order to notify all the other pois
        public void OnPOIFocusEnter(PointOfInterest focusedPOI)
        {
            foreach (var poi in allPOIs)
            {
                if (poi && poi != focusedPOI)
                {
                    poi?.OnAnyPoiFocus();
                }
            }
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {
        }

        public virtual void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            StartCoroutine(CloseAnyOpenCard(eventData));
            StartCoroutine(UpdateActivationOfPOIColliders());
        }

        public void OnPointerClicked(MixedRealityPointerEventData eventData)
        {
        }
    }
}