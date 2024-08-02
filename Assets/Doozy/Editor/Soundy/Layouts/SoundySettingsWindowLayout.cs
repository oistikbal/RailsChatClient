// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System.Collections.Generic;
using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.Components;
using Doozy.Editor.EditorUI.Components.Internal;
using Doozy.Editor.EditorUI.ScriptableObjects.Colors;
using Doozy.Editor.EditorUI.Utils;
using Doozy.Editor.Interfaces;
using Doozy.Runtime.Soundy.ScriptableObjects;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
// ReSharper disable UnusedType.Global

namespace Doozy.Editor.Soundy.Layouts
{
    public sealed class SoundySettingsWindowLayout : FluidWindowLayout, IDashboardSettingsWindowLayout
    {
        public int order => 0;

        public override string layoutName => "Soundy Settings";
        public override List<Texture2D> animatedIconTextures => EditorSpriteSheets.Soundy.Icons.Soundy;
        public override Color accentColor => EditorColors.Soundy.Color;
        public override EditorSelectableColorInfo selectableAccentColor => EditorSelectableColors.Soundy.Color;

        private SerializedObject serializedObject { get; set; }

        private SerializedProperty propertyAudioPlayerFullQualifiedTypeName { get; set; }
        private SerializedProperty propertyDestroyIdleAudioPlayers { get; set; }
        private SerializedProperty propertyIdleTime { get; set; }
        private SerializedProperty propertyIdleCheckInterval { get; set; }
        private SerializedProperty propertyMinSoundPlayersToKeepAlive { get; set; }
        private SerializedProperty propertyPreheatSoundPlayers { get; set; }
        private SerializedProperty propertyMinMusicPlayersToKeepAlive { get; set; }
        private SerializedProperty propertyPreheatMusicPlayers { get; set; }

        private TextField audioPlayerFullQualifiedTypeNameTextField { get; set; }
        private FluidToggleSwitch destroyIdleAudioPlayersSwitch { get; set; }
        private FloatField idleTimeFloatField { get; set; }
        private FloatField idleCheckIntervalFloatField { get; set; }
        private IntegerField minSoundPlayersToKeepAliveIntegerField { get; set; }
        private IntegerField preheatSoundPlayersIntegerField { get; set; }
        private IntegerField minMusicPlayersToKeepAliveIntegerField { get; set; }
        private IntegerField preheatMusicPlayersIntegerField { get; set; }

        private static IEnumerable<Texture2D> resetTextures => EditorSpriteSheets.EditorUI.Icons.Reset;
        private FluidButton resetAudioPlayerFullQualifiedTypeNameButton { get; set; }
        private FluidButton resetDestroyIdleAudioPlayersButton { get; set; }
        private FluidButton resetIdleTimeButton { get; set; }
        private FluidButton resetIdleCheckIntervalButton { get; set; }
        private FluidButton resetMinSoundPlayersToKeepAliveButton { get; set; }
        private FluidButton resetPreheatSoundPlayersButton { get; set; }
        private FluidButton resetMinMusicPlayersToKeepAliveButton { get; set; }
        private FluidButton resetPreheatMusicPlayersButton { get; set; }
        private FluidButton resetAllButton { get; set; }
        private FluidButton saveButton { get; set; }

        private FluidField audioPlayerFullQualifiedTypeNameFluidField { get; set; }
        private FluidField destroyIdleAudioPlayersFluidField { get; set; }
        private FluidField idleTimeFluidField { get; set; }
        private FluidField idleCheckIntervalFluidField { get; set; }
        private FluidField minSoundPlayersToKeepAliveFluidField { get; set; }
        private FluidField preheatSoundPlayersFluidField { get; set; }
        private FluidField minMusicPlayersToKeepAliveFluidField { get; set; }
        private FluidField preheatMusicPlayersFluidField { get; set; }

