// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

namespace Doozy.Runtime.Soundy
{
    /// <summary>
    /// Describes the current state of an audio player
    /// </summary>
    public enum PlayState
    {
        /// <summary>
        /// The audio player was just created.
        /// This state is used to initialize the audio player.
        /// </summary>
        Created,

        /// <summary> The audio player is in the pool, waiting to be used </summary>
        InPool,
        
        /// <summary> The audio player is idle </summary>
        Idle,
        
        /// <summary> The audio player is playing </summary>
        Playing,
        
        /// <summary> The audio player is paused </summary>
        Paused,
        
        /// <summary> The audio player is stopped </summary>
        Stopped
    }
}
