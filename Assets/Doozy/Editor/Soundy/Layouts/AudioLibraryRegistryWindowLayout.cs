// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.Components;
using Doozy.Editor.EditorUI.Components.Internal;
using Doozy.Editor.EditorUI.ScriptableObjects.Colors;
using Doozy.Editor.EditorUI.Utils;
using Doozy.Editor.UIElements;
using Doozy.Runtime.Soundy.ScriptableObjects.Internal;
using Doozy.Runtime.UIElements.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Doozy.Editor.Soundy.Layouts
{
    public abstract class AudioLibraryRegistryWindowLayout : FluidWindowLayout
    {
        protected const float k_ButtonWidth = 120;

        protected virtual IEnumerable<Texture2D> libraryIcon => EditorSpriteSheets.Soundy.Icons.Soundy;
        
        public override Color accentColor => EditorColors.Soundy.Color;
        public override EditorSelectableColorInfo selectableAccentColor => EditorSelectableColors.Soundy.Color;

        protected ScrollView contentScrollView { get; set; }

        protected FluidButton refreshButton { get; set; }
        protected FluidButton newLibraryButton { get; set; }

        protected FluidPlaceholder noLibrariesPlaceholder { get; set; }

        protected AudioLibraryRegistryWindowLayout()
        {
            //Side Menu - Settings
            sideMenu
                .SetMenuLevel(FluidSideMenu.MenuLevel.Level_2)
                .RemoveSearch();

            contentScrollView =
                new ScrollView()
                    .ResetLayout()
                    .SetName("Content ScrollView")
                    .SetStyleFlexGrow(1);

            contentScrollView.viewDataKey = $"${GetType().Name}.{nameof(contentScrollView)}";

            content
                .AddChild(contentScrollView);
        }

        protected override void LoadWindowState()
        {
            //do nothing
            //this is here to override the base method
        }

        protected override void SaveWindowState()
        {
            //do nothing
            //this is here to override the base method
        }

        protected virtual void Initialize()
        {
            sideMenu.toolbarContainer
                .SetStyleDisplay(DisplayStyle.Flex)
                .AddChild(refreshButton)
                .AddSpaceBlock()
                .AddChild(newLibraryButton);
        }

        protected void InjectEmptyPlaceholder(IEnumerable<Texture2D> placeholderIcon, string placeholderText, Action onRefreshCallback, Action onNewLibraryCallback)
        {
            noLibrariesPlaceholder =
                FluidPlaceholder
                    .Get(placeholderIcon)
                    .SetLabelText(placeholderText);

            FluidButton refreshMiniButton =
                FluidButton.Get()
                    .SetElementSize(ElementSize.Tiny)
                    .SetButtonStyle(ButtonStyle.Outline)
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Refresh)
                    .SetAccentColor(selectableAccentColor)
                    .SetOnClick(() => onRefreshCallback?.Invoke())
                    .SetLabelText("Refresh")
                    .SetTooltip("Refresh the library registry and database");

            FluidButton newLibraryMiniButton =
                FluidButton.Get()
                    .SetElementSize(ElementSize.Tiny)
                    .SetButtonStyle(ButtonStyle.Outline)
                    .SetIcon(EditorSpriteSheets.EditorUI.Icons.Plus)
                    .SetAccentColor(EditorSelectableColors.Default.Add)
                    .SetOnClick(() => onNewLibraryCallback?.Invoke())
                    .SetLabelText("New Library")
                    .SetTooltip("Create a new library");

            contentScrollView.contentContainer
                .RecycleAndClear()
                .SetStyleJustifyContent(Justify.Center)
                .AddChild(noLibrariesPlaceholder)
                .AddSpaceBlock(8)
                .AddChild
                (
                    DesignUtils.row
                        .SetStyleFlexGrow(0)
                        .SetStyleFlexShrink(0)
                        .AddFlexibleSpace()
                        .AddChild(refreshMiniButton)
                        .AddSpaceBlock()
                        .AddChild(newLibraryMiniButton)
                        .AddFlexibleSpace()
                );
        }


        protected FluidToggleButtonTab GetSideMenuButton(AudioLibrary library, bool isOn) =>
            sideMenu
                .AddButton(library.libraryName, selectableAccentColor)
                .SetIcon(libraryIcon)
                .SetLabelText(library.libraryName)
                .SetIsOn(isOn);
        
        protected EnabledIndicator GetBuildIndicator() =>
            EnabledIndicator.Get()
                .SetSize(18)
                .SetStyleFlexShrink(0)
                .SetEnabledColor(EditorColors.Default.Add)
                .SetDisabledColor(EditorColors.Default.Placeholder);


        protected FluidButton GetRefreshButton(Action callback, string buttonTooltip = "Refresh the library registry and database") =>
            DesignUtils
                .Buttons
                .RefreshDatabase
                (
                    "Refresh",
                    buttonTooltip,
                    selectableAccentColor,
                    () => callback?.Invoke()
                )
                .SetStyleWidth(k_ButtonWidth);

        protected FluidButton GetNewLibraryButton(Action callback, string buttonTooltip = "Create a new library") =>
            DesignUtils
                .Buttons
                .RefreshDatabase
                (
                    "New Library",
                    buttonTooltip,
                    EditorSelectableColors.Default.Add,
                    () => callback?.Invoke()
                )
                .SetIcon(EditorSpriteSheets.EditorUI.Icons.Plus)
                .SetStyleWidth(k_ButtonWidth);
    }
}
