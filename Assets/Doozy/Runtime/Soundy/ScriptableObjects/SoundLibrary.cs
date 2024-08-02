// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.Common.Extensions;
using Doozy.Runtime.Soundy.ScriptableObjects.Internal;
using UnityEngine;
// ReSharper disable MemberCanBePrivate.Global

namespace Doozy.Runtime.Soundy.ScriptableObjects
{
    /// <summary>
    /// Audio library that holds a list of sound objects.
    /// <para/> This library is a ScriptableObject and should be created in the Project view or from the Dashboard in the Soundy section.
    /// </summary>
    [CreateAssetMenu(fileName = "SoundLibrary", menuName = "Doozy/Soundy/Sound Library", order = -950)]
    public class SoundLibrary : AudioLibrary
    {
        /// <summary> Internal file name used for saving the library as an asset </summary>
        internal string assetFileName => $"{libraryName} {nameof(SoundLibrary)}";

        /// <summary> All the sound objects in this library </summary>
        [SerializeField] private List<SoundObject> Data = new List<SoundObject>();
        /// <summary> All the sound objects in this library </summary>
        public List<SoundObject> data
        {
            get => Data ?? (Data = new List<SoundObject>());
            private set => Data = value;
        }

        /// <summary>
        /// Include this library in the build (to be made available at runtime on the target platform).
        /// By adding this library to the build, all the sound objects it contains will be included in the build.
        /// </summary>
        public override (bool success, string message) AddLibraryToBuild() =>
            SoundLibraryDatabase.AddLibrary(this);

        /// <summary>
        /// Remove this library from the build (to be made unavailable at runtime on the target platform).
        /// By removing a library from the build, all the sound objects in that library will NOT be included in the build.
        /// </summary>
        public override (bool success, string message) RemoveLibraryFromBuild() =>
            SoundLibraryDatabase.RemoveLibrary(this);

        /// <summary>
        /// Check if this library is included in the build (to be made available at runtime on the target platform).
        /// </summary>
        public override bool IsLibraryAddedToBuild() =>
            SoundLibraryDatabase.ContainsLibrary(this);

        /// <summary>
        /// Create a new sound object and add it to this library.
        /// </summary>
        /// <param name="save"> Save the library after adding the new sound object </param>
        /// <returns> The newly created sound object </returns>
        public SoundObject AddNew(bool save = true)
        {
            #if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Add New Sound");
            #endif

            return AddNewNoUndo(save);
        }

        /// <summary>
        /// Create a new sound object and add it to this library (without undo).
        /// </summary>
        /// <param name="save"> Save the library after adding the new sound object </param>
        /// <returns> The newly created sound object </returns>
        public SoundObject AddNewNoUndo(bool save = true)
        {
            SoundObject soundObject = CreateInstance<SoundObject>();
            soundObject.name = SoundySettings.k_DefaultAudioName;
            soundObject.library = this;
            soundObject.audioName = soundObject.name;
            Data.Insert(0, soundObject);

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(soundObject);
            UnityEditor.AssetDatabase.AddObjectToAsset(soundObject, this);
            UnityEditor.EditorUtility.SetDirty(this);
            if (save)
            {
                UnityEditor.AssetDatabase.SaveAssetIfDirty(soundObject);
                UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            }
            #endif

            return soundObject;
        }

        /// <summary>
        /// Create a new sound object for each of the given audio clips and add them to this library.
        /// </summary>
        /// <param name="clips"> Audio clips to create sound objects for and to add to this library </param>
        public void AddNew(params AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0) return;

            #if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Add New Sounds");
            #endif

            foreach (AudioClip clip in clips)
            {
                if (clip == null) continue;
                SoundObject soundObject = AddNewNoUndo(false);
                soundObject.AddNewNoSave(clip);
            }

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            #endif

            SortAz();

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            #endif

            OnUpdate?.Invoke();
        }

        /// <summary>
        /// Remove the given sound object from this library.
        /// If the sound object is not part of this library, nothing will happen.
        /// </summary>
        /// <param name="soundObject"> Sound object to remove </param>
        public void Remove(SoundObject soundObject)
        {
            if (soundObject == null) return;

            #if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Remove Sound");
            UnityEditor.AssetDatabase.RemoveObjectFromAsset(soundObject);
            #endif

            Data.Remove(soundObject);

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            #endif
        }

        /// <summary>
        /// Remove the sound object with the given name from this library.
        /// If there are multiple sound objects with the same name, only the first one will be removed.
        /// </summary>
        /// <param name="soundName"> Sound name to remove </param>
        public void Remove(string soundName)
        {
            SoundObject soundObject = GetSoundObject(soundName);
            if (soundObject == null) return;
            Remove(soundObject);
        }

        /// <summary> Clear all the sound objects from this library </summary>
        public override void ClearLibrary()
        {
            #if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Clear Sound Library");
            foreach (SoundObject soundObject in Data.Where(item => item != null))
                UnityEditor.AssetDatabase.RemoveObjectFromAsset(soundObject);
            #endif

            Data.Clear();

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            #endif
        }

