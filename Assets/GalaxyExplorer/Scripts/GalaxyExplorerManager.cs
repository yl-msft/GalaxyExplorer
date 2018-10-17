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
        private float GalaxyScaleFactor = 1.0f;

        [SerializeField]
        private float SpiralGalaxyTintMultConstant = 1.0f;

        [SerializeField]
        private float SlateScaleFactor = 1.0f;

        [SerializeField]
        private float MagicWindowScaleFactor = 1.0f;

        [SerializeField]
        private float SolarSystemScaleFactor = 1.0f;

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

        public float GetMagicWindowScaleFactor
        {
            get { return MagicWindowScaleFactor; }
        }

        public float GetGalaxyScaleFactor
        {
            get { return GalaxyScaleFactor; }
        }

        public float GetSpiralGalaxyTintMultConstant
        {
            get { return SpiralGalaxyTintMultConstant; }
        }

        public float GetSlateScaleFactor
        {
            get { return SlateScaleFactor; }
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
