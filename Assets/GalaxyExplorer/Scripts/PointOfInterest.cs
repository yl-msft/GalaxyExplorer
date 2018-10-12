// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using System.Collections.Generic;
using UnityEngine;


namespace GalaxyExplorer
{
    public class PointOfInterest : MonoBehaviour, IInputHandler, IFocusable
    {
        public class POIFader : Fader
        {
            private MeshFilter filter;
            private List<Color> colors;

            public PointOfInterest parent;

            private void Awake()
            {
                filter = GetComponent<MeshFilter>();
                colors = new List<Color>(new Color[filter.sharedMesh.vertexCount]);
            }

            protected override bool CanAddMaterialsFromRenderer(Renderer renderer, Fader[] faders)
            {
                return false;
            }

            public override bool SetAlpha(float alphaValue)
            {
                if (parent && parent.MaterialToFade)
                {
                    parent.MaterialToFade.SetFloat("_TransitionAlpha", alphaValue);
                }
                else
                {
                    for (int i = 0; i < colors.Count; i++)
                    {
                        colors[i] = new Color(alphaValue, alphaValue, alphaValue, alphaValue);
                    }

                    filter.sharedMesh.SetColors(colors);
                }

                alpha = alphaValue;

                return true;
            }
        }

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

        [SerializeField]
        private Material MaterialToFade = null;

        protected Animator cardDescriptionAnimator = null;
        private Collider indicatorCollider = null;
        protected bool isCardActive = false;
        

        protected CardPOIManager cardPoiManager = null;
        protected GEFadeManager geFadeManager = null;

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

            //if (cardDescriptionAnimator)
            //{
            //    cardDescriptionAnimator.SetBool("hover", true);
            //}
        }

        public virtual void OnFocusExit()
        {
            if (CardDescription)
            {
                CardDescription.SetActive(false);
            }

            //if (cardDescriptionAnimator)
            //{
            //    cardDescriptionAnimator.SetBool("hover", false);
            //}
        }

        public virtual void OnInputDown(InputEventData eventData)
        {

        }

        public virtual void OnInputUp(InputEventData eventData)
        {
      
        }

        protected virtual void Start()
        {
            // Has to be global listener so it receives all taps and decides what to do depended on if this is the selected object
            // InputManager.Instance.AddGlobalListener(gameObject);

            cardDescriptionAnimator = CardDescription.GetComponent<Animator>();
            CardDescriptionMaterial = CardDescription.GetComponent<MeshRenderer>().material;

            cardPoiManager = FindObjectOfType<CardPOIManager>();
            cardPoiManager.RegisterPOI(this);

            geFadeManager = FindObjectOfType<GEFadeManager>();

            if (Indicator)
            {
                Indicator.AddComponent<NoAutomaticFade>();

                indicatorCollider = Indicator.GetComponentInChildren<Collider>();
            }

            // do this before start because orbit points of interest need to override the target position (with the orbit)
            if (Indicator != null && IndicatorLine != null && IndicatorLine.points.Length < 2)
            {
                //IndicatorDefaultWidth = IndicatorLine.width;
                IndicatorLine.points = new Transform[2];
                IndicatorLine.points[0] = TargetPoint.gameObject.transform;
                IndicatorLine.points[1] = gameObject.transform;
                IndicatorLine.material.color = IndicatorDefaultColor;
            }

            //MeshFilter meshFilter = Indicator.GetComponentInChildren<MeshFilter>();
            //if (meshFilter != null)
            //{
            //    meshFilter.gameObject.AddComponent<POIFader>().parent = this;
            //}
        }

        protected void LateUpdate()
        {
            // do not let the points of interest scale or rotate with the solar system
            float currentScale = Mathf.Max(gameObject.transform.lossyScale.x, gameObject.transform.lossyScale.y, gameObject.transform.lossyScale.z);
            float localScale = Mathf.Max(gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
            if (currentScale != 1.0f && currentScale != 0.0f && localScale != 0.0f)
            {
                float desiredScale = localScale / currentScale;
                gameObject.transform.localScale = new Vector3(desiredScale, desiredScale, desiredScale);
            }

            if (IndicatorLine != null && IndicatorLine.points != null && IndicatorLine.points.Length > 0)
            {
                gameObject.transform.position = IndicatorLine.points[0].position + indicatorOffset;
            }
        }

    }
}
