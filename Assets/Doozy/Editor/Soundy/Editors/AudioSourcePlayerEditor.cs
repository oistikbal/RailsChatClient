// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.Components;
using Doozy.Editor.EditorUI.ScriptableObjects.Colors;
using Doozy.Editor.EditorUI.Utils;
using Doozy.Runtime.Soundy;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
namespace Doozy.Editor.Soundy.Editors
{
    [CustomEditor(typeof(AudioSourcePlayer), true)]
    public class AudioSourcePlayerEditor : UnityEditor.Editor
    {
        private static Color accentColor => EditorColors.Soundy.Color;
        private static EditorSelectableColorInfo selectableAccentColor => EditorSelectableColors.Soundy.Color;

        private VisualElement root { get; set; }
        private FluidComponentHeader componentHeader { get; set; }
        
        private SerializedProperty propertySource { get; set; }
        
        private FluidField sourceFluidField { get; set; }
        private ObjectField sourceObjectField { get; set; }
        
        public override VisualElement CreateInspectorGUI()
        {
            FindSerializedProperties();
            Initialize();
            Compose();
            return root;
        }
        
        private void FindSerializedProperties()
        {
            propertySource = serializedObject.FindProperty("Source");
        }

        private void Initialize()
        {
            root = DesignUtils.editorRoot;

            componentHeader =
                DesignUtils.editorComponentHeader
                    .SetIcon(EditorSpriteSheets.Soundy.Icons.AudioPlayer)
                    .SetComponentNameText("AudioSource Player")
                    .SetAccentColor(accentColor)
                    .SetElementSize(ElementSize.Normal)
                    .SetStyleMarginBottom(DesignUtils.k_Spacing);

            sourceObjectField = DesignUtils.NewObjectField(propertySource, typeof(AudioSource)).SetStyleFlexGrow(1).SetTooltip("Target AudioSource");
            sourceFluidField = FluidField.Get().SetLabelText("Audio Source").SetIcon(EditorSpriteSheets.EditorUI.Icons.Sound).AddFieldContent(sourceObjectField);
        }

        private void Compose()
        {
            root
                .AddChild(componentHeader)
                .AddSpaceBlock()
                .AddChild(sourceFluidField)
                .AddEndOfLineSpace()
                ;
        }
    }
}
