// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

#if DOOZY_UIMANAGER

using System;
using System.Collections.Generic;
using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.ScriptableObjects.Colors;
using Doozy.Editor.EditorUI.Utils;
using Doozy.Editor.Nody;
using Doozy.Runtime.Nody;
using Doozy.Runtime.Soundy;
using Doozy.Runtime.Soundy.Nody.Nodes;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Doozy.Editor.Soundy.Nody.Nodes
{
    public class MusicNodeView : FlowNodeView
    {
        public override Type nodeType => typeof(MusicNode);
        public override Texture2D nodeIconTexture => EditorTextures.Soundy.Icons.MusicNode;                         // custom static icon
        public override IEnumerable<Texture2D> nodeIconTextures => EditorSpriteSheets.Soundy.Icons.MusicNode;       // custom animated icon
        public override Color nodeAccentColor => EditorColors.Soundy.Color;                                         // custom accent color
        public override EditorSelectableColorInfo nodeSelectableAccentColor => EditorSelectableColors.Soundy.Color; // custom selectable accent color

        private static Color enabledActionColor => SoundyEditorUtils.Nodes.enabledActionColor;
        private static Color disabledActionColor => SoundyEditorUtils.Nodes.disabledActionColor;
        private static Color validIdColor => SoundyEditorUtils.Nodes.validIdColor;
        private static Color invalidIdColor => SoundyEditorUtils.Nodes.invalidIdColor;

        private SerializedProperty propertyAllMusicAction { get; set; }
        private SerializedProperty propertyMusicLibraryId { get; set; }
        private SerializedProperty propertyLibraryAction { get; set; }
        private SerializedProperty propertyMusicId { get; set; }
        private SerializedProperty propertyMusicAction { get; set; }

        private Label allMusicActionLabel { get; }
        private Label libraryActionLabel { get; }
        private Label musicActionLabel { get; }
        private Label libraryIdLabel { get; }
        private Label musicIdLabel { get; }

        private string allMusicActionText { get; set; }
        private string libraryActionText { get; set; }
        private string musicActionText { get; set; }

        public MusicNodeView(FlowGraphView graphView, FlowNode node) : base(graphView, node)
        {
            propertyAllMusicAction = serializedObject.FindProperty(nameof(MusicNode.AllMusicAction));
            propertyMusicLibraryId = serializedObject.FindProperty(nameof(MusicNode.MusicLibraryId));
            propertyLibraryAction = serializedObject.FindProperty(nameof(MusicNode.LibraryAction));
            propertyMusicId = serializedObject.FindProperty(nameof(MusicNode.MusicId));
            propertyMusicAction = serializedObject.FindProperty(nameof(MusicNode.MusicAction));

            EnumField allMusicActionEnum = DesignUtils.NewEnumField(propertyAllMusicAction, true);
            EnumField libraryActionEnum = DesignUtils.NewEnumField(propertyLibraryAction, true);
            EnumField musicActionEnum = DesignUtils.NewEnumField(propertyMusicAction, true);
            TextField musicLibraryIdLibraryNameTextField = DesignUtils.NewTextField(propertyMusicLibraryId.FindPropertyRelative("LibraryName"), false, true);
            TextField musicIdLibraryNameTextField = DesignUtils.NewTextField(propertyMusicId.FindPropertyRelative("LibraryName"), false, true);
            TextField musicIdMusicNameTextField = DesignUtils.NewTextField(propertyMusicId.FindPropertyRelative("AudioName"), false, true);

            allMusicActionLabel = SoundyEditorUtils.Nodes.GetSectionTitleLabel().SetStyleFontSize(10);
            libraryActionLabel = SoundyEditorUtils.Nodes.GetSectionTitleLabel().SetStyleFontSize(10);
            musicActionLabel = SoundyEditorUtils.Nodes.GetSectionTitleLabel().SetStyleFontSize(10);

            libraryIdLabel = SoundyEditorUtils.Nodes.GetSectionTitleLabel().SetStyleFontSize(9)
                .SetStyleColor(validIdColor)
                .SetStyleMarginLeft(DesignUtils.k_Spacing)
                .SetStyleDisplay(DisplayStyle.None);

            musicIdLabel = SoundyEditorUtils.Nodes.GetSectionTitleLabel().SetStyleFontSize(9)
                .SetStyleColor(validIdColor)
                .SetStyleMarginLeft(DesignUtils.k_Spacing)
                .SetStyleDisplay(DisplayStyle.None);

            var allMusicContainer = SoundyEditorUtils.Nodes.GetNodeViewContainer(1).AddChild(allMusicActionLabel);
            var libraryContainer = SoundyEditorUtils.Nodes.GetNodeViewContainer(2).AddChild(libraryActionLabel).AddChild(libraryIdLabel);
            var musicContainer = SoundyEditorUtils.Nodes.GetNodeViewContainer(3).AddChild(musicActionLabel).AddChild(musicIdLabel);

            allMusicActionEnum.RegisterValueChangedCallback(_ => RefreshData());
            libraryActionEnum.RegisterValueChangedCallback(_ => RefreshData());
            musicActionEnum.RegisterValueChangedCallback(_ => RefreshData());
            musicLibraryIdLibraryNameTextField.RegisterValueChangedCallback(_ => RefreshData());
            musicIdLibraryNameTextField.RegisterValueChangedCallback(_ => RefreshData());
            musicIdMusicNameTextField.RegisterValueChangedCallback(_ => RefreshData());

            portDivider
                .SetStyleBackgroundColor(EditorColors.Nody.MiniMapBackground)
                .SetStyleMarginLeft(DesignUtils.k_Spacing)
                .SetStyleMarginRight(DesignUtils.k_Spacing)
                .SetStylePadding(DesignUtils.k_Spacing2X)
                .SetStyleBorderRadius(DesignUtils.k_Spacing)
                // .SetStyleJustifyContent(Justify.Center)
                // .SetStyleAlignItems(Align.Center)
                ;

            portDivider
                .AddChild(allMusicActionEnum)
                .AddChild(libraryActionEnum)
                .AddChild(musicActionEnum)
                .AddChild(musicLibraryIdLibraryNameTextField)
                .AddChild(musicIdLibraryNameTextField)
                .AddChild(musicIdMusicNameTextField)
                .AddChild(allMusicContainer)
                .AddChild(DesignUtils.dividerHorizontal)
                .AddChild(libraryContainer)
                .AddChild(DesignUtils.dividerHorizontal)
                .AddChild(musicContainer);

            portDivider.Bind(serializedObject);
        }

        public override void RefreshData()
        {
            base.RefreshData();

            var node = (MusicNode)flowNode;
            if (node == null) return;

            allMusicActionText = ObjectNames.NicifyVariableName(node.AllMusicAction.ToString());
            libraryActionText = ObjectNames.NicifyVariableName(node.LibraryAction.ToString());
            musicActionText = ObjectNames.NicifyVariableName(node.MusicAction.ToString());

            allMusicActionLabel.SetText(allMusicActionText);
            libraryActionLabel.SetText(libraryActionText);
            musicActionLabel.SetText(musicActionText);

            //update ids labels
            libraryIdLabel
                .SetStyleDisplay(node.LibraryAction == LibraryActionType.DoNothing ? DisplayStyle.None : DisplayStyle.Flex)
                .SetText(node.MusicLibraryId.libraryName);

            musicIdLabel
                .SetStyleDisplay(node.MusicAction == AudioActionType.DoNothing ? DisplayStyle.None : DisplayStyle.Flex)
                .SetText(node.MusicId.ToString());

            //set colors
            allMusicActionLabel.SetStyleColor(node.AllMusicAction == MusicActionType.DoNothing ? disabledActionColor : enabledActionColor);
            libraryActionLabel.SetStyleColor(node.LibraryAction == LibraryActionType.DoNothing ? disabledActionColor : enabledActionColor);
            musicActionLabel.SetStyleColor(node.MusicAction == AudioActionType.DoNothing ? disabledActionColor : enabledActionColor);
            libraryIdLabel.SetStyleColor(node.MusicLibraryId.isValid ? validIdColor : invalidIdColor);
            musicIdLabel.SetStyleColor(node.MusicId.isValid ? validIdColor : invalidIdColor);
        }
    }
}

#endif
