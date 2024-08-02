// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using Doozy.Runtime.Soundy.ScriptableObjects;
using UnityEngine.Audio;

namespace Doozy.Runtime.Soundy.Ids
{
    [Serializable]
    public class SoundId : AudioId
    {
        /// <summary> Check if the library name is valid (it exists in the database) </summary>
        public override bool isLibraryNameValid => 
            !string.IsNullOrEmpty(libraryName) &&
            !libraryName.Equals(SoundySettings.k_DefaultLibraryName) &&
            !libraryName.Equals(SoundySettings.k_None) &&
            GetSoundLibrary() != null;
        
        /// <summary> Check if the audio name is valid (it exists in the database) </summary>
        public override bool isAudioNameValid =>
            !string.IsNullOrEmpty(audioName) &&
            !audioName.Equals(SoundySettings.k_DefaultAudioName) &&
            !audioName.Equals(SoundySettings.k_None) &&
            GetSoundObject() != null;

        /// <summary> Check if the library name and audio name are valid (they exist in the database) and if the audio object (SoundObject or MusicObject) is valid </summary>
        public override bool isValid => 
            isLibraryNameValid && isAudioNameValid;
        
        /// <summary>
        /// Create a new SoundId with the default library name and audio name
        /// </summary>
        public SoundId() : this(SoundySettings.k_None, SoundySettings.k_None)
        {
        }
        
        /// <summary>
        /// Create a new SoundId with the given library name and audio name
        /// </summary>
        /// <param name="libraryName"> Name of the SoundLibrary </param>
        /// <param name="audioName"> Name of the SoundObject </param>
        public SoundId(string libraryName, string audioName)
        {
            this.libraryName = libraryName;
            this.audioName = audioName;
        }
        
        /// <summary> Get the AudioMixerGroup from the SoundLibrary that this SoundId is referencing </summary>
        /// <returns> AudioMixerGroup reference if found, null otherwise </returns>
        public AudioMixerGroup GetOutputAudioMixerGroup() =>
            GetSoundLibrary()?.OutputAudioMixerGroup;
        
        /// <summary> Get the corresponding SoundLibrary for this SoundId </summary>
        /// <returns> SoundLibrary reference if found, null otherwise </returns>
        public SoundLibrary GetSoundLibrary() =>
            SoundyService.GetSoundLibrary(libraryName);
        
        /// <summary> Get the corresponding SoundObject for this SoundId </summary>
        /// <returns> SoundObject reference if found, null otherwise </returns>
        public SoundObject GetSoundObject() =>
            SoundyService.GetSoundObject(libraryName, audioName);
    }
}
