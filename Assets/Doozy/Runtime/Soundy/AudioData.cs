// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using Doozy.Runtime.Soundy.ScriptableObjects;
using UnityEngine;

namespace Doozy.Runtime.Soundy
{
    /// <summary> Base class for all audio data </summary>
    [Serializable]
    public abstract class AudioData
    {
        /// <summary>
        /// Check if this audio data can be played.
        /// </summary>
        public bool canPlay => hasClip;
        
        /// <summary>
        /// The audio clip that will be played when this audio data is used.
        /// </summary>
        public AudioClip Clip;

        /// <summary>
        /// The volume of the audio clip that will be played when this audio data is used.
        /// </summary>
        public float Volume = SoundySettings.k_DefaultVolume;

        /// <summary>
        /// The pitch of the audio clip that will be played when this audio data is used.
        /// </summary>
        public float Pitch = SoundySettings.k_DefaultPitch;

        /// <summary>
        /// Weight used for randomization.
        /// The higher the weight, the more chances this sound has to be picked when using randomization.
        /// </summary>
        public int Weight = SoundySettings.k_DefaultWeight;
        
        /// <summary> Check if this audio data has a clip assigned to it </summary>
        public bool hasClip => Clip != null;

        /// <summary> Validate the settings of this audio data and make sure they are within the accepted range </summary>
        public virtual void Validate()
        {
            Volume = Mathf.Clamp(Volume, SoundySettings.k_MinVolume, SoundySettings.k_MaxVolume);
            Pitch = Mathf.Clamp(Pitch, SoundySettings.k_MinPitch, SoundySettings.k_MaxPitch);
            Weight = Mathf.Clamp(Weight, SoundySettings.k_MinWeight, SoundySettings.k_MaxWeight);
        }
    }

    public static class AudioDataExtensions
    {
        /// <summary> Set the clip of this audio data </summary>
        /// <param name="audioData"> Target audio data </param>
        /// <param name="clip"> Audio data clip </param>
        /// <typeparam name="T"> AudioData type </typeparam>
        /// <returns> Target audio data </returns>
        public static T SetClip<T>(this T audioData, AudioClip clip) where T : AudioData
        {
            audioData.Clip = clip;
            return audioData;
        }

        /// <summary> Set the volume of this audio data  </summary>
        /// <param name="audioData"> Target audio data </param>
        /// <param name="volume"> Audio data volume </param>
        /// <typeparam name="T"> AudioData type </typeparam>
        /// <returns> Target audio data </returns>
        public static T SetVolume<T>(this T audioData, float volume) where T : AudioData
        {
            audioData.Volume = volume;
            return audioData;
        }

        /// <summary> Set the pitch of this audio data </summary>
        /// <param name="audioData"> Target audio data </param>
        /// <param name="pitch"> Audio data pitch </param>
        /// <typeparam name="T"> AudioData type </typeparam>
        /// <returns> Target audio data </returns>
        public static T SetPitch<T>(this T audioData, float pitch) where T : AudioData
        {
            audioData.Pitch = pitch;
            return audioData;
        }
        
        /// <summary> Sets the weight of this sound data </summary>
        /// <param name="soundData"> Target sound data </param>
        /// <param name="weight"> Weight value </param>
        /// <typeparam name="T"> SoundData type </typeparam>
        /// <returns> Target sound data </returns>
        public static T SetWeight<T>(this T soundData, int weight) where T : AudioData
        {
            soundData.Weight = weight;
            return soundData;
        }
    }
}
