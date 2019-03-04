//-----------------------------------------------------------------------
// <copyright file="AuduiEventData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MRS.Audui
{
    /// <summary>
    /// The types of UI activity we are interested in.
    /// </summary>
    public enum UiAction
    {
        None,
        Focus,
        Blur,
        ActionStarted,
        ActionEnded,
        PrimaryAction,
        SecondaryAction
    }

    /// <summary>
    /// A wrapper around an action and a consumption flag, passed to event responders.
    /// Note that the naming and usage pattern of 'used' & Use() aligns with that of
    /// MRTK input event data.
    /// </summary>
    public class AuduiEventData
    {
        /// <summary>
        /// Read-only action type; set once via constructor.
        /// </summary>
        public UiAction action { get; private set; }

        /// <summary>
        /// Read-only consumption flag; see Use().
        /// </summary>
        public bool used { get; private set; }

        /// <summary>
        /// Construct a data instance, given an action.
        /// </summary>
        /// <param name="a">The action to be handled.</param>
        public AuduiEventData(UiAction a)
        {
            action = a;
            used = false;
        }

        /// <summary>
        /// Call to mark the event as consumed.
        /// </summary>
        public void Use()
        {
            used = true;
        }
    }
}
