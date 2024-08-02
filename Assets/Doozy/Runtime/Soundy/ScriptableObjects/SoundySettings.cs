// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using Doozy.Runtime.Common.Attributes;
using Doozy.Runtime.Common.ScriptableObjects;
using UnityEditor;

namespace Doozy.Runtime.Soundy.ScriptableObjects
{
    /// <summary>
    /// Global settings for Soundy
    /// </summary>
    [Serializable]
    public class SoundySettings : SingletonRuntimeScriptableObject<SoundySettings>
    {
        #if UNITY_EDITOR
        [MenuItem("Tools/Doozy/Refresh/Soundy", false, -450)]
        public static void Refresh()
        {
            Get();
            
            SoundLibraryRegistry.instance.Refresh();
            MusicLibraryRegistry.instance.Refresh();

            SoundLibraryDatabase.instance.Refresh();
            MusicLibraryDatabase.instance.Refresh();
        }
        #endif

        [RestoreData(nameof(SoundySettings))]
        public static SoundySettings Get() =>
            instance;
        
        public const string k_None = "None";
        public const string k_DefaultAudioName = "Unnamed";
        public const string k_DefaultLibraryName = "Unnamed";

        public const int k_MinPriority = 0;
        public const int k_MaxPriority = 256;
        public const int k_DefaultPriority = 128;

        public const float k_MinVolume = 0f;
        public const float k_MaxVolume = 1f;
        public const float k_DefaultVolume = 1f;

        public const float k_MinPitch = 0.2f;
        public const float k_MaxPitch = 3f;
        public const float k_DefaultPitch = 1f;

        public const float k_MinPanStereo = -1f;
        public const float k_MaxPanStereo = 1f;
        public const float k_DefaultPanStereo = 0f;

        public const float k_MinSpatialBlend = 0f;
        public const float k_MaxSpatialBlend = 1f;
        public const float k_DefaultSpatialBlend = 0f;

        public const float k_MinReverbZoneMix = 0f;
        public const float k_MaxReverbZoneMix = 1.1f;
        public const float k_DefaultReverbZoneMix = 1f;

        public const float k_MinDopplerLevel = 0f;
        public const float k_MaxDopplerLevel = 5f;
        public const float k_DefaultDopplerLevel = 1f;

        public const int k_MinSpread = 0;
        public const int k_MaxSpread = 360;
        public const int k_DefaultSpread = 0;

        public const float k_MinMinDistance = 0f;
        public const float k_MaxMinDistance = 1000f;
        public const float k_DefaultMinDistance = 1f;

        public const float k_MinMaxDistance = 0f;
        public const float k_MaxMaxDistance = 1000f;
        public const float k_DefaultMaxDistance = 500f;

        public const int k_MinWeight = 0;
        public const int k_MaxWeight = 100;
        public const int k_DefaultWeight = 100;

        public const bool k_DefaultLoop = false;
        public const bool k_DefaultIgnoreListenerPause = true;

        /// <summary> Default value for the Cross Fade for Music Players </summary>
        public const bool k_DefaultCrossFade = false;
        /// <summary> Default value for the Cross Fade Duration for Music Players </summary>
        public const float k_DefaultCrossFadeDuration = 0.5f;

        /// <summary>
        /// The full qualified type name of the audio player to use.
        /// This is used to create the audio players.
        /// The type must derive from BaseAudioPlayer.
        /// </summary>
        public string AudioPlayerFullQualifiedTypeName = Default.k_AudioPlayerFullQualifiedTypeName;

        /// <summary>
        /// Automatically destroy idle audio players.
        /// An audio player is considered idle if it hasn't been used for more than the IdleTime duration.
        /// </summary>
        public bool DestroyIdleAudioPlayers = Default.k_DestroyIdleAudioPlayers;

        /// <summary> The duration (in seconds) after which an audio player is considered idle, if it hasn't been used </summary>
        public float IdleTime = Default.k_IdleTime;

        /// <summary> The interval (in seconds) at which the idle audio players are checked and destroyed if they're idle </summary>
        public float IdleCheckInterval = Default.k_IdleCheckInterval;

        /// <summary>
        /// The minimum number of sound effect players to keep alive, even if they're idle.
        /// This means that, even if they're idle, they will not be destroyed if the number of sound effect players is less than this value.
        /// </summary>
        public int MinSoundPlayersToKeepAlive = Default.k_MinSoundPlayersToKeepAlive;

        /// <summary>
        /// The number of audio players, used to play sounds, to preheat (create and cache) when the AudioEngine starts.
        /// The audio players are created and cached in the background, so they don't affect the performance at runtime.
        /// </summary>
        public int PreheatSoundPlayers = Default.k_PreheatSoundPlayers;

        /// <summary>
        /// The minimum number of music players to keep alive, even if they're idle.
        /// This means that, even if they're idle, they will not be destroyed if the number of music players is less than this value.
        /// </summary>
        public int MinMusicPlayersToKeepAlive = Default.k_MinMusicPlayersToKeepAlive;

        /// <summary>
        /// The number of audio players, used to play music, to preheat (create and cache) when the AudioEngine starts.
        /// The audio players are created and cached in the background, so they don't affect the performance at runtime.
        /// </summary> 
        public int PreheatMusicPlayers = Default.k_PreheatMusicPlayers;

        public static class Default
        {
            public const bool k_DestroyIdleAudioPlayers = true;
            public const float k_IdleCheckInterval = 1f;
            public const float k_IdleTime = 1f;
            public const int k_MinSoundPlayersToKeepAlive = 20;
            public const int k_PreheatSoundPlayers = 20;
            public const int k_MinMusicPlayersToKeepAlive = 3;
            public const int k_PreheatMusicPlayers = 3;
            public const string k_AudioPlayerFullQualifiedTypeName = "Doozy.Runtime.Soundy.AudioSourcePlayer";
        }
    }
}
