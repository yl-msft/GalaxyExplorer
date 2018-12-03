// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

public interface IMouseHandler
{
    void OnMouseClickDown(GameObject clicker);
    void OnMouseOverObject(GameObject clicker);
    void OnMouseExitObject();
}
