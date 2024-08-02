// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using Doozy.Editor.Common.Extensions;
using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.Components;
using Doozy.Editor.EditorUI.Components.Internal;
using Doozy.Editor.EditorUI.Utils;
using Doozy.Editor.UIElements;
using Doozy.Runtime.Soundy;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using PlayMode = Doozy.Runtime.Soundy.PlayMode;

namespace Doozy.Editor.Soundy.Drawers
{
    [CustomPropertyDrawer(typeof(MusicPlaylist), true)]
    public class MusicPlaylistDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var target = property.GetTargetObjectOfProperty() as MusicPlaylist;

            var dataRows = new Dictionary<SerializedProperty, MusicIdRow>();
            var drawer = new VisualElement();
            var settingsContainer = new VisualElement();
            var toolbarContainer = SoundyEditorUtils.Elements.GetToolbarContainer();
            var dataContainer = SoundyEditorUtils.Elements.GetDataContainer();

            drawer
                .AddChild(settingsContainer)
                .AddSpaceBlock()
                .AddChild(toolbarContainer)
                .AddChild(dataContainer);

            var propertyPlayMode = property.FindPropertyRelative("PlayMode");
            var propertyLoopPlaylist = property.FindPropertyRelative("LoopPlaylist");
            var propertyLoopSong = property.FindPropertyRelative("LoopSong");

            var playModeEnumField =
                DesignUtils.NewEnumField(propertyPlayMode, true);

            var tabsGroup =
                FluidToggleGroup.Get().SetControlMode(FluidToggleGroup.ControlMode.OneToggleOnEnforced);

            FluidToggleButtonTab GetPlayModeTab(PlayMode playMode) =>
                FluidToggleButtonTab.Get()
                    .SetStyleFlexGrow(1)
                    .SetElementSize(ElementSize.Normal)
                    .SetContainerColorOff(EditorColors.Default.Background)
                    .SetToggleAccentColor(SoundyEditorUtils.selectableAccentColor)
                    .SetIsOn(propertyPlayMode.enumValueIndex == (int)playMode)
                    .SetOnClick(() =>
                    {
                        playModeEnumField.value = playMode;
                        property.serializedObject.ApplyModifiedProperties();
                        property.serializedObject.Update();
                    });

