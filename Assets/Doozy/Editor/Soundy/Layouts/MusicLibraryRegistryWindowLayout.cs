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

namespace Doozy.Editor.Soundy.Layouts
{
    public sealed class MusicLibraryRegistryWindowLayout : AudioLibraryRegistryWindowLayout, ISoundyWindowLayout
    {
        public int order => 120;

        public override string layoutName => "Music Libraries";
        public override Texture2D staticIconTexture => EditorTextures.Soundy.Icons.Music;
        public override List<Texture2D> animatedIconTextures => EditorSpriteSheets.Soundy.Icons.Music;

        protected override IEnumerable<Texture2D> libraryIcon => EditorSpriteSheets.Soundy.Icons.MusicLibrary;
        
        private MusicLibrary previousSelectedLibrary
        {
            get
            {
                string guid = EditorPrefs.GetString(EditorPrefsKey(nameof(selectedLibrary)), string.Empty);
                MusicLibrary library = MusicLibraryRegistry.GetLibraryByGuid(guid);
                library ??= MusicLibraryRegistry.instance.libraries.Count > 0 ? MusicLibraryRegistry.instance.libraries[0] : null;
                return library;
            }
        }
        
        private MusicLibrary m_SelectedLibrary;
        private MusicLibrary selectedLibrary
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
        
        private Dictionary<MusicLibrary, FluidToggleButtonTab> libraryButtons { get; set; }
        
        public MusicLibraryRegistryWindowLayout()
        {
            AddHeader("Music Libraries", "Registry of all the music libraries", animatedIconTextures);
            Initialize();
            Refresh(false);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            
            EditorUtility.SetDirty(MusicLibraryRegistry.instance);
            EditorUtility.SetDirty(MusicLibraryDatabase.instance);
            AssetDatabase.SaveAssetIfDirty(MusicLibraryRegistry.instance);
            AssetDatabase.SaveAssetIfDirty(MusicLibraryDatabase.instance);
        }

        private void CreateNewLibrary()
        {
            selectedLibrary = MusicLibraryRegistry.CreateLibrary();
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
                MusicLibraryRegistry.instance.Refresh();
                MusicLibraryDatabase.instance.Refresh();
                
                EditorUtility.SetDirty(MusicLibraryRegistry.instance);
                EditorUtility.SetDirty(MusicLibraryDatabase.instance);
                AssetDatabase.SaveAssetIfDirty(MusicLibraryRegistry.instance);
                AssetDatabase.SaveAssetIfDirty(MusicLibraryDatabase.instance);
            }

            sideMenu.buttons.ForEach(b => b.Dispose());
            sideMenu.buttons.Clear();

            MusicLibraryRegistry.Validate();
            MusicLibraryDatabase.Validate();
            
            libraryButtons ??= new Dictionary<MusicLibrary, FluidToggleButtonTab>();
            libraryButtons.Clear();

            foreach (MusicLibrary library in MusicLibraryRegistry.instance.libraries)
            {
                if(library == null) continue;

                
                FluidToggleButtonTab sideMenuButton = 
                    GetSideMenuButton(library, library == selectedLibrary);
                
                libraryButtons.Add(library, sideMenuButton);

                EnabledIndicator buildIndicator = GetBuildIndicator();
                
                bool IsAddedToBuild(MusicLibrary l) =>
                    MusicLibraryDatabase.ContainsLibrary(l);
                
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
                    if (!MusicLibraryRegistry.Contains(library))
                    {
                        selectedLibrary = null;
                        Refresh(true);
                        return;
                    }
                    selectedLibrary = library;
                    var editor = (MusicLibraryEditor)UnityEditor.Editor.CreateEditor(selectedLibrary);
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
            if (MusicLibraryRegistry.instance.libraries.Count == 0)
            {
                InjectEmptyPlaceholder
                (
                    EditorSpriteSheets.Soundy.Icons.MusicLibrary,
                    "No Music Libraries Found!\n\n" +
                    "Refresh to search for music libraries\n" +
                    "or create a new one",
                    () => Refresh(true),
                    CreateNewLibrary
                );
                return;
            }

            // if a library was selected before -> select it again (if it still exists)
            MusicLibrary libraryToShow = previousSelectedLibrary ? previousSelectedLibrary : MusicLibraryRegistry.instance.libraries[0];
            schedule.Execute(() => libraryButtons[libraryToShow].SetIsOn(true, false)).ExecuteLater(50);
        }

        protected override  void Initialize()
        {
            refreshButton = 
                GetRefreshButton(() => Refresh(true), "Refresh the music library registry and database");
            
            newLibraryButton = 
                GetNewLibraryButton(CreateNewLibrary, "Create a new music library");

            base.Initialize();
        }
    }
}
