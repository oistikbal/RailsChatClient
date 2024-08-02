// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.Components;
using Doozy.Editor.EditorUI.Components.Internal;
using Doozy.Editor.EditorUI.Utils;
using Doozy.Editor.UIElements;
using Doozy.Runtime.Common.Extensions;
using Doozy.Runtime.Soundy;
using Doozy.Runtime.Soundy.ScriptableObjects;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using PlayMode = Doozy.Runtime.Soundy.PlayMode;

namespace Doozy.Editor.Soundy.Editors
{
    [CustomEditor(typeof(SoundObject), true)]
    public class SoundObjectEditor : AudioObjectEditor
    {
        private SoundObject castedTarget => (SoundObject)target;

        private SerializedProperty propertyPlayMode { get; set; }
        private SerializedProperty propertyAutoResetSequence { get; set; }
        private SerializedProperty propertyAutoResetSequenceTime { get; set; }
        private SerializedProperty propertyData { get; set; }

        private FluidField playModeFluidField { get; set; }

        protected override void FindSerializedProperties()
        {
            base.FindSerializedProperties();

            propertyPlayMode = serializedObject.FindProperty(nameof(SoundObject.PlayMode));
            propertyAutoResetSequence = serializedObject.FindProperty(nameof(SoundObject.AutoResetSequence));
            propertyAutoResetSequenceTime = serializedObject.FindProperty(nameof(SoundObject.AutoResetSequenceTime));
            propertyData = serializedObject.FindProperty("Data");
        }

        protected override void Initialize()
        {
            base.Initialize();

            componentHeader
                .SetIcon(EditorSpriteSheets.Soundy.Icons.Sound)
                .SetComponentNameText("Sound");

            playerElement =
                new SoundyEditorPlayer.PlayerElement()
                    .SetAudioClipGetter(() =>
                    {
                        castedTarget.LoadNext();
                        return castedTarget.nextData?.Clip;
                    })
                    .SetVolumeGetter(() => castedTarget.GetVolume())
                    .SetPitchGetter(() => castedTarget.GetPitch())
                    .SetPriorityGetter(() => castedTarget.priority)
                    .SetPanStereoGetter(() => castedTarget.panStereo)
                    .SetSpatialBlendGetter(() => castedTarget.spatialBlend)
                    .SetReverbZoneMixGetter(() => castedTarget.reverbZoneMix)
                    .SetDopplerLevelGetter(() => castedTarget.dopplerLevel)
                    .SetSpreadGetter(() => castedTarget.spread)
                    .SetMinDistanceGetter(() => castedTarget.minDistance)
                    .SetMaxDistanceGetter(() => castedTarget.maxDistance)
                    .SetLoopGetter(() => castedTarget.loop)
                    .SetIgnoreListenerPauseGetter(() => castedTarget.ignoreListenerPause);


            InitializePlayMode();
            InitializeData();
            InitializeDataToolbar();

            audioNameTextField.RegisterValueChangedCallback(evt =>
            {
                if (evt?.newValue == null) return;
                castedTarget.OnUpdate?.Invoke();
            });
        }