            var sequentialTab =
                GetPlayModeTab(PlayMode.Sequential)
                    .SetTabPosition(TabPosition.TabOnLeft)
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Sequence)
                    .SetLabelText("Sequential")
                    .SetTooltip
                    (
                        "The songs will be played in a sequential order (in the order they were added)"
                    );

            var randomTab =
                GetPlayModeTab(PlayMode.Random)
                    .SetTabPosition(TabPosition.TabInCenter)
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Random)
                    .SetLabelText("Random")
                    .SetTooltip
                    (
                        "The songs will be played in a random order (the order they were added does not matter)"
                    );

            var randomNoRepeatTab =
                GetPlayModeTab(PlayMode.RandomNoRepeat)
                    .SetTabPosition(TabPosition.TabOnRight)
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Random)
                    .SetLabelText("Random No Repeat")
                    .SetTooltip
                    (
                        " The songs will be played in a random order, but each song will be played only once (no repeats)." +
                        "\n\n" +
                        " This mode is useful when you want to play a random song, but you don't want to play the same song twice in a row." +
                        "\n\n" +
                        " Note that this mode is more expensive than the Random mode, because it needs to keep track of the played songs."
                    );
            
            playModeEnumField.RegisterValueChangedCallback(evt =>
            {
                if (evt?.newValue == null) return;
                switch ((PlayMode)evt.newValue)
                {
                    case PlayMode.Sequential:
                        sequentialTab.SetIsOn(true);
                        break;
                    case PlayMode.Random:
                        randomTab.SetIsOn(true);
                        break;
                    case PlayMode.RandomNoRepeat:
                        randomNoRepeatTab.SetIsOn(true);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });

            var playModeFluidField =
                FluidField.Get()
                    .SetLabelText("Play Mode")
                    .AddFieldContent
                    (
                        DesignUtils.row
                            .AddChild(playModeEnumField)
                            .AddChild(sequentialTab)
                            .AddSpaceBlock()
                            .AddChild(randomTab)
                            .AddSpaceBlock()
                            .AddChild(randomNoRepeatTab)
                    );
            
            sequentialTab.AddToToggleGroup(tabsGroup);
            randomTab.AddToToggleGroup(tabsGroup);
            randomNoRepeatTab.AddToToggleGroup(tabsGroup);

            FluidToggleSwitch loopPlaylistToggle =
                FluidToggleSwitch.Get()
                    .SetLabelText("Loop Playlist")
                    .SetTooltip("Loop the playlist (works only if Play Mode is Sequential)")
                    .SetToggleAccentColor(SoundyEditorUtils.selectableAccentColor)
                    .BindToProperty(propertyLoopPlaylist);

            FluidToggleSwitch loopSongToggle =
                FluidToggleSwitch.Get()
                    .SetLabelText("Loop Song")
                    .SetTooltip
                    (
                        "Loop the current song over and over (if enabled)\n\n" +
                        "If enabled, LoadNext will start playing the current song again, instead of loading the next one"
                    )
                    .SetToggleAccentColor(SoundyEditorUtils.selectableAccentColor)
                    .BindToProperty(propertyLoopSong);

            FluidField loopsFluidField =
                FluidField.Get()
                    .AddFieldContent
                    (
                        DesignUtils.row
                            .AddChild(loopPlaylistToggle)
                            .AddSpaceBlock()
                            .AddChild(loopSongToggle)
                    );

            settingsContainer
                .AddChild(playModeFluidField)
                .AddSpaceBlock()
                .AddChild(loopsFluidField);

            var propertyIds = property.FindPropertyRelative("Ids");

            void ClearDataRows()
            {
                foreach (MusicIdRow row in dataRows.Values) row.DisposeIDisposableChildren().RemoveFromHierarchy();
                dataRows.Clear();
                dataContainer.RecycleAndClear();
            }

            void UpdateData()
            {
                if (propertyIds == null)
                    return;

                ClearDataRows();

                if (propertyIds.arraySize == 0)
                {
                    dataContainer
                        .AddChild
                        (
                            FluidPlaceholder.Get()
                                .SetIcon(EditorSpriteSheets.EditorUI.Icons.Music)
                                .SetLabelText("Empty Playlist")
                        )
                        .AddSpaceBlock(8);
                    return;
                }

                for (int i = 0; i <= propertyIds.arraySize - 1; i++)
                {
                    var rowProperty = propertyIds.GetArrayElementAtIndex(i);
                    // var row = GetOrCreateRow(rowProperty, index);
                    var row = new MusicIdRow(propertyIds, i, UpdateData);
                    dataContainer.AddChild(row);
                }

                dataContainer
                    .Bind(property.serializedObject);
            }

            var buttonClearData = SoundyEditorUtils.Elements.GetClearButton().SetOnClick(() =>
            {
                propertyIds.ClearArray();
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
                UpdateData();
            });

            var buttonAddNew = SoundyEditorUtils.Elements.GetAddButton().SetOnClick(() =>
            {
                Undo.RecordObject(property.serializedObject.targetObject, "Add New");
                target.AddNew();
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
                UpdateData();
            });

            toolbarContainer
                .AddChild(buttonClearData)
                .AddSpaceBlock()
                .AddFlexibleSpace()
                .AddSpaceBlock()
                .AddChild(buttonAddNew);

            UpdateData();

            int dataCount = propertyIds.arraySize;
            drawer.schedule.Execute(() =>
                {
                    if (propertyIds.arraySize == dataCount) return;
                    dataCount = propertyIds.arraySize;
                    UpdateData();
                })
                .Every(100);

            return drawer;
        }

        private class MusicIdRow : VisualElement
        {
            private SerializedProperty arrayProperty { get; }
            private int index { get; }
            private UnityAction updateData { get; }

            private VisualElement leftContainer { get; set; }
            private VisualElement middleContainer { get; set; }
            private VisualElement rightContainer { get; set; }

            public MusicIdRow(SerializedProperty arrayProperty, int index, UnityAction updateData)
            {
                this.arrayProperty = arrayProperty;
                this.index = index;
                this.updateData = updateData;

                Initialize();
            }

            private void Initialize()
            {
                this
                    .ResetLayout()
                    .SetName("Row")
                    .SetStyleFlexDirection(FlexDirection.Row)
                    .SetStyleMarginTop(DesignUtils.k_Spacing);

                leftContainer = SoundyEditorUtils.Elements.GetSideContainer().SetName("LeftContainer").SetStyleMinWidth(26);
                middleContainer = SoundyEditorUtils.Elements.GetSideContainer().SetName("MiddleContainer").SetStyleAlignItems(Align.Stretch).SetStyleFlexGrow(1);
                rightContainer = SoundyEditorUtils.Elements.GetSideContainer().SetName("RightContainer").SetStyleMinWidth(26);

                FluidButton moveUpButton =
                    SoundyEditorUtils.Elements.GetMoveUpButton()
                        .SetOnClick
                        (
                            () =>
                            {
                                arrayProperty.MoveArrayElement(index, index - 1);
                                arrayProperty.serializedObject.ApplyModifiedProperties();
                                arrayProperty.serializedObject.Update();
                                updateData?.Invoke();
                            }
                        );

                FluidButton moveDownButton =
                    SoundyEditorUtils.Elements.GetMoveDownButton()
                        .SetOnClick
                        (
                            () =>
                            {
                                arrayProperty.MoveArrayElement(index, index + 1);
                                arrayProperty.serializedObject.ApplyModifiedProperties();
                                arrayProperty.serializedObject.Update();
                                updateData?.Invoke();
                            }
                        );

                //hide relevant move buttons at the beginning and end of the list
                moveUpButton.SetStyleDisplay(index > 0 ? DisplayStyle.Flex : DisplayStyle.None);
                moveDownButton.SetStyleDisplay(index < arrayProperty.arraySize - 1 ? DisplayStyle.Flex : DisplayStyle.None);

                FluidButton removeButton =
                    SoundyEditorUtils.Elements.GetRemoveButton()
                        .SetOnClick
                        (
                            () =>
                            {
                                arrayProperty.DeleteArrayElementAtIndex(index);
                                arrayProperty.serializedObject.ApplyModifiedProperties();
                                arrayProperty.serializedObject.Update();
                                updateData?.Invoke();
                            }
                        );

                this
                    .AddChild
                    (
                        leftContainer
                            .AddChild(DesignUtils.fieldLabel.SetText($"{index}"))
                            .AddFlexibleSpace()
                            .AddChild(moveUpButton)
                            .AddChild(moveDownButton)
                    )
                    .AddSpaceBlock()
                    .AddChild
                    (
                        middleContainer
                            .AddChild(DesignUtils.NewPropertyField(arrayProperty.GetArrayElementAtIndex(index)))
                    )
                    .AddSpaceBlock()
                    .AddChild
                    (
                        rightContainer
                            .AddChild(removeButton)
                    );
            }
        }
    }
}
