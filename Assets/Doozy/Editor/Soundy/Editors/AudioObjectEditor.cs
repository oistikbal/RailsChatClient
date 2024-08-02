// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.Components;
using Doozy.Editor.EditorUI.Components.Internal;
using Doozy.Editor.EditorUI.ScriptableObjects.Colors;
using Doozy.Editor.EditorUI.Utils;
using Doozy.Runtime.Common.Extensions;
using Doozy.Runtime.Soundy.ScriptableObjects;
using Doozy.Runtime.Soundy.ScriptableObjects.Internal;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
// ReSharper disable MemberCanBePrivate.Global

namespace Doozy.Editor.Soundy.Editors
{
    /// <summary> Base class for all AudioObject editors </summary>
    public abstract class AudioObjectEditor : UnityEditor.Editor
    {
        protected static Color accentColor => EditorColors.Soundy.Color;
        protected static EditorSelectableColorInfo selectableAccentColor => EditorSelectableColors.Soundy.Color;

        protected VisualElement root { get; set; }
        protected FluidComponentHeader componentHeader { get; set; }

        protected TextField audioNameTextField { get; private set; }
        protected FluidField nameFluidField { get; set; }

        protected VisualElement firstTabsContainer { get; set; }
        protected VisualElement firstTabs { get; set; }
        protected VisualElement firstTabsContent { get; set; }
        protected FluidToggleGroup firstTabGroup { get; set; }
        protected FluidToggleButtonTab volumeTab { get; set; }
        protected FluidAnimatedContainer volumeAnimatedContainer { get; set; }
        protected FluidToggleButtonTab priorityTab { get; set; }
        protected FluidAnimatedContainer priorityAnimatedContainer { get; set; }

        protected VisualElement secondTabsContainer { get; set; }
        protected VisualElement secondTabs { get; set; }
        protected VisualElement secondTabsContent { get; set; }
        protected FluidToggleGroup secondTabGroup { get; set; }
        protected FluidToggleButtonTab panStereoTab { get; set; }
        protected FluidAnimatedContainer panStereoAnimatedContainer { get; set; }
        protected FluidToggleButtonTab spatialBlendTab { get; set; }
        protected FluidAnimatedContainer spatialBlendAnimatedContainer { get; set; }
        protected FluidToggleButtonTab reverbZoneMixTab { get; set; }
        protected FluidAnimatedContainer reverbZoneMixAnimatedContainer { get; set; }
        protected FluidToggleButtonTab _3DSettingsTab { get; set; }
        protected FluidAnimatedContainer _3DSettingsAnimatedContainer { get; set; }

        protected FluidField otherSettingsFluidField { get; set; }

        protected VisualElement toolbarContainer { get; set; }
        protected VisualElement dataContainer { get; set; }
        protected FluidDragAndDrop<AudioClip> dataFluidDragAndDrop { get; set; }
        protected SoundyEditorPlayer.PlayerElement playerElement { get; set; }

        protected SerializedProperty propertyAudioName { get; set; }
        protected SerializedProperty propertyPriority { get; set; }
        protected SerializedProperty propertyVolume { get; set; }
        protected SerializedProperty propertyPanStereo { get; set; }
        protected SerializedProperty propertySpatialBlend { get; set; }
        protected SerializedProperty propertyReverbZoneMix { get; set; }
        protected SerializedProperty propertyDopplerLevel { get; set; }
        protected SerializedProperty propertySpread { get; set; }
        protected SerializedProperty propertyMaxDistance { get; set; }
        protected SerializedProperty propertyMinDistance { get; set; }
        protected SerializedProperty propertyIgnoreListenerPause { get; set; }
        protected SerializedProperty propertyLoop { get; set; }

        protected virtual void OnEnable()
        {
            Undo.undoRedoPerformed -= UpdateData;
            Undo.undoRedoPerformed += UpdateData;
        }

        protected virtual void OnDisable()
        {
            Undo.undoRedoPerformed -= UpdateData;
        }

