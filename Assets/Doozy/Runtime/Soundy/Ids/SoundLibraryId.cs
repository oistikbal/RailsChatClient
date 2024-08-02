// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using Doozy.Runtime.Soundy.ScriptableObjects;
using UnityEngine.Audio;

namespace Doozy.Runtime.Soundy.Ids
{
    [Serializable]
    public class SoundLibraryId : AudioLibraryId
    {
        /// <summary> Check if the library name is valid (it exists in the database) </summary>
        public override bool isLibraryNameValid => 
            !string.IsNullOrEmpty(libraryName) &&
            !libraryName.Equals(SoundySettings.k_DefaultLibraryName) &&
            !libraryName.Equals(SoundySettings.k_None) &&
            GetSoundLibrary() != null;

        /// <summary>
        /// Create a new SoundLibraryId with the default library name
        /// </summary>
        public SoundLibraryId() : this(SoundySettings.k_None)
        {
        }
        
        /// <summary>
        /// Create a new SoundLibraryId with the given library name
        /// </summary>
        /// <param name="libraryName"> Name of the SoundLibrary </param>
        public SoundLibraryId(string libraryName)
        {
            this.libraryName = libraryName;
        }
        
        /// <summary> Get the AudioMixerGroup from the SoundLibrary that this SoundLibraryId is referencing </summary>
        /// <returns> AudioMixerGroup reference if found, null otherwise </returns>
        public AudioMixerGroup GetOutputAudioMixerGroup() =>
            GetSoundLibrary()?.OutputAudioMixerGroup;
        
        /// <summary> Get the corresponding SoundLibrary for this SoundLibraryId </summary>
        /// <returns> SoundLibrary reference if found, null otherwise </returns>
        public SoundLibrary GetSoundLibrary() =>
            SoundyService.GetSoundLibrary(libraryName);
    }
}
