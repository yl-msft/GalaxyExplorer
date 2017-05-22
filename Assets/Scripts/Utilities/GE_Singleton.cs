// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class GE_Singleton<T> : MonoBehaviour where T : GE_Singleton<T>
    {
        private static T _Instance;

        public static T Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = FindObjectOfType<T>();
                }
                return _Instance;
            }
        }
    }
}