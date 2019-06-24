// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;

namespace TouchScript.Examples.Portal
{
    /// <exclude />
    public class Vortex : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var planet = other.GetComponent<Planet>();
            if (planet == null) return;
            planet.Fall();
        }
    }
}