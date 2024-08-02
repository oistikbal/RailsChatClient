// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System.Collections.Generic;
using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.Components;
using Doozy.Editor.EditorUI.Components.Internal;
using Doozy.Editor.EditorUI.ScriptableObjects.Colors;
using Doozy.Editor.EditorUI.Utils;
using Doozy.Runtime.Colors;
using Doozy.Runtime.Common.Extensions;
using Doozy.Runtime.Soundy;
using Doozy.Runtime.Soundy.ScriptableObjects;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
// ReSharper disable MemberCanBePrivate.Global

namespace Doozy.Editor.Soundy
{
    public static class SoundyEditorUtils
    {
        public static Color accentColor => EditorColors.Soundy.Color;
        public static EditorSelectableColorInfo selectableAccentColor => EditorSelectableColors.Soundy.Color;

        public static IEnumerable<Texture2D> buildEnabledIndicatorEnabledIcon => EditorSpriteSheets.EditorUI.Icons.Prefab;
        public static IEnumerable<Texture2D> buildEnabledIndicatorDisabledIcon => EditorSpriteSheets.EditorUI.Icons.Prefab;
        
        public static class Elements
        {
            /// <summary>
            /// Get the FluidField for an Audio Clip property.
            /// This also adds audio preview controls (play/stop) to the field.
            /// </summary>
            /// <param name="clipProperty"> SerializedProperty for an AudioClip </param>
            /// <returns> The FluidField for the given property </returns>
            public static FluidField GetClipFluidField(SerializedProperty clipProperty)
            {
                Label clipDurationLabel =
                    DesignUtils.NewLabel()
                        .SetStyleTextAlign(TextAnchor.MiddleRight)
                        .SetStyleFontSize(9)
                        .SetStyleColor(EditorColors.Default.TextDescription)
                        .SetStyleMarginRight(DesignUtils.k_Spacing)
                        .SetStyleMarginLeft(DesignUtils.k_Spacing);

                ObjectField objectField =
                    DesignUtils.NewObjectField(clipProperty, typeof(AudioClip), false)
                        .SetStyleFlexGrow(1);

                objectField.RegisterValueChangedCallback(evt =>
                {
                    if (evt?.newValue == null)
                    {
                        clipDurationLabel.SetText(AudioUtils.GetAudioClipDurationPretty(null));
                        return;
                    }
                    var clip = evt.newValue as AudioClip;
                    clipDurationLabel.SetText($"{AudioUtils.GetAudioClipDurationPretty(clip)}");
                });

                clipDurationLabel.SetText($"{AudioUtils.GetAudioClipDurationPretty(clipProperty.objectReferenceValue as AudioClip)}");

                FluidField field = GetFluidField()
                    .SetLabelText("Audio Clip")
                    .AddFieldContent
                    (
                        DesignUtils.row
                            .AddChild(objectField)
                            .AddChild(clipDurationLabel)
                    );

                return field;
            }

            /// <summary> Get the FluidField a float property that represents a volume value </summary>
            /// <param name="volumeProperty"> SerializedProperty for a float used as a volume value </param>
            /// <returns> The FluidField for the given property </returns>
            public static FluidField GetVolumeFluidField(SerializedProperty volumeProperty)
            {
                FloatField floatField =
                    DesignUtils.NewFloatField(volumeProperty)
                        .SetStyleWidth(40, 40, 40);

                FluidRangeSlider rangeSlider =
                    new FluidRangeSlider(SoundySettings.k_MinVolume, SoundySettings.k_MaxVolume)
                        .SetStyleFlexGrow(1)
                        .SetSnapInterval(0.05f)
                        .SetSnapValues(0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f)
                        .SetSnapDistanceForSnapValues(0.05f)
                        .SetSliderValue(volumeProperty.floatValue)
                        .SetAccentColor(accentColor);

                floatField.RegisterValueChangedCallback(evt =>
                {
                    if (evt?.newValue == null) return;
                    rangeSlider.SetSliderValue(evt.newValue);
                });

                rangeSlider.onValueChanged.AddListener(value =>
                {
                    volumeProperty.floatValue = value;
                    volumeProperty.serializedObject.ApplyModifiedProperties();
                    volumeProperty.serializedObject.Update();
                });

                FluidField field = GetFluidField()
                    .AddFieldContent
                    (
                        DesignUtils.row
                            .SetStyleAlignItems(Align.Center)
                            .AddChild(rangeSlider)
                            .AddSpaceBlock()
                            .AddChild(floatField)
                            .AddSpaceBlock()
                            .AddChild
                            (
                                GetResetButton()
                                    .SetOnClick(() =>
                                    {
                                        volumeProperty.floatValue = SoundySettings.k_DefaultVolume;
                                        volumeProperty.serializedObject.ApplyModifiedProperties();
                                        volumeProperty.serializedObject.Update();
                                    })
                            )
                    );

                return field;
            }

