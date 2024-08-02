// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.Components;
using Doozy.Editor.EditorUI.Utils;
using Doozy.Editor.Soundy.Windows;
using Doozy.Editor.UIElements;
using Doozy.Runtime.Soundy.ScriptableObjects;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace Doozy.Editor.Soundy.Editors
{
    [CustomEditor(typeof(MusicLibrary), true)]
    public class MusicLibraryEditor : AudioLibraryEditor
    {
        private MusicLibrary castedTarget => (MusicLibrary)target;

        protected override bool isAddedToBuild => MusicLibraryDatabase.ContainsLibrary(castedTarget);
        protected override UnityAction addToBuildCallback => () => MusicLibraryDatabase.AddLibrary(castedTarget);
        protected override UnityAction removeFromBuildCallback => () => MusicLibraryDatabase.RemoveLibrary(castedTarget);

        private SerializedProperty propertyData { get; set; }

        private List<MusicObjectRow> musicObjectRows { get; set; }

        protected override void FindSerializedProperties()
        {
            base.FindSerializedProperties();

            propertyData = serializedObject.FindProperty("Data");
        }

        protected override void Initialize()
        {
            base.Initialize();

            componentHeader
                .SetComponentNameText("Music Library")
                .SetIcon(EditorSpriteSheets.Soundy.Icons.MusicLibrary);

            deleteLibraryButton
                .SetOnClick(() => MusicLibraryRegistry.DeleteLibrary(castedTarget));

            var dataCount = propertyData.arraySize;
            root.schedule.Execute(() =>
            {
                if (dataCount == propertyData.arraySize) return;
                dataCount = propertyData.arraySize;
                UpdateData();
            }).Every(100);
        }

        private void RemoveNullMusicObjects()
        {
            bool foundNullMusicObjects = false;

            for (int i = 0; i < castedTarget.data.Count; i++)
            {
                if (castedTarget.data[i] != null) continue;
                castedTarget.data.RemoveAt(i);
                i--;
                foundNullMusicObjects = true;
            }

            if (!foundNullMusicObjects) return;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            serializedObject.UpdateIfRequiredOrScript();
        }

        private void CheckForMissingMusicObjects()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(castedTarget));
            bool foundMissingMusicObjects = false;
            foreach (Object asset in assets)
            {
                if (asset == null) continue;
                if (asset == castedTarget) continue;
                if (asset is MusicObject musicObject)
                {
                    if (castedTarget.Contains(musicObject))
                        continue;
                    foundMissingMusicObjects = true;
                    castedTarget.data.Add(musicObject);
                }
            }
            if (foundMissingMusicObjects)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                serializedObject.UpdateIfRequiredOrScript();
            }
        }

        private void CheckForReferencedMusicObjectNotUnderLibrary()
        {
            bool foundReferencedMusicObjects = false;

            foreach (MusicObject musicObject in castedTarget.data)
            {
                if (musicObject == null) continue;
                if (AssetDatabase.IsSubAsset(musicObject)) continue;
                AssetDatabase.AddObjectToAsset(musicObject, castedTarget);
                foundReferencedMusicObjects = true;
            }

            if (!foundReferencedMusicObjects) return;
            EditorUtility.SetDirty(castedTarget);
            AssetDatabase.SaveAssetIfDirty(castedTarget);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            serializedObject.UpdateIfRequiredOrScript();
        }

        protected override void UpdateData()
        {
            root?.schedule.Execute(() =>
            {
                serializedObject.UpdateIfRequiredOrScript();
                musicObjectRows ??= new List<MusicObjectRow>();
                musicObjectRows.ForEach(row => row.Dispose());

                RemoveNullMusicObjects();
                CheckForMissingMusicObjects();
                CheckForReferencedMusicObjectNotUnderLibrary();

                if (dataContainer == null) return;
                dataContainer.RecycleAndClear();

                for (int i = 0; i < propertyData.arraySize; i++)
                {
                    SerializedProperty musicObjectProperty = propertyData.GetArrayElementAtIndex(i);
                    if (musicObjectProperty == null) continue;
                    var musicObject = musicObjectProperty.objectReferenceValue as MusicObject;
                    if (musicObject == null) continue;
                    var row = new MusicObjectRow(musicObjectProperty, musicObject, i, castedTarget);
                    musicObjectRows.Add(row);
                    dataContainer.AddChild(row);

                    //if this is the last element, we do not add a bottom margin
                    if (i != propertyData.arraySize - 1) continue;
                    row.SetStyleMarginBottom(0);
                }

                if (propertyData.arraySize == 0)
                {
                    dataContainer
                        .AddChild
                        (
                            FluidPlaceholder.Get()
                                .SetIcon(EditorSpriteSheets.Soundy.Icons.Music)
                                .SetLabelText("No Music Added")
                                .Play()
                        )
                        .AddSpaceBlock(8);
                }
            });
        }

        protected override void InitializeToolbar()
        {
            base.InitializeToolbar();

            buttonSortAz
                .SetOnClick(() =>
                {
                    castedTarget.SortByNameAz();
                    serializedObject.UpdateIfRequiredOrScript();
                    UpdateData();
                });

            buttonSortZa
                .SetOnClick(() =>
                {
                    castedTarget.SortByNameZa();
                    serializedObject.UpdateIfRequiredOrScript();
                    UpdateData();
                });

            buttonClearData
                .SetOnClick(() =>
                {
                    if (!EditorUtility.DisplayDialog
                        (
                            "Clear All Music",
                            "Are you sure you want to clear all music from this library?",
                            "Yes",
                            "No"
                        )
                       ) return;

                    castedTarget.ClearLibrary();
                    serializedObject.UpdateIfRequiredOrScript();
                    UpdateData();
                });

            buttonAddNew
                .SetOnClick(() => castedTarget.AddNew());
        }

        protected override void OnDragAnDropAudioClips()
        {
            castedTarget.AddNew(dataFluidDragAndDrop.references.ToArray());
            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();
            UpdateData();
        }

        private class MusicObjectRow : VisualElement, IDisposable
        {
            private const int k_OpenButtonWidth = 180;

            private SerializedProperty property { get; set; }
            private MusicObject musicObject { get; set; }
            private int index { get; set; }
            private SoundyEditorPlayer.PlayerElement playerElement { get; set; }
            private FluidButton openButton { get; set; }
            private FluidButton removeButton { get; set; }
            private MusicLibrary library { get; set; }

            public void Dispose()
            {
                if (musicObject != null)
                {
                    musicObject.OnUpdate -= OnUpdate;
                }

                property?.Dispose();
                openButton?.Dispose();
            }

            public MusicObjectRow(SerializedProperty property, MusicObject musicObject, int index, MusicLibrary library)
            {
                this.property = property;
                this.musicObject = musicObject;
                this.index = index;
                this.library = library;

                Initialize();
                Compose();

                if (musicObject != null)
                {
                    musicObject.OnUpdate -= OnUpdate;
                    musicObject.OnUpdate += OnUpdate;
                }

                schedule.Execute(() =>
                {
                    if (musicObject == null) return;
                    UpdateOpenButtonLabel(musicObject.audioName);
                });
            }

            private void OnUpdate()
            {
                if (playerElement?.player != null && playerElement.player.isPlaying)
                    playerElement.player.Stop();

                if (musicObject != null)
                {
                    UpdateOpenButtonLabel(musicObject.audioName);
                    return;
                }

                Dispose();
                RemoveFromHierarchy();
            }

            private void UpdateOpenButtonLabel(string text)
            {
                openButton
                    .SetLabelText(text)
                    .SetTooltip(text);

                // if the name of the music object is too long, we add a tooltip to the button and we truncate the name
                const int maxNameLength = 20;
                if (openButton.buttonLabel.text.Length > maxNameLength)
                    openButton.buttonLabel.text = openButton.buttonLabel.text.Substring(0, maxNameLength) + "...";
            }

            private void Initialize()
            {
                this
                    .SetName($"{musicObject.audioName}")
                    .SetStyleFlexDirection(FlexDirection.Row)
                    .SetStyleAlignItems(Align.Center)
                    .SetStyleFlexGrow(1)
                    .SetStyleBackgroundColor(EditorColors.Default.BoxBackground)
                    .SetStyleBorderRadius(DesignUtils.k_FieldBorderRadius)
                    .SetStylePadding(DesignUtils.k_Spacing)
                    .SetStylePaddingRight(DesignUtils.k_Spacing3X)
                    .SetStyleMarginBottom(DesignUtils.k_Spacing);

                openButton =
                    FluidButton.Get()
                        .SetIcon(EditorSpriteSheets.Soundy.Icons.Music)
                        .SetButtonStyle(ButtonStyle.Contained)
                        .SetElementSize(ElementSize.Normal)
                        .SetStyleWidth(k_OpenButtonWidth, k_OpenButtonWidth, k_OpenButtonWidth)
                        .SetStyleFlexShrink(0)
                        .SetOnClick(() =>
                        {
                            MusicObjectPopupWindow
                                .Open()
                                .LoadAsset(property.objectReferenceValue);
                        });

                // align the text to the left
                openButton.buttonLabel.SetStyleTextAlign(TextAnchor.MiddleLeft);

                // if the name of the music object is too long, we add a tooltip to the button and we truncate the name
                UpdateOpenButtonLabel(musicObject.audioName);

                removeButton =
                    SoundyEditorUtils.Elements.GetRemoveButton()
                        .SetElementSize(ElementSize.Small)
                        .SetOnClick(() => library.Remove(musicObject));

                playerElement =
                    new SoundyEditorPlayer.PlayerElement()
                        .SetAudioClipGetter(() => musicObject.data.Clip)
                        .SetVolumeGetter(() => musicObject.GetVolume())
                        .SetPitchGetter(() => musicObject.GetPitch())
                        .SetPriorityGetter(() => musicObject.priority)
                        .SetPanStereoGetter(() => musicObject.panStereo)
                        .SetSpatialBlendGetter(() => musicObject.spatialBlend)
                        .SetReverbZoneMixGetter(() => musicObject.reverbZoneMix)
                        .SetDopplerLevelGetter(() => musicObject.dopplerLevel)
                        .SetSpreadGetter(() => musicObject.spread)
                        .SetMinDistanceGetter(() => musicObject.minDistance)
                        .SetMaxDistanceGetter(() => musicObject.maxDistance)
                        .SetLoopGetter(() => musicObject.loop)
                        .SetIgnoreListenerPauseGetter(() => musicObject.ignoreListenerPause)
                        .SetOutputAudioMixerGroupGetter(() => library.OutputAudioMixerGroup);
            }

            private void Compose()
            {
                this
                    .AddChild(openButton)
                    .AddSpaceBlock()
                    .AddChild(playerElement)
                    .AddSpaceBlock(2)
                    .AddChild(DesignUtils.dividerVertical)
                    .AddSpaceBlock()
                    .AddChild(removeButton);
            }

        }
    }
}
