// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System.Collections.Generic;
using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.Components;
using Doozy.Editor.EditorUI.Components.Internal;
using Doozy.Editor.Soundy.Editors;
using Doozy.Editor.UIElements;
using Doozy.Runtime.Soundy.ScriptableObjects;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Doozy.Editor.Soundy.Layouts
{
    public sealed class SoundLibraryRegistryWindowLayout : AudioLibraryRegistryWindowLayout, ISoundyWindowLayout
    {
        public int order => 110;

        public override string layoutName => "Sound Libraries";
        public override Texture2D staticIconTexture => EditorTextures.Soundy.Icons.Sound;
        public override List<Texture2D> animatedIconTextures => EditorSpriteSheets.Soundy.Icons.Sound;

        protected override IEnumerable<Texture2D> libraryIcon => EditorSpriteSheets.Soundy.Icons.SoundLibrary;

        private SoundLibrary previousSelectedLibrary
        {
            get
            {
                string guid = EditorPrefs.GetString(EditorPrefsKey(nameof(selectedLibrary)), string.Empty);
                SoundLibrary library = SoundLibraryRegistry.GetLibraryByGuid(guid);
                library ??= SoundLibraryRegistry.instance.libraries.Count > 0 ? SoundLibraryRegistry.instance.libraries[0] : null;
                return library;
            }
        }

        private SoundLibrary m_SelectedLibrary;
        private SoundLibrary selectedLibrary
        {
            get => m_SelectedLibrary;
            set
            {
                m_SelectedLibrary = value;
                string guid = m_SelectedLibrary == null
                    ? ""
                    : AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(m_SelectedLibrary)).ToString();
                EditorPrefs.SetString(EditorPrefsKey(nameof(selectedLibrary)), guid);
            }
        }

        private Dictionary<SoundLibrary, FluidToggleButtonTab> libraryButtons { get; set; }

        public SoundLibraryRegistryWindowLayout()
        {
            AddHeader("Sound Libraries", "Registry of all the sound libraries", animatedIconTextures);
            Initialize();
            Refresh(false);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            
            EditorUtility.SetDirty(SoundLibraryRegistry.instance);
            EditorUtility.SetDirty(SoundLibraryDatabase.instance);
            AssetDatabase.SaveAssetIfDirty(SoundLibraryRegistry.instance);
            AssetDatabase.SaveAssetIfDirty(SoundLibraryDatabase.instance);
        }

        private void CreateNewLibrary()
        {
            selectedLibrary = SoundLibraryRegistry.CreateLibrary();
            Refresh(false);
        }

        /// <summary> Refresh the layouts and optionally refresh the data by calling Refresh on the registry and database </summary>
        /// <param name="refreshData"> Call Refresh on the registry and database? </param>
        public void Refresh(bool refreshData)
        {
            contentScrollView.contentContainer
                .RecycleAndClear()
                .SetStyleJustifyContent(Justify.FlexStart);

            if (refreshData)
            {
                SoundLibraryRegistry.instance.Refresh();
                SoundLibraryDatabase.instance.Refresh();
                
                EditorUtility.SetDirty(SoundLibraryRegistry.instance);
                EditorUtility.SetDirty(SoundLibraryDatabase.instance);
                AssetDatabase.SaveAssetIfDirty(SoundLibraryRegistry.instance);
                AssetDatabase.SaveAssetIfDirty(SoundLibraryDatabase.instance);
            }

            sideMenu.buttons.ForEach(b => b.Dispose());
            sideMenu.buttons.Clear();

            SoundLibraryRegistry.Validate();
            SoundLibraryDatabase.Validate();

            libraryButtons ??= new Dictionary<SoundLibrary, FluidToggleButtonTab>();
            libraryButtons.Clear();

            foreach (SoundLibrary library in SoundLibraryRegistry.instance.libraries)
            {
                if (library == null) continue;

                FluidToggleButtonTab sideMenuButton = 
                    GetSideMenuButton(library, library == selectedLibrary);

                libraryButtons.Add(library, sideMenuButton);

                EnabledIndicator buildIndicator = GetBuildIndicator();

                bool IsAddedToBuild(SoundLibrary l) =>
                    SoundLibraryDatabase.ContainsLibrary(l);

                if (IsAddedToBuild(library))
                {
                    buildIndicator
                        .SetIcon(SoundyEditorUtils.buildEnabledIndicatorEnabledIcon)
                        .SetEnabled(false, true);
                }
                else
                {
                    buildIndicator
                        .SetIcon(SoundyEditorUtils.buildEnabledIndicatorDisabledIcon)
                        .SetDisabled(false, true);
                }

                sideMenuButton
                    .buttonContainer
                    .AddChild(buildIndicator);

                // when the library has been updated -> refresh relevant data and ui elements
                library.OnUpdate += () =>
                {
                    sideMenuButton.SetLabelText(library.libraryName);

                    bool isAddedToBuild = IsAddedToBuild(library);
                    buildIndicator
                        .SetIcon
                        (
                            isAddedToBuild
                                ? SoundyEditorUtils.buildEnabledIndicatorEnabledIcon
                                : SoundyEditorUtils.buildEnabledIndicatorDisabledIcon
                        )
                        .Toggle(isAddedToBuild);
                };

                sideMenuButton.OnValueChanged += evt =>
                {
                    if (!evt.newValue) return;
                    if (!SoundLibraryRegistry.Contains(library))
                    {
                        selectedLibrary = null;
                        Refresh(true);
                        return;
                    }
                    selectedLibrary = library;
                    var editor = (SoundLibraryEditor)UnityEditor.Editor.CreateEditor(selectedLibrary);
                    VisualElement libraryVisualElement = editor.CreateInspectorGUI();

                    editor.componentHeader.SetStyleDisplay(DisplayStyle.None);     //hide the header
                    editor.pingButton.SetStyleDisplay(DisplayStyle.Flex);          //show the ping button
                    editor.deleteLibraryButton.SetStyleDisplay(DisplayStyle.Flex); //show the delete button
                    editor.deleteLibraryButton.AddOnClick(() => Refresh(true));    //refresh the layout when the library is deleted

                    contentScrollView.contentContainer
                        .RecycleAndClear()
                        .AddChild(libraryVisualElement)
                        .Bind(editor.serializedObject);
                };
            }

            // if no libraries are found -> show a placeholder
            if (SoundLibraryRegistry.instance.libraries.Count == 0)
            {
                InjectEmptyPlaceholder
                (
                    EditorSpriteSheets.Soundy.Icons.SoundLibrary,
                    "No Sound Libraries Found!\n\n" +
                    "Refresh to search for sound libraries\n" +
                    "or create a new one",
                    () => Refresh(true),
                    CreateNewLibrary
                );
                return;
            }

            // if a library was selected before -> select it again (if it still exists)
            SoundLibrary libraryToShow = previousSelectedLibrary ? previousSelectedLibrary : SoundLibraryRegistry.instance.libraries[0];
            schedule.Execute(() => libraryButtons[libraryToShow].SetIsOn(true, false)).ExecuteLater(50);
        }

        protected override  void Initialize()
        {
            refreshButton = 
                GetRefreshButton(() => Refresh(true), "Refresh the sound library registry and database");
            
            newLibraryButton = 
                GetNewLibraryButton(CreateNewLibrary, "Create a new sound library");

            base.Initialize();
        }
    }
}
