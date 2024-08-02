// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

namespace Doozy.Runtime.Soundy
{
    /// <summary> Describes the types audio actions available for a single sound or music object </summary>
    public enum AudioActionType
    {
        /// <summary> No action </summary>
        DoNothing,
        
        /// <summary> Play this audio </summary>
        Play,
        
        /// <summary> Stop this audio </summary>
        Stop,
        
        /// <summary> Fade out and stop this audio </summary>
        FadeOutAndStop,
        
        /// <summary> Pause this audio </summary>
        Pause,
        
        /// <summary> UnPause this audio </summary>
        UnPause,
        
        /// <summary> Mute this audio </summary>
        Mute,
        
        /// <summary> UnMute this audio </summary>
        UnMute
    }
}
