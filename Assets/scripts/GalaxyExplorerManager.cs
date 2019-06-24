﻿// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using MRS.FlowManager;
//using HoloToolkit.Unity;
//using HoloToolkit.Unity.InputModule;
using MRS.Audui;
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
            HoloLens,
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

        public AuduiEventWrangler AudioEventWrangler
        {
            get; set;
        }

        public TransitionManager TransitionManager
        {
            get; set;
        }

        public ToolManager ToolsManager
        {
            get; set;
        }

//        public GEMouseInputSource MouseInput
//        {
//            get; set;
//        }

//        public InputRouter InputRouter
//        {
//            get; set;
//        }

        public MusicManager MusicManagerScript
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

        public static bool IsHoloLens
        {
            get
            {
                return Platform == PlatformId.HoloLens;
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
                    case PlatformId.HoloLens:
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
                    case PlatformId.HoloLens:
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
                    case PlatformId.HoloLens:
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
                    case PlatformId.HoloLens:
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
                    case PlatformId.HoloLens:
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
                    case PlatformId.HoloLens:
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
                    case PlatformId.HoloLens:
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
                    case PlatformId.HoloLens:
                        return 1.0f;
                    case PlatformId.Desktop:
                    case PlatformId.Phone:
                        return 1.0f; 
                    default:
                        throw new System.Exception();
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
                    Platform = PlatformId.HoloLens;
                }
            }
            else 
            {
                Platform = PlatformId.Desktop;
//                GazeManager.Instance.enabled = false;
//                FocusManager.Instance.enabled = false;
            }

            if (MyAppPlatformManagerInitialized != null)
            {
                MyAppPlatformManagerInitialized.Invoke();
            }

            AudioEventWrangler = FindObjectOfType<AuduiEventWrangler>();
            TransitionManager = FindObjectOfType<TransitionManager>();
            VoManager = FindObjectOfType<VOManager>();
            GeFadeManager = FindObjectOfType<GEFadeManager>();
            ToolsManager = FindObjectOfType<ToolManager>();
//            MouseInput = FindObjectOfType<GEMouseInputSource>();
//            InputRouter = FindObjectOfType<InputRouter>();
            MusicManagerScript = FindObjectOfType<MusicManager>();
            ViewLoaderScript = FindObjectOfType<ViewLoader>();
            CardPoiManager = FindObjectOfType<CardPOIManager>();
            CameraControllerHandler = FindObjectOfType<CameraController>();
            FlowManagerHandler = FindObjectOfType<FlowManager>();
        }

    }
}
