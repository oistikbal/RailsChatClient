// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System.Collections.Generic;
using Doozy.Editor.Common.Extensions;
using Doozy.Editor.Common.ScriptableObjects;
using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.Components;
using Doozy.Editor.Soundy.Windows;
using Doozy.Runtime.Common.Extensions;
using Doozy.Runtime.Soundy.Ids;
using Doozy.Runtime.Soundy.ScriptableObjects;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Doozy.Editor.Soundy.Drawers
{
    [CustomPropertyDrawer(typeof(MusicLibraryId), true)]
    public class MusicLibraryIdDrawer : AudioLibraryIdDrawer
    {
        private static List<string> libraryNames { get; } = new List<string>();

        protected override List<string> GetLibraryNames() =>
            MusicLibraryRegistry.GetLibraryNames();
        
         public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            base.CreatePropertyGUI(property);
            var id = property.GetTargetObjectOfProperty() as MusicLibraryId;

            openLibraryWindowButton
                .SetIcon(EditorSpriteSheets.Soundy.Icons.MusicLibrary)
                .SetTooltip("Open Music Libraries Window")
                .SetOnClick(() =>
                {
                    MusicLibraryWindow.Open();
                    MusicLibraryWindow.instance.SelectLibrary(id.libraryName);
                });

            libraryNameLabel.SetText("Music Library");

            SerializedProperty propertyLibraryName = property.FindPropertyRelative("LibraryName");

            if (propertyLibraryName.stringValue == null || propertyLibraryName.stringValue.CleanName().IsNullOrEmpty())
            {
                propertyLibraryName.stringValue = SoundySettings.k_None;
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                property.serializedObject.Update();
            }

            FluidButton libraryNameButton = GetLibraryNameButton();

            libraryNameButton.SetOnClick(() =>
            {
                var searchWindowContext = new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition));
                DynamicSearchProvider dsp =
                    ScriptableObject.CreateInstance<DynamicSearchProvider>()
                        .AddItems(GetLibrarySearchMenuItems(propertyLibraryName, GetLibraryNames, () =>
                        {
                            ValidateLibraryName();
                        }));
                SearchWindow.Open(searchWindowContext, dsp);
            });

            drawer.schedule.Execute(() => UpdateButtonNames(propertyLibraryName, libraryNameButton)).Every(200);
            UpdateButtonNames(propertyLibraryName, libraryNameButton);
            Compose(drawer, container, libraryNameLabel, libraryNameButton, openLibraryWindowButton);

            drawer.schedule.Execute(() =>
            {
                ValidateLibraryName();

            }).Every(Random.Range(1000, 2000));

            void ValidateLibraryName()
            {
                libraryNames.Clear();
                libraryNames.AddRange(GetLibraryNames());
                bool libraryNameIsValid = propertyLibraryName.stringValue != SoundySettings.k_None && libraryNames.Contains(propertyLibraryName.stringValue);
                if (libraryNameIsValid)
                {
                    libraryNameButton.ResetAccentColor();
                    return;
                }
                libraryNameButton.SetAccentColor(EditorSelectableColors.Help.ErrorText);
            }

            ValidateLibraryName();
            return drawer;
        }
    }
}
