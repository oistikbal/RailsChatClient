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
    [CustomEditor(typeof(SoundNode))]
    public class SoundNodeEditor : FlowNodeEditor
    {
        public override IEnumerable<Texture2D> nodeIconTextures => EditorSpriteSheets.Soundy.Icons.SoundNode;

        private SerializedProperty propertyAllSoundsAction { get; set; }
        private SerializedProperty propertySoundLibraryId { get; set; }
        private SerializedProperty propertyLibraryAction { get; set; }
        private SerializedProperty propertySoundId { get; set; }
        private SerializedProperty propertySoundAction { get; set; }

        private SoundyEditorUtils.Nodes.NodeSettingsSection allSoundsSection { get; set; }
        private SoundyEditorUtils.Nodes.NodeSettingsSection soundLibrarySection { get; set; }
        private SoundyEditorUtils.Nodes.NodeSettingsSection soundSection { get; set; }

        protected override void FindProperties()
        {
            base.FindProperties();

            propertyAllSoundsAction = serializedObject.FindProperty(nameof(SoundNode.AllSoundsAction));
            propertySoundLibraryId = serializedObject.FindProperty(nameof(SoundNode.SoundLibraryId));
            propertyLibraryAction = serializedObject.FindProperty(nameof(SoundNode.LibraryAction));
            propertySoundId = serializedObject.FindProperty(nameof(SoundNode.SoundId));
            propertySoundAction = serializedObject.FindProperty(nameof(SoundNode.SoundAction));
        }

        protected override void InitializeEditor()
        {
            base.InitializeEditor();

            componentHeader
                .SetComponentNameText(ObjectNames.NicifyVariableName(nameof(SoundNode)))
                .SetAccentColor(EditorColors.Soundy.Color)
                .AddManualButton()
                .AddApiButton()
                .AddYouTubeButton();

            //All Sounds
            allSoundsSection =
                new SoundyEditorUtils.Nodes.NodeSettingsSection()
                    .SetOrder(1)
                    .SetSectionTitle("All Sounds")
                    .SetSectionSubtitle("Perform an action on all sounds");
            
            EnumField allSoundsActionEnumField = 
                DesignUtils.NewEnumField(propertyAllSoundsAction)
                    .SetStyleFlexShrink(0)
                    .SetStyleFlexGrow(1);
            
            allSoundsSection
                .dataContainer
                .AddChild(allSoundsActionEnumField);

            //Sound Library
            soundLibrarySection =
                new SoundyEditorUtils.Nodes.NodeSettingsSection()
                    .SetOrder(2)
                    .SetSectionTitle("Sound Library")
                    .SetSectionSubtitle("Perform an action on all sounds in a sound library");
            
            EnumField soundLibraryIdEnumField = 
                DesignUtils.NewEnumField(propertyLibraryAction)
                    .SetStyleFlexShrink(0)
                    .SetStyleFlexGrow(1);
            
            PropertyField soundLibraryIdPropertyField = 
                DesignUtils.NewPropertyField(propertySoundLibraryId)
                    .SetStyleFlexShrink(0)
                    .SetStyleFlexGrow(1);
            
            soundLibrarySection
                .dataContainer
                .AddChild(soundLibraryIdEnumField)
                .AddSpaceBlock()
                .AddChild(soundLibraryIdPropertyField);

            //Sound
            soundSection = 
                new SoundyEditorUtils.Nodes.NodeSettingsSection()
                    .SetOrder(3)
                    .SetSectionTitle("Sound")
                    .SetSectionSubtitle("Perform an action on a specific sound object");
            
            EnumField soundActionEnumField = 
                DesignUtils.NewEnumField(propertySoundAction)
                    .SetStyleFlexShrink(0)
                    .SetStyleFlexGrow(1);
            
            PropertyField soundIdPropertyField = 
                DesignUtils.NewPropertyField(propertySoundId)
                    .SetStyleFlexShrink(0)
                    .SetStyleFlexGrow(1);
            
            soundSection
                .dataContainer
                .AddChild(soundActionEnumField)
                .AddSpaceBlock()
                .AddChild(soundIdPropertyField);
        }

        protected override void Compose()
        {
            base.Compose();

            root
                .AddSpaceBlock(2)
                .AddChild(allSoundsSection)
                .AddSpaceBlock(2)
                .AddChild(soundLibrarySection)
                .AddSpaceBlock(2)
                .AddChild(soundSection)
                .AddEndOfLineSpace()
                ;
        }
    }
}

#endif