        public override void OnDestroy()
        {
            base.OnDestroy();
            destroyIdleAudioPlayersSwitch?.Recycle();

            resetDestroyIdleAudioPlayersButton?.Recycle();
            resetIdleCheckIntervalButton?.Recycle();
            resetIdleTimeButton?.Recycle();
            resetMinSoundPlayersToKeepAliveButton?.Recycle();
            resetPreheatSoundPlayersButton?.Recycle();
            resetMinMusicPlayersToKeepAliveButton?.Recycle();
            resetPreheatMusicPlayersButton?.Recycle();
            resetAudioPlayerFullQualifiedTypeNameButton?.Recycle();
            resetAllButton?.Recycle();
            saveButton?.Recycle();

            destroyIdleAudioPlayersFluidField?.Recycle();
            idleCheckIntervalFluidField?.Recycle();
            idleTimeFluidField?.Recycle();
            minSoundPlayersToKeepAliveFluidField?.Recycle();
            preheatSoundPlayersFluidField?.Recycle();
            minMusicPlayersToKeepAliveFluidField?.Recycle();
            preheatMusicPlayersFluidField?.Recycle();
            audioPlayerFullQualifiedTypeNameFluidField?.Recycle();
        }


        public SoundySettingsWindowLayout()
        {
            SoundySettings settings = SoundySettings.instance;
            serializedObject = new SerializedObject(settings);
            AddHeader("Soundy", "Global Settings", animatedIconTextures);
            menu.SetStyleDisplay(DisplayStyle.None);
            FindProperties();
            Initialize();
            Compose();
            content.Bind(serializedObject);
        }

        private void FindProperties()
        {
            propertyAudioPlayerFullQualifiedTypeName = serializedObject.FindProperty(nameof(SoundySettings.AudioPlayerFullQualifiedTypeName));
            propertyDestroyIdleAudioPlayers = serializedObject.FindProperty(nameof(SoundySettings.DestroyIdleAudioPlayers));
            propertyIdleTime = serializedObject.FindProperty(nameof(SoundySettings.IdleTime));
            propertyIdleCheckInterval = serializedObject.FindProperty(nameof(SoundySettings.IdleCheckInterval));
            propertyMinSoundPlayersToKeepAlive = serializedObject.FindProperty(nameof(SoundySettings.MinSoundPlayersToKeepAlive));
            propertyPreheatSoundPlayers = serializedObject.FindProperty(nameof(SoundySettings.PreheatSoundPlayers));
            propertyMinMusicPlayersToKeepAlive = serializedObject.FindProperty(nameof(SoundySettings.MinMusicPlayersToKeepAlive));
            propertyPreheatMusicPlayers = serializedObject.FindProperty(nameof(SoundySettings.PreheatMusicPlayers));
        }

