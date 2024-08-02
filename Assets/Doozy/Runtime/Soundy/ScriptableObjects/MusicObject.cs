// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using Doozy.Runtime.Common.Extensions;
using Doozy.Runtime.Soundy.ScriptableObjects.Internal;
using UnityEngine;

namespace Doozy.Runtime.Soundy.ScriptableObjects
{
    /// <summary>
    /// Audio Object used to play music.
    /// This gets referenced by a MusicLibrary and can be played using an AudioPlayer.
    /// </summary>
    [Serializable]
    public class MusicObject : AudioObject
    {
        /// <summary> Music Library that holds this music object </summary>
        public MusicLibrary musicLibrary => (MusicLibrary)library;
        
        /// <summary> Music data used to play the music </summary>
        [SerializeField] private MusicData Data = new MusicData();
        /// <summary> Music data used to play the music </summary>
        public MusicData data
        {
            get => Data ?? (Data = new MusicData());
            private set => Data = value;
        }

        /// <summary> Check if this music object can be played (if it has an audio clip) </summary>
        public override bool canPlay => Data != null && Data.canPlay;

        /// <summary> Sets the music clip </summary>
        /// <param name="clip"> Music clip </param>
        /// <returns> Self (useful for chaining) </returns>
        public MusicObject SetClip(AudioClip clip)
        {
            data.Clip = clip;

            bool setAudioClipNameAsMusicName = audioName.IsNullOrEmpty() || audioName.Equals(SoundySettings.k_DefaultAudioName);
            if (setAudioClipNameAsMusicName)
            {
                if (clip == null) return this;
                string newName = clip.name.CleanName();
                if (newName.IsNullOrEmpty()) return this;
                audioName = newName;
                name = audioName;
            }

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif

            return this;
        }

        /// <summary> Play the music using the provided player. </summary>
        /// <param name="player"> Audio player that will play the music </param>
        public void Play(AudioPlayer player)
        {
            if (data.canPlay)
            {
                Debug.LogWarning($"{nameof(MusicObject)} [{audioName}] is trying to play but it has no AudioClip set.");
                return;
            }

            player
                .SetClip(data.Clip)
                .SetVolume(GetVolume())
                .SetPitch(GetPitch())
                .SetPriority(priority)
                .SetPanStereo(panStereo)
                .SetSpatialBlend(spatialBlend)
                .SetReverbZoneMix(reverbZoneMix)
                .SetDopplerLevel(dopplerLevel)
                .SetSpread(spread)
                .SetMinDistance(minDistance)
                .SetMaxDistance(maxDistance)
                .SetLoop(loop)
                .SetIgnoreListenerPause(ignoreListenerPause)
                .Play();
        }

        /// <summary> Validate the music object </summary>
        public override void Validate()
        {
            base.Validate();
            Data ??= new MusicData();
            data.Validate();
        }
        
        /// <summary> Get the volume of the sound object for the next clip. </summary>
        /// <returns> Returns the volume of the sound object for the next clip adjusted by the volume of the sound data. </returns>
        public float GetVolume() =>
            data != null ? volume * data.Volume : volume;

        /// <summary> Get the pitch of the sound object for the next clip. </summary>
        /// <returns> Returns the pitch of the sound object for the next clip. </returns>
        public float GetPitch() =>
            data?.Pitch ?? SoundySettings.k_DefaultPitch;
    }
}