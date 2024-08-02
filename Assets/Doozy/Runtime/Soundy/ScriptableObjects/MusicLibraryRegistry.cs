// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.Common.Attributes;
using Doozy.Runtime.Common.Extensions;
using Doozy.Runtime.Common.Utils;
using Doozy.Runtime.Soundy.ScriptableObjects.Internal;
using UnityEngine;
// ReSharper disable UnusedMember.Global

namespace Doozy.Runtime.Soundy.ScriptableObjects
{
    /// <summary>
    /// Collection of all Music Libraries in the project.
    /// This database is used only in the Unity Editor, to get references to MusicObjects by their library name and audio name.
    /// </summary>
    public class MusicLibraryRegistry : LibraryRegistry<MusicLibraryRegistry>
    {
        /// <summary> All the Music Libraries in the project </summary>
        [SerializeField] private List<MusicLibrary> Libraries = new List<MusicLibrary>();
        /// <summary> All the Music Libraries in the project </summary>
        public List<MusicLibrary> libraries => Libraries;

        #if UNITY_EDITOR

        [RestoreData(nameof(MusicLibraryRegistry))]
        public static MusicLibraryRegistry Get() =>
            instance;

        [RefreshData(nameof(MusicLibraryRegistry))]
        public static void RefreshData() =>
            instance.Refresh();

