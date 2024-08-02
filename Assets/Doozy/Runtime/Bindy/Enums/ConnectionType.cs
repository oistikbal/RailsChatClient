// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

namespace Doozy.Runtime.Bindy
{
    /// <summary>
    /// Describes the connection type between a Bindable and a Bind.
    /// </summary>
    public enum ConnectionType
    {
        /// <summary>
        /// Bindable only sends value updates to the Bind, when its value changes.
        /// It does not receive value updates from the Bind.
        /// </summary>
        Sender,

        /// <summary>
        /// Bindable sends value updates to the Bind, when its value changes.
        /// It also receives value updates from the Bind.
        /// </summary>
        Bidirectional,

        /// <summary>
        /// Bindable only receives value updates from the Bind.
        /// It does not send value updates to the Bind, when its value changes.
        /// </summary>
        Receiver
    }
}
