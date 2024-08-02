// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using Doozy.Runtime.Soundy.ScriptableObjects;
using UnityEngine.Audio;

namespace Doozy.Runtime.Soundy.Ids
{
    [Serializable]
    public class MusicLibraryId : AudioLibraryId
    {
        /// <summary> Check if the library name is valid (it exists in the database) </summary>
        public override bool isLibraryNameValid => 
            !string.IsNullOrEmpty(libraryName) &&
            !libraryName.Equals(SoundySettings.k_DefaultLibraryName) &&
            !libraryName.Equals(SoundySettings.k_None) &&
            GetMusicLibrary() != null;
        
        /// <summary>
        /// Create a new MusicLibraryId with the default library name
        /// </summary>
        public MusicLibraryId() : this(SoundySettings.k_None)
        {
        }
        
        /// <summary>
        /// Create a new MusicLibraryId with the given library name
        /// </summary>
        /// <param name="libraryName"> Name of the MusicLibrary </param>
        public MusicLibraryId(string libraryName)
        {
            this.libraryName = libraryName;
        }
        
        // <summary> Get the AudioMixerGroup from the MusicLibrary that this MusicLibraryId is referencing </summary>
        /// <returns> AudioMixerGroup reference if found, null otherwise </returns>
        public AudioMixerGroup GetOutputAudioMixerGroup() =>
            GetMusicLibrary()?.OutputAudioMixerGroup;
        
        /// <summary> Get the corresponding MusicLibrary for this MusicLibraryId </summary>
        /// <returns> MusicLibrary reference if found, null otherwise </returns>
        public MusicLibrary GetMusicLibrary() =>
            SoundyService.GetMusicLibrary(libraryName);
    }
}