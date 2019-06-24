// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Handles fade
/// </summary>
namespace GalaxyExplorer
{
    public class GEFadeManager : MonoBehaviour
    {
        public enum FadeType
        {
            FadeIn,
            FadeOut
        }

        public delegate void FadeCompleteCallback(FadeType fadeType);

        public FadeCompleteCallback OnFadeComplete;

        public void SetAlphaOnFader(Fader fader, float alpha)
        {
            if (fader)
            {
                fader.SetAlpha(alpha);
            }
        }

        public void SetAlphaOnFader(Fader[] faders, float alpha)
        {
            foreach (var fader in faders)
            {
                SetAlphaOnFader(fader, alpha);
            }
        }

        public void SetAlphaOnFaderExcept(Fader fader, Type exceptType, float alpha)
        {
            if (fader && fader.GetType() != exceptType)
            {
                SetAlphaOnFader(fader, alpha);
            }
        }

        public void SetAlphaOnFaderExcept(Fader[] faders, Type exceptType, float alpha)
        {
            foreach (var fader in faders)
            {
                SetAlphaOnFaderExcept(fader, exceptType, alpha);
            }
        }

        public void Fade(Fader fader, GEFadeManager.FadeType type, float fadeDuration, AnimationCurve opacityCurve)
        {
            StartCoroutine(FadeContent(fader, type, fadeDuration, opacityCurve));
        }

        public void Fade(Fader[] allFaders, GEFadeManager.FadeType type, float fadeDuration, AnimationCurve opacityCurve)
        {
            foreach (var fader in allFaders)
            {
                Fade(fader, type, fadeDuration, opacityCurve);
            }

            if (allFaders.Length == 0)
            {
                OnFadeComplete?.Invoke(type);
            }
        }

        public void FadeExcept(Fader fader, Type exceptType, GameObject exceptObj, GEFadeManager.FadeType type, float fadeDuration, AnimationCurve opacityCurve)
        {
            if (fader && fader.GetType() != exceptType && fader.gameObject != exceptObj)
            {
                Fade(fader, type, fadeDuration, opacityCurve);
            }
            else if (fader == null)
            {
                OnFadeComplete?.Invoke(type);
            }
        }

        public void FadeExcept(Fader[] faders, Type except, GameObject exceptObj, GEFadeManager.FadeType type, float fadeDuration, AnimationCurve opacityCurve)
        {
            foreach (var fader in faders)
            {
                FadeExcept(fader, except, exceptObj, type, fadeDuration, opacityCurve);
            }

            if (faders.Length == 0)
            {
                OnFadeComplete?.Invoke(type);
            }
        }

        public IEnumerator FadeContent(Fader content, FadeType fadeType, float fadeDuration, AnimationCurve opacityCurve, float fadeTimeOffset = 0.0f)
        {
            if (content == null)
            {
                // Invoke callback to notify end of fade
                if (OnFadeComplete != null)
                {
                    OnFadeComplete.Invoke(fadeType);
                }

                yield break;
            }

            // Wait for the fade time offset to complete before alpha is changed on the faders
            float time = fadeTimeOffset;
            while (time > 0.0f)
            {
                time = Mathf.Clamp(time - Time.deltaTime, 0.0f, fadeTimeOffset);
                yield return null;
            }

            // Setup initial and final alpha values for the fade based on the type of fade
            Vector2 alpha = (fadeType == FadeType.FadeIn) ? Vector2.up : Vector2.right;
            float timeFraction = 0.0f;

            content.EnableFade();

            do
            {
                time += Time.deltaTime;
                timeFraction = Mathf.Clamp01(time / fadeDuration);

                float alphaValue = opacityCurve != null ? Mathf.Lerp(alpha.x, alpha.y, Mathf.Clamp01(opacityCurve.Evaluate(timeFraction))) : timeFraction;
                content.SetAlpha(alphaValue);

                yield return null;
            }
            while (timeFraction < 1.0f && content != null);

            content.DisableFade();

            // Invoke callback to notify end of fade
            if (OnFadeComplete != null)
            {
                OnFadeComplete.Invoke(fadeType);
            }
        }

        public IEnumerator FadeMaterial(Material content, FadeType fadeType, float fadeDuration, AnimationCurve opacityCurve)
        {
            Vector2 alpha = (fadeType == FadeType.FadeIn) ? Vector2.up : Vector2.right;
            float timeFraction = 0.0f;
            float time = 0.0f;

            do
            {
                time += Time.deltaTime;
                timeFraction = Mathf.Clamp01(time / fadeDuration);

                float alphaValue = opacityCurve != null ? Mathf.Lerp(alpha.x, alpha.y, Mathf.Clamp01(opacityCurve.Evaluate(timeFraction))) : timeFraction;
                content.SetFloat("_TransitionAlpha", alphaValue);

                yield return null;
            }
            while (timeFraction < 1.0f && content != null);
        }
    }
}