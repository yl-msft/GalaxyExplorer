// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using GalaxyExplorer.HoloToolkit.Unity;

namespace GalaxyExplorer
{
    public class GalaxyResizer : GE_Singleton<GalaxyResizer>
    {
        void Awake()
        {
            transform.localScale = transform.localScale * MyAppPlatformManager.GalaxyScaleFactor;
        }
        void Start()
        {
            SpiralGalaxy[] spirals = GetComponentsInChildren<SpiralGalaxy>();
            foreach (var spiral in spirals)
            {
                if (spiral.tintMult < 1)
                {
                    spiral.tintMult = MyAppPlatformManager.SpiralGalaxyTintMultConstant;
                    break;
                }
            }

        }
        void Update()
        { }
    }
}