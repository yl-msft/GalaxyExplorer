// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Examples.InteractiveElements;
using UnityEngine.Events;

// Galaxy Explorer button based on MRTK button
namespace GalaxyExplorer
{
    public class GEInteractiveToggle : InteractiveToggle
    {
        public UnityEvent OnGazeSelect;
        public UnityEvent OnGazeDeselect;


        public override void OnFocusEnter()
        {
            base.OnFocusEnter();

            if (((AllowDeselect && IsSelected) || !IsSelected) && !PassiveMode)
            {
                OnGazeSelect?.Invoke();
            }
        }

        public override void OnFocusExit()
        {
            base.OnFocusExit();

            if (((AllowDeselect && IsSelected) || !IsSelected) && !PassiveMode)
            {
                OnGazeDeselect?.Invoke();
            }
        }
    }
}
