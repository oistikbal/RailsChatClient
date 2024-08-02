// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Editor.EditorUI.Components;
using Doozy.Editor.EditorUI.Utils;
using Doozy.Runtime.Soundy.ScriptableObjects;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Doozy.Editor.Soundy.Drawers
{
    public abstract class AudioLibraryIdDrawer : PropertyDrawer
    {
        protected VisualElement drawer { get; set; }
        protected VisualElement container { get; set; }

        protected Label libraryNameLabel { get; set; }

        protected FluidButton openLibraryWindowButton { get; set; }

        protected abstract List<string> GetLibraryNames();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
        }

        protected FluidButton GetLibraryNameButton() =>
            AudioIdDrawer
                .GetButton()
                .SetStyleFlexShrink(0);

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            drawer = new VisualElement();
            container = DesignUtils.fieldContainer;

            openLibraryWindowButton = AudioIdDrawer.GetOpenWindowButton();

            libraryNameLabel = AudioIdDrawer.GetLabel().SetText("Library Name");

            return drawer;
        }

        protected static void Compose
        (
            VisualElement drawer,
            VisualElement container,
            Label libraryNameLabel,
            FluidButton libraryNameButton,
            FluidButton openLibraryWindowButton
        )
        {
            drawer
                .AddChild
                (
                    container
                        .AddChild
                        (
                            DesignUtils.column
                                .SetStyleFlexGrow(0)
                                .AddChild(libraryNameLabel)
                                .AddChild
                                (
                                    DesignUtils.row
                                        .SetStyleFlexGrow(0)
                                        .SetStyleAlignItems(Align.Center)
                                        .AddChild(libraryNameButton)
                                        .AddChild(openLibraryWindowButton)
                                )
                        )
                );
        }

        protected static void UpdateButtonNames(SerializedProperty propertyLibraryName, FluidButton libraryNameButton)
        {
            if (propertyLibraryName == null || libraryNameButton == null) return;

            string libraryName = propertyLibraryName.stringValue;
            libraryNameButton.SetLabelText(libraryName);

            // if the name of the music object is too long, we add a tooltip to the button and we truncate the name
            const int maxNameLength = 20;
            if (libraryNameButton.buttonLabel.text.Length > maxNameLength)
            {
                libraryNameButton.buttonLabel.text = libraryNameButton.buttonLabel.text.Substring(0, maxNameLength) + "...";
                libraryNameButton.SetTooltip(libraryName);
            }
        }
        
        /// <summary> Get a list of key value pairs used to populate the search menu </summary>
        /// <param name="propertyLibraryName"> SerializedProperty for the library name </param>
        /// <param name="getLibraryNames"> Callback used to get a list of library names </param>
        /// <param name="onLibraryChanged"> Callback invoked when the library name is changed </param>
        /// <returns> A list of key value pairs used to populate the search menu </returns>
        protected static List<KeyValuePair<string, UnityAction>> GetLibrarySearchMenuItems
        (
            SerializedProperty propertyLibraryName,
            Func<List<string>> getLibraryNames,
            UnityAction onLibraryChanged
        )
        {
            var keyValuePairsList = new List<KeyValuePair<string, UnityAction>>();
            var libraryNames = getLibraryNames?.Invoke() ?? new List<string>();

            if (libraryNames.Count == 0)
                return keyValuePairsList;

            libraryNames.Sort();
            libraryNames.Remove(SoundySettings.k_None);
            libraryNames = libraryNames.Distinct().ToList();
            libraryNames.Insert(0, SoundySettings.k_None);

            foreach (string libraryName in libraryNames)
            {
                keyValuePairsList.Add
                (
                    new KeyValuePair<string, UnityAction>
                    (
                        libraryName,
                        () =>
                        {
                            // bool libraryChanged = !libraryName.Equals(propertyLibraryName.stringValue);
                            propertyLibraryName.stringValue = libraryName;
                            propertyLibraryName.serializedObject.ApplyModifiedProperties();
                            propertyLibraryName.serializedObject.Update();
                            onLibraryChanged?.Invoke();
                        }
                    )
                );
            }

            return keyValuePairsList;
        }
    }
}
