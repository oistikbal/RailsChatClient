// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

namespace Doozy.Runtime.Soundy
{
    /// <summary> Describes the types of actions available for all music </summary>
    public enum MusicActionType
    {
        /// <summary> No action </summary>
        DoNothing,
        
        /// <summary> Stop all music </summary>
        StopAllMusic,
        
        /// <summary> Fade out and stop all music </summary>
        FadeOutAndStopAllMusic,
        
        /// <summary> Pause all music </summary>
        PauseAllMusic,
        
        /// <summary> UnPause all music </summary>
        UnPauseAllMusic,
        
        /// <summary> Mute all music </summary>
        MuteAllMusic,
        
        /// <summary> UnMute all music </summary>
        UnMuteAllMusic
    }
}