        /// <summary> Check if this library contains the given sound object </summary>
        /// <param name="soundObject"> Sound object to check </param>
        /// <returns> Returns TRUE if the sound object is found in this library </returns>
        public bool Contains(SoundObject soundObject) =>
            Data.Contains(soundObject);

        /// <summary> Check if this library contains a sound object with the given name </summary>
        /// <param name="soundName"> Sound name to check </param>
        /// <returns> Returns TRUE if a sound object with the given name is found in this library </returns>
        public bool Contains(string soundName) =>
            GetSoundObject(soundName) != null;

        /// <summary> Get a sound object from this library by its name </summary>
        /// <param name="soundName"> Sound name to search for </param>
        /// <returns> Returns the sound object with the given name, or NULL if not found </returns>
        public SoundObject GetSoundObject(string soundName)
        {
            if (soundName.IsNullOrEmpty()) return null;
            soundName = soundName.CleanName();
            if (soundName.IsNullOrEmpty()) return null;
            if (soundName.Equals(SoundySettings.k_DefaultAudioName)) return null;
            if (soundName.Equals(SoundySettings.k_None)) return null;

            SoundObject result = null;
            bool foundNull = false;

            for (int i = 0; i < Data.Count; i++)
            {
                SoundObject soundObject = Data[i];
                if (soundObject == null)
                {
                    foundNull = true;
                    continue;
                }

                //compare names, but ignore case
                if (!soundObject.audioName.CleanName().Equals(soundName, StringComparison.OrdinalIgnoreCase))
                    continue;

                result = soundObject;
                break;
            }

            if (foundNull)
            {
                Data.RemoveNulls();

                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
                #endif
            }

            return result;
        }

        /// <summary> Validate the library data </summary>
        public override void Validate()
        {
            //check if the name is null or empty
            if (libraryName.IsNullOrEmpty())
                libraryName = SoundySettings.k_DefaultLibraryName;

            //remove any numbers and special characters from the beginning of the name
            libraryName = libraryName.CleanName();

            //remove nulls
            Data = Data.RemoveNulls();

            //remove duplicates
            for (int i = 0; i < Data.Count; i++)
            {
                SoundObject soundObject = Data[i];
                for (int j = i + 1; j < Data.Count; j++)
                {
                    SoundObject otherSoundObject = Data[j];
                    if (!soundObject.audioName.Equals(otherSoundObject.audioName))
                        continue;
                    Data.RemoveAt(j);
                    j--;
                }
            }

            //validate sound objects
            for (int i = 0; i < Data.Count; i++)
            {
                SoundObject soundObject = Data[i];
                if (soundObject == null) continue;
                soundObject.library = this;
                soundObject.Validate();
            }

            SortAz();

            #if UNITY_EDITOR
            if (name != assetFileName)
                UnityEditor.AssetDatabase.RenameAsset(UnityEditor.AssetDatabase.GetAssetPath(this), assetFileName);

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            #endif
        }

        /// <summary>
        /// Sort the sound objects in this library alphabetically in ascending order
        /// </summary>
        private void SortAz() =>
            data = data.OrderBy(soundObject => soundObject.audioName).ToList();

        /// <summary> Sort the sound objects in this library alphabetically in ascending order </summary>
        /// <param name="save"> Whether to save the asset. </param>
        /// <returns> Returns the library </returns>
        public SoundLibrary SortByNameAz(bool save = true) =>
            Do("Sort Sounds A-Z", () => data.Sort((a, b) => string.Compare(a.audioName, b.audioName, StringComparison.Ordinal)), save);

        /// <summary> Sort the sound objects in this library alphabetically in descending order </summary>
        /// <param name="save"> Whether to save the asset. </param>
        /// <returns> Returns the library </returns>
        public SoundLibrary SortByNameZa(bool save = true) =>
            Do("Sort Sounds Z-A", () => data.Sort((a, b) => string.Compare(b.audioName, a.audioName, StringComparison.Ordinal)), save);

        /// <summary> Perform an action on library and record it in the undo system. </summary>
        /// <param name="undoName"> The name of the undo action. </param>
        /// <param name="action"> The action to perform. </param>
        /// <param name="save"> Whether to save the asset. </param>
        /// <returns> Returns the library. </returns>
        private SoundLibrary Do(string undoName, Action action, bool save = true)
        {
            // RunAction(undoName, action, save);
            // return this;

             #if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, undoName);
            #endif

            action.Invoke();

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            if (save) UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            #endif

            OnUpdate?.Invoke();
            return this;
        }

        /// <summary> Get a list of all the audio names in this library, sorted alphabetically and without duplicates </summary>
        /// <returns> Returns a list of all the audio names in this library, sorted alphabetically and without duplicates </returns>
        public override List<string> GetAudioNames()
        {
            var audioNames = new List<string>();
            foreach (SoundObject soundObject in Data)
            {
                if (soundObject == null) continue;
                if (soundObject.audioName == SoundySettings.k_DefaultAudioName) continue;
                audioNames.Add(soundObject.audioName);
            }
            audioNames.Sort();
            audioNames = audioNames.Distinct().ToList();
            return audioNames;
        }

    }
}