        private void InitializePlayMode()
        {
            var playModeEnumField =
                DesignUtils.NewEnumField(propertyPlayMode, true);

            var sequentialSettingsAnimatedContainer =
                new FluidAnimatedContainer(true)
                    .Hide(false)
                    .SetName("Sequential Settings Animated Container");

            sequentialSettingsAnimatedContainer.SetOnShowCallback(() =>
            {
                var autoResetSequenceToggleSwitch =
                    FluidToggleSwitch.Get()
                        .SetLabelText("Auto Reset Sequence after")
                        .SetTooltip("If PlayMode is set to Sequential, this will reset the sequence after a set idle time (in seconds)")
                        .SetToggleAccentColor(selectableAccentColor)
                        .BindToProperty(propertyAutoResetSequence);

                var autoResetSequenceTimeFloatField =
                    DesignUtils.NewFloatField(propertyAutoResetSequenceTime)
                        .SetStyleWidth(40, 40, 40);

                var sequentialSettings =
                    new VisualElement()
                        .ResetLayout()
                        .SetName("Sequential Settings")
                        .SetStyleMarginTop(DesignUtils.k_Spacing)
                        .SetStyleFlexDirection(FlexDirection.Row)
                        .AddChild(autoResetSequenceToggleSwitch)
                        .AddChild(autoResetSequenceTimeFloatField)
                        .AddChild
                        (
                            DesignUtils.NewLabel("seconds", 12)
                                .SetStyleMarginLeft(DesignUtils.k_Spacing2X)
                                .SetStyleColor(autoResetSequenceToggleSwitch.fluidElement.textColor)
                        );

                sequentialSettingsAnimatedContainer
                    .AddContent(sequentialSettings)
                    .Bind(serializedObject);
            });

            var tabsGroup =
                FluidToggleGroup.Get().SetControlMode(FluidToggleGroup.ControlMode.OneToggleOnEnforced);

            FluidToggleButtonTab GetPlayModeTab(PlayMode playMode) =>
                FluidToggleButtonTab.Get()
                    .SetStyleFlexGrow(1)
                    .SetElementSize(ElementSize.Normal)
                    .SetContainerColorOff(EditorColors.Default.Background)
                    .SetToggleAccentColor(selectableAccentColor)
                    .SetIsOn(propertyPlayMode.enumValueIndex == (int)playMode)
                    .SetOnClick(() =>
                    {
                        playModeEnumField.value = playMode;
                        serializedObject.ApplyModifiedProperties();
                        serializedObject.Update();
                    });

            var sequentialTab =
                    GetPlayModeTab(PlayMode.Sequential)
                        .SetTabPosition(TabPosition.TabOnLeft)
                        .SetIcon(EditorSpriteSheets.EditorUI.Icons.Sequence)
                        .SetLabelText("Sequential")
                        .SetTooltip
                        (
                            "The audio clips will be played in a sequential order (in the order they were added)"
                        )
                ;

            var randomTab =
                GetPlayModeTab(PlayMode.Random)
                    .SetTabPosition(TabPosition.TabInCenter)
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Random)
                    .SetLabelText("Random")
                    .SetTooltip
                    (
                        "The audio clips will be played in a random order respecting the weight of each audio clip"
                    );

