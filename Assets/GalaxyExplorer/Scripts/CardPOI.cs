// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using UnityEngine;
using System.Collections;

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
        private Quaternion cardDescriptionRotation = Quaternion.identity;
        private Vector3 descriptionStoppedLocalPosition = Vector3.zero;
        private Quaternion descriptionStoppedLocalRotation = Quaternion.identity;
        private Transform cardOffsetTransform = null; // Transform from which card remains static

        private GalaxyExplorerManager geManager = null;
        private POIMaterialsFader poiFader = null;

        public GameObject GetCardObject
        {
            get { return CardObject; }
        }


        protected override void Start()
        {
            base.Start();

            geManager = FindObjectOfType<GalaxyExplorerManager>();

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
        }

        new void LateUpdate()
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

        public override void OnInputClicked(InputClickedEventData eventData)
        {
            if (CardObject)
            {
                if (!CardObject.activeSelf)
                {
                    isCardActive = true;

                    StartCoroutine(geFadeManager.FadeContent(poiFader, GEFadeManager.FadeType.FadeOut, cardPoiManager.POIFadeOutTime, cardPoiManager.POIOpacityCurve));

                    CardObject.SetActive(true);

                    if (CardAnimator)
                    {
                        CardAnimator.SetBool("CardVisible", true);
                    }

                    if (CardAudio && voManager)
                    {
                        voManager.Stop(true);
                        voManager.PlayClip(CardAudio);
                    }

                    Vector3 forwardDirection = transform.position - Camera.main.transform.position;
                    CardObject.transform.rotation = Quaternion.LookRotation(forwardDirection.normalized, Camera.main.transform.up);
                    cardRotation = CardObject.transform.rotation;

                    CardObject.transform.position = transform.position;
                    cardOffset = cardOffsetTransform.position - transform.position;

                    StartCoroutine(SlideCardOut());
                }
                else
                {
                    isCardActive = false;

                    StartCoroutine(geFadeManager.FadeContent(poiFader, GEFadeManager.FadeType.FadeIn, cardPoiManager.POIFadeOutTime, cardPoiManager.POIOpacityCurve));

                    // TODO this need to be removed and happen in the animation, but it doesnt
                    CardObject.SetActive(false);

                    if (CardAnimator)
                    {
                        CardAnimator.SetBool("CardVisible", false);
                    }

                    if (voManager)
                    {
                        voManager.Stop(true);
                    }

                    StartCoroutine(SlideCardIn());
                }
            }
        }

        public override void OnInputDown(InputEventData eventData)
        {
            base.OnInputDown(eventData);
        }

        public override void OnInputUp(InputEventData eventData)
        {
            base.OnInputUp(eventData);
   
        }

        public override void OnFocusEnter()
        {
            base.OnFocusEnter();
        }

        public override void OnFocusExit()
        {
            if (CardDescription && !CardObject.activeSelf)
            {
                CardDescription.SetActive(false);
            }
        }

        private IEnumerator SlideCardOut()
        {
            if (Camera.main == null)
            {
                Debug.LogError("CardPointOfInterest: There is no main camera present, to the card description cannot slide out with the hydration of the card magic window.");
                yield break;
            }

            float time = 0.0f;
            Vector3 startPosition = CardObject.transform.position;
            Vector3 endPosition = CardObject.transform.position + CardObject.transform.TransformDirection(cardPoiManager.DescriptionSlideDirection * geManager.GetMagicWindowScaleFactor / 2.0f);
            cardDescriptionRotation = CardDescription.transform.rotation;

            do
            {
                time += Time.deltaTime;

                float timeFraction = Mathf.Clamp01(time / cardPoiManager.DescriptionSlideOutTime);
                float tValue = cardPoiManager.DescriptionSlideCurve.Evaluate(timeFraction);
                CardDescription.transform.position = Vector3.Lerp(startPosition, endPosition, tValue);
                cardDescriptionOffset = CardObject.transform.position - CardDescription.transform.position;

                yield return null;
            }
            while (time < cardPoiManager.DescriptionSlideOutTime);
        }

        private IEnumerator SlideCardIn()
        {
            float time = 0.0f;
 
            Vector3 startLocalPosition = CardDescription.transform.localPosition;
        
            do
            {
                time += Time.deltaTime;
        
                float timeFraction = Mathf.Clamp01(time / cardPoiManager.DescriptionSlideInTime);
                float tValue = cardPoiManager.DescriptionSlideCurve.Evaluate(timeFraction);
                CardDescription.transform.localPosition = Vector3.Lerp(startLocalPosition, descriptionStoppedLocalPosition, tValue);
                CardDescription.SetActive(true);

                yield return null;
            }
            while (time < cardPoiManager.DescriptionSlideInTime);

            CardDescription.transform.localPosition = descriptionStoppedLocalPosition;
            CardDescription.transform.localRotation = descriptionStoppedLocalRotation;
            CardDescription.SetActive(false);
        }
    }
}