            /// <summary> Get the FluidField a float property that represents a pitch value </summary>
            /// <param name="pitchProperty"> SerializedProperty for a float used as a pitch value </param>
            /// <returns> The FluidField for the given property </returns>
            public static FluidField GetPitchFluidField(SerializedProperty pitchProperty)
            {
                FloatField floatField =
                    DesignUtils.NewFloatField(pitchProperty)
                        .SetStyleWidth(40, 40, 40);

                FluidRangeSlider rangeSlider =
                    new FluidRangeSlider(SoundySettings.k_MinPitch, SoundySettings.k_MaxPitch)
                        .SetStyleFlexGrow(1)
                        .SetSnapInterval(0.05f)
                        .SetSnapValues(0.2f, 0.4f, 0.6f, 0.8f, 1f, 1.2f, 1.4f, 1.6f, 1.8f, 2f, 2.2f, 2.4f, 2.6f, 2.8f, 3f)
                        .SetSnapDistanceForSnapValues(0.05f)
                        .SetSliderValue(pitchProperty.floatValue)
                        .SetAccentColor(accentColor);

                floatField.RegisterValueChangedCallback(evt =>
                {
                    if (evt?.newValue == null) return;
                    rangeSlider.SetSliderValue(evt.newValue);
                });

                rangeSlider.onValueChanged.AddListener(value =>
                {
                    pitchProperty.floatValue = value;
                    pitchProperty.serializedObject.ApplyModifiedProperties();
                    pitchProperty.serializedObject.Update();
                });

                FluidField field = GetFluidField()
                    .AddFieldContent
                    (
                        DesignUtils.row
                            .SetStyleAlignItems(Align.Center)
                            .AddChild(rangeSlider)
                            .AddSpaceBlock()
                            .AddChild(floatField)
                            .AddSpaceBlock()
                            .AddChild
                            (
                                GetResetButton()
                                    .SetOnClick(() =>
                                    {
                                        pitchProperty.floatValue = SoundySettings.k_DefaultPitch;
                                        pitchProperty.serializedObject.ApplyModifiedProperties();
                                        pitchProperty.serializedObject.Update();
                                    })
                            )
                    );

                return field;
            }

            /// <summary> Get the FluidField an integer property that represents a priority value </summary>
            /// <param name="priorityProperty"> SerializedProperty for an integer used as a priority value </param>
            /// <returns> The FluidField for the given property </returns>
            public static FluidField GetPriorityFluidField(SerializedProperty priorityProperty)
            {
                IntegerField integerField =
                    DesignUtils.NewIntegerField(priorityProperty)
                        .SetStyleWidth(40, 40, 40);

                FluidRangeSlider rangeSlider =
                    new FluidRangeSlider(SoundySettings.k_MinPriority, SoundySettings.k_MaxPriority)
                        .SetStyleFlexGrow(1)
                        .SetSnapInterval(1)
                        .SetSnapValues(0, 32, 64, 96, 128, 160, 192, 224, 256)
                        .SetSnapDistanceForSnapValues(1)
                        .SetSliderLowAndHighValueLabelTexts("High", "Low")
                        .SetSliderValue(priorityProperty.intValue)
                        .SetAccentColor(accentColor);

                integerField.RegisterValueChangedCallback(evt =>
                {
                    if (evt?.newValue == null) return;
                    rangeSlider.SetSliderValue(evt.newValue);
                });

                rangeSlider.onValueChanged.AddListener(value =>
                {
                    priorityProperty.intValue = (int)value;
                    priorityProperty.serializedObject.ApplyModifiedProperties();
                    priorityProperty.serializedObject.Update();
                });

                FluidField field =
                    GetFluidField()
                        .SetTooltip("Priority of this sound (0 = highest priority, 255 = lowest priority)")
                        .AddFieldContent
                        (
                            DesignUtils.row
                                .SetStyleAlignItems(Align.Center)
                                .AddChild(rangeSlider)
                                .AddSpaceBlock()
                                .AddChild(integerField)
                                .AddSpaceBlock()
                                .AddChild
                                (
                                    GetResetButton()
                                        .SetOnClick(() =>
                                        {
                                            priorityProperty.intValue = SoundySettings.k_DefaultPriority;
                                            priorityProperty.serializedObject.ApplyModifiedProperties();
                                            priorityProperty.serializedObject.Update();
                                        })
                                )
                        );

                return field;
            }

