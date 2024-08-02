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
    [CustomPropertyDrawer(typeof(MusicId), true)]
    public class MusicIdDrawer : AudioIdDrawer
    {
        private static List<string> libraryNames { get; } = new List<string>();
        private static List<string> audioNames { get; } = new List<string>();

        protected override List<string> GetLibraryNames() =>
            MusicLibraryRegistry.GetLibraryNames();

        protected override List<string> GetAudioNames(string libraryName)
        {
            MusicLibrary library = MusicLibraryRegistry.GetLibrary(libraryName);
            return library == null ? new List<string>() : library.GetAudioNames();
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            base.CreatePropertyGUI(property);
            var id = property.GetTargetObjectOfProperty() as MusicId;

            openLibraryWindowButton
                .SetIcon(EditorSpriteSheets.Soundy.Icons.MusicLibrary)
                .SetTooltip("Open Music Libraries Window")
                .SetOnClick(() =>
                {
                    MusicLibraryWindow.Open();
                    MusicLibraryWindow.instance.SelectLibrary(id.libraryName);
                });

            openAssetEditorButton
                .SetIcon(EditorSpriteSheets.Soundy.Icons.Music)
                .SetTooltip("Open Music Object Editor")
                .SetOnClick(() =>
                {
                    var musicObject = id.GetMusicObject();
                    if (musicObject == null)
                    {
                        EditorUtility.DisplayDialog
                        (
                            "No Music Object Found",
                            $"No Music Object found for {id.libraryName} - {id.audioName}",
                            "Ok"
                        );
                        return;
                    }
                    SoundObjectPopupWindow.Open().LoadAsset(musicObject);
                });

            libraryNameLabel.SetText("Music Library");
            audioNameLabel.SetText("Music Name");

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
                    MusicObject musicObject = id.GetMusicObject();
                    return musicObject == null || !musicObject.canPlay ? null : id.GetMusicObject().data.Clip;
                })
                .SetVolumeGetter(() => id.GetMusicObject().GetVolume())
                .SetPitchGetter(() => id.GetMusicObject().GetPitch())
                .SetPriorityGetter(() => id.GetMusicObject().priority)
                .SetPanStereoGetter(() => id.GetMusicObject().panStereo)
                .SetSpatialBlendGetter(() => id.GetMusicObject().spatialBlend)
                .SetReverbZoneMixGetter(() => id.GetMusicObject().reverbZoneMix)
                .SetDopplerLevelGetter(() => id.GetMusicObject().dopplerLevel)
                .SetSpreadGetter(() => id.GetMusicObject().spread)
                .SetMinDistanceGetter(() => id.GetMusicObject().minDistance)
                .SetMaxDistanceGetter(() => id.GetMusicObject().maxDistance)
                .SetLoopGetter(() => id.GetMusicObject().loop)
                .SetIgnoreListenerPauseGetter(() => id.GetMusicObject().ignoreListenerPause)
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
