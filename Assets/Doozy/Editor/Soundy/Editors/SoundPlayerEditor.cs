// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.Components;
using Doozy.Editor.EditorUI.Components.Internal;
using Doozy.Editor.EditorUI.ScriptableObjects.Colors;
using Doozy.Editor.EditorUI.Utils;
using Doozy.Runtime.Soundy;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Doozy.Editor.Soundy.Editors
{
    [CustomEditor(typeof(SoundPlayer), true)]
    public class SoundPlayerEditor : UnityEditor.Editor
    {
        private static Color accentColor => EditorColors.Soundy.Color;
        private static EditorSelectableColorInfo selectableAccentColor => EditorSelectableColors.Soundy.Color;

        private SoundPlayer castedTarget => (SoundPlayer)target;

        private VisualElement root { get; set; }
        private FluidComponentHeader componentHeader { get; set; }

        private FluidToggleCheckbox playOnStartToggleCheckbox { get; set; }
        private FluidToggleCheckbox playOnEnableToggleCheckbox { get; set; }
        private FluidToggleCheckbox playOnDisableToggleCheckbox { get; set; }
        private FluidToggleCheckbox stopOnDisableToggleCheckbox { get; set; }
        private FluidToggleCheckbox stopOnDestroyToggleCheckbox { get; set; }

        private FluidField onStartFluidField { get; set; }
        private FluidField onEnableFluidField { get; set; }
        private FluidField onDisableFluidField { get; set; }
        private FluidField onDestroyFluidField { get; set; }
        
        private FluidField followTargetFluidField { get; set; }

        private SerializedProperty propertyId { get; set; }
        private SerializedProperty propertyPlayOnStart { get; set; }
        private SerializedProperty propertyPlayOnEnable { get; set; }
        private SerializedProperty propertyPlayOnDisable { get; set; }
        private SerializedProperty propertyStopOnDisable { get; set; }
        private SerializedProperty propertyStopOnDestroy { get; set; }
        private SerializedProperty propertyFollowTarget { get; set; }

        private void OnDisable()
        {
            componentHeader?.Recycle();
            playOnStartToggleCheckbox?.Recycle();
            playOnEnableToggleCheckbox?.Recycle();
            playOnDisableToggleCheckbox?.Recycle();
            stopOnDisableToggleCheckbox?.Recycle();
            stopOnDestroyToggleCheckbox?.Recycle();

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
            propertyId = serializedObject.FindProperty("Id");
            propertyPlayOnStart = serializedObject.FindProperty("PlayOnStart");
            propertyPlayOnEnable = serializedObject.FindProperty("PlayOnEnable");
            propertyPlayOnDisable = serializedObject.FindProperty("PlayOnDisable");
            propertyStopOnDisable = serializedObject.FindProperty("StopOnDisable");
            propertyStopOnDestroy = serializedObject.FindProperty("StopOnDestroy");
            propertyFollowTarget = serializedObject.FindProperty("FollowTarget");
        }

        private void InitializeEditor()
        {
            root = DesignUtils.editorRoot;
            componentHeader =
                DesignUtils.editorComponentHeader
                    .SetIcon(EditorSpriteSheets.Soundy.Icons.AudioPlayer)
                    .SetSecondaryIcon(EditorSpriteSheets.Soundy.Icons.Sound)
                    .SetComponentNameText("Sound Player")
                    .SetAccentColor(accentColor)
                    .AddManualButton()
                    .AddApiButton()
                    .AddYouTubeButton();

            playOnStartToggleCheckbox =
                FluidToggleCheckbox.Get()
                    .SetLabelText("Play")
                    .SetTooltip("Play the sound on Start")
                    .SetToggleAccentColor(selectableAccentColor)
                    .BindToProperty(propertyPlayOnStart);

            playOnEnableToggleCheckbox =
                FluidToggleCheckbox.Get()
                    .SetLabelText("Play")
                    .SetTooltip("Play the sound on Enable")
                    .SetToggleAccentColor(selectableAccentColor)
                    .BindToProperty(propertyPlayOnEnable);

            playOnDisableToggleCheckbox =
                FluidToggleCheckbox.Get()
                    .SetLabelText("Play")
                    .SetTooltip("Play the sound on Disable")
                    .SetToggleAccentColor(selectableAccentColor)
                    .BindToProperty(propertyPlayOnDisable);

            stopOnDisableToggleCheckbox =
                FluidToggleCheckbox.Get()
                    .SetLabelText("Stop")
                    .SetTooltip("Stop the sound on Disable")
                    .SetToggleAccentColor(selectableAccentColor)
                    .BindToProperty(propertyStopOnDisable);

            stopOnDestroyToggleCheckbox =
                FluidToggleCheckbox.Get()
                    .SetLabelText("Stop")
                    .SetTooltip("Stop the sound on Destroy")
                    .SetToggleAccentColor(selectableAccentColor)
                    .BindToProperty(propertyStopOnDestroy);

            onStartFluidField = FluidField.Get().SetLabelText("On Start").SetElementSize(ElementSize.Tiny)
                .AddFieldContent(playOnStartToggleCheckbox);


            onEnableFluidField = FluidField.Get().SetLabelText("On Enable").SetElementSize(ElementSize.Tiny)
                .AddFieldContent(playOnEnableToggleCheckbox);

            onDisableFluidField = FluidField.Get().SetLabelText("On Disable").SetElementSize(ElementSize.Tiny)
                .AddFieldContent
                (
                    DesignUtils.row
                        .SetStyleAlignItems(Align.Center)
                        .AddChild(playOnDisableToggleCheckbox)
                        .AddSpaceBlock()
                        .AddChild(stopOnDisableToggleCheckbox)
                );

            onDestroyFluidField = FluidField.Get().SetLabelText("On Destroy").SetElementSize(ElementSize.Tiny)
                .AddFieldContent(stopOnDestroyToggleCheckbox);

            followTargetFluidField =
                FluidField.Get()
                    .SetLabelText("Follow Target")
                    .AddFieldContent
                    (
                        DesignUtils.NewObjectField(propertyFollowTarget, typeof(Transform))
                            .SetTooltip("The Transform to follow when playing the sound")
                            .SetStyleFlexGrow(1)
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
                .AddChild(componentHeader)
                .AddSpaceBlock()
                .AddChild(onStartFluidField)
                .AddSpaceBlock()
                .AddChild(onEnableFluidField)
                .AddSpaceBlock()
                .AddChild(onDisableFluidField)
                .AddSpaceBlock()
                .AddChild(onDestroyFluidField)
                .AddSpaceBlock(2)
                .AddChild(DesignUtils.NewPropertyField(propertyId))
                .AddSpaceBlock(2)
                .AddChild(followTargetFluidField)
                .AddEndOfLineSpace()
                ;
        }
    }
}