            /// <summary> Get the FluidField a float property that represents a pan stereo value. </summary>
            /// <param name="panStereoProperty"> SerializedProperty for a float used as a pan stereo value </param>
            /// <returns> The FluidField for the given property </returns>
            public static FluidField GetPanStereoFluidField(SerializedProperty panStereoProperty)
            {
                FloatField floatField =
                    DesignUtils.NewFloatField(panStereoProperty)
                        .SetStyleWidth(40, 40, 40);

                FluidRangeSlider rangeSlider =
                    new FluidRangeSlider(SoundySettings.k_MinPanStereo, SoundySettings.k_MaxPanStereo)
                        .SetStyleFlexGrow(1)
                        .SetSnapInterval(0.05f)
                        .SetSnapValues(-1f, -0.75f, -0.5f, -0.25f, 0f, 0.25f, 0.5f, 0.75f, 1f)
                        .SetSnapDistanceForSnapValues(0.05f)
                        .SetSliderLowAndHighValueLabelTexts("Left", "Right")
                        .SetSliderValue(panStereoProperty.floatValue)
                        .SetAccentColor(accentColor);

                floatField.RegisterValueChangedCallback(evt =>
                {
                    if (evt?.newValue == null) return;
                    rangeSlider.SetSliderValue(evt.newValue);
                });

                rangeSlider.onValueChanged.AddListener(value =>
                {
                    panStereoProperty.floatValue = value;
                    panStereoProperty.serializedObject.ApplyModifiedProperties();
                    panStereoProperty.serializedObject.Update();
                });

                FluidField field =
                    GetFluidField()
                        .SetTooltip("Pans the sound in a stereo way (left or right). This only applies to sounds that are Mono or Stereo.")
                        .AddFieldContent
                        (
                            DesignUtils.row
                                .SetStyleAlignItems(Align.Center)
                                .AddChild(rangeSlider)
                                .AddSpaceBlock()
                                .AddChild(floatField)
                                .AddSpaceBlock()
                                .AddChild
                                (
                                    GetResetButton()
                                        .SetOnClick(() =>
                                        {
                                            panStereoProperty.floatValue = SoundySettings.k_DefaultPanStereo;
                                            panStereoProperty.serializedObject.ApplyModifiedProperties();
                                            panStereoProperty.serializedObject.Update();
                                        })
                                )
                        );

                return field;
            }

            /// <summary> Get the FluidField for a float property that represents a spatial blend value </summary>
            /// <param name="spatialBlendProperty"> SerializedProperty for a float used as a spatial blend value </param>
            /// <returns> The FluidField for the given property </returns>
            public static FluidField GetSpatialBlendFluidField(SerializedProperty spatialBlendProperty)
            {
                FloatField floatField =
                    DesignUtils.NewFloatField(spatialBlendProperty)
                        .SetStyleWidth(40, 40, 40);

                FluidRangeSlider rangeSlider =
                    new FluidRangeSlider(SoundySettings.k_MinSpatialBlend, SoundySettings.k_MaxSpatialBlend)
                        .SetStyleFlexGrow(1)
                        .SetSnapInterval(0.05f)
                        .SetSnapValues(0f, 0.25f, 0.5f, 0.75f, 1f)
                        .SetSnapDistanceForSnapValues(0.05f)
                        .SetSliderLowAndHighValueLabelTexts("2D", "3D")
                        .SetSliderValue(spatialBlendProperty.floatValue)
                        .SetAccentColor(accentColor);

                floatField.RegisterValueChangedCallback(evt =>
                {
                    if (evt?.newValue == null) return;
                    rangeSlider.SetSliderValue(evt.newValue);
                });

                rangeSlider.onValueChanged.AddListener(value =>
                {
                    spatialBlendProperty.floatValue = value;
                    spatialBlendProperty.serializedObject.ApplyModifiedProperties();
                    spatialBlendProperty.serializedObject.Update();
                });

                FluidField field =
                    GetFluidField()
                        .SetTooltip
                        (
                            "Spatial Blend factor for this sound that sets how much this sound is affected by 3D spatialisation calculations " +
                            "(attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D."
                        )
                        .AddFieldContent
                        (
                            DesignUtils.row
                                .SetStyleAlignItems(Align.Center)
                                .AddChild(rangeSlider)
                                .AddSpaceBlock()
                                .AddChild(floatField)
                                .AddSpaceBlock()
                                .AddChild
                                (
                                    GetResetButton()
                                        .SetOnClick(() =>
                                        {
                                            spatialBlendProperty.floatValue = SoundySettings.k_DefaultSpatialBlend;
                                            spatialBlendProperty.serializedObject.ApplyModifiedProperties();
                                            spatialBlendProperty.serializedObject.Update();
                                        })
                                )
                        );

                return field;
            }

