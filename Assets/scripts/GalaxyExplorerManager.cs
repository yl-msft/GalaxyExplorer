// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using MRS.FlowManager;
using System.Collections;
using TouchScript.Examples.CameraControl;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.WSA;

namespace GalaxyExplorer
{
    public class GalaxyExplorerManager : Singleton<GalaxyExplorerManager>
    {
        public enum PlatformId
        {
            HoloLensGen1,
            ArticulatedHandsPlatform,
            ImmersiveHMD,
            Desktop,
            Phone
        };

        public static PlatformId Platform { get; set; }

        public delegate void GalaxyExplorerManagerInitializedCallback();

        public static GalaxyExplorerManagerInitializedCallback MyAppPlatformManagerInitialized;

        public GEFadeManager GeFadeManager
        {
            get; set;
        }

        public VOManager VoManager
        {
            get; set;
        }

        public OnboardingManager OnboardingManager
        {
            get; set;
        }

        public TransitionManager TransitionManager
        {
            get; set;
        }

        public GGVMenuManager GGVMenuManager
        {
            get; set;
        }

        public ViewLoader ViewLoaderScript
        {
            get; set;
        }

        public CardPOIManager CardPoiManager
        {
            get; set;
        }

        public CameraController CameraControllerHandler
        {
            get; set;
        }

        public FlowManager FlowManagerHandler
        {
            get; set;
        }

        public static bool IsHoloLensGen1
        {
            get
            {
                return Platform == PlatformId.HoloLensGen1;
            }
        }

        public static bool IsHoloLens2
        {
            get
            {
                return Platform == PlatformId.ArticulatedHandsPlatform;
            }
        }

        public static bool IsImmersiveHMD
        {
            get
            {
                return Platform == PlatformId.ImmersiveHMD;
            }
        }

        public static bool IsDesktop
        {
            get
            {
                return Platform == PlatformId.Desktop;
            }
        }

        public static float GalaxyScaleFactor
        {
            get
            {
                switch (Platform)
                {
                    case PlatformId.ImmersiveHMD:
                        return 2.0f;

                    case PlatformId.HoloLensGen1:
                        return 1.0f;

                    case PlatformId.ArticulatedHandsPlatform:
                        return 1.0f;

                    case PlatformId.Desktop:
                    case PlatformId.Phone:
                        return 1.0f; // 0.75f;
                    default:
                        throw new System.Exception();
                }
            }
        }

        public static float SolarSystemScaleFactor
        {
            get
            {
                switch (Platform)
                {
                    case PlatformId.ImmersiveHMD:
                    case PlatformId.HoloLensGen1:
                        return 1.0f;

                    case PlatformId.ArticulatedHandsPlatform:
                        return 1.0f;

                    case PlatformId.Desktop:
                    case PlatformId.Phone:
                        return 1.0f; // 0.35f;
                    default:
                        throw new System.Exception();
                }
            }
        }

        public static float OrbitalTrailFixedWidth
        {
            get
            {
                switch (Platform)
                {
                    case PlatformId.ImmersiveHMD:
                        return 0.0035f;

                    case PlatformId.HoloLensGen1:
                    case PlatformId.ArticulatedHandsPlatform:
                    case PlatformId.Desktop:
                    case PlatformId.Phone:
                    default:
                        throw new System.Exception();
                }
            }
        }

        public static float MagicWindowScaleFactor
        {
            get
            {
                switch (Platform)
                {
                    case PlatformId.ImmersiveHMD:
                        return 3.0f;

                    case PlatformId.HoloLensGen1:
                    case PlatformId.ArticulatedHandsPlatform:
                    case PlatformId.Desktop:
                    case PlatformId.Phone:
                        return 1.0f;

                    default:
                        throw new System.Exception();
                }
            }
        }

        public static float SlateScaleFactor
        {
            get
            {
                switch (Platform)
                {
                    case PlatformId.ImmersiveHMD:
                        return 3.0f;

                    case PlatformId.HoloLensGen1:
                    case PlatformId.ArticulatedHandsPlatform:
                    case PlatformId.Desktop:
                    case PlatformId.Phone:
                        return 1.0f;

                    default:
                        throw new System.Exception();
                }
            }
        }

        // Pois position need to change depending on platform as each scene in each platform has different scale
        public static float PoiMoveFactor
        {
            get
            {
                float moveFactor = 1f;
                float MRFactor = (Platform == PlatformId.ImmersiveHMD) ? 2.0f : 1.0f;

                if (ViewLoader.CurrentView != null && ViewLoader.CurrentView.Equals("solar_system_view_scene"))
                {
                    moveFactor *= SolarSystemScaleFactor * MRFactor;
                }
                else if (ViewLoader.CurrentView != null && ViewLoader.CurrentView.Equals("galaxy_view_scene"))
                {
                    moveFactor *= GalaxyScaleFactor;
                }
                else if (ViewLoader.CurrentView != null && ViewLoader.CurrentView.Equals("galactic_center_view_scene"))
                {
                    moveFactor *= MRFactor;
                }

                return moveFactor;
            }
        }

