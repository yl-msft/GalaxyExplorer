// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

//using HoloToolkit.Unity.InputModule;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GalaxyExplorer
{
    public class PointOfInterest : MonoBehaviour, IMixedRealityPointerHandler, IMixedRealityFocusHandler//, IInputClickHandler, IFocusable, IControllerTouchpadHandler
    {
        [SerializeField]
        protected GameObject CardDescription = null;

        [SerializeField]
        protected GameObject Indicator = null;

        [SerializeField]
        protected BillboardLine IndicatorLine = null;

        [SerializeField]
        private GameObject TargetPoint = null;

        [SerializeField]
        protected GameObject LineBase = null;

        [SerializeField]
        private Color IndicatorDefaultColor = Color.cyan;

        [SerializeField]
        protected Vector3 indicatorOffset = Vector3.up * 0.4f;

        protected Animator cardDescriptionAnimator = null;
        private Collider indicatorCollider = null;
        protected bool isCardActive = false;

        // these are only used if there is no indicator line to determine the world position of the point of
        // interest (uses targetPosition) with scale, rotation, and offset and targetOffset to maintain the same
        // distance from that target
        protected Vector3 targetPosition;

        protected Vector3 targetOffset;

        protected float timer = 0.0f;
        protected POIState currentState = POIState.kIdle;
        protected float restingOnPoiTime = 0.5f;

        // A list of all colliders realted to this poi
        protected List<Collider> allPoiColliders = new List<Collider>();

        protected IAudioService audioService;
        

        protected enum POIState
        {
            kIdle,
            kOnFocusEnter,
            kOnFocusExit,
            kOnInputClicked
        }

        public Vector3 IndicatorOffset
        {
            get { return indicatorOffset; }
            private set { }
        }

        public BillboardLine GetIndicatorLine
        {
            get { return IndicatorLine; }
        }

        public GameObject GetTargetPoint
        {
            get { return TargetPoint; }
        }

        public bool IsCardActive
        {
            get { return isCardActive; }
        }

        public Collider IndicatorCollider
        {
            get { return indicatorCollider; }
        }

        public Material CardDescriptionMaterial
        {
            get; set;
        }

        // If any othe poi is focused then need to deactivate any card description that is on
        public virtual void OnAnyPoiFocus()
        {
            if (CardDescription && CardDescription.activeSelf)
            {
                CardDescription.SetActive(false);
            }
        }

        protected virtual void Awake()
        {
            // do this before start because orbit points of interest need to override the target position (with the orbit)
            if (Indicator != null && IndicatorLine != null && IndicatorLine.points.Length < 2)
            {
                //IndicatorDefaultWidth = IndicatorLine.width;
                IndicatorLine.points = new Transform[2];
                IndicatorLine.points[0] = TargetPoint.gameObject.transform;
                IndicatorLine.points[1] = gameObject.transform;
                IndicatorLine.material.color = IndicatorDefaultColor;
            }
        }

        protected virtual void Start()
        {
            audioService = MixedRealityToolkit.Instance.GetService<IAudioService>();
            
            cardDescriptionAnimator = CardDescription.GetComponent<Animator>();
            CardDescriptionMaterial = CardDescription.GetComponent<MeshRenderer>().material;

            if (Indicator)
            {
                Indicator.AddComponent<NoAutomaticFade>();

                indicatorCollider = Indicator.GetComponentInChildren<Collider>();

                StartCoroutine(ResizePOICollider());
            }

            allPoiColliders.Add(IndicatorCollider);
        }

        protected virtual void OnEnable()
        {
            StartCoroutine(RegisterPOICoroutine());
        }

        protected virtual void OnDisable()
        {
            GalaxyExplorerManager.Instance?.CardPoiManager?.UnRegisterPOI(this);
        }

        protected virtual void OnDestroy()
        {
        }

        protected void Update()
        {
            UpdateTransform();
            UpdateState();
        }

        protected void LateUpdate()
        {
        }

        // Need to register poi after the end of frame as sometimes it ends up the manager to be initialized after pois and the list of pois becomes null
        protected IEnumerator RegisterPOICoroutine()
        {
            yield return new WaitForEndOfFrame();

            if (GalaxyExplorerManager.IsInitialized)
            {
                GalaxyExplorerManager.Instance.CardPoiManager.RegisterPOI(this);
            }
        }

        protected virtual void UpdateState()
        {
            switch (currentState)
            {
                case POIState.kOnFocusExit:
                    timer += Time.deltaTime;

                    if (timer >= restingOnPoiTime)
                    {
                        if (CardDescription)
                        {
                            CardDescription.SetActive(false);
                        }
                    }

                    break;
            }
        }

        private void UpdateTransform()
        {
            // do not let the points of interest scale or rotate with the solar system
            float lossyScale = Mathf.Max(gameObject.transform.lossyScale.x, gameObject.transform.lossyScale.y, gameObject.transform.lossyScale.z);
            float localScale = Mathf.Max(gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
            float desiredScale = (!Mathf.Approximately(lossyScale, 0.0f)) ? localScale / lossyScale : 0.0f;
            if (!Mathf.Approximately(desiredScale, 0.0f))
            {
                gameObject.transform.localScale = new Vector3(desiredScale, desiredScale, desiredScale);
            }

            // This moves the poi indicator
            if (IndicatorLine != null && IndicatorLine.points != null && IndicatorLine.points.Length > 0)
            {
                gameObject.transform.position = IndicatorLine.points[0].position + indicatorOffset;
            }
            // this is for the poi without line, which is one in the solar system scene, above the sun about realistic view
            else
            {
                Vector3 scaledTargetPosition = new Vector3(
                    gameObject.transform.parent.lossyScale.x * targetPosition.x,
                    gameObject.transform.parent.lossyScale.y * targetPosition.y,
                    gameObject.transform.parent.lossyScale.z * targetPosition.z);
                gameObject.transform.position = gameObject.transform.parent.position + (transform.parent.rotation * scaledTargetPosition) + targetOffset;
            }
        }

        //        public void OnTouchpadTouched(InputEventData eventData)
        //        {
        //
        //        }
        //
        //        public void OnTouchpadReleased(InputEventData eventData)
        //        {
        //            // First touch focus on poi
        //            if (CardDescription && !CardDescription.activeSelf)
        //            {
        //                OnFocusEnter();
        //
        ////                GameObject focusedObj = (InputManager.Instance.OverrideFocusedObject) ? InputManager.Instance.OverrideFocusedObject : FocusManager.Instance.TryGetFocusedObject(eventData);
        ////                GalaxyExplorerManager.Instance.AudioEventWrangler?.OnFocusEnter(focusedObj);
        //                GalaxyExplorerManager.Instance.AudioEventWrangler.OverrideFocusedObject(null);
        //            }
        //            // Second touch select that poi
        //            else
        //            {
        //                OnInputClicked(null);
        //
        ////                GameObject focusedObj = (InputManager.Instance.OverrideFocusedObject) ? InputManager.Instance.OverrideFocusedObject : FocusManager.Instance.TryGetFocusedObject(eventData);
        ////                GalaxyExplorerManager.Instance.AudioEventWrangler.OverrideFocusedObject(focusedObj);
        //                GalaxyExplorerManager.Instance.AudioEventWrangler?.OnInputClicked(null);
        //                GalaxyExplorerManager.Instance.AudioEventWrangler.OverrideFocusedObject(null);
        //            }
        //        }
        //
        //        public void OnInputPositionChanged(InputPositionEventData eventData)
        //        {
        //
        //        }
        //

        // Scale POI collider in order to cover the whole POI + poi line.
        // Calculate the collider when collider is enabled so in end of transitions that has its final length
        protected IEnumerator ResizePOICollider()
        {
            yield return new WaitForEndOfFrame();

            while (indicatorCollider && indicatorCollider.enabled == false)
            {
                yield return null;
            }

            if (indicatorCollider && IndicatorLine && IndicatorLine.points != null && IndicatorLine.points.Length >= 2)
            {
                BoxCollider boxCollider = indicatorCollider as BoxCollider;
                if (boxCollider)
                {
                    Vector3 initialSize = boxCollider.size;
                    Vector3 lossyScale = boxCollider.gameObject.transform.lossyScale;
                    Vector3 temp = new Vector3(1.0f / lossyScale.x, 1.0f / lossyScale.y, 1.0f / lossyScale.z);
                    float sizeY = IndicatorLine.points[1].position.y - IndicatorLine.points[0].position.y;
                    boxCollider.size = new Vector3(initialSize.x, Mathf.Abs(sizeY) * temp.y + initialSize.y, initialSize.z);

                    // center it
                    float middleY = IndicatorLine.points[1].position.y - (boxCollider.size.y - initialSize.y) * 0.5f;
                    boxCollider.center = new Vector3(boxCollider.center.x, middleY, boxCollider.center.z);
                }
            }

            yield return null;
        }

        // If card description of this poi is on focus then keep poi on focus otherwise change state to on focus exit
        public void UpdateCardDescription(bool isFocused)
        {
            timer = 0.0f;
            currentState = (isFocused) ? POIState.kOnFocusEnter : currentState = POIState.kOnFocusExit;
        }

        // Switch on/off all colliders related to the poi
        public void UpdateCollidersActivation(bool isEnabled)
        {
            foreach (var item in allPoiColliders)
            {
                item.enabled = isEnabled;
            }
        }

        public virtual void OnPointerUp(MixedRealityPointerEventData eventData)
        {
        }

        public virtual void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            if (currentState == POIState.kOnFocusEnter)
            {
                audioService.PlayClip(AudioId.CardSelect);
            }
            else if (currentState == POIState.kOnFocusExit)
            {
                audioService.PlayClip(AudioId.CardDeselect);
            }
            currentState = POIState.kOnInputClicked;
        }

        public virtual void OnPointerClicked(MixedRealityPointerEventData eventData)
        {
        }

        public void OnBeforeFocusChange(FocusEventData eventData)
        {
        }

        public void OnFocusChanged(FocusEventData eventData)
        {
        }

        public virtual void OnFocusEnter(FocusEventData eventData)
        {
            currentState = POIState.kOnFocusEnter;
            timer = 0.0f;

            if (CardDescription)
            {
                CardDescription.SetActive(true);
                if (GalaxyExplorerManager.IsInitialized)
                {
                    GalaxyExplorerManager.Instance.CardPoiManager.OnPOIFocusEnter(this);
                }
            }
            audioService.PlayClip(AudioId.Focus);
        }

        public virtual void OnFocusExit(FocusEventData eventData)
        {
            currentState = POIState.kOnFocusExit;
            timer = 0.0f;
        }
    }
}