            /// <summary> Get the FluidField for a float property that represents a reverb zone mix value </summary>
            /// <param name="reverbZoneMixProperty"> SerializedProperty for a float used as a reverb zone mix value </param>
            /// <returns> The FluidField for the given property </returns>
            public static FluidField GetReverbZoneMixFluidField(SerializedProperty reverbZoneMixProperty)
            {
                FloatField floatField =
                    DesignUtils.NewFloatField(reverbZoneMixProperty)
                        .SetStyleWidth(40, 40, 40);

                FluidRangeSlider rangeSlider =
                    new FluidRangeSlider(SoundySettings.k_MinReverbZoneMix, SoundySettings.k_MaxReverbZoneMix)
                        .SetStyleFlexGrow(1)
                        .SetSnapInterval(0.01f)
                        .SetSnapValues(0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f, 1.1f)
                        .SetSnapDistanceForSnapValues(0.01f)
                        .SetSliderValue(reverbZoneMixProperty.floatValue)
                        .SetAccentColor(accentColor);

                floatField.RegisterValueChangedCallback(evt =>
                {
                    if (evt?.newValue == null) return;
                    rangeSlider.SetSliderValue(evt.newValue);

                });

                rangeSlider.onValueChanged.AddListener(value =>
                {
                    reverbZoneMixProperty.floatValue = value;
                    reverbZoneMixProperty.serializedObject.ApplyModifiedProperties();
                    reverbZoneMixProperty.serializedObject.Update();
                });

                FluidField field =
                    GetFluidField()
                        .SetTooltip("The amount by which the signal from this sound will be mixed into the global reverb associated with the Reverb Zones.")
                        .AddFieldContent
                        (
                            DesignUtils.row
                                .SetStyleAlignItems(Align.Center)
                                .AddChild(rangeSlider)
                                .AddSpaceBlock()
                                .AddChild(floatField)
                                .AddSpaceBlock()
                                .AddChild
                                (
                                    GetResetButton()
                                        .SetOnClick(() =>
                                        {
                                            reverbZoneMixProperty.floatValue = SoundySettings.k_DefaultReverbZoneMix;
                                            reverbZoneMixProperty.serializedObject.ApplyModifiedProperties();
                                            reverbZoneMixProperty.serializedObject.Update();
                                        })
                                )
                        );

                return field;
            }

            /// <summary> Get the FluidField for a float property that represents a doppler level value </summary>
            /// <param name="dopplerLevelProperty"> SerializedProperty for a float used as a doppler level value </param>
            /// <returns> The FluidField for the given property </returns>
            public static FluidField GetDopplerLevelFluidField(SerializedProperty dopplerLevelProperty)
            {
                FloatField floatField =
                    DesignUtils.NewFloatField(dopplerLevelProperty)
                        .SetStyleWidth(40, 40, 40);

                FluidRangeSlider rangeSlider =
                    new FluidRangeSlider(SoundySettings.k_MinDopplerLevel, SoundySettings.k_MaxDopplerLevel)
                        .SetStyleFlexGrow(1)
                        .SetSnapInterval(0.05f)
                        .SetSnapValues(0f, 0.5f, 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4, 4.5f, 5f)
                        .SetSnapDistanceForSnapValues(0.05f)
                        .SetSliderValue(dopplerLevelProperty.floatValue)
                        .SetAccentColor(accentColor);

                floatField.RegisterValueChangedCallback(evt =>
                {
                    if (evt?.newValue == null) return;
                    rangeSlider.SetSliderValue(evt.newValue);

                });

                rangeSlider.onValueChanged.AddListener(value =>
                {
                    dopplerLevelProperty.floatValue = value;
                    dopplerLevelProperty.serializedObject.ApplyModifiedProperties();
                    dopplerLevelProperty.serializedObject.Update();
                });

                FluidField field =
                    GetFluidField()
                        .SetLabelText("Doppler Level")
                        .SetTooltip("Set the Doppler scale for this sound. This is the amount of effect the listener's velocity has on the pitch of the sound.")
                        .SetElementSize(ElementSize.Small)
                        .AddFieldContent
                        (
                            DesignUtils.row
                                .SetStyleAlignItems(Align.Center)
                                .AddChild(rangeSlider)
                                .AddSpaceBlock()
                                .AddChild(floatField)
                                .AddSpaceBlock()
                                .AddChild
                                (
                                    GetResetButton()
                                        .SetOnClick(() =>
                                        {
                                            dopplerLevelProperty.floatValue = SoundySettings.k_DefaultDopplerLevel;
                                            dopplerLevelProperty.serializedObject.ApplyModifiedProperties();
                                            dopplerLevelProperty.serializedObject.Update();
                                        })
                                )
                        );

                return field;
            }

