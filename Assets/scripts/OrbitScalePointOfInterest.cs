// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

//using HoloToolkit.Unity.InputModule;
using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace GalaxyExplorer
{
    public class OrbitScalePointOfInterest : PointOfInterest
    {
        public AnimationCurve OrbitScaleHydrationCurve;
        public AnimationCurve OrbitScaleDeHydrationCurve;

        public AudioClip HydrationAudioFx;
        public AudioClip DeHydrationAudioFx;
        public AudioClip VO = null;
        public AudioSource FxAudioSource;

        public GameObject AlternateDescription;

        public Texture RealIcon;
        public Texture SimplifiedIcon;

        public float SimpleViewMaxScale = 13.5f;
        public float RealisticViewMaxScale = 2350.0f;

        private bool IsReal = false;
        private bool IsAnimating = false;

        private MeshRenderer indicatorRenderer;
        private TrueScaleSetting trueScale = null;

        protected override void Start()
        {
            base.Start();
            indicatorRenderer = Indicator.GetComponentInChildren<MeshRenderer>();

            targetPosition = new Vector3(transform.localPosition.x, 0.0f, transform.localPosition.z);
            targetOffset = new Vector3(0.0f, transform.localPosition.y, 0.0f) * GalaxyExplorerManager.OrbitScalePoiMoveFactor;

            trueScale = FindObjectOfType<TrueScaleSetting>();
        }

        private IEnumerator AnimateUsingCurve(AnimationCurve curve, Action onComplete)
        {
            if (FxAudioSource && HydrationAudioFx && DeHydrationAudioFx)
            {
                FxAudioSource.PlayOneShot(IsReal ? DeHydrationAudioFx : HydrationAudioFx);
            }

            if (curve != null)
            {
                var duration = curve.keys.Last().time;
                float currentTime = 0;

                while (currentTime <= duration)
                {
                    var currentValue = curve.Evaluate(currentTime);
                    trueScale.CurrentRealismScale = currentValue;
                    trueScale.CurrentRealismScale = Mathf.Clamp(trueScale.CurrentRealismScale, 0.0f, 1.0f);

                    currentTime += Time.deltaTime;

                    yield return null;
                }

                var lastValue = curve.Evaluate(duration);
                trueScale.CurrentRealismScale = lastValue;
                trueScale.CurrentRealismScale = Mathf.Clamp(trueScale.CurrentRealismScale, 0.0f, 1.0f);
            }

            if (onComplete != null)
            {
                onComplete();
            }

            IsAnimating = false;
        }

        public override void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            if (!IsAnimating)
            {
                indicatorRenderer.sharedMaterial.mainTexture = IsReal ? SimplifiedIcon : RealIcon;

                // Each view of the solar system has a different max zoom size.
                if (IsReal)
                {
                    // Set simplified view max zoom
                    //ToolManager.Instance.LargestZoom = SimpleViewMaxScale;
                }
                else
                {
                    // Set realistic view max zoom
                    //ToolManager.Instance.LargestZoom = RealisticViewMaxScale;
                }

                IsAnimating = true;

                StartCoroutine(AnimateUsingCurve(IsReal ? OrbitScaleDeHydrationCurve : OrbitScaleHydrationCurve, () => { IsReal = !IsReal; }));

                if (!IsReal && VO && GalaxyExplorerManager.Instance.VoManager)
                {
                    GalaxyExplorerManager.Instance.VoManager.Stop(true);
                    GalaxyExplorerManager.Instance.VoManager.PlayClip(VO);
                }
                else if (IsReal && GalaxyExplorerManager.Instance.VoManager)
                {
                    GalaxyExplorerManager.Instance.VoManager.Stop(true);
                }

                if (AlternateDescription != null)
                {
                    GameObject tempDescription = CardDescription;
                    CardDescription = AlternateDescription;
                    AlternateDescription = tempDescription;

                    CardDescription.SetActive(false);
                    AlternateDescription.SetActive(false);
                }
            }
        }
    }
}