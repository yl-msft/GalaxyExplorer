// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using UnityEngine;

namespace GalaxyExplorer
{
    public class StarBackgroundManager : MonoBehaviour
    {
        public float FadeInOutTime = 1.0f;
        public AnimationCurve StarBackgroundFadeCurve = null;
        public GameObject Stars = null;

        private void Start()
        {
            gameObject.SetActive(GalaxyExplorerManager.IsImmersiveHMD);
            //TransitionManager.Instance.ViewVolume.GetComponent<PlacementControl>().ContentPlaced += UpdateShaderProperties;
            GalaxyExplorerManager.Instance.ViewLoaderScript.OnSceneIsLoaded += UpdateShaderProperties;
            //ToolManager.Instance.ContentZoomChanged += UpdateShaderProperties;
        }

        private void UpdateShaderProperties()
        {
            GameObject currentContent = GalaxyExplorerManager.Instance.TransitionManager.CurrentActiveScene;
            if (currentContent)
            {
                SceneTransition sceneSizer = currentContent.GetComponent<SceneTransition>();
                if (sceneSizer)
                {
                    float scalar = sceneSizer.GetScalar();
                    Vector3 contentWP = currentContent.transform.position;
                    Renderer renderer = GetComponentInChildren<Renderer>();
                    if (renderer)
                    {
                        Material mat = renderer.sharedMaterial;
                        if (mat)
                        {
                            mat.SetFloat("_ContentRadius", scalar);
                            mat.SetVector("_ContentWorldPos", contentWP);
                        }
                    }
                }
            }
        }

        public void FadeInOut(bool fadeIn)
        {
            if (fadeIn)
            {
                gameObject.SetActive(true);
            }

            GalaxyExplorerManager.Instance.GeFadeManager.Fade(
                Stars.GetComponentInChildren<Fader>(),
                fadeIn ? GEFadeManager.FadeType.FadeIn : GEFadeManager.FadeType.FadeOut,
                FadeInOutTime, StarBackgroundFadeCurve);
        }
    }
}