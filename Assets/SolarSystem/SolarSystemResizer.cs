// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace GalaxyExplorer
{
    public class SolarSystemResizer : GE_Singleton<SolarSystemResizer>
    {
        void Awake()
        {
            transform.localScale = transform.localScale * MyAppPlatformManager.SolarSystemScaleFactor;
        }
    }
}