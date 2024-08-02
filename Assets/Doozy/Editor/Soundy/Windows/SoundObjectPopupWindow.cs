// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using Doozy.Editor.EditorUI.Utils;
using Doozy.Editor.UIElements;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Doozy.Editor.Soundy.Windows
{
    public class SoundObjectPopupWindow : EditorWindow
    {
        private const string k_WindowTitle = "Sound Object";
        
        public static SoundObjectPopupWindow Open() =>
            GetWindow<SoundObjectPopupWindow>(true, k_WindowTitle, true);
        
        public Object asset { get; private set; }
        protected VisualElement root => rootVisualElement;
        private ScrollView scrollView { get; set; }
        private VisualElement assetEditorContainer { get; set; }
        
        public SoundObjectPopupWindow LoadAsset(Object target)
        {
            assetEditorContainer.RecycleAndClear();
            asset = target;
            if (asset == null)
            {
                EditorUtility.DisplayDialog
                (
                    "Can't load asset",
                    "The asset is null",
                    "Ok"
                );
                return this;
            }
            var editor = UnityEditor.Editor.CreateEditor(asset);
            VisualElement editorRoot = editor.CreateInspectorGUI();
            editorRoot.Bind(editor.serializedObject);
            assetEditorContainer.AddChild(editorRoot);
            editorRoot.SetStylePadding(DesignUtils.k_Spacing2X);
            return this;
        }

        private void CreateGUI()
        {
            assetEditorContainer = new VisualElement();
            scrollView = new ScrollView().SetStyleFlexGrow(1);
            scrollView.contentContainer.AddChild(assetEditorContainer);
            root
                .RecycleAndClear()
                .AddChild(scrollView);
        }

        private void OnEnable()
        {
            minSize = new Vector2(500, 500);
            
            //close window when play mode state changes
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            
            //close window when compiling
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            Close();
        }
        
        private void OnEditorUpdate()
        {
            if (EditorApplication.isCompiling) 
                Close();
        }
    }
}
