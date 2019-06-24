//// Copyright Microsoft Corporation. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.
//
//using HoloToolkit.Unity;
////using HoloToolkit.Unity.InputModule;
//using System.Collections.Generic;
//using UnityEngine;
//
//namespace GalaxyExplorer
//{
//    public class GETouchScreenInputSource : BaseInputSource
//    {
//        const float kContactEpsilon = 2.0f / 60.0f;
//
//        [SerializeField]
//        [Tooltip("Time in seconds to determine if the contact registers as a tap or a hold")]
//        protected float MaxTapContactTime = 0.5f;
//
//        private List<PersistentTouch> ActiveTouches = new List<PersistentTouch>(0);
//
//        private class PersistentTouch
//        {
//            public Touch touchData;
//            public Ray screenpointRay;
//            public float lifetime;
//            public PersistentTouch(Touch touch, Ray ray)
//            {
//                touchData = touch;
//                this.screenpointRay = ray;
//                lifetime = 0.0f;
//            }
//        }
//
//
//        #region Unity methods
//
//        protected virtual void Start()
//        {
//            // Disable the inputsource if not supported by the device
//            if (!Input.touchSupported)
//            {
//                this.enabled = false;
//            }
//        }
//
//        protected virtual void Update()
//        {
//            foreach (Touch touch in Input.touches)
//            {
//                // Construct a ray from the current touch coordinates
//                Ray ray = CameraCache.Main.ScreenPointToRay(touch.position);
//
//                switch (touch.phase)
//                {
//                    case TouchPhase.Began:
//                    case TouchPhase.Moved:
//                    case TouchPhase.Stationary:
//                        UpdateTouch(touch, ray);
//                        break;
//
//                    case TouchPhase.Ended:
//                    case TouchPhase.Canceled:
//                        RemoveTouch(touch);
//                        break;
//                }
//            }
//        }
//
//        #endregion // Unity methods
//
//        public bool UpdateTouch(Touch touch, Ray ray)
//        {
//            if (touch.phase == TouchPhase.Ended)
//            {
//                //InputManager.Instance.OverrideFocusedObject = null;
//            }
//            else if (touch.phase == TouchPhase.Began)
//            {
//                Ray screenRay = Camera.main.ScreenPointToRay(touch.position);
//
//                RaycastHit hit;
//                if (Physics.Raycast(screenRay, out hit))
//                {
////                    InputManager.Instance.OverrideFocusedObject = hit.collider.gameObject;
//                }
//            }
//
//            PersistentTouch knownTouch = GetPersistentTouch(touch.fingerId);
//            if (knownTouch != null)
//            {
//                knownTouch.lifetime += Time.deltaTime;
//
//                return true;
//            }
//            else
//            {
//                ActiveTouches.Add(new PersistentTouch(touch, ray));
//                OnHoldStartedEvent(touch.fingerId);
//                return false;
//            }
//        }
//
//        public void RemoveTouch(Touch touch)
//        {
//            PersistentTouch knownTouch = GetPersistentTouch(touch.fingerId);
//            if (knownTouch != null)
//            {
//                if (touch.phase == TouchPhase.Ended)
//                {
//                    if (knownTouch.lifetime < kContactEpsilon)
//                    {
//                        OnHoldCanceledEvent(touch.fingerId);
//                    }
//                    else if (knownTouch.lifetime < MaxTapContactTime)
//                    {
//                        OnHoldCanceledEvent(touch.fingerId);
//                        OnTappedEvent(touch.fingerId, touch.tapCount);
//                    }
//                    else
//                    {
//                        OnHoldCompletedEvent(touch.fingerId);
//                    }
//                }
//                else
//                {
//                    OnHoldCanceledEvent(touch.fingerId);
//                }
//                ActiveTouches.Remove(knownTouch);
//            }
//        }
//
//        private PersistentTouch GetPersistentTouch(int id)
//        {
//            for (int i = 0; i < ActiveTouches.Count; ++i)
//            {
//                if (ActiveTouches[i].touchData.fingerId == id)
//                {
//                    return ActiveTouches[i];
//                }
//            }
//            return null;
//        }
//
//        private Touch? GetTouch(int id)
//        {
//            PersistentTouch knownTouch = GetPersistentTouch(id);
//            if (knownTouch != null)
//            {
//                return knownTouch.touchData;
//            }
//            else
//            {
//                return null;
//            }
//        }
//
//        protected void OnTappedEvent(int id, int tapCount)
//        {
////            InputManager.Instance.RaiseTouchpadReleased(this, (uint)id);
////            InputManager.Instance.OverrideFocusedObject = null;
//        }
//
//        protected void OnHoldStartedEvent(int id)
//        {
//
//        }
//
//        protected void OnHoldCanceledEvent(int id)
//        {
//
//        }
//
//        protected void OnHoldCompletedEvent(int id)
//        {
//
//        }
//
//        #region Base Input Source Methods
//
//        public override bool TryGetSourceKind(uint sourceId, out InteractionSourceInfo sourceKind)
//        {
//            sourceKind = InteractionSourceInfo.Hand;
//            return true;
//        }
//
//        public override bool TryGetPointerPosition(uint sourceId, out Vector3 position)
//        {
//            Touch? knownTouch = GetTouch((int)sourceId);
//            position = (knownTouch.HasValue) ? (Vector3)knownTouch.Value.position : Vector3.zero;
//            return knownTouch.HasValue;
//        }
//
//        public override bool TryGetPointerRotation(uint sourceId, out Quaternion rotation)
//        {
//            rotation = Quaternion.identity;
//            return false;
//        }
//
//        public override bool TryGetPointingRay(uint sourceId, out Ray pointingRay)
//        {
//            PersistentTouch knownTouch = GetPersistentTouch((int)sourceId);
//            if (knownTouch != null)
//            {
//                pointingRay = knownTouch.screenpointRay;
//                return true;
//            }
//            pointingRay = default(Ray);
//            return false;
//        }
//
//        public override bool TryGetGripPosition(uint sourceId, out Vector3 position)
//        {
//            position = Vector3.zero;
//            return false;
//        }
//
//        public override bool TryGetGripRotation(uint sourceId, out Quaternion rotation)
//        {
//            rotation = Quaternion.identity;
//            return false;
//        }
//
//        public override SupportedInputInfo GetSupportedInputInfo(uint sourceId)
//        {
//            return SupportedInputInfo.PointerPosition | SupportedInputInfo.Pointing;
//        }
//
//        public override bool TryGetThumbstick(uint sourceId, out bool isPressed, out Vector2 position)
//        {
//            isPressed = false;
//            position = Vector2.zero;
//            return false;
//        }
//
//        public override bool TryGetTouchpad(uint sourceId, out bool isPressed, out bool isTouched, out Vector2 position)
//        {
//            isPressed = false;
//            isTouched = false;
//            position = Vector2.zero;
//            return false;
//        }
//
//        public override bool TryGetSelect(uint sourceId, out bool isPressed, out double pressedAmount)
//        {
//            isPressed = false;
//            pressedAmount = 0.0;
//            return false;
//        }
//
//        public override bool TryGetGrasp(uint sourceId, out bool isPressed)
//        {
//            isPressed = false;
//            return false;
//        }
//
//        public override bool TryGetMenu(uint sourceId, out bool isPressed)
//        {
//            isPressed = false;
//            return false;
//        }
//
//        #endregion // Base Input Source Methods
//    }
//}
