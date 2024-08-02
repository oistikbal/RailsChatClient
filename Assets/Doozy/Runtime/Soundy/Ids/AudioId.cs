// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using Doozy.Runtime.Common.Extensions;
using Doozy.Runtime.Soundy.ScriptableObjects;
using UnityEngine;
// ReSharper disable PartialTypeWithSinglePart

namespace Doozy.Runtime.Soundy.Ids
{
    /// <summary>
    /// Base class for all audio ids (SoundId and MusicId)
    /// </summary>
    [Serializable]
    public abstract class AudioId
    {
        /// <summary> Check if the library name is valid (it exists in the database) </summary>
        public abstract bool isLibraryNameValid { get; }
        
        /// <summary> Check if the audio name is valid (it exists in the database) </summary>
        public abstract bool isAudioNameValid { get; }
        
        /// <summary> Check if the library name and audio name are valid (they exist in the database) and if the audio object (SoundObject or MusicObject) is valid </summary>
        public abstract bool isValid { get; }
        
        [SerializeField] private string LibraryName = SoundySettings.k_None;
        /// <summary> Sound Library Name where this audio is located </summary>
        public string libraryName
        {
            get => LibraryName;
            set
            {
                string newName = value.CleanName();
                newName = newName.IsNullOrEmpty() ? SoundySettings.k_None : newName;
                LibraryName = newName;
            }
        }
        
        [SerializeField] private string AudioName = SoundySettings.k_None;
        /// <summary> Audio Name from the Sound Library </summary>
        public string audioName
        {
            get => AudioName;
            set
            {
                string newName = value.CleanName();
                newName = newName.IsNullOrEmpty() ? SoundySettings.k_None : newName;
                AudioName = newName;
            }
        }

        /// <summary> Set the library name and audio name to the default values </summary>
        public void Reset()
        {
            LibraryName = SoundySettings.k_None;
            AudioName = SoundySettings.k_None;
        }
        
        /// <summary> Set the library name and audio name to the given values </summary>
        /// <param name="newLibraryName"> Name of the SoundLibrary </param>
        /// <param name="newAudioName"> Name of the SoundObject </param>
        public void Set(string newLibraryName, string newAudioName)
        {
            libraryName = newLibraryName;
            audioName = newAudioName;
        }
     
        /// <summary> Set the library name and audio name to the values of the given AudioId </summary>
        /// <param name="audioId"> AudioId reference </param>
        public void Set(AudioId audioId)
        {
            libraryName = audioId.libraryName;
            audioName = audioId.audioName;
        }

        /// <summary>
        /// Get the library name and audio name in a single string.
        /// <para/> LibraryName - AudioName
        ///  </summary>
        public override string ToString() =>
            $"{libraryName} - {audioName}";
    }
}
