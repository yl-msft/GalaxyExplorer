// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using MRS.Audui;
using UnityEngine;

namespace GalaxyExplorer
{
    public class GEMouseInputSource : BaseInputSource
    {
        private MousePhase mousePhase = MousePhase.NonePhase;
        private GameObject focusedObject = null;
        private uint mouseInputId = 60000;

        private enum MousePhase
        {
            HoverPhase,
            NonePhase
        }

        #region Unity methods

        protected virtual void Start()
        {
            Input.simulateMouseWithTouches = false;
        }

        protected virtual void Update()
        {
            if (Input.touches.Length > 0)
            {
                Debug.Log("Touch input so discard mouse input");
                return;
            }

            // On left mouse click down
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    InputManager.Instance.OverrideFocusedObject = hit.collider.gameObject;
                    OnTappedEvent(mouseInputId);
                    InputManager.Instance.OverrideFocusedObject = null;
                }
            }
            else
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    // If mouse was over a specific entity but not any more
                    if (mousePhase == MousePhase.HoverPhase && focusedObject != hit.collider.gameObject)
                    {
                        OnUnHoverEvent(focusedObject);

                        focusedObject = null;
                        mousePhase = MousePhase.NonePhase;
                    }
                    else if (mousePhase == MousePhase.NonePhase)
                    {
                        OnHoverEvent(hit.collider.gameObject);
                        
                        focusedObject = hit.collider.gameObject;
                        mousePhase = MousePhase.HoverPhase;

                        Debug.Log("On mouse hover");
                    }
                }
                // if mouse isnt over any entity
                else
                {
                    if (mousePhase == MousePhase.HoverPhase)
                    {
                        OnUnHoverEvent(focusedObject);

                        focusedObject = null;
                        mousePhase = MousePhase.NonePhase;
                    }
                }
            }
        }
        #endregion // Unity methods

        protected void OnTappedEvent(uint id)
        {
            InputManager.Instance.RaiseSourceUp(this, id, InteractionSourcePressInfo.Select);
        }

        protected void OnHoverEvent(GameObject focus)
        {
            InputManager.Instance.RaiseFocusEnter(focus);
        }

        protected void OnUnHoverEvent(GameObject focus)
        {
            InputManager.Instance.RaiseFocusExit(focus);
        }

        #region Base Input Source Methods

        public override bool TryGetSourceKind(uint sourceId, out InteractionSourceInfo sourceKind)
        {
            sourceKind = InteractionSourceInfo.Hand;
            return true;
        }

        public override bool TryGetPointerPosition(uint sourceId, out Vector3 position)
        {
            //Touch? knownTouch = GetTouch((int)sourceId);
            //position = (knownTouch.HasValue) ? (Vector3)knownTouch.Value.position : Vector3.zero;
            //return knownTouch.HasValue;
            position = Vector3.zero;
            return false;
        }

        public override bool TryGetPointerRotation(uint sourceId, out Quaternion rotation)
        {
            rotation = Quaternion.identity;
            return false;
        }

        public override bool TryGetPointingRay(uint sourceId, out Ray pointingRay)
        {
            //PersistentTouch knownTouch = GetPersistentTouch((int)sourceId);
            //if (knownTouch != null)
            //{
            //    pointingRay = knownTouch.screenpointRay;
            //    return true;
            //}
            pointingRay = default(Ray);
            return false;
        }

        public override bool TryGetGripPosition(uint sourceId, out Vector3 position)
        {
            position = Vector3.zero;
            return false;
        }

        public override bool TryGetGripRotation(uint sourceId, out Quaternion rotation)
        {
            rotation = Quaternion.identity;
            return false;
        }

        public override SupportedInputInfo GetSupportedInputInfo(uint sourceId)
        {
            return SupportedInputInfo.PointerPosition | SupportedInputInfo.Pointing;
        }

        public override bool TryGetThumbstick(uint sourceId, out bool isPressed, out Vector2 position)
        {
            isPressed = false;
            position = Vector2.zero;
            return false;
        }

        public override bool TryGetTouchpad(uint sourceId, out bool isPressed, out bool isTouched, out Vector2 position)
        {
            isPressed = false;
            isTouched = false;
            position = Vector2.zero;
            return false;
        }

        public override bool TryGetSelect(uint sourceId, out bool isPressed, out double pressedAmount)
        {
            isPressed = false;
            pressedAmount = 0.0;
            return false;
        }

        public override bool TryGetGrasp(uint sourceId, out bool isPressed)
        {
            isPressed = false;
            return false;
        }

        public override bool TryGetMenu(uint sourceId, out bool isPressed)
        {
            isPressed = false;
            return false;
        }

        #endregion // Base Input Source Methods
    }
}
