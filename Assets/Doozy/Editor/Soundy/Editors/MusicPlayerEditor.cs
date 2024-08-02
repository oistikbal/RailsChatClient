// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.Components;
using Doozy.Editor.EditorUI.Components.Internal;
using Doozy.Editor.EditorUI.ScriptableObjects.Colors;
using Doozy.Editor.EditorUI.Utils;
using Doozy.Editor.Soundy.Components;
using Doozy.Runtime.Soundy;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Doozy.Editor.Soundy.Editors
{
    [CustomEditor(typeof(MusicPlayer), true)]
    public class MusicPlayerEditor : UnityEditor.Editor
    {
        private static Color accentColor => EditorColors.Soundy.Color;
        private static EditorSelectableColorInfo selectableAccentColor => EditorSelectableColors.Soundy.Color;

        private MusicPlayer castedTarget => (MusicPlayer)target;

        private VisualElement root { get; set; }
        private FluidComponentHeader componentHeader { get; set; }

        private MusicPlayerControls musicPlayerControls { get; set; }

        private FluidToggleSwitch dontDestroyOnSceneChangeToggleSwitch { get; set; }
        private FluidToggleSwitch autoAdvanceToggleSwitch { get; set; }
        private FluidToggleSwitch crossFadeToggleToggle { get; set; }
        private FloatField crossFadeDurationFloatField { get; set; }
        private FluidToggleCheckbox playOnStartToggleCheckbox { get; set; }
        private FluidToggleCheckbox playOnEnableToggleCheckbox { get; set; }
        private FluidToggleCheckbox playOnDisableToggleCheckbox { get; set; }
        private FluidToggleCheckbox stopOnDisableToggleCheckbox { get; set; }
        private FluidToggleCheckbox stopOnDestroyToggleCheckbox { get; set; }
        private FluidToggleCheckbox pauseOnDisableToggleCheckbox { get; set; }
        private FluidToggleCheckbox unPauseOnEnableToggleCheckbox { get; set; }
        private FluidToggleCheckbox resetPlaylistOnEnableToggleCheckbox { get; set; }
        private FluidToggleCheckbox resetPlaylistOnDisableToggleCheckbox { get; set; }

        private FluidField dontDestroyOnSceneChangeFluidField { get; set; }
        private FluidField autoAdvanceAndCrossFadeFluidField { get; set; }
        private FluidField onStartFluidField { get; set; }
        private FluidField onEnableFluidField { get; set; }
        private FluidField onDisableFluidField { get; set; }
        private FluidField onDestroyFluidField { get; set; }
        
        private FluidField followTargetFluidField { get; set; }

        private SerializedProperty propertyDontDestroyOnSceneChange { get; set; }
        private SerializedProperty propertyPlaylist { get; set; }
        private SerializedProperty propertyAutoAdvance { get; set; }
        private SerializedProperty propertyCrossFade { get; set; }
        private SerializedProperty propertyCrossFadeDuration { get; set; }
        private SerializedProperty propertyPlayOnStart { get; set; }
        private SerializedProperty propertyPlayOnEnable { get; set; }
        private SerializedProperty propertyPlayOnDisable { get; set; }
        private SerializedProperty propertyStopOnDisable { get; set; }
        private SerializedProperty propertyStopOnDestroy { get; set; }
        private SerializedProperty propertyPauseOnDisable { get; set; }
        private SerializedProperty propertyUnPauseOnEnable { get; set; }
        private SerializedProperty propertyResetPlaylistOnEnable { get; set; }
        private SerializedProperty propertyResetPlaylistOnDisable { get; set; }
        private SerializedProperty propertyFollowTarget { get; set; }

        private void OnDisable()
        {
            componentHeader?.Recycle();
            dontDestroyOnSceneChangeToggleSwitch?.Recycle();
            autoAdvanceToggleSwitch?.Recycle();
            crossFadeToggleToggle?.Recycle();
            playOnStartToggleCheckbox?.Recycle();
            playOnEnableToggleCheckbox?.Recycle();
            playOnDisableToggleCheckbox?.Recycle();
            stopOnDisableToggleCheckbox?.Recycle();
            stopOnDestroyToggleCheckbox?.Recycle();
            pauseOnDisableToggleCheckbox?.Recycle();
            unPauseOnEnableToggleCheckbox?.Recycle();
            resetPlaylistOnEnableToggleCheckbox?.Recycle();
            resetPlaylistOnDisableToggleCheckbox?.Recycle();

            dontDestroyOnSceneChangeFluidField?.Dispose();
            autoAdvanceAndCrossFadeFluidField?.Dispose();
            onStartFluidField?.Dispose();
            onEnableFluidField?.Dispose();
            onDisableFluidField?.Dispose();
            onDestroyFluidField?.Dispose();
            
            followTargetFluidField?.Dispose();
        }

        public override VisualElement CreateInspectorGUI()
        {
            FindSerializedProperties();
            InitializeEditor();
            Compose();
            return root;
        }

        private void FindSerializedProperties()
        {
            propertyDontDestroyOnSceneChange = serializedObject.FindProperty("DontDestroyOnSceneChange");
            propertyPlaylist = serializedObject.FindProperty("Playlist");
            propertyAutoAdvance = serializedObject.FindProperty("AutoAdvance");
            propertyCrossFade = serializedObject.FindProperty("CrossFade");
            propertyCrossFadeDuration = serializedObject.FindProperty("CrossFadeDuration");
            propertyPlayOnStart = serializedObject.FindProperty("PlayOnStart");
            propertyPlayOnEnable = serializedObject.FindProperty("PlayOnEnable");
            propertyPlayOnDisable = serializedObject.FindProperty("PlayOnDisable");
            propertyStopOnDisable = serializedObject.FindProperty("StopOnDisable");
            propertyStopOnDestroy = serializedObject.FindProperty("StopOnDestroy");
            propertyPauseOnDisable = serializedObject.FindProperty("PauseOnDisable");
            propertyUnPauseOnEnable = serializedObject.FindProperty("UnPauseOnEnable");
            propertyResetPlaylistOnEnable = serializedObject.FindProperty("ResetPlaylistOnEnable");
            propertyResetPlaylistOnDisable = serializedObject.FindProperty("ResetPlaylistOnDisable");
            propertyFollowTarget = serializedObject.FindProperty("FollowTarget");
        }

        private void InitializeEditor()
        {
            root = DesignUtils.editorRoot;
            componentHeader =
                DesignUtils.editorComponentHeader
                    .SetIcon(EditorSpriteSheets.Soundy.Icons.AudioPlayer)
                    .SetSecondaryIcon(EditorSpriteSheets.Soundy.Icons.Music)
                    .SetComponentNameText("Music Player")
                    .SetAccentColor(accentColor)
                    .AddManualButton()
                    .AddApiButton()
                    .AddYouTubeButton();

            InitializeDontDestroyOnSceneChange();
            InitializeAutoAdvanceAndCrossFade();
            InitializeOnStartSettings();
            InitializeOnEnableSettings();
            InitializeOnDisableSettings();
            InitializeOnDestroySettings();
            InitializeFollowTarget();
            InitializePlayerControls();
        }

        private void InitializeOnStartSettings()
        {

            playOnStartToggleCheckbox =
                FluidToggleCheckbox.Get()
                    .SetLabelText("Play")
                    .SetTooltip
                    (
                        "If enabled, the Music Player will automatically start playing the (first or) next track in the playlist on Start."
                    )
                    .SetToggleAccentColor(selectableAccentColor)
                    .BindToProperty(propertyPlayOnStart);

            onStartFluidField = FluidField.Get().SetLabelText("On Start").SetElementSize(ElementSize.Tiny)
                .AddFieldContent(playOnStartToggleCheckbox);
        }

        private void InitializeOnEnableSettings()
        {
            playOnEnableToggleCheckbox =
                FluidToggleCheckbox.Get()
                    .SetLabelText("Play")
                    .SetTooltip
                    (
                        "If enabled, the Music Player will automatically start playing the (first or) next track in the playlist on Enable."
                    )
                    .SetToggleAccentColor(selectableAccentColor)
                    .BindToProperty(propertyPlayOnEnable);

            unPauseOnEnableToggleCheckbox =
                FluidToggleCheckbox.Get()
                    .SetLabelText("Resume")
                    .SetTooltip
                    (
                        "If enabled, the Music Player will automatically resume playing the current track in the playlist, if it was previously paused,  on Enable."
                    )
                    .SetToggleAccentColor(selectableAccentColor)
                    .BindToProperty(propertyUnPauseOnEnable);

            resetPlaylistOnEnableToggleCheckbox =
                FluidToggleCheckbox.Get()
                    .SetLabelText("Reset Playlist")
                    .SetTooltip
                    (
                        "If enabled, the Music Player will automatically reset the playlist on Enable."
                    )
                    .SetToggleAccentColor(selectableAccentColor)
                    .BindToProperty(propertyResetPlaylistOnEnable);

            onEnableFluidField = FluidField.Get().SetLabelText("On Enable").SetElementSize(ElementSize.Tiny)
                .AddFieldContent
                (
                    DesignUtils.row
                        .SetStyleAlignItems(Align.Center)
                        .AddChild(playOnEnableToggleCheckbox)
                        .AddSpaceBlock()
                        .AddChild(unPauseOnEnableToggleCheckbox)
                        .AddSpaceBlock()
                        .AddChild(resetPlaylistOnEnableToggleCheckbox)
                );
        }

        private void InitializeOnDisableSettings()
        {
            playOnDisableToggleCheckbox =
                FluidToggleCheckbox.Get()
                    .SetLabelText("Play")
                    .SetTooltip
                    (
                        "If enabled, the Music Player will automatically start playing the (first or) next track in the playlist on Disable."
                    )
                    .SetToggleAccentColor(selectableAccentColor)
                    .BindToProperty(propertyPlayOnDisable);

            pauseOnDisableToggleCheckbox =
                FluidToggleCheckbox.Get()
                    .SetLabelText("Pause")
                    .SetTooltip
                    (
                        "If enabled, the Music Player will automatically pause playing the current track in the playlist on Disable."
                    )
                    .SetToggleAccentColor(selectableAccentColor)
                    .BindToProperty(propertyPauseOnDisable);

            stopOnDisableToggleCheckbox =
                FluidToggleCheckbox.Get()
                    .SetLabelText("Stop")
                    .SetTooltip
                    (
                        "If enabled, the Music Player will automatically stop playing on Disable."
                    )
                    .SetToggleAccentColor(selectableAccentColor)
                    .BindToProperty(propertyStopOnDisable);

            resetPlaylistOnDisableToggleCheckbox =
                FluidToggleCheckbox.Get()
                    .SetLabelText("Reset Playlist")
                    .SetTooltip
                    (
                        "If enabled, the Music Player will automatically reset the playlist on Disable."
                    )
                    .SetToggleAccentColor(selectableAccentColor)
                    .BindToProperty(propertyResetPlaylistOnDisable);

            onDisableFluidField = FluidField.Get().SetLabelText("On Disable").SetElementSize(ElementSize.Tiny)
                .AddFieldContent
                (
                    DesignUtils.row
                        .SetStyleAlignItems(Align.Center)
                        .AddChild(playOnDisableToggleCheckbox)
                        .AddSpaceBlock()
                        .AddChild(pauseOnDisableToggleCheckbox)
                        .AddSpaceBlock()
                        .AddChild(stopOnDisableToggleCheckbox)
                        .AddSpaceBlock()
                        .AddChild(resetPlaylistOnDisableToggleCheckbox)
                );
        }

        private void InitializeOnDestroySettings()
        {

            stopOnDestroyToggleCheckbox =
                FluidToggleCheckbox.Get()
                    .SetLabelText("Stop")
                    .SetTooltip
                    (
                        "If enabled, the Music Player will automatically stop playing on Destroy."
                    )
                    .SetToggleAccentColor(selectableAccentColor)
                    .BindToProperty(propertyStopOnDestroy);

            onDestroyFluidField = FluidField.Get().SetLabelText("On Destroy").SetElementSize(ElementSize.Tiny)
                .AddFieldContent(stopOnDestroyToggleCheckbox);
        }

        private void InitializeFollowTarget()
        {
            followTargetFluidField =
                FluidField.Get()
                    .SetLabelText("Follow Target")
                    .AddFieldContent
                    (
                        DesignUtils.NewObjectField(propertyFollowTarget, typeof(Transform))
                            .SetTooltip("The Transform to follow when playing the music")
                            .SetStyleFlexGrow(1)
                    );
        }
        
        private void InitializeAutoAdvanceAndCrossFade()
        {
            autoAdvanceToggleSwitch =
                FluidToggleSwitch.Get()
                    .SetLabelText("Auto Advance")
                    .SetTooltip
                    (
                        "If enabled, the Music Player will automatically advance to the next track in the playlist when the current track finishes playing."
                    )
                    .SetToggleAccentColor(selectableAccentColor)
                    .BindToProperty(propertyAutoAdvance);

            crossFadeToggleToggle =
                FluidToggleSwitch.Get()
                    .SetLabelText("Cross Fade")
                    .SetTooltip
                    (
                        "If enabled, the Music Player will cross fade between tracks when advancing to the next track in the playlist.\n\n" +
                        "Cross fade works only when auto advance is enabled and fade in and fade out durations are greater than 0."
                    )
                    .SetToggleAccentColor(selectableAccentColor)
                    .BindToProperty(propertyCrossFade);

            crossFadeDurationFloatField =
                DesignUtils.NewFloatField(propertyCrossFadeDuration)
                    .SetTooltip("Fade in and out duration (in seconds) of the audio player's volume when starting or stopping a sound.")
                    .SetStyleWidth(40);

            autoAdvanceAndCrossFadeFluidField =
                FluidField.Get()
                    .AddFieldContent
                    (
                        DesignUtils.row
                            .SetStyleAlignItems(Align.Center)
                            .AddChild(autoAdvanceToggleSwitch)
                            .AddSpaceBlock()
                            .AddChild(DesignUtils.dividerVertical)
                            .AddSpaceBlock()
                            .AddChild(crossFadeToggleToggle)
                            .AddSpaceBlock()
                            .AddChild(DesignUtils.NewFieldNameLabel("Duration").SetStyleFontSize(12))
                            .AddSpaceBlock()
                            .AddChild(crossFadeDurationFloatField)
                            .AddSpaceBlock()
                            .AddChild(DesignUtils.fieldLabel.SetText("seconds"))
                    );
        }

        private void InitializeDontDestroyOnSceneChange()
        {
            dontDestroyOnSceneChangeToggleSwitch =
                FluidToggleSwitch.Get()
                    .SetLabelText("Dont Destroy On Scene Change")
                    .SetTooltip
                    (
                        "If enabled, the Music Player will not be destroyed when loading a new scene.\n\n" +
                        "This is useful for music players that need to persist between scene changes."
                    )
                    .SetToggleAccentColor(selectableAccentColor)
                    .BindToProperty(propertyDontDestroyOnSceneChange);

            dontDestroyOnSceneChangeFluidField =
                FluidField.Get()
                    .AddFieldContent(dontDestroyOnSceneChangeToggleSwitch);
        }

        private void InitializePlayerControls()
        {
            musicPlayerControls =
                new MusicPlayerControls()
                    .Create
                    (
                        previousCallback: () => castedTarget.PlayPrevious(),
                        playCallback: () =>
                        {
                            if (castedTarget.isPlaying) return;
                            castedTarget.Play();
                        },
                        stopCallback: () => castedTarget.Stop(),
                        nextCallback: () => castedTarget.PlayNext()
                    );
        }

        private void Compose()
        {
            const float width = 60f;
            onStartFluidField.fieldLabel.SetStyleWidth(width).SetStylePaddingLeft(DesignUtils.k_Spacing);
            onEnableFluidField.fieldLabel.SetStyleWidth(width).SetStylePaddingLeft(DesignUtils.k_Spacing);
            onDisableFluidField.fieldLabel.SetStyleWidth(width).SetStylePaddingLeft(DesignUtils.k_Spacing);
            onDestroyFluidField.fieldLabel.SetStyleWidth(width).SetStylePaddingLeft(DesignUtils.k_Spacing);

            root
                .AddChild(musicPlayerControls)
                .AddChild(componentHeader)
                .AddSpaceBlock()
                .AddChild(dontDestroyOnSceneChangeFluidField)
                .AddSpaceBlock()
                .AddChild(autoAdvanceAndCrossFadeFluidField)
                .AddSpaceBlock(2)
                .AddChild(onStartFluidField)
                .AddSpaceBlock()
                .AddChild(onEnableFluidField)
                .AddSpaceBlock()
                .AddChild(onDisableFluidField)
                .AddSpaceBlock()
                .AddChild(onDestroyFluidField)
                .AddSpaceBlock(2)
                .AddChild(followTargetFluidField)
                .AddSpaceBlock(2)
                .AddChild(DesignUtils.NewPropertyField(propertyPlaylist))
                .AddEndOfLineSpace()
                ;
        }
    }
}
