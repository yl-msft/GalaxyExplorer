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
        private POIContent CardObject = null;

        [SerializeField]
        private Animator CardAnimator = null;

        [SerializeField]
        private AudioClip CardAudio = null;

        private Quaternion cardRotation = Quaternion.identity;
        private Vector3 cardOffset = Vector3.zero;
        private Transform cardOffsetTransform = null; // Transform from which card remains static

        private POIMaterialsFader poiFader = null;


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

            cardOffsetTransform = transform.parent.parent.parent;

            // Scale card/magic window based on platform
            CardObject.transform.localScale *= GalaxyExplorerManager.MagicWindowScaleFactor;
        }

        private void LateUpdate()
        {
            // If the card of this poi is open, then override the card's and descriptions's position and rotation so these are moved with the rotation animation
            if (CardObject && CardObject.gameObject.activeSelf)
            {
                CardObject.transform.rotation = cardRotation;
                CardObject.transform.position = cardOffsetTransform.position - cardOffset;

                // Card description needs to keep the same distance from the card
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

                    }

                    break;
            }
        }

        public override void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (CardObject)
            {
                if (!CardObject.gameObject.activeSelf)
                {
                    isCardActive = true;
                    StartCoroutine(GalaxyExplorerManager.Instance.GeFadeManager.FadeContent(poiFader, GEFadeManager.FadeType.FadeOut, GalaxyExplorerManager.Instance.CardPoiManager.POIFadeOutTime, GalaxyExplorerManager.Instance.CardPoiManager.POIOpacityCurve));

                    CardObject.gameObject.SetActive(true);

                    if (CardAnimator)
                    {
                        CardAnimator.SetBool("CardVisible", true);
                    }
                    CardObject.ShowContents();

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

                    audioService.PlayClip(AudioId.CardSelect);
                }
                else
                {
                    isCardActive = false;

                    StartCoroutine(GalaxyExplorerManager.Instance.GeFadeManager.FadeContent(poiFader, GEFadeManager.FadeType.FadeIn, GalaxyExplorerManager.Instance.CardPoiManager.POIFadeOutTime, GalaxyExplorerManager.Instance.CardPoiManager.POIOpacityCurve));

                    CardObject.HideContents();
                    // TODO this need to be removed and happen in the animation, but it doesnt
                    CardObject.gameObject.SetActive(false);

                    if (CardAnimator)
                    {
                        CardAnimator.SetBool("CardVisible", false);
                    }

                    if (GalaxyExplorerManager.Instance.VoManager)
                    {
                        GalaxyExplorerManager.Instance.VoManager.Stop(true);
                    }

                    audioService?.PlayClip(AudioId.CardDeselect);
                }
            }
        }
        
        public void CloseAnyOpenCard()
        {
            GalaxyExplorerManager.Instance.CardPoiManager.CloseAnyOpenCard();
            GalaxyExplorerManager.Instance.CardPoiManager.OnPointerDown(null);
        }
    }
}