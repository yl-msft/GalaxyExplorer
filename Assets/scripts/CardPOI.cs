// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

//using HoloToolkit.Unity.InputModule;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using UnityEngine;

/// <summary>
/// Its attached to the poi if the poi is supposed to launch a card when selected
/// </summary>
namespace GalaxyExplorer
{
    public class CardPOI : PointOfInterest
    {
        [SerializeField]
        private GameObject CardObject = null;

        [SerializeField]
        private Animator CardAnimator = null;

        [SerializeField]
        private AudioClip CardAudio = null;

        private Quaternion cardRotation = Quaternion.identity;
        private Vector3 cardOffset = Vector3.zero;
        private Vector3 cardDescriptionOffset = Vector3.zero; // offset of card description from the actual card
        private Vector3 descriptionStoppedLocalPosition = Vector3.zero;
        private Quaternion descriptionStoppedLocalRotation = Quaternion.identity;
        private Transform cardOffsetTransform = null; // Transform from which card remains static

        private POIMaterialsFader poiFader = null;

        public GameObject GetCardObject
        {
            get { return CardObject; }
        }

        protected override void Start()
        {
            base.Start();

            // Find poi fader which lives in the same scene as this object and not the one that might exist in the previous scene
            POIMaterialsFader[] allPoiFaders = FindObjectsOfType<POIMaterialsFader>();
            foreach (var fader in allPoiFaders)
            {
                if (fader.gameObject.scene.name == gameObject.scene.name)
                {
                    poiFader = fader;
                    break;
                }
            }

            descriptionStoppedLocalPosition = CardDescription.transform.localPosition;
            descriptionStoppedLocalRotation = CardDescription.transform.localRotation;

            cardOffsetTransform = transform.parent.parent.parent;

            // Scale card/magic window based on platform
            CardObject.transform.localScale *= GalaxyExplorerManager.MagicWindowScaleFactor;
        }

        private new void LateUpdate()
        {
            base.LateUpdate();

            // If the card of this poi is open, then override the card's and descriptions's position and rotation so these are moved with the rotation animation
            if (CardObject && CardObject.activeSelf)
            {
                CardObject.transform.rotation = cardRotation;
                CardObject.transform.position = cardOffsetTransform.position - cardOffset;

                // Card description needs to keep the same distance from the card
                CardDescription.transform.position = CardObject.transform.position - cardDescriptionOffset;
            }
        }

        protected override void UpdateState()
        {
            switch (currentState)
            {
                case POIState.kOnFocusExit:
                    timer += Time.deltaTime;

                    if (timer >= restingOnPoiTime)
                    {
                        timer = 0.0f;

                        if (CardDescription && !CardObject.activeSelf)
                        {
                            CardDescription.SetActive(false);
                        }
                    }

                    break;
            }
        }

        public override void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (CardObject)
            {
                if (!CardObject.activeSelf)
                {
                    isCardActive = true;

                    StartCoroutine(GalaxyExplorerManager.Instance.GeFadeManager.FadeContent(poiFader, GEFadeManager.FadeType.FadeOut, GalaxyExplorerManager.Instance.CardPoiManager.POIFadeOutTime, GalaxyExplorerManager.Instance.CardPoiManager.POIOpacityCurve));

                    CardObject.SetActive(true);

                    if (CardAnimator)
                    {
                        CardAnimator.SetBool("CardVisible", true);
                    }

                    if (CardAudio && GalaxyExplorerManager.Instance.VoManager)
                    {
                        GalaxyExplorerManager.Instance.VoManager.Stop(true);
                        GalaxyExplorerManager.Instance.VoManager.PlayClip(CardAudio);
                    }

                    if (LineBase)
                    {
                        CardObject.transform.position = LineBase.transform.position;
                    }
                    else
                    {
                        CardObject.transform.position = transform.position;
                    }

                    Vector3 forwardDirection = transform.position - Camera.main.transform.position;
                    CardObject.transform.rotation = Quaternion.LookRotation(forwardDirection.normalized, Camera.main.transform.up);
                    cardRotation = CardObject.transform.rotation;

                    cardOffset = cardOffsetTransform.position - CardObject.transform.position;

                    StartCoroutine(SlideCardOut());
                    audioService.PlayClip(AudioId.CardSelect);
                }
                else
                {
                    isCardActive = false;

                    StartCoroutine(GalaxyExplorerManager.Instance.GeFadeManager.FadeContent(poiFader, GEFadeManager.FadeType.FadeIn, GalaxyExplorerManager.Instance.CardPoiManager.POIFadeOutTime, GalaxyExplorerManager.Instance.CardPoiManager.POIOpacityCurve));

                    // TODO this need to be removed and happen in the animation, but it doesnt
                    CardObject.SetActive(false);

                    if (CardAnimator)
                    {
                        CardAnimator.SetBool("CardVisible", false);
                    }

                    if (GalaxyExplorerManager.Instance.VoManager)
                    {
                        GalaxyExplorerManager.Instance.VoManager.Stop(true);
                    }

                    StartCoroutine(SlideCardIn());
                    
                    audioService.PlayClip(AudioId.CardDeselect);
                }
            }
        }

        public override void OnFocusEnter(FocusEventData eventData)
        {
            base.OnFocusEnter(eventData);
        }

        public override void OnFocusExit(FocusEventData eventData)
        {
            base.OnFocusExit(eventData);
        }

        private IEnumerator SlideCardOut()
        {
            if (Camera.main == null)
            {
                Debug.LogError("CardPointOfInterest: There is no main camera present, to the card description cannot slide out with the hydration of the card magic window.");
                yield break;
            }

            float time = 0.0f;
            Vector3 startPosition = CardDescription.transform.position;
            // Find radius of magic window
            MeshCollider collider = CardObject.GetComponentInChildren<MeshCollider>();
            float radius = (collider) ? collider.bounds.extents.y : 1.0f;
            Vector3 endPosition = CardObject.transform.position + radius * GalaxyExplorerManager.Instance.CardPoiManager.DescriptionSlideDirection.normalized;

            do
            {
                time += Time.deltaTime;

                float timeFraction = Mathf.Clamp01(time / GalaxyExplorerManager.Instance.CardPoiManager.DescriptionSlideOutTime);
                float tValue = GalaxyExplorerManager.Instance.CardPoiManager.DescriptionSlideCurve.Evaluate(timeFraction);
                CardDescription.transform.position = Vector3.Lerp(startPosition, endPosition, tValue);
                cardDescriptionOffset = CardObject.transform.position - CardDescription.transform.position;

                yield return null;
            }
            while (time < GalaxyExplorerManager.Instance.CardPoiManager.DescriptionSlideOutTime);
        }

        private IEnumerator SlideCardIn()
        {
            float time = 0.0f;

            Vector3 startLocalPosition = CardDescription.transform.localPosition;

            do
            {
                time += Time.deltaTime;

                float timeFraction = Mathf.Clamp01(time / GalaxyExplorerManager.Instance.CardPoiManager.DescriptionSlideInTime);
                float tValue = GalaxyExplorerManager.Instance.CardPoiManager.DescriptionSlideCurve.Evaluate(timeFraction);
                CardDescription.transform.localPosition = Vector3.Lerp(startLocalPosition, descriptionStoppedLocalPosition, tValue);
                CardDescription.SetActive(true);

                yield return null;
            }
            while (time < GalaxyExplorerManager.Instance.CardPoiManager.DescriptionSlideInTime);

            CardDescription.transform.localPosition = descriptionStoppedLocalPosition;
            CardDescription.transform.localRotation = descriptionStoppedLocalRotation;
            CardDescription.SetActive(false);
        }
    }
}