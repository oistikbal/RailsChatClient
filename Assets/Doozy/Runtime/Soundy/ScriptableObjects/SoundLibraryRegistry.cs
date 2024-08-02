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
using ObjectNames = Doozy.Runtime.Common.Utils.ObjectNames;
// ReSharper disable UnusedMember.Global

namespace Doozy.Runtime.Soundy.ScriptableObjects
{
    /// <summary>
    /// Collection of all Sound Libraries in the project.
    /// This database is used only in the Unity Editor, to get references to SoundObjects by their library name and audio name.
    /// </summary>
    public class SoundLibraryRegistry : LibraryRegistry<SoundLibraryRegistry>
    {
        /// <summary> All the Sound Libraries in the project </summary>
        [SerializeField] private List<SoundLibrary> Libraries = new List<SoundLibrary>();
        /// <summary> All the Sound Libraries in the project </summary>
        public List<SoundLibrary> libraries => Libraries;

        #if UNITY_EDITOR

        [RestoreData(nameof(SoundLibraryRegistry))]
        public static SoundLibraryRegistry Get() =>
            instance;

        [RefreshData(nameof(SoundLibraryRegistry))]
        public static void RefreshData() =>
            instance.Refresh();

        public override void Refresh(bool saveAssets = false, bool refreshAssetDatabase = false)
        {
            // Debug.Log($"{nameof(SoundLibraryRegistry)} -> Refresh");
            Libraries ??= new List<SoundLibrary>();
            Libraries.Clear();
            string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(SoundLibrary)}");
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                SoundLibrary library = UnityEditor.AssetDatabase.LoadAssetAtPath<SoundLibrary>(path);
                if (library == null) continue;
                library.Validate();
                Libraries.Add(library);
            }
            Sort();
            UnityEditor.EditorUtility.SetDirty(this);
            if (saveAssets) UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            if (refreshAssetDatabase) UnityEditor.AssetDatabase.Refresh();
        }

        public static SoundLibrary CreateLibrary()
        {
            string path = UnityEditor.EditorUtility.OpenFolderPanel("Select a folder to save the Sound Library", "Assets", "");
            if (path.IsNullOrEmpty())
            {
                Debug.Log($"{ObjectNames.NicifyVariableName(nameof(CreateLibrary))} -> User canceled the operation or the selected path is invalid");
                return null;
            }
            path = PathUtils.ToRelativePath(path);
            SoundLibrary library = CreateInstance<SoundLibrary>();
            library.libraryName = GetUniqueLibraryName(library.libraryName);
            library.name = library.assetFileName;
            UnityEditor.AssetDatabase.CreateAsset(library, path + $"/{library.name}.asset");
            UnityEditor.AssetDatabase.Refresh();
            instance.Libraries.Add(library);
            Validate();
            return library;
        }

        public static void DeleteLibrary(SoundLibrary library)
        {
            if (library == null) return;

            if (!UnityEditor.EditorUtility.DisplayDialog
                (
                    "Delete Sound Library",
                    $"Are you sure you want to delete the '{library.libraryName}' Sound Library?\n" +
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

        /// <summary>
        /// Get a sound object by its name and its library name.
        /// <para/> This method searches for the first SoundLibrary with the given name.
        /// Then it searches for the SoundObject with the given name in that library.
        /// And returns the SoundObject reference if found, otherwise returns null.
        /// </summary>
        /// <param name="libraryName"> Library name </param>
        /// <param name="soundName"> Sound name </param>
        /// <returns> SoundObject reference if found, otherwise returns null </returns>
        public static SoundObject GetSoundObject(string libraryName, string soundName)
        {
            SoundLibrary library = GetLibrary(libraryName);
            return library == null ? null : library.GetSoundObject(soundName);
        }

        /// <summary> Check if a sound library is already registered with the given name </summary>
        /// <param name="libraryName"> Library name </param>
        /// <returns> TRUE if the sound library, with the given name, is already registered </returns>
        public static bool Contains(string libraryName) =>
            GetLibrary(libraryName) != null;

        /// <summary> Check if a sound library is already registered </summary>
        /// <param name="library"> SoundLibrary reference </param>
        /// <returns> TRUE if the sound library, with the given name, is already registered </returns>
        public static bool Contains(SoundLibrary library) =>
            Contains(library.libraryName);

        /// <summary>
        /// Get a SoundLibrary by its name.
        /// Returns the first SoundLibrary with the given name.
        /// Returns null if the name is empty or if the SoundLibrary was not found.
        /// </summary>
        /// <param name="libraryName"> Library name </param>
        /// <returns> SoundLibrary reference, if found. Null otherwise </returns>
        public static SoundLibrary GetLibrary(string libraryName)
        {
            if (libraryName == null)
                return null;

            libraryName = libraryName.CleanName();

            if (libraryName.IsNullOrEmpty())
                return null;

            SoundLibrary soundLibrary = null;
            bool foundNull = false;
            for (int i = 0; i < instance.Libraries.Count; i++)
            {
                SoundLibrary library = instance.Libraries[i];
                if (library == null)
                {
                    foundNull = true;
                    continue;
                }

                //compare names, but ignore case
                if (library.libraryName.CleanName().Equals(libraryName, StringComparison.OrdinalIgnoreCase))
                {
                    soundLibrary = library;
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

            return soundLibrary;
        }

        /// <summary>
        /// Get a SoundLibrary by its GUID.
        /// This method works only in the Editor (it uses UnityEditor.AssetDatabase).
        /// Returns null if the GUID is empty or if the SoundLibrary was not found.
        /// </summary>
        /// <param name="guid"> SoundLibrary GUID </param>
        /// <returns> SoundLibrary reference, if found. Null otherwise </returns>
        public static SoundLibrary GetLibraryByGuid(string guid)
        {
            if (guid.IsNullOrEmpty()) return null;
            foreach (SoundLibrary library in instance.Libraries)
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
                SoundLibrary library = instance.Libraries[i];
                UnityEditor.GUID libraryGuid = UnityEditor.AssetDatabase.GUIDFromAssetPath(UnityEditor.AssetDatabase.GetAssetPath(library));
                if (library == null) continue;
                for (int j = i + 1; j < instance.Libraries.Count; j++)
                {
                    SoundLibrary otherLibrary = instance.Libraries[j];
                    if (otherLibrary == null) continue;
                    UnityEditor.GUID otherLibraryGuid = UnityEditor.AssetDatabase.GUIDFromAssetPath(UnityEditor.AssetDatabase.GetAssetPath(otherLibrary));
                    //compare the GUIDs instead of the library names to avoid issues with duplicate library names
                    if (!libraryGuid.Equals(otherLibraryGuid)) continue;
                    instance.Libraries.RemoveAt(j);
                    j--;
                }
            }
        }

        /// <summary> Get a list of all the registered SoundLibraries (unsorted). </summary>
        /// <returns> List of all the registered SoundLibraries (unsorted) </returns>
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
