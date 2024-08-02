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
    [CustomPropertyDrawer(typeof(SoundId), true)]
    public class SoundIdDrawer : AudioIdDrawer
    {
        private static List<string> libraryNames { get; } = new List<string>();
        private static List<string> audioNames { get; } = new List<string>();

        protected override List<string> GetLibraryNames() =>
            SoundLibraryRegistry.GetLibraryNames();

        protected override List<string> GetAudioNames(string libraryName)
        {
            SoundLibrary library = SoundLibraryRegistry.GetLibrary(libraryName);
            return library == null ? new List<string>() : library.GetAudioNames();
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            base.CreatePropertyGUI(property);
            var id = property.GetTargetObjectOfProperty() as SoundId;

            openLibraryWindowButton
                .SetIcon(EditorSpriteSheets.Soundy.Icons.SoundLibrary)
                .SetTooltip("Open Sound Libraries Window")
                .SetOnClick(() =>
                {
                    SoundLibraryWindow.Open();
                    SoundLibraryWindow.instance.SelectLibrary(id.libraryName);
                });

            openAssetEditorButton
                .SetIcon(EditorSpriteSheets.Soundy.Icons.Sound)
                .SetTooltip("Open Sound Object Editor")
                .SetOnClick(() =>
                {
                    var soundObject = id.GetSoundObject();
                    if (soundObject == null)
                    {
                        EditorUtility.DisplayDialog
                        (
                            "No Sound Object Found",
                            $"No Sound Object found for {id.libraryName} - {id.audioName}",
                            "Ok"
                        );
                        return;
                    }
                    SoundObjectPopupWindow.Open().LoadAsset(soundObject);
                });

            libraryNameLabel.SetText("Sound Library");
            audioNameLabel.SetText("Sound Name");

            SerializedProperty propertyLibraryName = property.FindPropertyRelative("LibraryName");
            SerializedProperty propertyAudioName = property.FindPropertyRelative("AudioName");

            if (propertyLibraryName.stringValue == null || propertyLibraryName.stringValue.CleanName().IsNullOrEmpty())
            {
                propertyLibraryName.stringValue = SoundySettings.k_None;
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                property.serializedObject.Update();
            }

            if (propertyAudioName.stringValue == null || propertyAudioName.stringValue.CleanName().IsNullOrEmpty())
            {
                propertyAudioName.stringValue = SoundySettings.k_None;
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                property.serializedObject.Update();
            }

            FluidButton libraryNameButton = GetLibraryNameButton();
            FluidButton audioNameButton = GetAudioNameButton();

            libraryNameButton.SetOnClick(() =>
            {
                playerElement?.player?.Stop();
                var searchWindowContext = new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition));
                DynamicSearchProvider dsp =
                    ScriptableObject.CreateInstance<DynamicSearchProvider>()
                        .AddItems(GetLibrarySearchMenuItems(propertyLibraryName, propertyAudioName, GetLibraryNames, () =>
                        {
                            ValidateLibraryName();
                            ValidateAudioName();
                        }));
                SearchWindow.Open(searchWindowContext, dsp);
            });

            audioNameButton.SetOnClick(() =>
            {
                playerElement?.player?.Stop();
                var searchWindowContext = new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition));
                DynamicSearchProvider dsp =
                    ScriptableObject.CreateInstance<DynamicSearchProvider>()
                        .AddItems(GetAudioSearchMenuItems(propertyLibraryName, propertyAudioName, GetAudioNames, () =>
                        {
                            ValidateLibraryName();
                            ValidateAudioName();
                        }));
                SearchWindow.Open(searchWindowContext, dsp);
            });

            playerElement
                .SetAudioClipGetter(() =>
                {
                    property.serializedObject.Update();
                    SoundObject soundObject = id.GetSoundObject();
                    return soundObject == null || !soundObject.canPlay ? null : soundObject.LoadNext();
                })
                .SetVolumeGetter(() => id.GetSoundObject().GetVolume())
                .SetPitchGetter(() => id.GetSoundObject().GetPitch())
                .SetPriorityGetter(() => id.GetSoundObject().priority)
                .SetPanStereoGetter(() => id.GetSoundObject().panStereo)
                .SetSpatialBlendGetter(() => id.GetSoundObject().spatialBlend)
                .SetReverbZoneMixGetter(() => id.GetSoundObject().reverbZoneMix)
                .SetDopplerLevelGetter(() => id.GetSoundObject().dopplerLevel)
                .SetSpreadGetter(() => id.GetSoundObject().spread)
                .SetMinDistanceGetter(() => id.GetSoundObject().minDistance)
                .SetMaxDistanceGetter(() => id.GetSoundObject().maxDistance)
                .SetLoopGetter(() => id.GetSoundObject().loop)
                .SetIgnoreListenerPauseGetter(() => id.GetSoundObject().ignoreListenerPause)
                .SetOutputAudioMixerGroupGetter(() => id.GetOutputAudioMixerGroup())
                ;

            drawer.schedule.Execute(() => UpdateButtonNames(propertyLibraryName, propertyAudioName, libraryNameButton, audioNameButton)).Every(200);
            UpdateButtonNames(propertyLibraryName, propertyAudioName, libraryNameButton, audioNameButton);
            Compose(drawer, container, libraryNameLabel, libraryNameButton, openLibraryWindowButton, audioNameLabel, audioNameButton, playerElement, openAssetEditorButton);

            drawer.schedule.Execute(() =>
            {
                ValidateLibraryName();
                ValidateAudioName();

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

            void ValidateAudioName()
            {
                audioNames.Clear();
                audioNames.AddRange(GetAudioNames(propertyLibraryName.stringValue));
                bool audioNameIsValid = propertyAudioName.stringValue != SoundySettings.k_None && audioNames.Contains(propertyAudioName.stringValue);
                if (audioNameIsValid)
                {
                    audioNameButton.ResetAccentColor();
                    return;
                }
                audioNameButton.SetAccentColor(EditorSelectableColors.Help.ErrorText);
            }

            ValidateLibraryName();
            ValidateAudioName();
            return drawer;
        }
    }
}
