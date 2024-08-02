// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

namespace Doozy.Runtime.Soundy
{
    /// <summary>
    /// Describes the types of actions available for all sounds
    /// </summary>
    public enum SoundActionType
    {
        /// <summary> No action </summary>
        DoNothing,
        
        /// <summary> Stop all sounds </summary>
        StopAllSounds,
        
        /// <summary> Fade out and stop all sounds </summary>
        FadeOutAndStopAllSounds,
        
        /// <summary> Pause all sounds </summary>
        PauseAllSounds,
        
        /// <summary> UnPause all sounds </summary>
        UnPauseAllSounds,
        
        /// <summary> Mute all sounds </summary>
        MuteAllSounds,
        
        /// <summary> UnMute all sounds </summary>
        UnMuteAllSounds
    }
}
