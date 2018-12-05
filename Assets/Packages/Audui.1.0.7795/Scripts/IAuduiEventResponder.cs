//-----------------------------------------------------------------------
// <copyright file="IAuduiEventResponder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MRS.Audui
{
    /// <summary>
    /// An interface enabling interested objects to be notified when a UI Audio event occurs.
    /// </summary>
    public interface IAuduiEventResponder
    {
        void HandleAuduiEvent(AuduiEventData eventData);
    }
}
