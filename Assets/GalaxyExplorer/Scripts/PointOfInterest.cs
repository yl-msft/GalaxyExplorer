// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using MRS.Audui;
using System.Collections;
using UnityEngine;

namespace GalaxyExplorer
{
    public class PointOfInterest : MonoBehaviour, IInputHandler, IInputClickHandler, IFocusable, IControllerTouchpadHandler
    {
        [SerializeField]
        protected GameObject CardDescription = null;

        [SerializeField]
        protected GameObject Indicator = null;

        [SerializeField]
        private BillboardLine IndicatorLine = null;

        [SerializeField]
        private GameObject TargetPoint = null;

        [SerializeField]
        private Color IndicatorDefaultColor = Color.cyan;

        [SerializeField]
        protected Vector3 indicatorOffset = Vector3.up * 0.4f;

        protected Animator cardDescriptionAnimator = null;
        private Collider indicatorCollider = null;
        protected bool isCardActive = false;
     
        protected CardPOIManager cardPoiManager = null;
        protected GEFadeManager geFadeManager = null;
        protected VOManager voManager = null;
        protected AuduiEventWrangler audioEventWrangler = null;

        // these are only used if there is no indicator line to determine the world position of the point of
        // interest (uses targetPosition) with scale, rotation, and offset and targetOffset to maintain the same
        // distance from that target
        protected Vector3 targetPosition;
        protected Vector3 targetOffset;

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

        public virtual void OnFocusEnter()
        {
            if (CardDescription)
            {
                CardDescription.SetActive(true);
            }
        }

        public virtual void OnFocusExit()
        {
            if (CardDescription)
            {
                CardDescription.SetActive(false);
            }
        }

        public virtual void OnInputDown(InputEventData eventData)
        {

        }

        public virtual void OnInputUp(InputEventData eventData)
        {
            
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
            cardDescriptionAnimator = CardDescription.GetComponent<Animator>();
            CardDescriptionMaterial = CardDescription.GetComponent<MeshRenderer>().material;

            geFadeManager = FindObjectOfType<GEFadeManager>();

            cardPoiManager = FindObjectOfType<CardPOIManager>();
            cardPoiManager.RegisterPOI(this);

            voManager = FindObjectOfType<VOManager>();
            audioEventWrangler = FindObjectOfType<AuduiEventWrangler>();

            if (Indicator)
            {
                Indicator.AddComponent<NoAutomaticFade>();

                indicatorCollider = Indicator.GetComponentInChildren<Collider>();

                StartCoroutine(ResizePOICollider());
            }
        }

        protected virtual void OnDestroy()
        {
            cardPoiManager.UnRegisterPOI(this);
        }

        protected void Update()
        {
            UpdateTransform();
        }

        protected void LateUpdate()
        {

        }

        private void UpdateTransform()
        {
            // do not let the points of interest scale or rotate with the solar system
            float lossyScale = Mathf.Max(gameObject.transform.lossyScale.x, gameObject.transform.lossyScale.y, gameObject.transform.lossyScale.z);
            float localScale =  Mathf.Max(gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
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

        public void OnTouchpadTouched(InputEventData eventData)
        {
  
        }

        public void OnTouchpadReleased(InputEventData eventData)
        {
            // First touch focus on poi
            if (CardDescription && !CardDescription.activeSelf)
            {
                OnFocusEnter();

                GameObject focusedObj = (InputManager.Instance.OverrideFocusedObject) ? InputManager.Instance.OverrideFocusedObject : FocusManager.Instance.TryGetFocusedObject(eventData);
                audioEventWrangler?.OnFocusEnter(focusedObj);
                audioEventWrangler.OverrideFocusedObject(null);
            }
            // Second touch select that poi
            else
            {
                OnInputClicked(null);

                GameObject focusedObj = (InputManager.Instance.OverrideFocusedObject) ? InputManager.Instance.OverrideFocusedObject : FocusManager.Instance.TryGetFocusedObject(eventData);
                audioEventWrangler.OverrideFocusedObject(focusedObj);
                audioEventWrangler?.OnInputClicked(null);
                audioEventWrangler.OverrideFocusedObject(null);
            }
        }

        public void OnInputPositionChanged(InputPositionEventData eventData)
        {

        }

        public virtual void OnInputClicked(InputClickedEventData eventData)
        {
            
        }

        // Scale POI collider in order to cover the whole POI + poi line 
        protected IEnumerator ResizePOICollider()
        {
            yield return new WaitForSeconds(1);

            if (indicatorCollider && IndicatorLine && IndicatorLine.points != null && IndicatorLine.points.Length >= 2)
            {
                BoxCollider boxCollider = (BoxCollider)indicatorCollider;
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
    }
}