        private void Initialize()
        {

            // Audio Player Full Qualified Type Name
            audioPlayerFullQualifiedTypeNameTextField =
                DesignUtils.NewTextField(propertyAudioPlayerFullQualifiedTypeName)
                    .SetStyleMinWidth(120);

            resetAudioPlayerFullQualifiedTypeNameButton =
                DesignUtils.SystemButton(resetTextures)
                    .SetStyleAlignSelf(Align.Center)
                    .SetTooltip("Reset")
                    .SetOnClick(() =>
                    {
                        propertyAudioPlayerFullQualifiedTypeName.stringValue = SoundySettings.Default.k_AudioPlayerFullQualifiedTypeName;
                        serializedObject.ApplyModifiedProperties();
                    });

            audioPlayerFullQualifiedTypeNameFluidField =
                FluidField.Get()
                    .SetStyleFlexGrow(0)
                    .SetElementSize(ElementSize.Normal)
                    .SetLabelText("Audio Player Full Qualified Type Name")
                    .SetTooltip
                    (
                        "The full qualified type name of the audio player to use. " +
                        "This is used to create the audio players. " +
                        "The type must derive from BaseAudioPlayer."
                    )
                    .AddFieldContent
                    (
                        DesignUtils.row
                            .AddFlexibleSpace()
                            .AddChild(audioPlayerFullQualifiedTypeNameTextField)
                            .AddSpaceBlock()
                            .AddChild(resetAudioPlayerFullQualifiedTypeNameButton)
                    );

            // Destroy Idle Audio Players
            destroyIdleAudioPlayersSwitch =
                FluidToggleSwitch.Get()
                    .SetToggleAccentColor(selectableAccentColor)
                    .BindToProperty(propertyDestroyIdleAudioPlayers);

            resetDestroyIdleAudioPlayersButton =
                DesignUtils.SystemButton(resetTextures)
                    .SetStyleAlignSelf(Align.Center)
                    .SetTooltip("Reset")
                    .SetOnClick(() =>
                    {
                        propertyDestroyIdleAudioPlayers.boolValue = SoundySettings.Default.k_DestroyIdleAudioPlayers;
                        serializedObject.ApplyModifiedProperties();
                    });

            destroyIdleAudioPlayersFluidField =
                FluidField.Get()
                    .SetStyleFlexGrow(0)
                    .SetElementSize(ElementSize.Normal)
                    .SetLabelText("Destroy Idle Audio Players")
                    .SetTooltip
                    (
                        "Automatically destroy idle audio players. " +
                        "An audio player is considered idle if it hasn't been used for more than the IdleTime duration."
                    )
                    .AddFieldContent
                    (
                        DesignUtils.row
                            .AddFlexibleSpace()
                            .AddChild(destroyIdleAudioPlayersSwitch)
                            .AddSpaceBlock()
                            .AddChild(resetDestroyIdleAudioPlayersButton)
                    );

            //Idle Time
            idleTimeFloatField =
                DesignUtils.NewFloatField(propertyIdleTime)
                    .SetStyleWidth(60);

            resetIdleTimeButton =
                DesignUtils.SystemButton(resetTextures)
                    .SetStyleAlignSelf(Align.Center)
                    .SetTooltip("Reset")
                    .SetOnClick(() =>
                    {
                        propertyIdleTime.floatValue = SoundySettings.Default.k_IdleTime;
                        serializedObject.ApplyModifiedProperties();
                    });

            idleTimeFluidField =
                FluidField.Get()
                    .SetStyleFlexGrow(0)
                    .SetElementSize(ElementSize.Normal)
                    .SetLabelText("Idle Time (seconds)")
                    .SetTooltip
                    (
                        "The duration (in seconds) after which an audio player is considered idle, if it hasn't been used."
                    )
                    .AddFieldContent
                    (
                        DesignUtils.row
                            .AddFlexibleSpace()
                            .AddChild(idleTimeFloatField)
                            .AddSpaceBlock()
                            .AddChild(resetIdleTimeButton)
                    );

            //Idle Check Interval
            idleCheckIntervalFloatField =
                DesignUtils.NewFloatField(propertyIdleCheckInterval)
                    .SetStyleWidth(60);

            resetIdleCheckIntervalButton =
                DesignUtils.SystemButton(resetTextures)
                    .SetStyleAlignSelf(Align.Center)
                    .SetTooltip("Reset")
                    .SetOnClick(() =>
                    {
                        propertyIdleCheckInterval.floatValue = SoundySettings.Default.k_IdleCheckInterval;
                        serializedObject.ApplyModifiedProperties();
                    });

            idleCheckIntervalFluidField =
                FluidField.Get()
                    .SetStyleFlexGrow(0)
                    .SetElementSize(ElementSize.Normal)
                    .SetLabelText("Idle Check Interval (seconds)")
                    .SetTooltip
                    (
                        "The interval (in seconds) at which the idle audio players are checked and destroyed if they're idle"
                    )
                    .AddFieldContent
                    (
                        DesignUtils.row
                            .AddFlexibleSpace()
                            .AddChild(idleCheckIntervalFloatField)
                            .AddSpaceBlock()
                            .AddChild(resetIdleCheckIntervalButton)
                    );

            //Preheat Sound Players
            preheatSoundPlayersIntegerField =
                DesignUtils.NewIntegerField(propertyPreheatSoundPlayers)
                    .SetStyleWidth(60);

            resetPreheatSoundPlayersButton =
                DesignUtils.SystemButton(resetTextures)
                    .SetStyleAlignSelf(Align.Center)
                    .SetTooltip("Reset")
                    .SetOnClick(() =>
                    {
                        propertyPreheatSoundPlayers.intValue = SoundySettings.Default.k_PreheatSoundPlayers;
                        serializedObject.ApplyModifiedProperties();
                    });

            preheatSoundPlayersFluidField =
                FluidField.Get()
                    .SetStyleFlexGrow(0)
                    .SetElementSize(ElementSize.Normal)
                    .SetIcon(EditorSpriteSheets.Soundy.Icons.AudioPlayer)
                    .SetLabelText("Preheat Sound Players")
                    .SetTooltip
                    (
                        "The number of audio players, used to play sounds, to preheat (create and cache) when the AudioEngine starts. " +
                        "The audio players are created and cached in the background, so they don't affect the performance at runtime."
                    )
                    .AddFieldContent
                    (
                        DesignUtils.row
                            .AddFlexibleSpace()
                            .AddChild(preheatSoundPlayersIntegerField)
                            .AddSpaceBlock()
                            .AddChild(resetPreheatSoundPlayersButton)
                    );


            //Min Sound Players To Keep Alive
            minSoundPlayersToKeepAliveIntegerField =
                DesignUtils.NewIntegerField(propertyMinSoundPlayersToKeepAlive)
                    .SetStyleWidth(60);

            resetMinSoundPlayersToKeepAliveButton =
                DesignUtils.SystemButton(resetTextures)
                    .SetStyleAlignSelf(Align.Center)
                    .SetTooltip("Reset")
                    .SetOnClick(() =>
                    {
                        propertyMinSoundPlayersToKeepAlive.intValue = SoundySettings.Default.k_MinSoundPlayersToKeepAlive;
                        serializedObject.ApplyModifiedProperties();
                    });

            minSoundPlayersToKeepAliveFluidField =
                FluidField.Get()
                    .SetStyleFlexGrow(0)
                    .SetElementSize(ElementSize.Normal)
                    .SetIcon(EditorSpriteSheets.Soundy.Icons.AudioPlayer)
                    .SetLabelText("Min Sound Players To Keep Alive")
                    .SetTooltip
                    (
                        "The minimum number of sound players to keep alive, even if they're idle. " +
                        "This means that, even if they're idle, they will not be destroyed if the number of sound effect players is less than this value."
                    )
                    .AddFieldContent
                    (
                        DesignUtils.row
                            .AddFlexibleSpace()
                            .AddChild(minSoundPlayersToKeepAliveIntegerField)
                            .AddSpaceBlock()
                            .AddChild(resetMinSoundPlayersToKeepAliveButton)
                    );

            //Preheat Music Players
            preheatMusicPlayersIntegerField =
                DesignUtils.NewIntegerField(propertyPreheatMusicPlayers)
                    .SetStyleWidth(60);

            resetPreheatMusicPlayersButton =
                DesignUtils.SystemButton(resetTextures)
                    .SetStyleAlignSelf(Align.Center)
                    .SetTooltip("Reset")
                    .SetOnClick(() =>
                    {
                        propertyPreheatMusicPlayers.intValue = SoundySettings.Default.k_PreheatMusicPlayers;
                        serializedObject.ApplyModifiedProperties();
                    });

            preheatMusicPlayersFluidField =
                FluidField.Get()
                    .SetStyleFlexGrow(0)
                    .SetElementSize(ElementSize.Normal)
                    .SetIcon(EditorSpriteSheets.Soundy.Icons.AudioPlayer)
                    .SetLabelText("Preheat Music Players")
                    .SetTooltip
                    (
                        "The number of audio players, used to play music, to preheat (create and cache) when the AudioEngine starts. " +
                        "The audio players are created and cached in the background, so they don't affect the performance at runtime."
                    )
                    .AddFieldContent
                    (
                        DesignUtils.row
                            .AddFlexibleSpace()
                            .AddChild(preheatMusicPlayersIntegerField)
                            .AddSpaceBlock()
                            .AddChild(resetPreheatMusicPlayersButton)
                    );

            //Min Music Players To Keep Alive
            minMusicPlayersToKeepAliveIntegerField =
                DesignUtils.NewIntegerField(propertyMinMusicPlayersToKeepAlive)
                    .SetStyleWidth(60);

            resetMinMusicPlayersToKeepAliveButton =
                DesignUtils.SystemButton(resetTextures)
                    .SetStyleAlignSelf(Align.Center)
                    .SetTooltip("Reset")
                    .SetOnClick(() =>
                    {
                        propertyMinMusicPlayersToKeepAlive.intValue = SoundySettings.Default.k_MinMusicPlayersToKeepAlive;
                        serializedObject.ApplyModifiedProperties();
                    });

            minMusicPlayersToKeepAliveFluidField =
                FluidField.Get()
                    .SetStyleFlexGrow(0)
                    .SetElementSize(ElementSize.Normal)
                    .SetIcon(EditorSpriteSheets.Soundy.Icons.AudioPlayer)
                    .SetLabelText("Min Music Players To Keep Alive")
                    .SetTooltip
                    (
                        "The minimum number of music players to keep alive, even if they're idle. " +
                        "This means that, even if they're idle, they will not be destroyed if the number of music players is less than this value."
                    )
                    .AddFieldContent
                    (
                        DesignUtils.row
                            .AddFlexibleSpace()
                            .AddChild(minMusicPlayersToKeepAliveIntegerField)
                            .AddSpaceBlock()
                            .AddChild(resetMinMusicPlayersToKeepAliveButton)
                    );




            //Reset All
            resetAllButton =
                FluidButton.Get()
                    .SetButtonStyle(ButtonStyle.Contained)
                    .SetElementSize(ElementSize.Small)
                    .SetLabelText("Reset All")
                    .SetIcon(resetTextures)
                    .SetOnClick(() =>
                    {
                        propertyAudioPlayerFullQualifiedTypeName.stringValue = SoundySettings.Default.k_AudioPlayerFullQualifiedTypeName;
                        propertyDestroyIdleAudioPlayers.boolValue = SoundySettings.Default.k_DestroyIdleAudioPlayers;
                        propertyIdleTime.floatValue = SoundySettings.Default.k_IdleTime;
                        propertyIdleCheckInterval.floatValue = SoundySettings.Default.k_IdleCheckInterval;
                        propertyMinSoundPlayersToKeepAlive.intValue = SoundySettings.Default.k_MinSoundPlayersToKeepAlive;
                        propertyPreheatSoundPlayers.intValue = SoundySettings.Default.k_PreheatSoundPlayers;
                        propertyMinMusicPlayersToKeepAlive.intValue = SoundySettings.Default.k_MinMusicPlayersToKeepAlive;
                        propertyPreheatMusicPlayers.intValue = SoundySettings.Default.k_PreheatMusicPlayers;
                        serializedObject.ApplyModifiedProperties();
                    });

            //Save
            saveButton =
                FluidButton.Get()
                    .SetButtonStyle(ButtonStyle.Contained)
                    .SetElementSize(ElementSize.Small)
                    .SetLabelText("Save")
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Save)
                    .SetOnClick(() =>
                    {
                        EditorUtility.SetDirty(SoundySettings.instance);
                        AssetDatabase.SaveAssets();
                    });

        }

