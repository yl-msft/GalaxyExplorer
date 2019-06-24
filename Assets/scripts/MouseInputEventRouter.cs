// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.Events;

public class MouseInputEventRouter : MonoBehaviour
    {
        [Tooltip("Interactable to which the press events are being routed. Defaults to the object of the component.")]
        public Interactable routingTarget;

        public UnityEvent OnClick;

        RaycastHit[] raycastResults = new RaycastHit[1];
        private bool mouseDown;
        

        private void Awake()
        {
            if (routingTarget == null)
            {
                routingTarget = GetComponent<Interactable>();
            }
        }

        public void OnHandleClick()
        {
            if (routingTarget != null)
            {
                routingTarget.SetPhysicalTouch(true);
                routingTarget.SetPress(true);
                routingTarget.OnPointerClicked(null);
                routingTarget.SetPress(false);
            }
            else
            {
                OnClick?.Invoke();
            }
        }
        
        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && !mouseDown)
            {
                if (!mouseDown)
                {
                    mouseDown = true;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    var layerMask = 1 << LayerMask.NameToLayer("Default");
                    if(Physics.RaycastNonAlloc(ray, raycastResults, float.PositiveInfinity, layerMask ) > 0)
                    {
                        foreach (var raycastHit in raycastResults)
                        {
                            if (raycastHit.collider.gameObject == gameObject)
                            {
                                    OnHandleClick();
                            }
                        }

                    }
                }
            }
            else
            {
                mouseDown = false;
            }
        }
    }