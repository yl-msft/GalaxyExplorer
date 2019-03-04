using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BlackHoleToggleSkybox : MonoBehaviour
{
    public Material BlackHoleMaterial;
    public bool SkyboxEnabled = true;

    private void Update()
    {
        if (BlackHoleMaterial == null)
            return;

        if(SkyboxEnabled)
        {
            BlackHoleMaterial.EnableKeyword("_SKYBOX_ENABLED");
        }
        else
        {
            BlackHoleMaterial.DisableKeyword("_SKYBOX_ENABLED");
        }
    }
}
