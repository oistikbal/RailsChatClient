// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System.Collections.Generic;
using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.Components;
using Doozy.Editor.EditorUI.ScriptableObjects.Colors;
using Doozy.Editor.EditorUI.Utils;
using Doozy.Runtime.Soundy.ScriptableObjects.Internal;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UIElements;
// ReSharper disable MemberCanBePrivate.Global

namespace Doozy.Editor.Soundy.Editors
{
    /// <summary> Base class for Audio Library editors </summary>
    public abstract class AudioLibraryEditor : UnityEditor.Editor
    {
        protected AudioLibrary audioLibrary => (AudioLibrary)target;

        protected Color accentColor => EditorColors.Soundy.Color;
        protected EditorSelectableColorInfo selectableAccentColor => EditorSelectableColors.Soundy.Color;

        protected VisualElement root { get; set; }
        internal FluidComponentHeader componentHeader { get; set; }

        //Ping Button
        // - used to ping the Audio Library asset in the Project window
        // - this button is hidden in the Inspector, but visible in the library window
        internal FluidButton pingButton { get; set; }

        //Delete Library Button
        // - used to delete the Audio Library asset
        // - this button is hidden in the Inspector, but visible in the library window
        internal FluidButton deleteLibraryButton { get; set; }

        protected SerializedProperty propertyName { get; set; }
        protected SerializedProperty propertyOutputAudioMixerGroup { get; set; }

        // Build Settings
        // - used to add/remove the library to/from the build
        protected abstract bool isAddedToBuild { get; }
        protected abstract UnityAction addToBuildCallback { get; }
        protected abstract UnityAction removeFromBuildCallback { get; }
        protected FluidButton addToBuildButton { get; set; }
        protected FluidButton removeFromBuildButton { get; set; }
        protected VisualElement buildSettingsContainer { get; set; }
        protected EnabledIndicator buildIndicator { get; set; }
        protected Label buildSettingsLabel { get; set; }

        //Name and Output Audio Mixer Group
        // - used to change the name and the output audio mixer group of the library
        protected Label nameLabel { get; set; }
        protected TextField nameTextField { get; set; }
        protected ObjectField outputAudioMixerGroupObjectField { get; set; }
        protected FluidField renameHelpFluidField { get; set; }
        protected FluidField nameFluidField { get; set; }
        protected FluidField outputAudioMixerGroupFluidField { get; set; }

        //Content
        protected VisualElement content { get; set; }

        protected VisualElement toolbarContainer { get; set; }
        protected VisualElement dataContainer { get; set; }

        protected FluidDragAndDrop<AudioClip> dataFluidDragAndDrop { get; set; }

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
            propertyName = serializedObject.FindProperty("Name");
            propertyOutputAudioMixerGroup = serializedObject.FindProperty(nameof(AudioLibrary.OutputAudioMixerGroup));
        }

        protected virtual void Initialize()
        {
            root = DesignUtils.editorRoot;

            componentHeader =
                DesignUtils.editorComponentHeader
                    .SetIcon(EditorSpriteSheets.Soundy.Icons.Soundy)
                    .SetComponentNameText("Audio Library")
                    .SetAccentColor(accentColor)
                    .SetElementSize(ElementSize.Large)
                    .SetStyleMarginBottom(DesignUtils.k_Spacing);

            pingButton =
                DesignUtils.Buttons.PingAsset(target)
                    .SetStyleFlexShrink(0)
                    .SetButtonStyle(ButtonStyle.Contained)
                    .SetElementSize(ElementSize.Tiny)
                    .SetStyleMarginLeft(DesignUtils.k_Spacing)
                    .SetStyleDisplay(DisplayStyle.None);

            deleteLibraryButton =
                FluidButton.Get("Delete Library")
                    .SetTooltip("Delete this library")
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Close)
                    .SetAccentColor(EditorSelectableColors.Default.Remove)
                    .SetButtonStyle(ButtonStyle.Contained)
                    .SetElementSize(ElementSize.Tiny)
                    .SetStyleFlexShrink(0)
                    .SetStyleMarginLeft(DesignUtils.k_Spacing)
                    .SetStyleDisplay(DisplayStyle.None);

            content =
                new VisualElement()
                    .ResetLayout()
                    .SetName("Content");