            /// <summary> Get the FluidField for a float property that represents a spread value </summary>
            /// <param name="spreadProperty"> SerializedProperty for a float used as a spread value </param>
            /// <returns> The FluidField for the given property </returns>
            public static FluidField GetSpreadFluidField(SerializedProperty spreadProperty)
            {
                IntegerField integerField =
                    DesignUtils.NewIntegerField(spreadProperty)
                        .SetStyleWidth(40, 40, 40);

                FluidRangeSlider rangeSlider =
                    new FluidRangeSlider(SoundySettings.k_MinSpread, SoundySettings.k_MaxSpread)
                        .SetStyleFlexGrow(1)
                        .SetSnapInterval(1)
                        //range from 0 to 360 with 0 as default
                        .SetSnapValues(0, 45, 90, 135, 180, 225, 270, 315, 360)
                        .SetSnapDistanceForSnapValues(1)
                        .SetSliderValue(spreadProperty.intValue)
                        .SetAccentColor(accentColor);

                integerField.RegisterValueChangedCallback(evt =>
                {
                    if (evt?.newValue == null) return;
                    rangeSlider.SetSliderValue(evt.newValue);
                });

                rangeSlider.onValueChanged.AddListener(value =>
                {
                    spreadProperty.intValue = (int)value;
                    spreadProperty.serializedObject.ApplyModifiedProperties();
                    spreadProperty.serializedObject.Update();
                });

                FluidField field =
                    GetFluidField()
                        .SetLabelText("Spread")
                        .SetTooltip("Set the spread angle (in degrees) of a 3d stereo or multichannel sound in speaker space.")
                        .SetElementSize(ElementSize.Small)
                        .AddFieldContent
                        (
                            DesignUtils.row
                                .SetStyleAlignItems(Align.Center)
                                .AddChild(rangeSlider)
                                .AddSpaceBlock()
                                .AddChild(integerField)
                                .AddSpaceBlock()
                                .AddChild
                                (
                                    GetResetButton()
                                        .SetOnClick(() =>
                                        {
                                            spreadProperty.intValue = SoundySettings.k_DefaultSpread;
                                            spreadProperty.serializedObject.ApplyModifiedProperties();
                                            spreadProperty.serializedObject.Update();
                                        })
                                )
                        );

                return field;
            }

            /// <summary> Get the FluidField for a float property that represents a min distance value </summary>
            /// <param name="minDistanceProperty"> SerializedProperty for a float used as a min distance value </param>
            /// <returns> The FluidField for the given property </returns>
            public static FluidField GetMinDistanceFluidField(SerializedProperty minDistanceProperty)
            {
                FloatField floatField =
                    DesignUtils.NewFloatField(minDistanceProperty)
                        .SetStyleFlexGrow(1);

                FluidField field =
                    GetFluidField()
                        .SetLabelText("Min Distance")
                        .SetTooltip("Within the Min distance the sound will cease to grow louder in volume")
                        .SetElementSize(ElementSize.Small)
                        .AddFieldContent
                        (
                            DesignUtils.row
                                .SetStyleAlignItems(Align.Center)
                                .AddChild(floatField)
                                .AddSpaceBlock()
                                .AddChild
                                (
                                    GetResetButton()
                                        .SetOnClick(() =>
                                        {
                                            minDistanceProperty.floatValue = SoundySettings.k_DefaultMinDistance;
                                            minDistanceProperty.serializedObject.ApplyModifiedProperties();
                                            minDistanceProperty.serializedObject.Update();
                                        })
                                )
                        );

                return field;
            }

            /// <summary> Get the FluidField for a float property that represents a max distance value </summary>
            /// <param name="maxDistanceProperty"> SerializedProperty for a float used as a max distance value </param>
            /// <returns> The FluidField for the given property </returns>
            public static FluidField GetMaxDistanceFluidField(SerializedProperty maxDistanceProperty)
            {
                FloatField floatField =
                    DesignUtils.NewFloatField(maxDistanceProperty)
                        .SetStyleFlexGrow(1);

                FluidField field =
                    GetFluidField()
                        .SetLabelText("Max Distance")
                        .SetTooltip("(Logarithmic rolloff) MaxDistance is the distance a sound stops attenuating at.")
                        .SetElementSize(ElementSize.Small)
                        .AddFieldContent
                        (
                            DesignUtils.row
                                .SetStyleAlignItems(Align.Center)
                                .AddChild(floatField)
                                .AddSpaceBlock()
                                .AddChild
                                (
                                    GetResetButton()
                                        .SetOnClick(() =>
                                        {
                                            maxDistanceProperty.floatValue = SoundySettings.k_DefaultMaxDistance;
                                            maxDistanceProperty.serializedObject.ApplyModifiedProperties();
                                            maxDistanceProperty.serializedObject.Update();
                                        })
                                )
                        );

                return field;
            }

