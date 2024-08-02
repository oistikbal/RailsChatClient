// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using Doozy.Runtime.Soundy.ScriptableObjects;
using UnityEngine.Audio;

namespace Doozy.Runtime.Soundy.Ids
{
    /// <summary>
    /// Library Name and Audio Name pair that uniquely identifies a MusicObject.
    /// Id is used to find the corresponding MusicObject in the Music Registry (in the Editor) or in the Music Database (in the Build).
    /// </summary>
    [Serializable]
    public class MusicId : AudioId
    {
        /// <summary> Check if the library name is valid (it exists in the database) </summary>
        public override bool isLibraryNameValid =>
            !string.IsNullOrEmpty(libraryName) &&
            !libraryName.Equals(SoundySettings.k_DefaultLibraryName) &&
            !libraryName.Equals(SoundySettings.k_None) &&
            GetMusicLibrary() != null;

        /// <summary> Check if the audio name is valid (it exists in the database) </summary>
        public override bool isAudioNameValid =>
            !string.IsNullOrEmpty(audioName) &&
            !audioName.Equals(SoundySettings.k_DefaultAudioName) &&
            !audioName.Equals(SoundySettings.k_None) &&
            GetMusicObject() != null;

        /// <summary> Check if the library name and audio name are valid (they exist in the database) and if the audio object (SoundObject or MusicObject) is valid </summary>
        public override bool isValid =>
            isLibraryNameValid && isAudioNameValid;

        /// <summary>
        /// Create a new MusicId with the default library name and audio name
        /// </summary>
        public MusicId() : this(SoundySettings.k_None, SoundySettings.k_None)
        {
        }

        /// <summary>
        /// Create a new MusicId with the given library name and audio name
        /// </summary>
        /// <param name="libraryName"> Name of the MusicLibrary </param>
        /// <param name="audioName"> Name of the MusicObject </param>
        public MusicId(string libraryName, string audioName)
        {
            this.libraryName = libraryName;
            this.audioName = audioName;
        }

        /// <summary> Get the AudioMixerGroup from the MusicLibrary that this MusicId is referencing </summary>
        /// <returns> AudioMixerGroup reference if found, null otherwise </returns>
        public AudioMixerGroup GetOutputAudioMixerGroup() =>
            GetMusicLibrary()?.OutputAudioMixerGroup;

        /// <summary> Get the corresponding MusicLibrary for this MusicId </summary>
        /// <returns> MusicLibrary reference if found, null otherwise </returns>
        public MusicLibrary GetMusicLibrary() =>
            SoundyService.GetMusicLibrary(libraryName);

        /// <summary> Get the corresponding MusicObject for this MusicId </summary>
        /// <returns> MusicObject reference if found, null otherwise </returns>
        public MusicObject GetMusicObject() =>
            SoundyService.GetMusicObject(libraryName, audioName);
    }
}