        // Move factor just for the orbit scale poi in solar system
        public static float OrbitScalePoiMoveFactor
        {
            get
            {
                float moveFactor = 1f;
                float MRFactor = (Platform == PlatformId.ImmersiveHMD) ? 1.25f : 1.0f;

                if (ViewLoader.CurrentView != null && ViewLoader.CurrentView.Equals("solar_system_view_scene"))
                {
                    moveFactor *= SolarSystemScaleFactor * MRFactor;
                }

                return moveFactor;
            }
        }

        public static float PoiScaleFactor
        {
            get
            {
                switch (Platform)
                {
                    case PlatformId.ImmersiveHMD:
                        return 1.5f;

                    case PlatformId.HoloLensGen1:
                        return 1.0f;

                    case PlatformId.ArticulatedHandsPlatform:
                        return 1.0f;

                    case PlatformId.Desktop:
                    case PlatformId.Phone:
                        return 0.75f;

                    default:
                        throw new System.Exception();
                }
            }
        }

        public static float SpiralGalaxyTintMultConstant
        {
            get
            {
                switch (Platform)
                {
                    case PlatformId.ImmersiveHMD:
                        return 0.22f;

                    case PlatformId.HoloLensGen1:
                    case PlatformId.ArticulatedHandsPlatform:
                    case PlatformId.Desktop:
                    case PlatformId.Phone:
                        return 0.3f;

                    default:
                        throw new System.Exception();
                }
            }
        }

        // This factor is multiplied with the editor value for Billboard line width in order to have thicker line depending on platform
        public static float BillboardLineWidthFactor
        {
            get
            {
                switch (Platform)
                {
                    case PlatformId.ImmersiveHMD:
                        return 2.0f;

                    case PlatformId.HoloLensGen1:
                        return 1.0f;

                    case PlatformId.ArticulatedHandsPlatform:
                        return 1.0f;

                    case PlatformId.Desktop:
                    case PlatformId.Phone:
                        return 1.0f;

                    default:
                        throw new System.Exception();
                }
            }
        }

        public static float ForcePullToCamFixedDistance
        {
            get
            {
                switch (Platform)
                {
                    case PlatformId.ImmersiveHMD:
                        return 1.0f;

                    case PlatformId.HoloLensGen1:
                        return 2.0f;

                    case PlatformId.ArticulatedHandsPlatform:
                        return 1.0f;

                    case PlatformId.Desktop:
                    case PlatformId.Phone:
                        return 1.0f;

                    default:
                        throw new System.Exception();
                }
            }
        }

        private IEnumerator CheckForArticulatedHands()
        {
            while (MixedRealityToolkit.InputSystem.DetectedControllers == null || MixedRealityToolkit.InputSystem.DetectedControllers.Count == 0)
            {
                yield return null;
            }

            foreach (var detectedController in MixedRealityToolkit.InputSystem.DetectedControllers)
            {
                var hand = detectedController as IMixedRealityHand;
                if (hand != null)
                {
                    Platform = PlatformId.ArticulatedHandsPlatform;
                }
            }
        }

        protected override void Awake()
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            base.Awake();

            if (XRDevice.isPresent)
            {
                if (HolographicSettings.IsDisplayOpaque)
                {
                    Platform = PlatformId.ImmersiveHMD;
                }
                else
                {
                    Platform = PlatformId.HoloLensGen1;

                    StartCoroutine(CheckForArticulatedHands());
                }
            }
            else
            {
                Platform = PlatformId.Desktop;
            }

            if (MyAppPlatformManagerInitialized != null)
            {
                MyAppPlatformManagerInitialized.Invoke();
            }

            TransitionManager = FindObjectOfType<TransitionManager>();
            VoManager = FindObjectOfType<VOManager>();
            OnboardingManager = FindObjectOfType<OnboardingManager>();
            GeFadeManager = FindObjectOfType<GEFadeManager>();
            GGVMenuManager = FindObjectOfType<GGVMenuManager>();
            ViewLoaderScript = FindObjectOfType<ViewLoader>();
            CardPoiManager = FindObjectOfType<CardPOIManager>();
            CameraControllerHandler = FindObjectOfType<CameraController>();
            FlowManagerHandler = FindObjectOfType<FlowManager>();
        }
    }
}