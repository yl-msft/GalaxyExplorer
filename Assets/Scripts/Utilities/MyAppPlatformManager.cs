// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace GalaxyExplorer
{
    public class MyAppPlatformManager : GE_Singleton<MyAppPlatformManager>
    {
        public enum PlatformId
        {
            HoloLens,
            ImmersiveHMD,
            Desktop,
            Phone
        };

        public PlatformId Platform { get; private set; }

        public float SlateScaleFactor
        {
            get
            {
                switch (Platform)
                {
                    case MyAppPlatformManager.PlatformId.ImmersiveHMD:
                        return 3.0f;
                    case MyAppPlatformManager.PlatformId.HoloLens:
                    case MyAppPlatformManager.PlatformId.Desktop:
                        return 1.0f;
                    default:
                        throw new System.Exception();
                }
            }
        }

        public float OrbitalTrailFixedWidth
        {
            get
            {
                switch (Platform)
                {
                    case MyAppPlatformManager.PlatformId.ImmersiveHMD:
                        return 0.0035f;
                    case MyAppPlatformManager.PlatformId.HoloLens:
                    case MyAppPlatformManager.PlatformId.Desktop:
                    default:
                        throw new System.Exception();
                }
            }
        }

        public float GalaxyScaleFactor
        {
            get
            {
                switch (Platform)
                {
                    case MyAppPlatformManager.PlatformId.ImmersiveHMD:
                        return 3.0f;
                    case MyAppPlatformManager.PlatformId.HoloLens:
                        return 1.0f;
                    case MyAppPlatformManager.PlatformId.Desktop:
                        return 0.75f;
                    default:
                        throw new System.Exception();
                }
            }
        }

        public float SolarSystemScaleFactor
        {
            get
            {
                switch (Platform)
                {
                    case MyAppPlatformManager.PlatformId.ImmersiveHMD:
                    case MyAppPlatformManager.PlatformId.HoloLens:
                        return 1.0f;
                    case MyAppPlatformManager.PlatformId.Desktop:
                        return 0.35f;
                    default:
                        throw new System.Exception();
                }
            }
        }

        public float PoiMoveFactor
        {
            get
            {
                float moveFactor = 1f;
                if (ViewLoader.Instance.CurrentView.Equals("SolarSystemView"))
                {
                    moveFactor *= SolarSystemScaleFactor;
                }
                else if (ViewLoader.Instance.CurrentView.Equals("GalaxyView"))
                {
                    moveFactor *= GalaxyScaleFactor;
                }
                return moveFactor;
            }
        }

        public float PoiScaleFactor
        {
            get
            {
                switch (Platform)
                {
                    case MyAppPlatformManager.PlatformId.ImmersiveHMD:
                        return 3.0f;
                    case MyAppPlatformManager.PlatformId.HoloLens:
                        return 1.0f;
                    case MyAppPlatformManager.PlatformId.Desktop:
                        return 0.75f;
                    default:
                        throw new System.Exception();
                }
            }
        }

        public float SpiralGalaxyTintMultConstant
        {
            get
            {
                switch (Platform)
                {
                    case MyAppPlatformManager.PlatformId.ImmersiveHMD:
                        return 0.22f;
                    case MyAppPlatformManager.PlatformId.HoloLens:
                    case MyAppPlatformManager.PlatformId.Desktop:
                        return 0.3f;
                    default:
                        throw new System.Exception();
                }
            }
        }

        public static event Action MyAppPlatformManagerInitialized;

        public static string DeviceFamilyString = "Windows.Desktop";
        // Use this for initialization
        void Awake()
        {
            switch (DeviceFamilyString)
            {
                case "Windows.Holographic":
                    Platform = MyAppPlatformManager.PlatformId.HoloLens;
                    break;
                case "Windows.Desktop":
                    if (!UnityEngine.VR.VRDevice.isPresent)
                    {
                        Platform = MyAppPlatformManager.PlatformId.Desktop;
                    }
                    else
                    {
                        if (UnityEngine.VR.WSA.HolographicSettings.IsDisplayOpaque)
                        {
                            Platform = MyAppPlatformManager.PlatformId.ImmersiveHMD;
                        }
                        else
                        {
                            Platform = MyAppPlatformManager.PlatformId.HoloLens;
                        }
                    }
                    break;
                case "Windows.Mobile":
                    Platform = MyAppPlatformManager.PlatformId.Phone;
                    break;
                default:
                    Platform = MyAppPlatformManager.PlatformId.Desktop;
                    break;
            }
            Debug.LogFormat("MyAppPlatformManager says its Platform is {0}", Platform.ToString());
            if (MyAppPlatformManagerInitialized != null)
            {
                MyAppPlatformManagerInitialized();
            }
        }
    }
}