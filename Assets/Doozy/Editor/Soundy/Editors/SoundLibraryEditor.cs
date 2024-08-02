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
    [CustomEditor(typeof(SoundLibrary), true)]
    public class SoundLibraryEditor : AudioLibraryEditor
    {
        private SoundLibrary castedTarget => (SoundLibrary)target;

        protected override bool isAddedToBuild => SoundLibraryDatabase.ContainsLibrary(castedTarget);
        protected override UnityAction addToBuildCallback => () => SoundLibraryDatabase.AddLibrary(castedTarget);
        protected override UnityAction removeFromBuildCallback => () => SoundLibraryDatabase.RemoveLibrary(castedTarget);

        private SerializedProperty propertyData { get; set; }

        private List<SoundObjectRow> soundObjectRows { get; set; }

        protected override void FindSerializedProperties()
        {
            base.FindSerializedProperties();

            propertyData = serializedObject.FindProperty("Data");
        }

        protected override void Initialize()
        {
            base.Initialize();

            componentHeader
                .SetComponentNameText("Sound Library")
                .SetIcon(EditorSpriteSheets.Soundy.Icons.SoundLibrary);

            deleteLibraryButton
                .SetOnClick(() => SoundLibraryRegistry.DeleteLibrary(castedTarget));

            var dataCount = propertyData.arraySize;
            root.schedule.Execute(() =>
            {
                if (dataCount == propertyData.arraySize) return;
                dataCount = propertyData.arraySize;
                UpdateData();
            }).Every(100);
        }

        private void RemoveNullSoundObjects()
        {
            bool foundNullSoundObjects = false;

            for (int i = 0; i < castedTarget.data.Count; i++)
            {
                if (castedTarget.data[i] != null) continue;
                castedTarget.data.RemoveAt(i);
                i--;
                foundNullSoundObjects = true;
            }

            if (!foundNullSoundObjects) return;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            serializedObject.UpdateIfRequiredOrScript();
        }

        private void CheckForMissingSoundObjects()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(castedTarget));
            bool foundMissingSoundObjects = false;
            foreach (Object asset in assets)
            {
                if (asset == null) continue;
                if (asset == castedTarget) continue;
                if (asset is SoundObject soundObject)
                {
                    if (castedTarget.Contains(soundObject))
                        continue;
                    foundMissingSoundObjects = true;
                    castedTarget.data.Add(soundObject);
                }
            }
            if (foundMissingSoundObjects)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                serializedObject.UpdateIfRequiredOrScript();
            }
        }

        private void CheckForReferencedSoundObjectNotUnderLibrary()
        {
            bool foundReferencedSoundObjects = false;

            foreach (SoundObject soundObject in castedTarget.data)
            {
                if (soundObject == null) continue;
                if (AssetDatabase.IsSubAsset(soundObject)) continue;
                AssetDatabase.AddObjectToAsset(soundObject, castedTarget);
                foundReferencedSoundObjects = true;
            }

            if (!foundReferencedSoundObjects) return;
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
                soundObjectRows ??= new List<SoundObjectRow>();
                soundObjectRows.ForEach(row => row.Dispose());

                RemoveNullSoundObjects();
                CheckForMissingSoundObjects();
                CheckForReferencedSoundObjectNotUnderLibrary();

                if (dataContainer == null) return;
                dataContainer.RecycleAndClear();

                for (int i = 0; i < propertyData.arraySize; i++)
                {
                    SerializedProperty soundObjectProperty = propertyData.GetArrayElementAtIndex(i);
                    if (soundObjectProperty == null) continue;
                    var soundObject = soundObjectProperty.objectReferenceValue as SoundObject;
                    if (soundObject == null) continue;
                    var row = new SoundObjectRow(soundObjectProperty, soundObject, i, castedTarget);
                    soundObjectRows.Add(row);
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
                                .SetIcon(EditorSpriteSheets.Soundy.Icons.Sound)
                                .SetLabelText("No Sounds Added")
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
                            "Clear All Sounds",
                            "Are you sure you want to clear all sounds from this library?",
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

        private class SoundObjectRow : VisualElement, IDisposable
        {
            private const int k_OpenButtonWidth = 180;

            private SerializedProperty property { get; set; }
            private SoundObject soundObject { get; set; }
            private int index { get; set; }
            private SoundyEditorPlayer.PlayerElement playerElement { get; set; }
            private FluidButton openButton { get; set; }
            private FluidButton removeButton { get; set; }
            private SoundLibrary library { get; set; }

            public void Dispose()
            {
                if (soundObject != null)
                {
                    soundObject.OnUpdate -= OnUpdate;
                }

                property?.Dispose();
                openButton?.Dispose();
                removeButton?.Dispose();
            }

            public SoundObjectRow(SerializedProperty property, SoundObject soundObject, int index, SoundLibrary library)
            {
                this.property = property;
                this.soundObject = soundObject;
                this.index = index;
                this.library = library;

                Initialize();
                Compose();

                if (soundObject != null)
                {
                    soundObject.OnUpdate -= OnUpdate;
                    soundObject.OnUpdate += OnUpdate;
                }

                schedule.Execute(() =>
                {
                    if (soundObject == null) return;
                    UpdateOpenButtonLabel(soundObject.audioName);
                });
            }

            private void OnUpdate()
            {
                if (playerElement?.player != null && playerElement.player.isPlaying)
                    playerElement.player.Stop();

                if (soundObject != null)
                {
                    UpdateOpenButtonLabel(soundObject.audioName);
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
                    .SetName($"{soundObject.audioName}")
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
                        .SetIcon(EditorSpriteSheets.Soundy.Icons.Sound)
                        .SetButtonStyle(ButtonStyle.Contained)
                        .SetElementSize(ElementSize.Normal)
                        .SetStyleWidth(k_OpenButtonWidth, k_OpenButtonWidth, k_OpenButtonWidth)
                        .SetStyleFlexShrink(0)
                        .SetOnClick(() =>
                        {
                            SoundObjectPopupWindow
                                .Open()
                                .LoadAsset(property.objectReferenceValue);

                        });

                // align the text to the left
                openButton.buttonLabel.SetStyleTextAlign(TextAnchor.MiddleLeft);

                // if the name of the sound object is too long, we add a tooltip to the button and we truncate the name
                UpdateOpenButtonLabel(soundObject.audioName);

                removeButton =
                    SoundyEditorUtils.Elements.GetRemoveButton()
                        .SetElementSize(ElementSize.Small)
                        .SetOnClick(() => library.Remove(soundObject));

                playerElement =
                    new SoundyEditorPlayer.PlayerElement()
                        .SetSoundObjectGetter
                        (
                            () => soundObject,
                            () => library != null ? library.OutputAudioMixerGroup : null
                        );
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
