// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.Common.Extensions;
using Doozy.Runtime.Soundy.ScriptableObjects.Internal;
using UnityEngine;
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Doozy.Runtime.Soundy.ScriptableObjects
{
    /// <summary>
    /// Audio library that holds a list of music objects.
    /// <para/> This is a ScriptableObject and should be created in the Project view or from the Dashboard in the Soundy section.
    /// </summary>
    [CreateAssetMenu(fileName = "MusicLibrary", menuName = "Doozy/Soundy/Music Library", order = -950)]
    public class MusicLibrary : AudioLibrary
    {
        /// <summary> Internal file name used to save this library as an asset </summary>
        internal string assetFileName => $"{libraryName} {nameof(MusicLibrary)}";

        /// <summary> All the music objects in this library </summary>
        [SerializeField] private List<MusicObject> Data = new List<MusicObject>();
        /// <summary> All the music objects in this library </summary>
        public List<MusicObject> data
        {
            get => Data ?? (Data = new List<MusicObject>());
            private set => Data = value;
        }

        /// <summary>
        /// Include this library in the build (to be made available at runtime on the target platform).
        /// By adding this library to the build, all the music objects it contains will be included in the build.
        /// </summary>
        public override (bool success, string message) AddLibraryToBuild() =>
            MusicLibraryDatabase.AddLibrary(this);

        /// <summary>
        /// Remove this library from the build (to be made unavailable at runtime on the target platform).
        /// By removing a library from the build, all the music objects in that library will NOT be included in the build.
        /// </summary>
        public override (bool success, string message) RemoveLibraryFromBuild() =>
            MusicLibraryDatabase.RemoveLibrary(this);

        /// <summary>
        /// Check if this library is included in the build (to be made available at runtime on the target platform).
        /// </summary>
        public override bool IsLibraryAddedToBuild() =>
            MusicLibraryDatabase.ContainsLibrary(this);


        /// <summary>
        /// Create a new music object and add it to this library
        /// </summary>
        /// <param name="save"> Save the library after adding the new music object </param>
        /// <returns> The newly created music object </returns>
        public MusicObject AddNew(bool save = true)
        {
            #if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Add New Music");
            #endif

            return AddNewNoUndo(save);
        }
        
        /// <summary>
        /// Create a new music object and add it to this library (without undo).
        /// </summary>
        /// <param name="save"> Save the library after adding the new music object </param>
        /// <returns> The newly created music object </returns>
        public MusicObject AddNewNoUndo(bool save = true)
        {
            MusicObject musicObject = CreateInstance<MusicObject>();
            musicObject.name = SoundySettings.k_DefaultAudioName;
            musicObject.library = this;
            musicObject.audioName = musicObject.name;
            Data.Insert(0, musicObject);

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(musicObject);
            UnityEditor.AssetDatabase.AddObjectToAsset(musicObject, this);
            UnityEditor.EditorUtility.SetDirty(this);
            if (save)
            {
                UnityEditor.AssetDatabase.SaveAssetIfDirty(musicObject);
                UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            }
            #endif

            return musicObject;
        }
        
        /// <summary>
        /// Create a new music object for each of the given audio clips and add them to this library.
        /// </summary>
        /// <param name="clips"> Audio clips to create music objects for and to add to this library </param>
        public void AddNew(params AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0) return;

            #if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Add New Music");
            #endif

            foreach (AudioClip clip in clips)
            {
                if (clip == null) continue;
                MusicObject musicObject = AddNewNoUndo(false);
                musicObject.SetClip(clip);
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
        /// Remove the given music object from this library.
        /// If the music object is not part of this library, nothing will happen.
        /// </summary>
        /// <param name="musicObject"> Music object to remove </param>
        public void Remove(MusicObject musicObject)
        {
            if(musicObject == null) return;
            
            #if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Remove Sound");
            UnityEditor.AssetDatabase.RemoveObjectFromAsset(musicObject);
            #endif

            Data.Remove(musicObject);

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            #endif
        }

        /// <summary>
        /// Remove the music object with the given name from this library.
        /// If there are multiple music objects with the same name, only the first one will be removed.
        /// </summary>
        /// <param name="musicName"> Music name to remove </param>
        public void Remove(string musicName)
        {
            MusicObject musicObject = GetMusicObject(musicName);
            if (musicObject == null) return;
            Remove(musicObject);
        }
        
        /// <summary> Clear all the music objects from this library </summary>
        public override void ClearLibrary()
        {
            #if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Clear Music Library");
            foreach (MusicObject musicObject in Data.Where(item => item != null)) 
                UnityEditor.AssetDatabase.RemoveObjectFromAsset(musicObject);
            #endif

            Data.Clear();

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            #endif
        }
        
        /// <summary> Check if this library contains the given music object </summary>
        /// <param name="musicObject"> Music object to check </param>
        /// <returns> Returns TRUE if the music object is found in this library </returns>
        public bool Contains(MusicObject musicObject) =>
            Data.Contains(musicObject);
        
        /// <summary> Check if this library contains a music object with the given name </summary>
        /// <param name="musicName"> Music name to check </param>
        /// <returns> Returns TRUE if a music object with the given name is found in this library </returns>
        public bool Contains(string musicName) =>
            GetMusicObject(musicName) != null;
        
        /// <summary> Get a music object from this library by its name </summary>
        /// <param name="musicName"> Music name to search for </param>
        /// <returns> Returns the music object with the given name, or NULL if not found </returns>
        public MusicObject GetMusicObject(string musicName)
        {
            if(musicName == null) 
                return null;
            
            musicName = musicName.CleanName();

            if (string.IsNullOrEmpty(musicName))
                return null;

            if (musicName.Equals(SoundySettings.k_DefaultAudioName))
                return null;

            MusicObject result = null;
            bool foundNull = false;

            for (int i = 0; i < Data.Count; i++)
            {
                MusicObject musicObject = Data[i];
                if (musicObject == null)
                {
                    foundNull = true;
                    continue;
                }
                
                //compare names, but ignore case
                if (!musicObject.audioName.CleanName().Equals(musicName, StringComparison.OrdinalIgnoreCase))
                    continue;

                result = musicObject;
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
                MusicObject musicObject = Data[i];
                for (int j = i + 1; j < Data.Count; j++)
                {
                    MusicObject otherMusicObject = Data[j];
                    if (!musicObject.audioName.Equals(otherMusicObject.audioName))
                        continue;
                    Data.RemoveAt(j);
                    j--;
                }
            }

            //validate music objects
            for (int i = 0; i < Data.Count; i++)
            {
                MusicObject musicObject = Data[i];
                if (musicObject == null) continue;
                musicObject.library = this;
                musicObject.Validate();
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
        /// Sort the music objects in this library alphabetically in ascending order
        /// </summary>
        private void SortAz() =>
            data = data.OrderBy(musicObject => musicObject.audioName).ToList();
        
        /// <summary> Sort the music objects in this library alphabetically in ascending order </summary>
        /// <param name="save"> Whether to save the asset. </param>
        /// <returns> Returns the library </returns>
        public MusicLibrary SortByNameAz(bool save = true) =>
            Do("Sort Sounds A-Z", () => data.Sort((a, b) => string.Compare(a.audioName, b.audioName, StringComparison.Ordinal)), save);

        /// <summary> Sort the music objects in this library alphabetically in descending order </summary>
        /// <param name="save"> Whether to save the asset. </param>
        /// <returns> Returns the library </returns>
        public MusicLibrary SortByNameZa(bool save = true) =>
            Do("Sort Sounds Z-A", () => data.Sort((a, b) => string.Compare(b.audioName, a.audioName, StringComparison.Ordinal)), save);
        
        /// <summary> Perform an action on library and record it in the undo system. </summary>
        /// <param name="undoName"> The name of the undo action. </param>
        /// <param name="action"> The action to perform. </param>
        /// <param name="save"> Whether to save the asset. </param>
        /// <returns> Returns the library </returns>
        private MusicLibrary Do(string undoName, Action action, bool save = true)
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
            foreach (MusicObject musicObject in Data)
            {
                if (musicObject == null) continue;
                if (musicObject.audioName == SoundySettings.k_DefaultAudioName) continue;
                audioNames.Add(musicObject.audioName);
            }
            audioNames.Sort();
            audioNames = audioNames.Distinct().ToList();
            return audioNames;
        }
    }
}
