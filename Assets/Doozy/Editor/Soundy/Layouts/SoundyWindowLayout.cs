// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.Components;
using Doozy.Editor.EditorUI.Components.Internal;
using Doozy.Editor.EditorUI.ScriptableObjects.Colors;
using Doozy.Editor.Interfaces;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Doozy.Editor.Soundy.Layouts
{
    public class SoundyWindowLayout : FluidWindowLayout, IDashboardWindowLayout
    {
        public bool isValid => true;

        public int order => 400;

        public override string layoutName => "Soundy";
        public sealed override List<Texture2D> animatedIconTextures => EditorSpriteSheets.Soundy.Icons.Soundy;

        public override Color accentColor => EditorColors.Default.UnityThemeInversed;
        public override EditorSelectableColorInfo selectableAccentColor => EditorSelectableColors.Default.UnityThemeInversed;

        public SoundyWindowLayout()
        {
            content.ResetLayout();

            sideMenu
                .SetMenuLevel(FluidSideMenu.MenuLevel.Level_1)
                .RemoveSearch()
                .IsCollapsable(true)
                .SetCustomWidth(200);

            Initialize();
            Compose();
        }

        private void Initialize()
        {
            #region Side Menu

            //get all the types that implement the ISoundyWindowLayout interface
            //they are used to generate the side menu buttons and to get/display the corresponding content
            IEnumerable<ISoundyWindowLayout> layouts =
                TypeCache.GetTypesDerivedFrom(typeof(ISoundyWindowLayout))               //get all the types that derive from ISoundyWindowLayout
                    .Select(type => (ISoundyWindowLayout)Activator.CreateInstance(type)) //create an instance of the type
                    .OrderBy(l => l.order)                                               //sort the layouts by order (set in each layout's class)
                    .ThenBy(l => l.layoutName);                                          //sort the layouts by name (set in each layout's class)

            //order indicator used to add spacing between the tabs, when the difference is greater or equal to 50
            int previousOrder = -1;

            //add buttons to side menu
            foreach (ISoundyWindowLayout l in layouts)
            {
                //INJECT SPACE
                if (previousOrder != -1 && Mathf.Abs(previousOrder - l.order) >= 50) //if the layout order difference is greater or equal to 50
                    sideMenu.AddSpaceBetweenButtons();                               //add a space between the buttons
                previousOrder = l.order;                                             //keep track of the previous layout order

                //SIDE MENU BUTTON
                FluidToggleButtonTab sideMenuButton = sideMenu.AddButton(l.layoutName, l.selectableAccentColor);

                //ADD SIDE MENU BUTTON ICON (animated or static)
                if (l.animatedIconTextures?.Count > 0)
                    sideMenuButton.SetIcon(l.animatedIconTextures); // <<< ANIMATED ICON
                else if (l.staticIconTexture != null)
                    sideMenuButton.SetIcon(l.staticIconTexture); // <<< STATIC ICON

                //WINDOW LAYOUT (added to the content container when the button is pressed)
                VisualElement customWindowLayout = ((VisualElement)l).SetStyleFlexGrow(1);

                sideMenuButton.SetToggleAccentColor(((ISoundyWindowLayout)customWindowLayout).selectableAccentColor);

                sideMenuButton.OnValueChanged += evt =>
                {
                    if (!evt.newValue) return;
                    content.Clear();
                    content.AddChild(customWindowLayout);
                };
            }

            #endregion
        }

        private void Compose()
        {

        }
    }
}
