// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace GalaxyExplorer
{
    public class GETouchScreenInputSource : Singleton<GETouchScreenInputSource>
    {
        private List<ITouchHandler> allTouchHandlers = new List<ITouchHandler>();

        public GameObject TouchedObject = null;

        public void RegisterTouchEntity(ITouchHandler touchHandler)
        {
            allTouchHandlers.Add(touchHandler);
        }

        public void UnRegisterTouchEntity(ITouchHandler touchHandler)
        {
            allTouchHandlers.Remove(touchHandler);
        }

        void Update()
        {
            int nbTouches = Input.touchCount;

            if (nbTouches > 0)
            {
                for (int i = 0; i < nbTouches; i++)
                {
                    Touch touch = Input.GetTouch(i);

                    if (touch.phase == TouchPhase.Ended && TouchedObject)
                    {
                        ITouchHandler touchHandler = TouchedObject.GetComponentInParent<ITouchHandler>();
                        if (touchHandler != null)
                        {
                            // Deactivate any open card descriptions that might be selected
                            DeactivateAllTouchHandlers(touchHandler);

                            touchHandler.OnHoldCompleted();
                            TouchedObject = null;
                        }
                    }
                    else if (touch.phase == TouchPhase.Began)
                    {
                        Ray screenRay = Camera.main.ScreenPointToRay(touch.position);

                        RaycastHit hit;
                        if (Physics.Raycast(screenRay, out hit))
                        {
                            TouchedObject = (hit.collider.gameObject.GetComponentInParent<ITouchHandler>() != null) ? hit.collider.gameObject : null;
                        }
                    }
                }
            }
        }

        // Deactivate all handlers that might be selected except the current selected one
        private void DeactivateAllTouchHandlers(ITouchHandler selected)
        {
            foreach (var handler in allTouchHandlers)
            {
                if (handler != selected)
                {
                    handler.OnHoldCanceled();
                }
            }
        }
    }
}