            /// <summary> Get the FluidField a float property that represents a weight value (used for randomization) </summary>
            /// <param name="weightProperty"> SerializedProperty for a float used as a weight value </param>
            /// <returns> The FluidField for the given property </returns>
            public static FluidField GetWeightFluidField(SerializedProperty weightProperty)
            {
                IntegerField integerField =
                    DesignUtils.NewIntegerField(weightProperty)
                        .SetStyleWidth(40, 40, 40);

                FluidRangeSlider rangeSlider =
                    new FluidRangeSlider(SoundySettings.k_MinWeight, SoundySettings.k_MaxWeight)
                        .SetStyleFlexGrow(1)
                        .SetSnapInterval(1)
                        .SetSnapValues(0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100)
                        .SetSnapDistanceForSnapValues(1)
                        .SetSliderValue(weightProperty.intValue)
                        .SetAccentColor(accentColor);

                integerField.RegisterValueChangedCallback(evt =>
                {
                    if (evt?.newValue == null) return;
                    rangeSlider.SetSliderValue(evt.newValue);
                });

                rangeSlider.onValueChanged.AddListener(value =>
                {
                    weightProperty.intValue = (int)value;
                    weightProperty.serializedObject.ApplyModifiedProperties();
                    weightProperty.serializedObject.Update();
                });

                FluidField field =
                    GetFluidField()
                        .SetTooltip
                        (
                            "Weight used for randomization. " +
                            "The higher the weight, the more chances this sound has to be picked when using randomization."
                        )
                        .AddFieldContent
                        (
                            DesignUtils.row
                                .SetStyleAlignItems(Align.Center)
                                .AddChild(rangeSlider)
                                .AddSpaceBlock()
                                .AddChild(integerField)
                                .AddSpaceBlock()
                                .AddChild
                                (
                                    GetResetButton()
                                        .SetOnClick(() =>
                                        {
                                            weightProperty.intValue = SoundySettings.k_DefaultWeight;
                                            weightProperty.serializedObject.ApplyModifiedProperties();
                                            weightProperty.serializedObject.Update();
                                        })
                                )
                        );

                return field;
            }

            /// <summary> Get a FluidField used for properties (volume, pitch, weight, etc.). </summary>
            /// <returns> The FluidField with a customized style </returns>
            public static FluidField GetFluidField() =>
                FluidField.Get()
                    .SetElementSize(ElementSize.Tiny);

            /// <summary> Get a FluidButton for an add button used to add new items to a list. </summary>
            /// <returns> The FluidButton for an add button </returns>
            public static FluidButton GetAddButton() =>
                FluidButton.Get()
                    .SetLabelText("Add New")
                    .SetTooltip("Add a new item")
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Plus)
                    .SetAccentColor(EditorSelectableColors.Default.Add)
                    .SetButtonStyle(ButtonStyle.Contained)
                    .SetElementSize(ElementSize.Small)
                    .SetStyleFlexShrink(0);

            /// <summary> Get a FluidButton for a reset button. </summary>
            /// <returns> The FluidButton for a reset button </returns>
            public static FluidButton GetResetButton() =>
                FluidButton.Get()
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Reset)
                    .SetTooltip("Reset to default value")
                    .SetButtonStyle(ButtonStyle.Clear)
                    .SetElementSize(ElementSize.Tiny)
                    .SetStyleFlexShrink(0)
                    .SetAccentColor(selectableAccentColor);

            /// <summary> Get a FluidToggleButtonTab for a tab used for a property (volume, pitch, weight, etc.) </summary>
            /// <returns> The FluidToggleButtonTab for a tab used for a property </returns>
            public static FluidToggleButtonTab GetTab() =>
                FluidToggleButtonTab.Get()
                    .SetStyleFlexGrow(1)
                    .SetElementSize(ElementSize.Small)
                    .SetContainerColorOff(EditorColors.Default.FieldBackground)
                    .SetToggleAccentColor(selectableAccentColor);

            /// <summary>
            /// Get a VisualElement used as a container for a property (this is the drawer).
            /// </summary>
            /// <returns></returns>
            public static VisualElement GetPropertyContainer() =>
                new VisualElement()
                    .ResetLayout()
                    .SetStyleMarginBottom(DesignUtils.k_Spacing)
                    .SetStylePadding(DesignUtils.k_Spacing)
                    .SetStyleBorderRadius(DesignUtils.k_Spacing)
                    .SetStyleBackgroundColor(EditorColors.Default.BoxBackground);

