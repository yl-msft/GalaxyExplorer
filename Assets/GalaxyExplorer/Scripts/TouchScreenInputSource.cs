// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class TouchScreenInputSource : MonoBehaviour
    {

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

                            PointOfInterest touchHandler = hit.collider.gameObject.GetComponentInParent<PointOfInterest>();
                            if (touchHandler != null)
                            {
                                touchHandler.OnHoldCompleted();
                            }
                        }
                    }

                }
            }
        }
    }
}