        public override void Refresh(bool saveAssets = false, bool refreshAssetDatabase = false)
        {
            // Debug.Log($"{nameof(MusicLibraryRegistry)} -> Refresh");
            Libraries ??= new List<MusicLibrary>();
            Libraries.Clear();
            string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(MusicLibrary)}");
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                MusicLibrary library = UnityEditor.AssetDatabase.LoadAssetAtPath<MusicLibrary>(path);
                if (library == null) continue;
                library.Validate();
                Libraries.Add(library);
            }
            Libraries.Sort((a, b) => string.Compare(a.libraryName, b.libraryName, System.StringComparison.Ordinal));
            UnityEditor.EditorUtility.SetDirty(this);
            if (saveAssets) UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            if (refreshAssetDatabase) UnityEditor.AssetDatabase.Refresh();
        }

        public static MusicLibrary CreateLibrary()
        {
            string path = UnityEditor.EditorUtility.OpenFolderPanel("Select a folder to save the Music Library", "Assets", "");
            if (path.IsNullOrEmpty())
            {
                Debug.Log($"{ObjectNames.NicifyVariableName(nameof(CreateLibrary))} -> User canceled the operation or the selected path is invalid");
                return null;
            }
            path = PathUtils.ToRelativePath(path);
            MusicLibrary library = CreateInstance<MusicLibrary>();
            library.libraryName = GetUniqueLibraryName(library.libraryName);
            library.name = library.assetFileName;
            UnityEditor.AssetDatabase.CreateAsset(library, path + $"/{library.name}.asset");
            UnityEditor.AssetDatabase.Refresh();
            instance.Libraries.Add(library);
            Validate();
            return library;
        }

        public static void DeleteLibrary(MusicLibrary library)
        {
            if (library == null) return;

            if (!UnityEditor.EditorUtility.DisplayDialog
                (
                    "Delete Music Library",
                    $"Are you sure you want to delete the '{library.libraryName}' Music Library?\n" +
                    $"This action cannot be undone!",
                    "Yes",
                    "No"
                )
               )
                return;

            (bool libraryRemovedFromBuild, string _) = library.RemoveLibraryFromBuild();
            instance.Libraries.Remove(library);

            if (libraryRemovedFromBuild)
            {
                UnityEditor.EditorUtility.SetDirty(SoundLibraryDatabase.instance);
                UnityEditor.AssetDatabase.SaveAssetIfDirty(SoundLibraryDatabase.instance);
            }
            UnityEditor.EditorUtility.SetDirty(instance);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(instance);
            UnityEditor.AssetDatabase.DeleteAsset(UnityEditor.AssetDatabase.GetAssetPath(library));
            UnityEditor.AssetDatabase.Refresh();

            Validate();
        }

        /// <summary>
        /// Get a music object by its name and its library name
        /// <para/> This method searches for the first MusicLibrary with the given name.
        /// Then it searches for the MusicObject with the given name in that library.
        /// And returns the MusicObject reference if found, otherwise it returns null.
        /// </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="musicName"> Music name </param>
        /// <returns> MusicObject reference if found, otherwise it returns null </returns>
        public static MusicObject GetMusicObject(string libraryName, string musicName)
        {
            MusicLibrary library = GetLibrary(libraryName);
            return library == null ? null : library.GetMusicObject(musicName);
        }

        private static string GetUniqueLibraryName(string libraryName = SoundySettings.k_DefaultLibraryName)
        {
            libraryName = libraryName.CleanName();
            int index = 0;
            while (Contains(libraryName))
            {
                index++;
                libraryName = $"{SoundySettings.k_DefaultLibraryName} {index}";
            }
            return libraryName;
        }

        /// <summary> Check if a music library is already registered with the given name </summary>
        /// <param name="libraryName"> Library name </param>
        /// <returns> TRUE if the music library, with the given name, is already registered </returns>
        public static bool Contains(string libraryName) =>
            GetLibrary(libraryName) != null;

        /// <summary> Check if a music library is already registered </summary>
        /// <param name="library"> MusicLibrary reference </param>
        /// <returns> TRUE if the music library, with the given name, is already registered </returns>
        public static bool Contains(MusicLibrary library) =>
            Contains(library.libraryName);

        /// <summary> Get a MusicLibrary by its name </summary>
        /// <param name="libraryName"> Library name </param>
        /// <returns> MusicLibrary reference, if found. Null otherwise </returns>
        public static MusicLibrary GetLibrary(string libraryName)
        {
            if (libraryName == null)
                return null;

            libraryName = libraryName.CleanName();

            if (libraryName.IsNullOrEmpty())
                return null;

            MusicLibrary musicLibrary = null;
            bool foundNull = false;
            for (int i = 0; i < instance.Libraries.Count; i++)
            {
                MusicLibrary library = instance.Libraries[i];
                if (library == null)
                {
                    foundNull = true;
                    continue;
                }

                //compare names, but ignore case
                if (library.libraryName.CleanName().Equals(libraryName, StringComparison.OrdinalIgnoreCase))
                {
                    musicLibrary = library;
                    break;
                }
            }

            if (foundNull)
            {
                instance.Libraries.RemoveNulls();
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(instance);
                UnityEditor.AssetDatabase.SaveAssetIfDirty(instance);
                #endif
            }

            return musicLibrary;
        }

        /// <summary>
        /// Get a MusicLibrary by its GUID.
        /// This method works only in the Editor (it uses UnityEditor.AssetDatabase).
        /// Returns null if the GUID is empty or if the SoundLibrary was not found.
        /// </summary>
        /// <param name="guid"> MusicLibrary GUID </param>
        /// <returns> MusicLibrary reference, if found. Null otherwise </returns>
        public static MusicLibrary GetLibraryByGuid(string guid)
        {
            if (guid.IsNullOrEmpty()) return null;
            foreach (MusicLibrary library in instance.Libraries)
            {
                if (library == null) continue;
                string libraryGuid = UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(library));
                if (libraryGuid.Equals(guid))
                    return library;
            }
            return null;
        }

        /// <summary> Remove all null references from the libraries list and sort the list alphabetically </summary>
        public static void Validate()
        {
            RemoveNulls();
            RemoveDuplicates();
            Sort();
        }

        /// <summary> Remove all null references from the libraries list </summary>
        private static void RemoveNulls()
        {
            if (instance == null) return;
            instance.Libraries = instance.Libraries.RemoveNulls();
        }

        /// <summary> Sort the libraries list alphabetically </summary>
        private static void Sort()
        {
            if (instance == null) return;
            instance.Libraries.Sort((a, b) => string.Compare(a.libraryName, b.libraryName, System.StringComparison.Ordinal));
        }

        /// <summary> Remove all duplicate libraries from the libraries list </summary>
        private static void RemoveDuplicates()
        {
            if (instance == null) return;
            for (int i = 0; i < instance.Libraries.Count; i++)
            {
                MusicLibrary library = instance.Libraries[i];
                UnityEditor.GUID libraryGuid = UnityEditor.AssetDatabase.GUIDFromAssetPath(UnityEditor.AssetDatabase.GetAssetPath(library));
                if (library == null) continue;
                for (int j = i + 1; j < instance.Libraries.Count; j++)
                {
                    MusicLibrary otherLibrary = instance.Libraries[j];
                    if (otherLibrary == null) continue;
                    UnityEditor.GUID otherLibraryGuid = UnityEditor.AssetDatabase.GUIDFromAssetPath(UnityEditor.AssetDatabase.GetAssetPath(otherLibrary));
                    //compare the GUIDs instead of the library names to avoid issues with duplicate library names
                    if (!libraryGuid.Equals(otherLibraryGuid)) continue;
                    instance.Libraries.RemoveAt(j);
                    j--;
                }
            }
        }

        /// <summary> Get a list of all the registered MusicLibraries (unsorted). </summary>
        /// <returns> List of all the registered MusicLibraries (unsorted) </returns>
        public static List<string> GetLibraryNames() =>
            (from library in instance.Libraries where library != null select library.libraryName).ToList();

        /// <summary> Get a list of all the registered AudioNames (unsorted). </summary>
        /// <param name="libraryName"> Library name that contains the audio names </param>
        /// <returns> List of all the registered AudioNames (unsorted) </returns>
        public static List<string> GetAudioNames(string libraryName)
        {
            var library = GetLibrary(libraryName);
            return library == null ? new List<string>() : library.GetAudioNames();
        }

        #endif //UNITY_EDITOR
    }
}
