// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class MouseClickHandler : MonoBehaviour
    {

        [SerializeField]
        [Tooltip("Receiver of mouse click on this entity")]
        private GameObject Receiver = null;

        void OnMouseDown()
        { 
            if (Receiver != null)
            {
                IMouseHandler mousehandler = Receiver.GetComponent<IMouseHandler>();
                mousehandler?.OnMouseClickDown(gameObject);
            }
        }

        void OnMouseOver()
        {
            if (Receiver != null)
            {
                IMouseHandler mousehandler = Receiver.GetComponent<IMouseHandler>();
                mousehandler?.OnMouseOverObject(gameObject);
            }
        }

        void OnMouseExit()
        {
            if (Receiver != null)
            {
                IMouseHandler mousehandler = Receiver.GetComponent<IMouseHandler>();
                mousehandler?.OnMouseExitObject();
            }
        }


    }
}
