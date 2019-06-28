using System;
using System.Collections;
using System.Collections.Generic;
using GalaxyExplorer;
using UnityEngine;

public class PlatformGameObjectEnabler : MonoBehaviour
{
   [SerializeField] private bool shouldEnable;
   [SerializeField] private List<GalaxyExplorerManager.PlatformId> targetPlatforms;

   private void Awake()
   {
      if (targetPlatforms.Contains(GalaxyExplorerManager.Platform))
      {
         gameObject.SetActive(shouldEnable);
      }
   }
}
