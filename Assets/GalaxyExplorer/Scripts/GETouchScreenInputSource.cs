// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;

namespace GalaxyExplorer
{
    public class GETouchScreenInputSource : MonoBehaviour
    {
        private List<ITouchHandler> allTouchHandlers = new List<ITouchHandler>();

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

                    if (touch.phase == TouchPhase.Ended)
                    {
                        Ray screenRay = Camera.main.ScreenPointToRay(touch.position);

                        RaycastHit hit;
                        if (Physics.Raycast(screenRay, out hit))
                        {
                            print("User tapped on game object " + hit.collider.gameObject.name);

                            ITouchHandler touchHandler = hit.collider.gameObject.GetComponentInParent<ITouchHandler>();
                            if (touchHandler != null)
                            {
                                // Deactivate any open card descriptions that might be selected
                                DeactivateAllTouchHandlers(touchHandler);

                                touchHandler.OnHoldCompleted();
                            }
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