        protected abstract void UpdateData();

        public override VisualElement CreateInspectorGUI()
        {
            FindSerializedProperties();
            Initialize();
            Compose();
            return root;
        }

        protected virtual void FindSerializedProperties()
        {
            propertyAudioName = serializedObject.FindProperty("AudioName");
            propertyPriority = serializedObject.FindProperty("Priority");
            propertyVolume = serializedObject.FindProperty("Volume");
            propertyPanStereo = serializedObject.FindProperty("PanStereo");
            propertySpatialBlend = serializedObject.FindProperty("SpatialBlend");
            propertyReverbZoneMix = serializedObject.FindProperty("ReverbZoneMix");
            propertyDopplerLevel = serializedObject.FindProperty("DopplerLevel");
            propertySpread = serializedObject.FindProperty("Spread");
            propertyMaxDistance = serializedObject.FindProperty("MaxDistance");
            propertyMinDistance = serializedObject.FindProperty("MinDistance");
            propertyIgnoreListenerPause = serializedObject.FindProperty("IgnoreListenerPause");
            propertyLoop = serializedObject.FindProperty("Loop");
        }

        protected virtual void Initialize()
        {
            root = DesignUtils.editorRoot;

            componentHeader =
                DesignUtils.editorComponentHeader
                    .SetIcon(EditorSpriteSheets.Soundy.Icons.Soundy)
                    .SetComponentNameText("Audio Object")
                    .SetAccentColor(accentColor)
                    .SetElementSize(ElementSize.Normal)
                    .SetStyleMarginBottom(DesignUtils.k_Spacing);

            InitializeName();

            InitializeFirstTabs();
            InitializePriority();
            InitializeVolume();

            firstTabs
                .AddChild(volumeTab)
                .AddSpaceBlock()
                .AddChild(priorityTab);

            firstTabsContent
                .AddChild(volumeAnimatedContainer)
                .AddChild(priorityAnimatedContainer);

            InitializeSecondTabs();
            InitializePanStereo();
            InitializeSpatialBlend();
            InitializeReverbZoneMix();
            Initialize3DSettings();

            secondTabs
                .AddChild(panStereoTab)
                .AddSpaceBlock()
                .AddChild(spatialBlendTab)
                .AddSpaceBlock()
                .AddChild(reverbZoneMixTab)
                .AddSpaceBlock()
                .AddChild(_3DSettingsTab);

            secondTabsContent
                .AddChild(panStereoAnimatedContainer)
                .AddChild(spatialBlendAnimatedContainer)
                .AddChild(reverbZoneMixAnimatedContainer)
                .AddChild(_3DSettingsAnimatedContainer);

            InitializeOtherSettings();

            float volumeValue = propertyVolume.floatValue + 1;
            int priorityValue = propertyPriority.intValue + 1;
            float panStereoValue = propertyPanStereo.floatValue + 1;
            float spatialBlendValue = propertySpatialBlend.floatValue + 1;
            float reverbZoneMixValue = propertyReverbZoneMix.floatValue + 1;

            root.schedule.Execute(() =>
            {
                if (!(Math.Abs(volumeValue - propertyVolume.floatValue) < Mathf.Epsilon))
                {
                    volumeValue = propertyVolume.floatValue;
                    volumeTab.SetLabelText($"Volume: {volumeValue * 100:0}%");
                }

                if (!(Math.Abs(priorityValue - propertyPriority.intValue) < Mathf.Epsilon))
                {
                    priorityValue = propertyPriority.intValue;
                    priorityTab.SetLabelText($"Priority: {priorityValue}");
                }

                if (!(Math.Abs(panStereoValue - propertyPanStereo.floatValue) < Mathf.Epsilon))
                {
                    panStereoValue = propertyPanStereo.floatValue;
                    panStereoTab.SetLabelText($"Pan Stereo: {panStereoValue:0.00} ({(panStereoValue < 0 ? "Left" : panStereoValue > 0 ? "Right" : "Center")})");
                }

                if (!(Math.Abs(spatialBlendValue - propertySpatialBlend.floatValue) < Mathf.Epsilon))
                {
                    spatialBlendValue = propertySpatialBlend.floatValue;
                    spatialBlendTab.SetLabelText($"Spatial Blend: {spatialBlendValue:0.00} ({(spatialBlendValue < 0.5f ? "2D" : "3D")})");
                }

                if (!(Math.Abs(reverbZoneMixValue - propertyReverbZoneMix.floatValue) < Mathf.Epsilon))
                {
                    reverbZoneMixValue = propertyReverbZoneMix.floatValue;
                    reverbZoneMixTab.SetLabelText($"Reverb Zone Mix: {reverbZoneMixValue:0.00}");
                }

            }).Every(100);
        }

