// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using System.Collections;

namespace TouchScript.Examples
{
    /// <exclude />
    public class KillMe : MonoBehaviour
    {
        public float Delay = 1f;

        private IEnumerator Start()
        {
            if (Delay != 0) yield return new WaitForSeconds(Delay);
            Destroy(gameObject);
        }
    }
}