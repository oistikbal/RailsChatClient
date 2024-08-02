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
    public class SoundNodeView : FlowNodeView
    {
        public override Type nodeType => typeof(SoundNode);
        public override Texture2D nodeIconTexture => EditorTextures.Soundy.Icons.SoundNode;                         // custom static icon
        public override IEnumerable<Texture2D> nodeIconTextures => EditorSpriteSheets.Soundy.Icons.SoundNode;       // custom animated icon
        public override Color nodeAccentColor => EditorColors.Soundy.Color;                                         // custom accent color
        public override EditorSelectableColorInfo nodeSelectableAccentColor => EditorSelectableColors.Soundy.Color; // custom selectable accent color

        private static Color enabledActionColor => SoundyEditorUtils.Nodes.enabledActionColor;
        private static Color disabledActionColor => SoundyEditorUtils.Nodes.disabledActionColor;
        private static Color validIdColor => SoundyEditorUtils.Nodes.validIdColor;
        private static Color invalidIdColor => SoundyEditorUtils.Nodes.invalidIdColor;

        private SerializedProperty propertyAllSoundsAction { get; set; }
        private SerializedProperty propertySoundLibraryId { get; set; }
        private SerializedProperty propertyLibraryAction { get; set; }
        private SerializedProperty propertySoundId { get; set; }
        private SerializedProperty propertySoundAction { get; set; }

        private Label allSoundsActionLabel { get; }
        private Label libraryActionLabel { get; }
        private Label soundActionLabel { get; }
        private Label libraryIdLabel { get; }
        private Label soundIdLabel { get; }

        private string allSoundsActionText { get; set; }
        private string libraryActionText { get; set; }
        private string soundActionText { get; set; }

        public SoundNodeView(FlowGraphView graphView, FlowNode node) : base(graphView, node)
        {
            propertyAllSoundsAction = serializedObject.FindProperty(nameof(SoundNode.AllSoundsAction));
            propertySoundLibraryId = serializedObject.FindProperty(nameof(SoundNode.SoundLibraryId));
            propertyLibraryAction = serializedObject.FindProperty(nameof(SoundNode.LibraryAction));
            propertySoundId = serializedObject.FindProperty(nameof(SoundNode.SoundId));
            propertySoundAction = serializedObject.FindProperty(nameof(SoundNode.SoundAction));

            EnumField allSoundsActionEnum = DesignUtils.NewEnumField(propertyAllSoundsAction, true);
            EnumField libraryActionEnum = DesignUtils.NewEnumField(propertyLibraryAction, true);
            EnumField soundActionEnum = DesignUtils.NewEnumField(propertySoundAction, true);
            TextField soundLibraryIdLibraryNameTextField = DesignUtils.NewTextField(propertySoundLibraryId.FindPropertyRelative("LibraryName"), false, true);
            TextField soundIdLibraryNameTextField = DesignUtils.NewTextField(propertySoundId.FindPropertyRelative("LibraryName"), false, true);
            TextField soundIdSoundNameTextField = DesignUtils.NewTextField(propertySoundId.FindPropertyRelative("AudioName"), false, true);

            allSoundsActionLabel = SoundyEditorUtils.Nodes.GetSectionTitleLabel().SetStyleFontSize(10);
            libraryActionLabel = SoundyEditorUtils.Nodes.GetSectionTitleLabel().SetStyleFontSize(10);
            soundActionLabel = SoundyEditorUtils.Nodes.GetSectionTitleLabel().SetStyleFontSize(10);

            libraryIdLabel = SoundyEditorUtils.Nodes.GetSectionTitleLabel().SetStyleFontSize(9)
                .SetStyleColor(validIdColor)
                .SetStyleMarginLeft(DesignUtils.k_Spacing)
                .SetStyleDisplay(DisplayStyle.None);

            soundIdLabel = SoundyEditorUtils.Nodes.GetSectionTitleLabel().SetStyleFontSize(9)
                .SetStyleColor(validIdColor)
                .SetStyleMarginLeft(DesignUtils.k_Spacing)
                .SetStyleDisplay(DisplayStyle.None);

            var allSoundsContainer = SoundyEditorUtils.Nodes.GetNodeViewContainer(1).AddChild(allSoundsActionLabel);
            var libraryContainer = SoundyEditorUtils.Nodes.GetNodeViewContainer(2).AddChild(libraryActionLabel).AddChild(libraryIdLabel);
            var soundContainer = SoundyEditorUtils.Nodes.GetNodeViewContainer(3).AddChild(soundActionLabel).AddChild(soundIdLabel);

            allSoundsActionEnum.RegisterValueChangedCallback(_ => RefreshData());
            libraryActionEnum.RegisterValueChangedCallback(_ => RefreshData());
            soundActionEnum.RegisterValueChangedCallback(_ => RefreshData());
            soundLibraryIdLibraryNameTextField.RegisterValueChangedCallback(_ => RefreshData());
            soundIdLibraryNameTextField.RegisterValueChangedCallback(_ => RefreshData());
            soundIdSoundNameTextField.RegisterValueChangedCallback(_ => RefreshData());

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
                .AddChild(allSoundsActionEnum)
                .AddChild(libraryActionEnum)
                .AddChild(soundActionEnum)
                .AddChild(soundLibraryIdLibraryNameTextField)
                .AddChild(soundIdLibraryNameTextField)
                .AddChild(soundIdSoundNameTextField)
                .AddChild(allSoundsContainer)
                .AddChild(DesignUtils.dividerHorizontal)
                .AddChild(libraryContainer)
                .AddChild(DesignUtils.dividerHorizontal)
                .AddChild(soundContainer);

            portDivider.Bind(serializedObject);
        }



        public override void RefreshData()
        {
            base.RefreshData();

            var node = (SoundNode)flowNode;
            if (node == null) return;

            allSoundsActionText = ObjectNames.NicifyVariableName(node.AllSoundsAction.ToString());
            libraryActionText = ObjectNames.NicifyVariableName(node.LibraryAction.ToString());
            soundActionText = ObjectNames.NicifyVariableName(node.SoundAction.ToString());

            allSoundsActionLabel.SetText(allSoundsActionText);
            libraryActionLabel.SetText(libraryActionText);
            soundActionLabel.SetText(soundActionText);

            //update ids labels
            libraryIdLabel
                .SetStyleDisplay(node.LibraryAction == LibraryActionType.DoNothing ? DisplayStyle.None : DisplayStyle.Flex)
                .SetText(node.SoundLibraryId.libraryName);

            soundIdLabel
                .SetStyleDisplay(node.SoundAction == AudioActionType.DoNothing ? DisplayStyle.None : DisplayStyle.Flex)
                .SetText(node.SoundId.ToString());

            //set colors
            allSoundsActionLabel.SetStyleColor(node.AllSoundsAction == SoundActionType.DoNothing ? disabledActionColor : enabledActionColor);
            libraryActionLabel.SetStyleColor(node.LibraryAction == LibraryActionType.DoNothing ? disabledActionColor : enabledActionColor);
            soundActionLabel.SetStyleColor(node.SoundAction == AudioActionType.DoNothing ? disabledActionColor : enabledActionColor);
            libraryIdLabel.SetStyleColor(node.SoundLibraryId.isValid ? validIdColor : invalidIdColor);
            soundIdLabel.SetStyleColor(node.SoundId.isValid ? validIdColor : invalidIdColor);
        }
    }
}

#endif
