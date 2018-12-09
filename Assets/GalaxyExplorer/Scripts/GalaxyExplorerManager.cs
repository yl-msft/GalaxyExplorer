// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.WSA;

namespace GalaxyExplorer
{
    public class GalaxyExplorerManager : MonoBehaviour
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

        [SerializeField]
        private float SpiralGalaxyTintMultConstant = 1.0f;


        [SerializeField]
        private float PoiScaleFactor = 1.0f;

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
        
        public float GetPoiScaleFactor
        {
            get { return PoiScaleFactor; }
        }


        public float PoiMoveFactor
        {
            get
            {
                return GalaxyScaleFactor;
            }
        }

        public float GetGalaxyScaleFactor
        {
            get { return GalaxyScaleFactor; }
        }

        public float GetSpiralGalaxyTintMultConstant
        {
            get { return SpiralGalaxyTintMultConstant; }
        }

        public static float GalaxyScaleFactor
        {
            get
            {
                switch (Platform)
                {
                    case PlatformId.ImmersiveHMD:
                        return 1.0f;  // 3.0f;
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
                        return 1.0f; // 3.0f;
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
                        return 1.0f; // 3.0f;
                    case PlatformId.HoloLens:
                    case PlatformId.Desktop:
                    case PlatformId.Phone:
                        return 1.0f;
                    default:
                        throw new System.Exception();
                }
            }
        }

        void Awake()
        {
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
            }

            if (MyAppPlatformManagerInitialized != null)
            {
                MyAppPlatformManagerInitialized.Invoke();
            }
        }

    }
}
