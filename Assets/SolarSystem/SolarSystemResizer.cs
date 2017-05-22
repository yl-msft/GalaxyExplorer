// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using GalaxyExplorer.HoloToolkit.Unity;

namespace GalaxyExplorer
{
    public class SolarSystemResizer : GE_Singleton<SolarSystemResizer>
    {
        void Awake()
        {
            transform.localScale = transform.localScale * MyAppPlatformManager.Instance.SolarSystemScaleFactor;
        }
    }
}