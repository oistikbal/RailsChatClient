// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using Doozy.Runtime.Common.Attributes;
using Doozy.Runtime.Common.Extensions;
using Doozy.Runtime.Common.ScriptableObjects;
using UnityEngine;

namespace Doozy.Runtime.Soundy.ScriptableObjects
{
    /// <summary>
    /// Collection of Sound libraries that will get added to the build.
    /// This database is used only in the Build, to get a references to SoundObjects by their library name and sound name.
    /// </summary>
    [Serializable]
    public class SoundLibraryDatabase : SingletonRuntimeScriptableObject<SoundLibraryDatabase>
    {
        [SerializeField] private List<SoundLibrary> Libraries = new List<SoundLibrary>();

        #if UNITY_EDITOR
        [RefreshData(nameof(SoundLibraryDatabase))]
        public static void RefreshData() =>
            instance.Refresh();

        /// <summary>
        /// Refresh the database by clearing all the libraries that are not valid anymore (null) and removing any duplicates
        /// </summary>
        public void Refresh()
        {
            // Debug.Log($"[{nameof(SoundLibraryDatabase)}] Refreshing...");
            Libraries ??= new List<SoundLibrary>();
            Libraries.RemoveNulls();
            for (int i = 0; i < Libraries.Count; i++)
            {
                SoundLibrary library = Libraries[i];
                if (library == null) continue;
                for (int j = i + 1; j < Libraries.Count; j++)
                {
                    SoundLibrary otherLibrary = Libraries[j];
                    if (otherLibrary == null) continue;
                    if (library.name.Equals(otherLibrary.name))
                    {
                        Libraries.RemoveAt(j);
                        j--;
                    }
                }
            }
            Libraries.Sort((a, b) => string.Compare(a.libraryName, b.libraryName, StringComparison.InvariantCultureIgnoreCase));
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
        }
        #endif

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

        /// <summary> Get a SoundLibrary by its name </summary>
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

        /// <summary> Check if a SoundLibrary with the given name exists in the database </summary>
        /// <param name="libraryName"> Library name </param>
        /// <returns> True if a SoundLibrary with the given name exists in the database </returns>
        public static bool ContainsLibrary(string libraryName)
        {
            if (libraryName == null)
                return false;

            libraryName = libraryName.CleanName();

            if (libraryName.IsNullOrEmpty())
                return false;

            bool result = false;
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
                if (library.name.Equals(libraryName, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = true;
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

            return result;
        }

        /// <summary> Check if a SoundLibrary exists in the database </summary>
        /// <param name="library"> SoundLibrary reference </param>
        /// <returns> True if the SoundLibrary exists in the database </returns>
        public static bool ContainsLibrary(SoundLibrary library) =>
            library != null && instance.Libraries.Contains(library);

        /// <summary>
        /// Add a Sound Library to the database.
        /// These libraries will be added to the build and will be available at runtime on any platform.
        /// </summary>
        /// <param name="library"> SoundLibrary reference </param>
        /// <returns> True if the SoundLibrary was added successfully </returns>
        public static (bool success, string message) AddLibrary(SoundLibrary library)
        {
            if (library == null)
                return (false, "Cannot add a null Sound Library to the database");

            if (library.libraryName == null || library.libraryName.CleanName().IsNullOrEmpty())
                return (false, $"The '{library.name}.asset' Sound Library has no name");

            if (ContainsLibrary(library))
                return (false, $"The '{library.name}.asset' Sound Library has already been added to the database, with the name '{library.libraryName}'");

            if (ContainsLibrary(library.libraryName))
                return
                    (
                        false,
                        $"Cannot add the '{library.name}.asset' Sound Library to the database. " +
                        $"There is already a Sound Library with the name '{library.libraryName}' in the database. " +
                        $"The other library is '{GetLibrary(library.libraryName).name}.asset'"
                    );

            #if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(instance, "Add Sound Library");
            #endif

            instance.Libraries.Add(library);

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(instance);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(instance);
            #endif

            return (true, $"The ''{library.name}.asset' Sound Library, with name '{library.libraryName}', was added to the database");
        }

        /// <summary> Remove a Sound Library from the database </summary>
        /// <param name="library"> SoundLibrary reference </param>
        /// <returns> True if the SoundLibrary was removed successfully from the database </returns>
        public static (bool success, string message) RemoveLibrary(SoundLibrary library)
        {
            if (library == null)
                return (false, "Cannot remove a null Sound Library from the database");

            if (!ContainsLibrary(library))
                return (false, $"The '{library.name}.asset' Sound Library is not in the database");

            instance.Libraries.Remove(library);
            return (true, $"The '{library.name}.asset' Sound Library was removed from the database");
        }

        /// <summary> Remove a Sound Library from the database </summary>
        /// <param name="libraryName"> SoundLibrary name </param>
        /// <returns> True if the SoundLibrary was removed successfully </returns>
        public static (bool success, string message) RemoveLibrary(string libraryName)
        {
            if (libraryName == null)
                return (false, "Cannot remove a null Sound Library from the database");

            libraryName = libraryName.CleanName();

            if (libraryName.IsNullOrEmpty())
                return (false, "Cannot remove a Sound Library with a null or empty name from the database");

            if (!ContainsLibrary(libraryName))
                return (false, $"The '{libraryName}.asset' Sound Library is not in the");

            SoundLibrary library = GetLibrary(libraryName);
            instance.Libraries.Remove(library);
            return (true, $"The '{libraryName}.asset' Sound Library was removed from the database");
        }

        /// <summary> Remove all Sound Libraries from the database </summary>
        public static void ClearLibraries() =>
            instance.Libraries.Clear();

        /// <summary>
        /// Remove all null references from the database and sort the libraries alphabetically by name.
        /// </summary>
        public static void Validate()
        {
            RemoveNulls();
            Sort();
        }

        /// <summary> Remove null references from the database </summary>
        private static void RemoveNulls()
        {
            if (instance == null) return;
            instance.Libraries = instance.Libraries.RemoveNulls();
        }

        /// <summary> Sort the libraries alphabetically by name </summary>
        private static void Sort()
        {
            if (instance == null) return;
            instance.Libraries.Sort((a, b) => string.Compare(a.libraryName, b.libraryName, StringComparison.Ordinal));
        }
    }
}