            InitializeBuildSettings();
            InitializeName();
            InitializeOutputAudioMixerGroup();
            InitializeToolbar();
            InitializeData();
        }

       

        private void InitializeBuildSettings()
        {
            FluidButton BuildButton(string label, string tooltip, List<Texture2D> icon) =>
                FluidButton.Get()
                    .SetStyleFlexShrink(0)
                    .SetLabelText(label)
                    .SetTooltip(tooltip)
                    .SetElementSize(ElementSize.Small)
                    .SetButtonStyle(ButtonStyle.Contained)
                    .SetIcon(icon);

            addToBuildButton =
                BuildButton
                    (
                        "Add To Build",
                        "Make this Library available in a built project",
                        EditorSpriteSheets.EditorUI.Icons.Plus
                    )
                    .SetAccentColor(EditorSelectableColors.Default.Add)
                ;

            removeFromBuildButton =
                BuildButton
                (
                    "Remove From Build",
                    "Remove this Library from the build",
                    EditorSpriteSheets.EditorUI.Icons.Minus
                );

            addToBuildButton.SetOnClick(() =>
            {
                Undo.RecordObject(audioLibrary, "Add To Build");
                addToBuildCallback?.Invoke();
                RefreshBuildSettings();
                audioLibrary.OnUpdate?.Invoke();
                EditorUtility.SetDirty(audioLibrary);
                AssetDatabase.SaveAssetIfDirty(audioLibrary);
            });

            removeFromBuildButton.SetOnClick(() =>
            {
                if (!EditorUtility.DisplayDialog
                    (
                        "Remove From Build",
                        "Are you sure you want to remove this library from the build?\n\n" +
                        "This will make this library unavailable in a built project and all the sounds in it will not be playable on the target platform.",
                        "Yes",
                        "Cancel"
                    )
                   )
                    return;

                Undo.RecordObject(audioLibrary, "Remove From Build");
                removeFromBuildCallback?.Invoke();
                RefreshBuildSettings();
                audioLibrary.OnUpdate?.Invoke();
                EditorUtility.SetDirty(audioLibrary);
                AssetDatabase.SaveAssetIfDirty(audioLibrary);
            });

            buildIndicator =
                EnabledIndicator.Get()
                    .SetSize(24)
                    .SetStyleFlexShrink(0)
                    .SetEnabledColor(EditorColors.Default.Add)
                    .SetDisabledColor(EditorColors.Default.Placeholder);

            if (isAddedToBuild)
            {
                buildIndicator
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Prefab)
                    .SetEnabled(false, true);
            }
            else
            {
                buildIndicator
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Close)
                    .SetDisabled(false, true);
            }

            buildSettingsLabel =
                DesignUtils.NewLabel()
                    .SetStyleFlexShrink(1)
                    .SetStyleColor(EditorColors.Default.TextSubtitle);

            buildSettingsContainer =
                DesignUtils.row
                    .SetName("Build Settings")
                    .SetStyleOverflow(Overflow.Hidden)
                    .SetStylePadding(DesignUtils.k_Spacing)
                    .SetStyleBackgroundColor(EditorColors.Default.MenuBackgroundLevel0)
                    .SetStyleBorderRadius(DesignUtils.k_FieldBorderRadius);

            buildSettingsContainer
                .AddChild(buildIndicator)
                .AddSpaceBlock()
                .AddChild(buildSettingsLabel)
                .AddFlexibleSpace()
                .AddSpaceBlock()
                .AddChild(addToBuildButton)
                .AddChild(removeFromBuildButton)
                ;

            void RefreshBuildSettings()
            {
                buildIndicator
                    .SetIcon
                    (
                        isAddedToBuild
                            ? SoundyEditorUtils.buildEnabledIndicatorEnabledIcon
                            : SoundyEditorUtils.buildEnabledIndicatorDisabledIcon
                    )
                    .Toggle(isAddedToBuild);


                addToBuildButton.SetStyleDisplay(isAddedToBuild ? DisplayStyle.None : DisplayStyle.Flex);
                removeFromBuildButton.SetStyleDisplay(isAddedToBuild ? DisplayStyle.Flex : DisplayStyle.None);

                buildSettingsLabel
                    .SetStyleColor
                    (
                        isAddedToBuild
                            ? EditorColors.Default.Add
                            : EditorColors.Default.Placeholder
                    )
                    .SetText
                    (
                        isAddedToBuild
                            ? "This library will be available in the build"
                            : "This library will NOT be available in the build"
                    );
            }

            RefreshBuildSettings();
            audioLibrary.OnUpdate += RefreshBuildSettings;
        }

        private void InitializeName()
        {
            nameLabel =
                DesignUtils.NewFieldNameLabel(propertyName.stringValue)
                    .SetStyleFontSize(14)
                    .SetStyleColor(EditorColors.Default.TextTitle)
                    .SetStyleAlignSelf(Align.Center)
                    .SetStyleFlexGrow(1);

            nameTextField =
                DesignUtils.NewTextField(propertyName, true)
                    .SetStyleFlexGrow(1)
                    .SetStyleDisplay(DisplayStyle.None);

            FluidButton renameButton =
                FluidButton.Get()
                    .SetElementSize(ElementSize.Tiny)
                    .SetButtonStyle(ButtonStyle.Contained)
                    .SetLabelText("Rename")
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Edit)
                    .SetStyleFlexShrink(0);

            FluidButton cancelRenameButton =
                FluidButton.Get()
                    .SetElementSize(ElementSize.Tiny)
                    .SetButtonStyle(ButtonStyle.Contained)
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Close)
                    .SetAccentColor(EditorSelectableColors.Default.Remove)
                    .SetStyleMarginLeft(DesignUtils.k_Spacing)
                    .SetStyleDisplay(DisplayStyle.None)
                    .SetStyleFlexShrink(0);

            cancelRenameButton
                .SetOnClick(CancelRename);

            void CancelRename()
            {
                nameLabel.SetStyleDisplay(DisplayStyle.Flex);
                nameTextField.SetStyleDisplay(DisplayStyle.None);
                nameTextField.value = nameLabel.text; //name before rename
                renameButton
                    .SetLabelText("Rename")
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Edit);
                cancelRenameButton
                    .SetStyleDisplay(DisplayStyle.None);
            }

            void PerformRename()
            {
                var library = (AudioLibrary)target;
                nameLabel.SetStyleDisplay(DisplayStyle.Flex);
                nameTextField.SetStyleDisplay(DisplayStyle.None);
                library.libraryName = nameTextField.value;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                serializedObject.Update();
                library.Validate();
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                serializedObject.Update();
                renameButton
                    .SetLabelText("Rename")
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Edit);
                cancelRenameButton
                    .SetStyleDisplay(DisplayStyle.None);

                //Deselect the library asset in the Project view (if it is selected)
                //This is done to force Unity to refresh the inspector (otherwise it will not refresh)
                if (Selection.activeObject == library)
                    Selection.activeObject = null;

                Debug.Log($"[Audio Library] Renamed from <b>{nameLabel.text}</b> to <b>{library.libraryName}</b> and saved to disk at <b>{AssetDatabase.GetAssetPath(library)}</b>");

                nameLabel.text = library.libraryName; //update the name label with the new name
                library.OnUpdate?.Invoke();           //notify the library that it was updated
            }

            void StartRename()
            {
                if (!EditorUtility.DisplayDialog
                    (
                        "Rename Audio Library",
                        "Are you sure you want to rename this library?" +
                        "\n\n" +
                        "Note that all the UI settings (and code references) that use this library will NOT get automatically updated." +
                        "\n\n" +
                        "You will have to manually update all the references to this library in your UI settings and code." +
                        "\n\n" +
                        "This action cannot be undone and will also rename the asset file on the disk.",
                        "Rename",
                        "Cancel")
                   )
                    return;

                nameLabel.text = nameTextField.value;
                nameLabel.SetStyleDisplay(DisplayStyle.None);
                nameTextField.SetStyleDisplay(DisplayStyle.Flex);
                renameButton
                    .SetLabelText("Save")
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Save)
                    .SetAccentColor(EditorSelectableColors.Default.Add);
                cancelRenameButton
                    .SetStyleDisplay(DisplayStyle.Flex);
            }

            renameButton
                .SetOnClick(() =>
                {
                    //rename was in progress -> save
                    if (nameTextField.GetStyleDisplay() == DisplayStyle.Flex)
                    {
                        PerformRename();
                        return;
                    }

                    //rename was not in progress -> start rename
                    StartRename();
                });

            nameTextField.RegisterCallback<KeyUpEvent>(keyUpEvent =>
            {
                if (!nameTextField.IsFocused()) return;
                switch (keyUpEvent.keyCode)
                {
                    case KeyCode.Return:
                        if (nameTextField.GetStyleDisplay() == DisplayStyle.Flex)
                            PerformRename();
                        return;
                    case KeyCode.Escape:
                        if (nameTextField.GetStyleDisplay() == DisplayStyle.Flex)
                            CancelRename();
                        return;
                    default:
                        return;
                }
            });

            nameFluidField =
                FluidField.Get()
                    .SetElementSize(ElementSize.Tiny)
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Label)
                    .SetTooltip("Name of this audio library")
                    .AddFieldContent
                    (
                        DesignUtils.row
                            .SetStyleAlignItems(Align.Center)
                            .AddChild(nameLabel)
                            .AddChild(nameTextField)
                            .AddSpaceBlock()
                            .AddChild(renameButton)
                            .AddChild(cancelRenameButton)
                    );
        }

        private void InitializeOutputAudioMixerGroup()
        {
            outputAudioMixerGroupObjectField =
                DesignUtils.NewObjectField(propertyOutputAudioMixerGroup, typeof(AudioMixerGroup))
                    .SetStyleFlexGrow(1);

            outputAudioMixerGroupFluidField =
                FluidField.Get()
                    .SetElementSize(ElementSize.Tiny)
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.AudioMixerGroup)
                    // .SetLabelText("Output Mixer Group")
                    .SetTooltip("Output Audio Mixer Group that will be used by audio players when playing audio clips from this library")
                    .AddFieldContent(outputAudioMixerGroupObjectField);
        }

        protected FluidButton buttonSortAz { get; set; }
        protected FluidButton buttonSortZa { get; set; }
        protected FluidButton buttonClearData { get; set; }
        protected FluidButton buttonAddNew { get; set; }
        
        protected virtual void InitializeToolbar()
        {
            toolbarContainer = SoundyEditorUtils.Elements.GetToolbarContainer();
            buttonSortAz = SoundyEditorUtils.Elements.GetSortAzButton();
            buttonSortZa = SoundyEditorUtils.Elements.GetSortZaButton();
            buttonClearData = SoundyEditorUtils.Elements.GetClearButton();
            buttonAddNew = SoundyEditorUtils.Elements.GetAddButton();
            dataFluidDragAndDrop = new FluidDragAndDrop<AudioClip>(OnDragAnDropAudioClips);
            
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

            content
                .AddChild(toolbarContainer);
        }

        protected abstract void OnDragAnDropAudioClips();

        protected virtual void InitializeData()
        {
            dataContainer = SoundyEditorUtils.Elements.GetDataContainer();
            content.AddChild(dataContainer);
            UpdateData();
        }
        
        protected FluidButton ToolbarButton(string label, string tooltip, UnityAction onClick) =>
            FluidButton.Get()
                .SetName($"{label} Button")
                .SetElementSize(ElementSize.Tiny)
                .SetButtonStyle(ButtonStyle.Contained)
                .SetLabelText(label)
                .SetTooltip(tooltip)
                .SetOnClick(onClick);

        protected virtual void Compose()
        {
            root
                .AddChild(componentHeader)
                .AddChild(buildSettingsContainer)
                .AddSpaceBlock(2)
                .AddChild(DesignUtils.dividerHorizontal)
                .AddSpaceBlock(2)
                .AddChild(renameHelpFluidField)
                .AddChild
                (
                    DesignUtils.row
                        .SetStyleAlignItems(Align.Center)
                        .AddChild(nameFluidField)
                        .AddChild(pingButton)
                        .AddChild(deleteLibraryButton)
                )
                .AddSpaceBlock()
                .AddChild(outputAudioMixerGroupFluidField)
                .AddSpaceBlock(2)
                .AddChild(DesignUtils.dividerHorizontal)
                .AddSpaceBlock(2)
                .AddChild(content)
                .AddEndOfLineSpace();
        }

    }
}