            var randomNoRepeatTab =
                GetPlayModeTab(PlayMode.RandomNoRepeat)
                    .SetTabPosition(TabPosition.TabOnRight)
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Random)
                    .SetLabelText("Random No Repeat")
                    .SetTooltip
                    (
                        " The audio clips will be played in a random order, but each audio clip will be played only once (no repeats)." +
                        "\n\n" +
                        " This mode is useful when you want to play a random sound, but you don't want to play the same sound twice in a row." +
                        "\n\n" +
                        " Note that this mode is more expensive than the Random mode, because it needs to keep track of the played audio clips."
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

            sequentialTab.AddToToggleGroup(tabsGroup);
            randomTab.AddToToggleGroup(tabsGroup);
            randomNoRepeatTab.AddToToggleGroup(tabsGroup);

            sequentialTab.SetOnValueChanged(evt => sequentialSettingsAnimatedContainer.Toggle(evt.newValue));
            if (sequentialTab.isOn) sequentialSettingsAnimatedContainer.Show(false);

            playModeFluidField =
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
                    )
                    .AddFieldContent(sequentialSettingsAnimatedContainer);
        }

        protected override void InitializeDataToolbar()
        {
            base.InitializeDataToolbar();

            var buttonSortAz = SoundyEditorUtils.Elements.GetSortAzButton().SetOnClick(() =>
            {
                castedTarget.SortByAudioClipNameAz();
                UpdateData();
            });

            var buttonSortZa = SoundyEditorUtils.Elements.GetSortZaButton().SetOnClick(() =>
            {
                castedTarget.SortByAudioClipNameZa();
                UpdateData();
            });

            var buttonClearData = SoundyEditorUtils.Elements.GetClearButton().SetOnClick(() =>
            {
                castedTarget.ClearData();
                UpdateData();
            });

            var buttonAddNew = SoundyEditorUtils.Elements.GetAddButton().SetOnClick(() => castedTarget.AddNew());

            toolbarContainer
                .AddChild(buttonSortAz)
                .AddChild(buttonSortZa)
                .AddSpaceBlock()
                .AddChild(DesignUtils.dividerVertical)
                .AddSpaceBlock()
                .AddChild(buttonClearData)
                .AddSpaceBlock()
                .AddFlexibleSpace()
                .AddSpaceBlock()
                .AddChild(dataFluidDragAndDrop)
                .AddChild(buttonAddNew);
        }

        protected override void OnDragAndDropAudioClip()
        {
            castedTarget.AddNew(dataFluidDragAndDrop.references.ToArray());
            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();
            // UpdateData();
        }

        protected override void InitializeData()
        {
            base.InitializeData();

            int dataCount = propertyData.arraySize;
            root.schedule.Execute(() =>
                {
                    if (propertyData.arraySize == dataCount) return;
                    dataCount = propertyData.arraySize;
                    UpdateData();
                })
                .Every(100);
        }

        private Dictionary<SerializedProperty, VisualElement> dataRows { get; } = new Dictionary<SerializedProperty, VisualElement>();

        private VisualElement GetOrCreateDataRow(SerializedProperty property, int index)
        {
            if (dataRows.TryGetValue(property, out var row))
                return row;

            row = CreateDataRow(property, index);
            dataRows.Add(property, row);
            return row;
        }

        private VisualElement CreateDataRow(SerializedProperty property, int index)
        {
            var row =
                new VisualElement()
                    .ResetLayout()
                    .SetName("Row")
                    .SetStyleFlexDirection(FlexDirection.Row)
                    .SetStyleMarginTop(DesignUtils.k_Spacing);

            var leftContainer = SoundyEditorUtils.Elements.GetSideContainer().SetName("LeftContainer").SetStyleMinWidth(26);
            var playerContainer = SoundyEditorUtils.Elements.GetSideContainer().SetName("PlayerContainer");
            var optionsContainer = SoundyEditorUtils.Elements.GetSideContainer().SetName("OptionsContainer");
            var rightContainer = SoundyEditorUtils.Elements.GetSideContainer().SetName("RightContainer");

            FluidButton moveUpButton =
                SoundyEditorUtils.Elements.GetMoveUpButton()
                    .SetOnClick
                    (
                        () =>
                        {
                            propertyData.MoveArrayElement(index, index - 1);
                            serializedObject.ApplyModifiedProperties();
                            serializedObject.Update();
                            UpdateData();
                        }
                    );

            FluidButton moveDownButton =
                SoundyEditorUtils.Elements.GetMoveDownButton()
                    .SetOnClick
                    (
                        () =>
                        {
                            propertyData.MoveArrayElement(index, index + 1);
                            serializedObject.ApplyModifiedProperties();
                            serializedObject.Update();
                            UpdateData();
                        }
                    );

            //hide relevant move buttons at the beginning and end of the list
            moveUpButton.SetStyleDisplay(index > 0 ? DisplayStyle.Flex : DisplayStyle.None);
            moveDownButton.SetStyleDisplay(index < propertyData.arraySize - 1 ? DisplayStyle.Flex : DisplayStyle.None);

            FluidButton updateNameButton =
                SoundyEditorUtils.Elements.GetUpdateNameButton()
                    .SetTooltip("Set the Sound name to match the AudioClip name (in a pretty way)")
                    .SetOnClick(() =>
                    {
                        var clip = propertyData.GetArrayElementAtIndex(index).FindPropertyRelative(nameof(SoundData.Clip)).objectReferenceValue as AudioClip;
                        if (clip == null) return;
                        propertyAudioName.stringValue = clip.name.CleanName();
                        serializedObject.ApplyModifiedProperties();
                        serializedObject.Update();
                        UpdateData();
                        root.schedule.Execute(() => castedTarget.OnUpdate?.Invoke());
                    });

            FluidButton removeButton =
                SoundyEditorUtils.Elements.GetRemoveButton()
                    .SetOnClick
                    (
                        () =>
                        {
                            propertyData.DeleteArrayElementAtIndex(index);
                            serializedObject.ApplyModifiedProperties();
                            serializedObject.Update();
                            UpdateData();
                        }
                    );

            FluidButton duplicateButton =
                SoundyEditorUtils.Elements.GetDuplicateButton()
                    .SetOnClick
                    (
                        () =>
                        {
                            propertyData.InsertArrayElementAtIndex(index);
                            serializedObject.ApplyModifiedProperties();
                            serializedObject.Update();
                            UpdateData();
                        }
                    );


            var miniPlayer =
                new SoundyEditorPlayer.MiniPlayerElement()
                    .SetAudioClipGetter(() => propertyData.GetArrayElementAtIndex(index).FindPropertyRelative(nameof(SoundData.Clip)).objectReferenceValue as AudioClip)
                    .SetVolumeGetter(() => propertyData.GetArrayElementAtIndex(index).FindPropertyRelative(nameof(SoundData.Volume)).floatValue)
                    .SetPitchGetter(() => propertyData.GetArrayElementAtIndex(index).FindPropertyRelative(nameof(SoundData.Pitch)).floatValue);

            row
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
                    playerContainer
                        .AddChild(miniPlayer)
                )
                .AddSpaceBlock()
                .AddChild(DesignUtils.NewPropertyField(propertyData.GetArrayElementAtIndex(index)))
                .AddSpaceBlock()
                .AddChild
                (
                    optionsContainer
                        .AddChild(updateNameButton)
                        .AddFlexibleSpace()
                        .AddChild(duplicateButton)
                )
                .AddSpaceBlock()
                .AddChild
                (
                    rightContainer
                        .AddChild(removeButton)
                );

            return row;
        }

        private void ClearDataRows()
        {
            foreach (VisualElement row in dataRows.Values) row.RemoveFromHierarchy();
            dataRows.Clear();
            dataContainer?.RecycleAndClear();
        }

        protected override void UpdateData()
        {
            if (propertyData == null)
                return;

            if (propertyData.arraySize == 0)
            {
                ClearDataRows();
                dataContainer
                    .AddChild
                    (
                        FluidPlaceholder.Get()
                            .SetIcon(EditorSpriteSheets.EditorUI.Icons.Sound)
                            .SetLabelText("No AudioClips added")
                    )
                    .AddSpaceBlock(8);
                return;
            }

            dataContainer.Clear();
            List<SerializedProperty> rowProperties = new List<SerializedProperty>();
            for (int i = 0; i < propertyData.arraySize; i++)
            {
                int index = i;
                var property = propertyData.GetArrayElementAtIndex(index);
                rowProperties.Add(property);
                var row = GetOrCreateDataRow(property, index);
                dataContainer.AddChild(row);
            }

            //remove unused rows
            foreach (SerializedProperty rowProperty in dataRows.Keys.ToList())
            {
                if (rowProperties.Contains(rowProperty)) continue;
                var row = dataRows[rowProperty];
                row.RecycleAndClear();
                dataRows.Remove(rowProperty);
            }

            dataContainer
                .Bind(serializedObject);
        }

        protected override void Compose()
        {
            base.Compose();

            root
                .AddSpaceBlock()
                .AddChild(playModeFluidField)
                .AddSpaceBlock()
                .AddChild(DesignUtils.dividerHorizontal)
                .AddSpaceBlock()
                .AddChild(playerElement)
                .AddSpaceBlock()
                .AddChild(toolbarContainer)
                .AddChild(dataContainer)
                .AddEndOfLineSpace();
        }
    }
}
