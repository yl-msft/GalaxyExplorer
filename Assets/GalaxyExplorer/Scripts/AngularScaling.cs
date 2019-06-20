// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class AngularScaling : MonoBehaviour
    {
        public float DefaultSizeDistance = 2f;
        public float MinSizeRatio = .5f;
        public float MaxSizeRatio = 6f;
        public float LerpSpeed = 6f;
        private Vector3 initialScale;
        private float lastSizeRatio = 1f;

        void Start()
        {
            initialScale = transform.localScale;
        }

        void Update()
        {
            float currentDistance = (this.transform.position - Camera.main.transform.position).magnitude;

            float newSizeRatio = currentDistance / DefaultSizeDistance;

            float targetSizeRatio = Mathf.Clamp(newSizeRatio, MinSizeRatio, MaxSizeRatio);
            newSizeRatio = Mathf.Lerp(lastSizeRatio, targetSizeRatio, Time.deltaTime * LerpSpeed);
            lastSizeRatio = newSizeRatio;

            this.transform.localScale = initialScale * newSizeRatio;
        }
    }
}
