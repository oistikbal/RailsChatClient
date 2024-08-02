// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

#if DOOZY_UIMANAGER

// ReSharper disable RedundantUsingDirective

using System.Collections.Generic;
using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.Utils;
using Doozy.Editor.Nody.Nodes.Internal;
using Doozy.Runtime.Soundy.Nody.Nodes;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Doozy.Editor.Soundy.Nody.Nodes
{
    [CustomEditor(typeof(MusicNode))]
    public class MusicNodeEditor : FlowNodeEditor
    {
        public override IEnumerable<Texture2D> nodeIconTextures => EditorSpriteSheets.Soundy.Icons.MusicNode;

        private SerializedProperty propertyAllMusicAction { get; set; }
        private SerializedProperty propertyMusicLibraryId { get; set; }
        private SerializedProperty propertyLibraryAction { get; set; }
        private SerializedProperty propertyMusicId { get; set; }
        private SerializedProperty propertyMusicAction { get; set; }

        private SoundyEditorUtils.Nodes.NodeSettingsSection allMusicSection { get; set; }
        private SoundyEditorUtils.Nodes.NodeSettingsSection musicLibrarySection { get; set; }
        private SoundyEditorUtils.Nodes.NodeSettingsSection musicSection { get; set; }

        protected override void FindProperties()
        {
            base.FindProperties();

            propertyAllMusicAction = serializedObject.FindProperty(nameof(MusicNode.AllMusicAction));
            propertyMusicLibraryId = serializedObject.FindProperty(nameof(MusicNode.MusicLibraryId));
            propertyLibraryAction = serializedObject.FindProperty(nameof(MusicNode.LibraryAction));
            propertyMusicId = serializedObject.FindProperty(nameof(MusicNode.MusicId));
            propertyMusicAction = serializedObject.FindProperty(nameof(MusicNode.MusicAction));
        }

        protected override void InitializeEditor()
        {
            base.InitializeEditor();

            componentHeader
                .SetComponentNameText(ObjectNames.NicifyVariableName(nameof(MusicNode)))
                .SetAccentColor(EditorColors.Soundy.Color)
                .AddManualButton()
                .AddApiButton()
                .AddYouTubeButton();

            //All Music
            allMusicSection =
                new SoundyEditorUtils.Nodes.NodeSettingsSection()
                    .SetOrder(1)
                    .SetSectionTitle("All Music")
                    .SetSectionSubtitle("Perform an action on all the music");

            EnumField allMusicActionEnumField =
                DesignUtils.NewEnumField(propertyAllMusicAction)
                    .SetStyleFlexShrink(0)
                    .SetStyleFlexGrow(1);

            allMusicSection
                .dataContainer
                .AddChild(allMusicActionEnumField);

            //Music Library
            musicLibrarySection =
                new SoundyEditorUtils.Nodes.NodeSettingsSection()
                    .SetOrder(2)
                    .SetSectionTitle("Music Library")
                    .SetSectionSubtitle("Perform an action on all music in a music library");

            EnumField libraryActionEnumField =
                DesignUtils.NewEnumField(propertyLibraryAction)
                    .SetStyleFlexShrink(0)
                    .SetStyleFlexGrow(1);

            PropertyField musicLibraryIdPropertyField =
                DesignUtils.NewPropertyField(propertyMusicLibraryId)
                    .SetStyleFlexShrink(0)
                    .SetStyleFlexGrow(1);

            musicLibrarySection
                .dataContainer
                .AddChild(libraryActionEnumField)
                .AddSpaceBlock()
                .AddChild(musicLibraryIdPropertyField);

            //Music
            musicSection =
                new SoundyEditorUtils.Nodes.NodeSettingsSection()
                    .SetOrder(3)
                    .SetSectionTitle("Music")
                    .SetSectionSubtitle("Perform an action on a specific music object");

            EnumField musicActionEnumField =
                DesignUtils.NewEnumField(propertyMusicAction)
                    .SetStyleFlexShrink(0)
                    .SetStyleFlexGrow(1);

            PropertyField musicIdPropertyField =
                DesignUtils.NewPropertyField(propertyMusicId)
                    .SetStyleFlexShrink(0)
                    .SetStyleFlexGrow(1);

            musicSection
                .dataContainer
                .AddChild(musicActionEnumField)
                .AddSpaceBlock()
                .AddChild(musicIdPropertyField);
        }

        protected override void Compose()
        {
            base.Compose();
            
            root
                .AddSpaceBlock(2)
                .AddChild(allMusicSection)
                .AddSpaceBlock(2)
                .AddChild(musicLibrarySection)
                .AddSpaceBlock(2)
                .AddChild(musicSection)
                .AddEndOfLineSpace()
                ;
        }
    }
}

#endif