        private void InitializeName()
        {
            audioNameTextField =
                DesignUtils.NewTextField(propertyAudioName, true)
                    .SetStyleFlexGrow(1);

            nameFluidField =
                FluidField.Get("Name")
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Label)
                    .AddFieldContent(audioNameTextField);

            audioNameTextField.RegisterValueChangedCallback(evt =>
            {
                if (evt?.newValue == null) return;
                string newName = evt.newValue.CleanName();

                if (newName.IsNullOrEmpty())
                    newName = SoundySettings.k_DefaultAudioName;

                if (newName != evt.newValue)
                {
                    propertyAudioName.stringValue = newName;
                    audioNameTextField.SetValueWithoutNotify(newName);
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    serializedObject.Update();
                    root.schedule.Execute(() => ((AudioObject)target).OnUpdate?.Invoke());
                }

                ((AudioObject)target).name = newName;
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssetIfDirty(target);
            });
        }

        private void InitializeVolume()
        {
            volumeAnimatedContainer =
                new FluidAnimatedContainer("Volume", true)
                    .Hide(false);

            volumeTab =
                GetTab()
                    .SetTooltip("Volume multiplier for this sound")
                    .SetTabPosition(TabPosition.TabOnLeft)
                    .SetElementSize(ElementSize.Normal)
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Sound)
                    .SetOnValueChanged(evt => volumeAnimatedContainer.Toggle(evt.newValue));

            volumeTab.AddToToggleGroup(firstTabGroup);

