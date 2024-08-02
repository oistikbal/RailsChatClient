// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace Doozy.Runtime.Soundy.ScriptableObjects.Internal
{
    /// <summary> Base class for all Audio Library ScriptableObjects </summary>
    public abstract class AudioLibrary : ScriptableObject
    {
        /// <summary> The name of this library </summary>
        [SerializeField] private string Name;

        /// <summary> Library name </summary>
        public string libraryName
        {
            get => Name;
            set => Name = value;
        }
        
        /// <summary> Output Audio Mixer Group that will be used by audio players when playing audio clips from this library </summary>
        public AudioMixerGroup OutputAudioMixerGroup;

        /// <summary> Internal use only. Used by the Soundy system to know when this library has been updated and trigger editor updates. </summary>
        public UnityAction OnUpdate;
        
        protected AudioLibrary()
        {
            Name = SoundySettings.k_DefaultLibraryName;
            OutputAudioMixerGroup = null;
        }

        /// <summary> Add this library to the collection of libraries that will be included in the build </summary>
        /// <returns> Returns TRUE if the library was added successfully </returns>
        public abstract (bool success, string message) AddLibraryToBuild();

        /// <summary> Remove this library from the collection of libraries that will be included in the build </summary>
        /// <returns> Returns TRUE if the library was removed successfully </returns>
        public abstract (bool success, string message) RemoveLibraryFromBuild();

        /// <summary> Check if this library will be included in the build </summary>
        public abstract bool IsLibraryAddedToBuild();

        /// <summary> Validate the library </summary>
        public abstract void Validate();
     
        /// <summary> Clear the library </summary>
        public abstract void ClearLibrary();

        /// <summary> Get a list of all the audio names in this library, sorted alphabetically and without duplicates </summary>
        /// <returns> Returns a list of all the audio names in this library, sorted alphabetically and without duplicates </returns>
        public abstract List<string> GetAudioNames();
        
        /// <summary> Perform an action on library and record it in the undo system. </summary>
        /// <param name="undoName"> The name of the undo action. </param>
        /// <param name="action"> The action to perform. </param>
        /// <param name="save"> Whether to save the asset. </param>
        /// <returns> Returns the library. </returns>
        protected void RunAction(string undoName, Action action, bool save = true)
        {
            #if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, undoName);
            #endif

            action.Invoke();

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            if (save) UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            #endif

            OnUpdate?.Invoke();
        }
    }
}
