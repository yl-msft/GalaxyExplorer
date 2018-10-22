// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GalaxyExplorer
{
    public interface ITouchHandler 
    {
        void OnHoldStarted();
        void OnHoldCompleted();
        void OnHoldCanceled();
    }
}