        private void Compose()
        {
            content
                .AddChild(DesignUtils.NewFieldNameLabel("Audio Player").SetStyleMarginLeft(4))
                .AddSpaceBlock()
                .AddChild(audioPlayerFullQualifiedTypeNameFluidField)
                .AddSpaceBlock(4)
                .AddChild(DesignUtils.NewFieldNameLabel("Memory Management").SetStyleMarginLeft(4))
                .AddSpaceBlock()
                .AddChild(destroyIdleAudioPlayersFluidField)
                .AddSpaceBlock()
                .AddChild(idleTimeFluidField)
                .AddSpaceBlock()
                .AddChild(idleCheckIntervalFluidField)
                .AddSpaceBlock(4)
                .AddChild(DesignUtils.NewFieldNameLabel("Sound Players").SetStyleMarginLeft(4))
                .AddSpaceBlock()
                .AddChild(preheatSoundPlayersFluidField)
                .AddSpaceBlock()
                .AddChild(minSoundPlayersToKeepAliveFluidField)
                .AddSpaceBlock(4)
                .AddChild(DesignUtils.NewFieldNameLabel("Music Players").SetStyleMarginLeft(4))
                .AddSpaceBlock()
                .AddChild(preheatMusicPlayersFluidField)
                .AddSpaceBlock()
                .AddChild(minMusicPlayersToKeepAliveFluidField)
                .AddSpaceBlock(4)
                .AddChild
                (
                    DesignUtils.row
                        .SetStyleFlexGrow(0)
                        .AddFlexibleSpace()
                        .AddChild(resetAllButton)
                        .AddSpaceBlock()
                        .AddChild(saveButton)
                )
                ;
        }
    }
}
