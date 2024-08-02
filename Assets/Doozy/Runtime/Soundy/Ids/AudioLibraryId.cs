// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using Doozy.Runtime.Common.Extensions;
using Doozy.Runtime.Soundy.ScriptableObjects;
using UnityEngine;

namespace Doozy.Runtime.Soundy.Ids
{
    /// <summary>
    /// Base class for all audio library ids (sound and music)
    /// </summary>
    [Serializable]
    public abstract class AudioLibraryId
    {
        /// <summary> Check if the library name is valid (it exists in the database) </summary>
        public abstract bool isLibraryNameValid { get; }
        
        /// <summary> Check if the library name is valid (it exists in the database) </summary>
        public bool isValid => isLibraryNameValid;
        
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
        
        /// <summary> Set the library name to the default values </summary>
        public void Reset()
        {
            LibraryName = SoundySettings.k_None;
        }
        
        /// <summary> Set the library name to the given value </summary>
        /// <param name="newLibraryName"> Name of the SoundLibrary </param>
        public void Set(string newLibraryName)
        {
            libraryName = newLibraryName;
        }
        
        /// <summary> Set the library name to the values of the given AudioLibraryId </summary>
        /// <param name="audioLibraryId"> AudioLibraryId reference </param>
        public void Set(AudioLibraryId audioLibraryId)
        {
            libraryName = audioLibraryId.libraryName;
        }

        /// <summary> Set the library name to the value in the given AudioId </summary>
        /// <param name="audioId"> AudioId reference </param>
        public void Set(AudioId audioId)
        {
            libraryName = audioId.libraryName;
        }
    }
}