            /// <summary>
            /// Get a VisualElement used as a container for a side element.
            /// These are usually vertical containers put to either side of a main element.
            /// They are used to display additional information or action buttons.
            /// </summary>
            /// <returns> The VisualElement used as a container for a side element </returns>
            public static VisualElement GetSideContainer() =>
                GetPropertyContainer()
                    .SetName("Container")
                    .SetStyleAlignItems(Align.Center);

            /// <summary> Get the copy button for duplicating an item in a list </summary>
            public static FluidButton GetDuplicateButton() =>
                GetActionButton()
                    .SetTooltip("Duplicate Item")
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Copy)
                    .SetElementSize(ElementSize.Small);

            /// <summary> Get the minus button for removing an item from a list </summary>
            public static FluidButton GetRemoveButton() =>
                FluidButton.Get()
                    .SetTooltip("Remove Item")
                    .SetButtonStyle(ButtonStyle.Clear)
                    .SetElementSize(ElementSize.Small)
                    .SetStyleFlexShrink(0)
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Minus)
                    .SetAccentColor(EditorSelectableColors.Default.Remove);

            /// <summary> Get a small clear button used on toolbars. </summary>
            /// <returns> The small clear button used on toolbars </returns>
            public static FluidButton GetToolbarButton() =>
                FluidButton.Get()
                    .SetStyleFlexShrink(0)
                    .SetButtonStyle(ButtonStyle.Clear)
                    .SetElementSize(ElementSize.Small)
                    .SetAccentColor(EditorSelectableColors.Default.Action);

            public static FluidButton GetSortAzButton() =>
                GetToolbarButton().SetTooltip("Sort A-Z").SetIcon(EditorSpriteSheets.EditorUI.Icons.SortAz);

            public static FluidButton GetSortZaButton() =>
                GetToolbarButton().SetTooltip("Sort Z-A").SetIcon(EditorSpriteSheets.EditorUI.Icons.SortZa);

            public static FluidButton GetSortLowHighButton() =>
                GetToolbarButton().SetTooltip("Sort Low-High").SetIcon(EditorSpriteSheets.EditorUI.Icons.SortAz);

            public static FluidButton GetSortHighLowButton() =>
                GetToolbarButton().SetTooltip("Sort High-Low").SetIcon(EditorSpriteSheets.EditorUI.Icons.SortZa);

            public static FluidButton GetClearButton() =>
                GetToolbarButton().SetTooltip("Clear").SetIcon(EditorSpriteSheets.EditorUI.Icons.Clear);

            public static FluidButton GetUpdateNameButton() =>
                GetToolbarButton().SetTooltip("Update Name").SetIcon(EditorSpriteSheets.EditorUI.Icons.Label);

            /// <summary> Get a tiny clear button used for actions (move up, move down, etc.). </summary>
            /// <returns> The tiny clear button used for actions </returns>
            public static FluidButton GetActionButton() =>
                FluidButton.Get()
                    .SetStyleFlexShrink(0)
                    .SetButtonStyle(ButtonStyle.Clear)
                    .SetElementSize(ElementSize.Tiny)
                    .SetAccentColor(EditorSelectableColors.Default.Action);

            /// <summary> Get reorder button for moving an item up in a list </summary>
            public static FluidButton GetMoveUpButton() =>
                GetActionButton().SetTooltip("Move Up").SetIcon(EditorSpriteSheets.EditorUI.Arrows.ChevronUp);

            /// <summary> Get reorder button for moving an item down in a list </summary>
            public static FluidButton GetMoveDownButton() =>
                GetActionButton().SetTooltip("Move Down").SetIcon(EditorSpriteSheets.EditorUI.Arrows.ChevronDown);

            /// <summary> Get a VisualElement used as a container for a toolbar. </summary>
            public static VisualElement GetToolbarContainer() =>
                DesignUtils.GetToolbarContainer()
                    .SetName("Toolbar Container")
                    .SetStyleBorderTopLeftRadius(DesignUtils.k_Spacing)
                    .SetStyleBorderTopRightRadius(DesignUtils.k_Spacing);

            /// <summary> Get a VisualElement used as a data container under a toolbar. </summary>
            public static VisualElement GetDataContainer() =>
                new VisualElement()
                    .ResetLayout()
                    .SetName("Data Container")
                    .SetStyleBackgroundColor(EditorColors.Default.Background)
                    .SetStyleBorderBottomLeftRadius(DesignUtils.k_FieldBorderRadius)
                    .SetStyleBorderBottomRightRadius(DesignUtils.k_FieldBorderRadius)
                    .SetStylePadding(DesignUtils.k_Spacing);
        }

        #if DOOZY_UIMANAGER
        public static class Nodes
        {
            public static Color enabledActionColor => EditorColors.Soundy.Color;
            public static Color disabledActionColor => EditorColors.Nody.NodeTitle;
            public static Color validIdColor => EditorColors.Default.TextTitle;
            public static Color invalidIdColor => EditorColors.Default.Remove;
            
            public static VisualElement GetNodeViewContainer(int orderNumber) =>
                new VisualElement()
                    .SetStyleFlexDirection(FlexDirection.Row)
                    .SetStyleAlignItems(Align.Center)
                    .AddChild
                    (
                        GetOrderLabel()
                            .SetStyleWidth(12)
                            .SetStyleFontSize(12)
                            .SetText(orderNumber.ToString())
                    );

            public class NodeSettingsSection : VisualElement
            {
                public Label orderLabel { get; private set; }
                public VisualElement orderContainer { get; private set; }
                public Label sectionTitleLabel { get; private set; }
                public Label sectionSubtitleLabel { get; private set; }
                public VisualElement titleContainer { get; private set; }
                public VisualElement dataContainer { get; private set; }

                public NodeSettingsSection()
                {
                    orderLabel =
                        GetOrderLabel();

                    orderContainer = new VisualElement()
                        .SetStyleFlexDirection(FlexDirection.Row)
                        .SetStyleMarginLeft(DesignUtils.k_Spacing)
                        .SetStyleMarginRight(DesignUtils.k_Spacing)
                        .SetStyleAlignItems(Align.Center)
                        .SetStyleDisplay(DisplayStyle.None)
                        .AddChild(orderLabel)
                        .AddSpaceBlock()
                        .AddChild(DesignUtils.dividerVertical)
                        .AddSpaceBlock();

                    sectionTitleLabel =
                        GetSectionTitleLabel();

                    sectionSubtitleLabel =
                        GetSectionSubtitleLabel()
                            .SetStyleMarginTop(DesignUtils.k_Spacing / 2f)
                            .SetStyleDisplay(DisplayStyle.None);

                    titleContainer =
                        Elements.GetToolbarContainer()
                            .SetStyleHeight(StyleKeyword.Auto)
                            .SetStyleAlignItems(Align.FlexStart)
                            .SetStyleJustifyContent(Justify.FlexStart)
                            .SetStyleFlexDirection(FlexDirection.Column)
                            .SetStylePadding(DesignUtils.k_Spacing2X);

                    dataContainer =
                        Elements.GetDataContainer()
                            .SetStylePadding(DesignUtils.k_Spacing2X);

                    titleContainer
                        .AddChild
                        (
                            DesignUtils.row
                                .AddChild(orderContainer)
                                .AddChild
                                (
                                    DesignUtils.column
                                        .AddChild(sectionTitleLabel)
                                        .AddChild(sectionSubtitleLabel)
                                )
                        );

                    this
                        .AddChild(titleContainer)
                        .AddChild(dataContainer)
                        ;
                }

                public NodeSettingsSection SetOrder(int order, bool show = true)
                {
                    orderLabel.SetText(order.ToString());
                    orderContainer.SetStyleDisplay(show ? DisplayStyle.Flex : DisplayStyle.None);
                    return this;
                }


                public NodeSettingsSection SetSectionTitle(string title)
                {
                    sectionTitleLabel.SetText(title);
                    return this;
                }

                public NodeSettingsSection SetSectionSubtitle(string subtitle)
                {
                    sectionSubtitleLabel.SetText(subtitle);
                    sectionSubtitleLabel.SetStyleDisplay(subtitle.IsNullOrEmpty() ? DisplayStyle.None : DisplayStyle.Flex);
                    return this;
                }

            }

            public static Label GetOrderLabel() =>
                DesignUtils.fieldLabel
                    .SetStyleFontSize(20)
                    .SetStyleColor(EditorColors.Default.FieldIcon.WithAlpha(0.2f));


            public static Label GetSectionTitleLabel() =>
                DesignUtils.NewLabel()
                    .SetStyleFontSize(12)
                    .SetStyleColor(accentColor);

            public static Label GetSectionSubtitleLabel() =>
                DesignUtils.NewLabel()
                    .SetWhiteSpace(WhiteSpace.Normal)
                    .SetStyleFontSize(10)
                    .SetStyleColor(EditorColors.Default.TextSubtitle);
        }
        #endif //DOOZY_UIMANAGER
    }
}
