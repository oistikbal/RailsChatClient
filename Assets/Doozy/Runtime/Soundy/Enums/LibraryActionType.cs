// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

namespace Doozy.Runtime.Soundy
{
    /// <summary> Describes the audio actions available for a library </summary>
    public enum LibraryActionType
    {
        /// <summary> No action </summary>
        DoNothing, 
        
        /// <summary> Stop all audio in this library </summary>
        StopLibrary,
        
        /// <summary> Fade out and stop all audio in this library </summary>
        FadeOutAndStopLibrary,
        
        /// <summary> Pause all audio in this library </summary>
        PauseLibrary, 
        
        /// <summary> UnPause all audio in this library </summary>
        UnPauseLibrary, 
        
        /// <summary> Mute all audio in this library </summary>
        MuteLibrary, 
        
        /// <summary> UnMute all audio in this library </summary>
        UnMuteLibrary
    }
}
