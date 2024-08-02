// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.Components;
using Doozy.Editor.EditorUI.ScriptableObjects.Colors;
using Doozy.Editor.EditorUI.Utils;
using Doozy.Runtime.Soundy;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Doozy.Editor.Soundy.Editors
{
    [CustomEditor(typeof(SoundyService), true)]
    public class SoundyServiceEditor : UnityEditor.Editor
    {
        private static Color accentColor => EditorColors.Soundy.Color;
        private static EditorSelectableColorInfo selectableAccentColor => EditorSelectableColors.Soundy.Color;

        private SoundyService castedTarget => (SoundyService)target;

        private VisualElement root { get; set; }
        private FluidComponentHeader componentHeader { get; set; }

        private PlayersStats soundPlayersStats { get; set; }
        private PlayersStats musicPlayersStats { get; set; }

        public override VisualElement CreateInspectorGUI()
        {
            Initialize();
            Compose();
            return root;
        }

        private void Initialize()
        {
            root = DesignUtils.editorRoot;

            componentHeader =
                DesignUtils.editorComponentHeader
                    .SetIcon(EditorSpriteSheets.Soundy.Icons.AudioEngine)
                    .SetComponentNameText("Soundy Service")
                    .SetAccentColor(accentColor)
                    .SetElementSize(ElementSize.Large)
                    .SetStyleMarginBottom(DesignUtils.k_Spacing)
                    .AddManualButton()
                    .AddApiButton()
                    .AddYouTubeButton();

            soundPlayersStats =
                new PlayersStats()
                    .SetSecondaryIcon(EditorSpriteSheets.Soundy.Icons.Sound)
                    .SetTitle("Sound Players")
                    .SetInPoolCountGetter(() => castedTarget.soundPlayers.inPoolPlayersCount)
                    .SetIdleCountGetter(() => castedTarget.soundPlayers.isIdlePlayersCount)
                    .SetPlayingCountGetter(() => castedTarget.soundPlayers.isPlayingPlayersCount)
                    .SetPausedCountGetter(() => castedTarget.soundPlayers.isPausedPlayersCount)
                    .SetStoppedCountGetter(() => castedTarget.soundPlayers.isStoppedPlayersCount)
                    .Update();

            musicPlayersStats =
                new PlayersStats()
                    .SetSecondaryIcon(EditorSpriteSheets.Soundy.Icons.Music)
                    .SetTitle("Music Players")
                    .SetInPoolCountGetter(() => castedTarget.musicPlayers.inPoolPlayersCount)
                    .SetIdleCountGetter(() => castedTarget.musicPlayers.isIdlePlayersCount)
                    .SetPlayingCountGetter(() => castedTarget.musicPlayers.isPlayingPlayersCount)
                    .SetPausedCountGetter(() => castedTarget.musicPlayers.isPausedPlayersCount)
                    .SetStoppedCountGetter(() => castedTarget.musicPlayers.isStoppedPlayersCount)
                    .Update();

            root.schedule.Execute(() =>
            {
                soundPlayersStats.Update();
                musicPlayersStats.Update();
            }).Every(50);
        }

        private void Compose()
        {
            root
                .AddChild(componentHeader)
                .AddSpaceBlock()
                .AddChild(soundPlayersStats)
                .AddSpaceBlock(2)
                .AddChild(musicPlayersStats)
                .AddEndOfLineSpace()
                ;
        }


        private class PlayersStats : VisualElement
        {
            public FluidComponentHeader header { get; set; }
            public Label playersInPoolCountLabel { get; set; }
            public Label playersIdleCountLabel { get; set; }
            public Label playersPlayingCountLabel { get; set; }
            public Label playersPausedCountLabel { get; set; }
            public Label playersStoppedCountLabel { get; set; }

            public Func<int> inPoolCountGetter { get; set; }
            public Func<int> idleCountGetter { get; set; }
            public Func<int> playingCountGetter { get; set; }
            public Func<int> pausedCountGetter { get; set; }
            public Func<int> stoppedCountGetter { get; set; }

            private VisualElement dataContainer { get; set; }

            public PlayersStats()
            {
                header =
                    DesignUtils.editorComponentHeader
                        .SetElementSize(ElementSize.Small)
                        .SetAccentColor(accentColor);

                header.barContainer
                    .SetStyleBorderBottomLeftRadius(0)
                    .SetStyleBorderBottomRightRadius(0)
                    .SetStyleBackgroundColor(EditorColors.Default.BoxBackground);

                playersStoppedCountLabel = GetCountLabel();
                playersPausedCountLabel = GetCountLabel();
                playersPlayingCountLabel = GetCountLabel();
                playersIdleCountLabel = GetCountLabel();
                playersInPoolCountLabel = GetCountLabel();

                dataContainer = SoundyEditorUtils.Elements.GetDataContainer();

                dataContainer
                    .AddSpaceBlock()
                    .AddChild(GetRow("In Pool", playersInPoolCountLabel))
                    .AddSpaceBlock()
                    .AddChild(GetRow("Idle", playersIdleCountLabel))
                    .AddSpaceBlock()
                    .AddChild(GetRow("Playing", playersPlayingCountLabel))
                    .AddSpaceBlock()
                    .AddChild(GetRow("Paused", playersPausedCountLabel))
                    .AddSpaceBlock()
                    .AddChild(GetRow("Stopped", playersStoppedCountLabel))
                    ;

                this
                    .AddChild(header)
                    .AddChild(dataContainer);
            }

            private VisualElement GetRow(string rowName, Label counterLabel)
            {
                var row =
                    SoundyEditorUtils.Elements.GetPropertyContainer()
                        .SetName($"{rowName}")
                        .SetStyleFlexGrow(1)
                        .SetStyleFlexDirection(FlexDirection.Row)
                        .SetStyleAlignItems(Align.Center);

                row
                    .AddChild(GetTitleLabel().SetText(rowName))
                    .AddSpaceBlock()
                    .AddFlexibleSpace()
                    .AddSpaceBlock()
                    .AddChild(counterLabel);

                return row;
            }



            public PlayersStats SetTitle(string title)
            {
                header.SetComponentNameText(title);
                return this;
            }

            public PlayersStats SetIcon(List<Texture2D> iconTextures)
            {
                header.SetIcon(iconTextures);
                return this;
            }
            
            public PlayersStats SetSecondaryIcon(List<Texture2D> iconTextures)
            {
                header.SetSecondaryIcon(iconTextures);
                return this;
            }

            public PlayersStats SetInPoolCountGetter(Func<int> getter)
            {
                inPoolCountGetter = getter;
                return this;
            }

            public PlayersStats SetIdleCountGetter(Func<int> getter)
            {
                idleCountGetter = getter;
                return this;
            }

            public PlayersStats SetPlayingCountGetter(Func<int> getter)
            {
                playingCountGetter = getter;
                return this;
            }

            public PlayersStats SetPausedCountGetter(Func<int> getter)
            {
                pausedCountGetter = getter;
                return this;
            }

            public PlayersStats SetStoppedCountGetter(Func<int> getter)
            {
                stoppedCountGetter = getter;
                return this;
            }

            public Color counterZeroColor => DesignUtils.fieldNameTextColor;
            public Color counterNonZeroColor => accentColor;
            
            public PlayersStats Update()
            {
                int inPoolCount = inPoolCountGetter?.Invoke() ?? 0;
                playersInPoolCountLabel
                    .SetText(inPoolCount.ToString())
                    .SetStyleColor(inPoolCount > 0 ? counterNonZeroColor : counterZeroColor);
                
                int idleCount = idleCountGetter?.Invoke() ?? 0;
                playersIdleCountLabel
                    .SetText(idleCount.ToString())
                    .SetStyleColor(idleCount > 0 ? counterNonZeroColor : counterZeroColor);
                
                int playingCount = playingCountGetter?.Invoke() ?? 0;
                playersPlayingCountLabel
                    .SetText(playingCount.ToString())
                    .SetStyleColor(playingCount > 0 ? counterNonZeroColor : counterZeroColor);
                
                int pausedCount = pausedCountGetter?.Invoke() ?? 0;
                playersPausedCountLabel
                    .SetText(pausedCount.ToString())
                    .SetStyleColor(pausedCount > 0 ? counterNonZeroColor : counterZeroColor);
                
                int stoppedCount = stoppedCountGetter?.Invoke() ?? 0;
                playersStoppedCountLabel
                    .SetText(stoppedCount.ToString())
                    .SetStyleColor(stoppedCount > 0 ? counterNonZeroColor : counterZeroColor);

                return this;
            }

            private static Label GetTitleLabel() =>
                DesignUtils.NewLabel();

            private static Label GetCountLabel() =>
                DesignUtils
                    .fieldLabel
                    .SetStyleFontSize(10);
        }
    }
}
