// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

/// <summary>
/// Scene transition used when loaded and unloaded
/// Each scene needs tohave one on the top parent entity
/// </summary>
namespace GalaxyExplorer
{
    public class SceneTransition : MonoBehaviour {

        [SerializeField]
        protected GameObject SceneObject = null;

        [SerializeField]
        protected SphereCollider SceneFocusCollider = null;

        [SerializeField]
        protected bool IsSinglePlanet = false;

        protected bool isFading = false;

        protected GEFadeManager fadeManager = null;

        public delegate void UnloadCompleteCallback();
        public UnloadCompleteCallback OnUnloadComplete;

        public GameObject ThisSceneObject
        {
            get { return SceneObject; }
            set { SceneObject = value; }
        }

        public SphereCollider ThisSceneFocusCollider
        {
            get { return SceneFocusCollider; }
            set { SceneFocusCollider = value; }
        }

        public bool IsSinglePlanetTransition
        {
            get { return IsSinglePlanet; }
            private set { }
        }

        public bool IsFading
        {
            get { return isFading; }
        }

        protected virtual void Start()
        {
            fadeManager = FindObjectOfType<GEFadeManager>();
            fadeManager.OnFadeComplete += OnFadeComplete;
        }

        // Callback received when fade coroutine is completed
        protected void OnFadeComplete(GEFadeManager.FadeType type)
        {
            isFading = false;
        }

        public void Fade(Fader fader, GEFadeManager.FadeType type, float fadeDuration, AnimationCurve opacityCurve)
        {
            if (fader)
            {
                isFading = true;
                StartCoroutine(fadeManager.FadeContent(fader, type, fadeDuration, opacityCurve));
            }
        }

        public void Fade(Fader[] allFaders, GEFadeManager.FadeType type, float fadeDuration, AnimationCurve opacityCurve)
        {
            foreach (var fader in allFaders)
            {
                Fade(fader, type, fadeDuration, opacityCurve);
            }
        }

        public void FadeExcept(Fader fader, Type exceptType, GameObject exceptObj, GEFadeManager.FadeType type, float fadeDuration, AnimationCurve opacityCurve)
        {
            if (fader)
            {
                if (fader.GetType() != exceptType && fader.gameObject != exceptObj)
                {
                    Fade(fader, type, fadeDuration, opacityCurve);
                }
            }
        }

        public void FadeExcept(Fader[] faders, Type except, GameObject exceptObj, GEFadeManager.FadeType type, float fadeDuration, AnimationCurve opacityCurve)
        {
            foreach (var fader in faders)
            {
                FadeExcept(fader, except, exceptObj, type, fadeDuration, opacityCurve);
            }
        }
    }
}