            volumeAnimatedContainer.SetOnShowCallback(() =>
            {
                volumeAnimatedContainer
                    .AddContent(SoundyEditorUtils.Elements.GetVolumeFluidField(propertyVolume).SetStyleMarginTop(DesignUtils.k_Spacing))
                    .Bind(serializedObject);
            });
        }

        private void InitializePriority()
        {
            priorityAnimatedContainer =
                new FluidAnimatedContainer("Priority", true)
                    .Hide(false);

            priorityTab =
                GetTab()
                    .SetTooltip("Priority of this sound (0 = highest priority, 255 = lowest priority")
                    .SetTabPosition(TabPosition.TabOnRight)
                    .SetElementSize(ElementSize.Normal)
                    .SetIcon(EditorSpriteSheets.EditorUI.Arrows.ChevronUp)
                    .SetOnValueChanged(evt => priorityAnimatedContainer.Toggle(evt.newValue));

            priorityTab.AddToToggleGroup(firstTabGroup);

            priorityAnimatedContainer.SetOnShowCallback(() =>
            {
                priorityAnimatedContainer
                    .AddContent(SoundyEditorUtils.Elements.GetPriorityFluidField(propertyPriority).SetStyleMarginTop(DesignUtils.k_Spacing))
                    .Bind(serializedObject);
            });
        }

        private void InitializePanStereo()
        {
            panStereoAnimatedContainer =
                new FluidAnimatedContainer("Pan Stereo", true)
                    .Hide(false);

            panStereoTab =
                GetTab()
                    .SetTooltip("Sets the panning (left/right) stereo level for this sound")
                    .SetTabPosition(TabPosition.TabOnLeft)
                    .SetOnValueChanged(evt => panStereoAnimatedContainer.Toggle(evt.newValue));

            panStereoTab.AddToToggleGroup(secondTabGroup);

            panStereoAnimatedContainer.SetOnShowCallback(() =>
            {
                panStereoAnimatedContainer
                    .AddContent(SoundyEditorUtils.Elements.GetPanStereoFluidField(propertyPanStereo).SetStyleMarginTop(DesignUtils.k_Spacing))
                    .Bind(serializedObject);
            });
        }

        private void InitializeSpatialBlend()
        {
            spatialBlendAnimatedContainer =
                new FluidAnimatedContainer("Spatial Blend", true)
                    .Hide(false);

            spatialBlendTab =
                GetTab()
                    .SetTooltip("Sets how much this sound is affected by 3D spatialization calculations")
                    .SetTabPosition(TabPosition.TabInCenter)
                    .SetOnValueChanged(evt => spatialBlendAnimatedContainer.Toggle(evt.newValue));

            spatialBlendTab.AddToToggleGroup(secondTabGroup);

            spatialBlendAnimatedContainer.SetOnShowCallback(() =>
            {
                spatialBlendAnimatedContainer
                    .AddContent(SoundyEditorUtils.Elements.GetSpatialBlendFluidField(propertySpatialBlend).SetStyleMarginTop(DesignUtils.k_Spacing))
                    .Bind(serializedObject);
            });
        }

        private void InitializeReverbZoneMix()
        {
            reverbZoneMixAnimatedContainer =
                new FluidAnimatedContainer("Reverb Zone Mix", true)
                    .Hide(false);

            reverbZoneMixTab =
                GetTab()
                    .SetTooltip("Sets the amount by which the signal from this sound is mixed into the global reverb associated with the Reverb Zones")
                    .SetTabPosition(TabPosition.TabInCenter)
                    .SetOnValueChanged(evt => reverbZoneMixAnimatedContainer.Toggle(evt.newValue));

            reverbZoneMixTab.AddToToggleGroup(secondTabGroup);

            reverbZoneMixAnimatedContainer.SetOnShowCallback(() =>
            {
                reverbZoneMixAnimatedContainer
                    .AddContent(SoundyEditorUtils.Elements.GetReverbZoneMixFluidField(propertyReverbZoneMix).SetStyleMarginTop(DesignUtils.k_Spacing))
                    .Bind(serializedObject);
            });
        }

        private void Initialize3DSettings()
        {
            _3DSettingsAnimatedContainer =
                new FluidAnimatedContainer("3D Settings", true)
                    .Hide(false);

            _3DSettingsTab =
                GetTab()
                    .SetLabelText("3D Settings")
                    .SetTooltip("3D Settings for this sound")
                    .SetTabPosition(TabPosition.TabOnRight)
                    .SetOnValueChanged(evt => _3DSettingsAnimatedContainer.Toggle(evt.newValue));

            _3DSettingsTab.AddToToggleGroup(secondTabGroup);

            _3DSettingsAnimatedContainer.SetOnShowCallback(() =>
            {
                _3DSettingsAnimatedContainer
                    .AddContent
                    (
                        DesignUtils.column
                            .AddChild
                                (SoundyEditorUtils.Elements.GetDopplerLevelFluidField(propertyDopplerLevel))
                            .AddSpaceBlock()
                            .AddChild(DesignUtils.dividerHorizontal)
                            .AddSpaceBlock()
                            .AddChild(SoundyEditorUtils.Elements.GetSpreadFluidField(propertySpread))
                    )
                    .AddContent(DesignUtils.spaceBlock)
                    .AddContent(DesignUtils.dividerHorizontal)
                    .AddContent(DesignUtils.spaceBlock)
                    .AddContent
                    (
                        DesignUtils.row
                            .AddChild(SoundyEditorUtils.Elements.GetMinDistanceFluidField(propertyMinDistance))
                            .AddSpaceBlock()
                            .AddChild(SoundyEditorUtils.Elements.GetMaxDistanceFluidField(propertyMaxDistance))
                    )
                    .Bind(serializedObject);
            });
        }

        private void InitializeOtherSettings()
        {
            var ignoreListenerPauseToggleSwitch =
                FluidToggleSwitch.Get()
                    .SetLabelText("Ignore Listener Pause")
                    .SetTooltip("If enabled, the audio will not be paused when the listener is paused")
                    .SetToggleAccentColor(selectableAccentColor)
                    .BindToProperty(propertyIgnoreListenerPause);

            var loopToggleSwitch =
                FluidToggleSwitch.Get()
                    .SetLabelText("Loop")
                    .SetTooltip("If enabled, the audio will loop")
                    .SetToggleAccentColor(selectableAccentColor)
                    .BindToProperty(propertyLoop);

            otherSettingsFluidField =
                FluidField.Get()
                    // .SetLabelText("Other Settings")
                    .AddFieldContent
                    (
                        DesignUtils.row
                            .AddChild(loopToggleSwitch)
                            .AddSpaceBlock()
                            .AddChild(ignoreListenerPauseToggleSwitch)
                    );
        }

        protected virtual void InitializeDataToolbar()
        {
            toolbarContainer = SoundyEditorUtils.Elements.GetToolbarContainer();
            dataFluidDragAndDrop = new FluidDragAndDrop<AudioClip>(OnDragAndDropAudioClip);
        }

        protected abstract void OnDragAndDropAudioClip();

        protected virtual void InitializeData()
        {
            dataContainer = SoundyEditorUtils.Elements.GetDataContainer();
            UpdateData();
        }

        protected virtual void Compose()
        {
            root
                .AddChild(componentHeader)
                .AddSpaceBlock()
                .AddChild(nameFluidField)
                .AddSpaceBlock()
                .AddChild(firstTabsContainer)
                .AddSpaceBlock()
                .AddChild(secondTabsContainer)
                .AddSpaceBlock()
                .AddChild(otherSettingsFluidField)
                ;
        }

        #region Tabs

        private static FluidToggleGroup newTabGroup =>
            new FluidToggleGroup()
                .SetControlMode(FluidToggleGroup.ControlMode.OneToggleOn);

        private static VisualElement newTabs =>
            new VisualElement()
                .ResetLayout()
                .SetStyleFlexGrow(1)
                .SetStyleFlexDirection(FlexDirection.Row)
                .SetStyleJustifyContent(Justify.Center)
                .SetStyleAlignItems(Align.Center);

        private static VisualElement newTabsContent =>
            new VisualElement().ResetLayout();

        private static VisualElement newTabContainer =>
            new VisualElement()
                .ResetLayout()
                .SetStylePadding(DesignUtils.k_Spacing)
                .SetStyleBackgroundColor(DesignUtils.fieldBackgroundColor)
                .SetStyleBorderRadius(DesignUtils.k_FieldBorderRadius);

        private void InitializeFirstTabs()
        {
            firstTabGroup = newTabGroup;
            firstTabs = newTabs.SetName("First Tabs");
            firstTabsContent = newTabsContent.SetName("First Tabs Content");
            firstTabsContainer = newTabContainer.SetName("First Tabs Container");

            firstTabsContainer
                .AddChild(firstTabs)
                .AddChild(firstTabsContent);
        }

        private void InitializeSecondTabs()
        {
            secondTabGroup = newTabGroup;
            secondTabs = newTabs.SetName("Second Tabs");
            secondTabsContent = newTabsContent.SetName("Second Tabs Content");
            secondTabsContainer = newTabContainer.SetName("Second Tabs Container");

            secondTabsContainer
                .AddChild(secondTabs)
                .AddChild(secondTabsContent);
        }

        #endregion


        protected static FluidToggleButtonTab GetTab() =>
            FluidToggleButtonTab.Get()
                .SetStyleFlexGrow(1)
                .SetElementSize(ElementSize.Small)
                .SetContainerColorOff(EditorColors.Default.Background)
                .SetToggleAccentColor(selectableAccentColor);
    }
}
