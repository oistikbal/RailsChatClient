// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.Components;
using Doozy.Editor.EditorUI.Utils;
using Doozy.Runtime.Common.Extensions;
using Doozy.Runtime.Soundy.ScriptableObjects;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Doozy.Editor.Soundy.Drawers
{
    public abstract class AudioIdDrawer : PropertyDrawer
    {
        private const int k_LibraryNameButtonWidth = 136;

        protected VisualElement drawer { get; set; }
        protected VisualElement container { get; set; }

        protected Label libraryNameLabel { get; set; }

        protected Label audioNameLabel { get; set; }

        protected FluidButton openLibraryWindowButton { get; set; }
        protected FluidButton openAssetEditorButton { get; set; }

        protected SoundyEditorPlayer.MiniPlayerElement playerElement { get; set; }

        protected abstract List<string> GetLibraryNames();
        protected abstract List<string> GetAudioNames(string libraryName);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
        }

        protected FluidButton GetLibraryNameButton() =>
            GetButton()
                .SetStyleWidth(k_LibraryNameButtonWidth, k_LibraryNameButtonWidth, k_LibraryNameButtonWidth)
                .SetStyleFlexShrink(0);

        protected FluidButton GetAudioNameButton() =>
            GetButton();

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            drawer = new VisualElement();
            container = DesignUtils.fieldContainer;

            openLibraryWindowButton = GetOpenWindowButton();
            openAssetEditorButton = GetOpenWindowButton();

            libraryNameLabel = GetLabel().SetText("Library Name");
            audioNameLabel = GetLabel().SetText("Audio Name");

            playerElement =
                new SoundyEditorPlayer.MiniPlayerElement()
                    .SetStyleLeft(-1)
                    .SetStyleMarginLeft(-20)
                    .SetStyleFlexShrink(0);

            return drawer;
        }

        protected static void Compose
        (
            VisualElement drawer,
            VisualElement container,
            Label libraryNameLabel,
            FluidButton libraryNameButton,
            FluidButton openLibraryWindowButton,
            Label audioNameLabel,
            FluidButton audioNameButton,
            SoundyEditorPlayer.MiniPlayerElement playerElement,
            FluidButton openAssetEditorButton
        )
        {
            drawer
                .AddChild
                (
                    container
                        .AddChild
                        (
                            DesignUtils.row
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
                                .AddSpaceBlock()
                                .AddChild(DesignUtils.dividerVertical)
                                .AddSpaceBlock()
                                .AddChild
                                (
                                    DesignUtils.column
                                        .AddChild(audioNameLabel)
                                        .AddChild
                                        (
                                            DesignUtils.row
                                                .SetStyleAlignItems(Align.Center)
                                                .AddChild(audioNameButton)
                                                .AddChild(playerElement)
                                                .AddChild(openAssetEditorButton)
                                        )
                                )
                        )
                );
        }

        protected static void UpdateButtonNames(SerializedProperty propertyLibraryName, SerializedProperty propertyAudioName, FluidButton libraryNameButton, FluidButton audioNameButton)
        {
            if (propertyLibraryName == null || propertyAudioName == null || libraryNameButton == null || audioNameButton == null) return;

            string libraryName = propertyLibraryName.stringValue;
            libraryNameButton.SetLabelText(libraryName);

            // if the name of the music object is too long, we add a tooltip to the button and we truncate the name
            const int maxNameLength = 20;
            if (libraryNameButton.buttonLabel.text.Length > maxNameLength)
            {
                libraryNameButton.buttonLabel.text = libraryNameButton.buttonLabel.text.Substring(0, maxNameLength) + "...";
                libraryNameButton.SetTooltip(libraryName);
            }

            audioNameButton.SetLabelText(propertyAudioName.stringValue);
        }

        /// <summary> Get a label used to describe the field </summary>
        /// <returns> A new label </returns>
        protected internal static Label GetLabel() =>
            DesignUtils.fieldLabel;

        /// <summary> Get a button used to select the sound library name or the sound name </summary>
        /// <returns> A new button </returns>
        protected internal static FluidButton GetButton()
        {
            FluidButton button =
                FluidButton.Get()
                    .SetLabelText(SoundySettings.k_None)
                    .SetButtonStyle(ButtonStyle.Contained)
                    .SetElementSize(ElementSize.Small)
                    .SetStyleFlexGrow(1)
                    .SetStyleFlexShrink(0);

            button.buttonLabel.SetStyleTextAlign(TextAnchor.MiddleLeft);

            return button;
        }

        /// <summary> Get a button used to open a popup window </summary>
        /// <returns> A new button </returns>
        protected internal static FluidButton GetOpenWindowButton() =>
            DesignUtils.Buttons.OpenDatabaseButton()
                .SetButtonStyle(ButtonStyle.Contained)
                .SetElementSize(ElementSize.Small)
                .SetStyleMargins(0)
                .SetStyleMarginLeft(DesignUtils.k_Spacing);


        /// <summary> Get a list of key value pairs used to populate the search menu </summary>
        /// <param name="propertyLibraryName"> SerializedProperty for the library name </param>
        /// <param name="propertyAudioName"> SerializedProperty for the audio name </param>
        /// <param name="getLibraryNames"> Callback used to get a list of library names </param>
        /// <param name="onLibraryChanged"> Callback invoked when the library name is changed </param>
        /// <returns> A list of key value pairs used to populate the search menu </returns>
        protected static List<KeyValuePair<string, UnityAction>> GetLibrarySearchMenuItems
        (
            SerializedProperty propertyLibraryName,
            SerializedProperty propertyAudioName,
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
                            bool libraryChanged = !libraryName.Equals(propertyLibraryName.stringValue);
                            propertyLibraryName.stringValue = libraryName;
                            if (libraryChanged) propertyAudioName.stringValue = SoundySettings.k_None;
                            propertyLibraryName.serializedObject.ApplyModifiedProperties();
                            propertyLibraryName.serializedObject.Update();
                            onLibraryChanged?.Invoke();
                        }
                    )
                );
            }

            return keyValuePairsList;
        }

        /// <summary> Get a list of key value pairs used to populate the search menu </summary>
        /// <param name="libraryNameProperty"> SerializedProperty for the library name </param>
        /// <param name="audioNameProperty"> SerializedProperty for the audio name </param>
        /// <param name="getAudioNames"> Callback used to get a list of audio names </param>
        /// <param name="onAudioNameChanged"> Callback invoked when the audio name is changed </param>
        /// <returns> A list of key value pairs used to populate the search menu </returns>
        protected static IEnumerable<KeyValuePair<string, UnityAction>> GetAudioSearchMenuItems
        (
            SerializedProperty libraryNameProperty,
            SerializedProperty audioNameProperty,
            Func<string, List<string>> getAudioNames,
            UnityAction onAudioNameChanged
        )
        {
            var keyValuePairsList = new List<KeyValuePair<string, UnityAction>>();
            string libraryName = libraryNameProperty.stringValue;

            if (libraryName.IsNullOrEmpty() || libraryName.Equals(SoundySettings.k_None))
                return keyValuePairsList;

            List<string> audioNames = getAudioNames?.Invoke(libraryName) ?? new List<string>();

            if (audioNames.Count == 0)
                return keyValuePairsList;

            audioNames.Sort();
            audioNames.Remove(SoundySettings.k_None);
            audioNames.Insert(0, SoundySettings.k_None);

            foreach (string audioName in audioNames)
            {
                keyValuePairsList.Add
                (
                    new KeyValuePair<string, UnityAction>
                    (
                        audioName,
                        () =>
                        {
                            audioNameProperty.stringValue = audioName;
                            audioNameProperty.serializedObject.ApplyModifiedProperties();
                            audioNameProperty.serializedObject.Update();
                            onAudioNameChanged?.Invoke();
                        }
                    )
                );
            }

            return keyValuePairsList;
        }
    }
}
