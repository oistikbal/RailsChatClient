// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.Components;
using Doozy.Editor.EditorUI.Utils;
using Doozy.Editor.Reactor.Internal;
using Doozy.Editor.UIElements;
using Doozy.Runtime.Reactor.Extensions;
using Doozy.Runtime.Reactor.Internal;
using Doozy.Runtime.Reactor.Reactions;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Doozy.Editor.Soundy.Components
{
    public class MusicPlayerControls : VisualElement, IDisposable
    {
        public VisualElement layoutContainer { get; }
        public VisualElement content { get; }
        public Image icon { get; set; }
        public Texture2DReaction iconReaction { get; set; }

        public void Dispose()
        {
            layoutContainer.RecycleAndClear();
        }

        public MusicPlayerControls()
        {
            layoutContainer =
                new VisualElement()
                    .SetName("Layout Container")
                    .SetStyleFlexGrow(1)
                    .SetStyleOverflow(Overflow.Hidden);

            content =
                new VisualElement()
                    .SetName("Content")
                    .SetStylePadding(8, 4, 8, 4)
                    .SetStyleFlexDirection(FlexDirection.Row)
                    .SetStyleAlignSelf(Align.Center)
                    .SetStyleAlignItems(Align.Center);

            Initialize();
            Compose();

            this
                .SetStyleMarginTop(DesignUtils.k_Spacing2X)
                .SetStyleMarginBottom(DesignUtils.k_Spacing2X)
                .ResetBackgroundColor()
                .SetIcon(EditorSpriteSheets.Soundy.Icons.Soundy)
                .SetStyleDisplay(EditorApplication.isPlayingOrWillChangePlaymode ? DisplayStyle.Flex : DisplayStyle.None);
        }

        private void Initialize()
        {
            content
                .SetStyleFlexGrow(1);

            icon =
                new Image()
                    .SetName("Icon")
                    .SetStyleAlignSelf(Align.Center)
                    .SetStyleBackgroundImageTintColor(EditorColors.Soundy.Color)
                    .SetStyleSize(20)
                    .SetStyleFlexShrink(0);
            icon.AddManipulator(new Clickable(() => iconReaction?.Play()));
            icon.RegisterCallback<PointerEnterEvent>(evt => iconReaction?.Play());
        }

        private void Compose()
        {
            this
                .AddChild
                (
                    layoutContainer
                        .AddChild(content)
                );
        }

        public static FluidButton GetNewButton(Texture2D texture, string tooltip = "") =>
            FluidButton.Get()
                .SetIcon(texture)
                .SetAccentColor(SoundyEditorUtils.selectableAccentColor)
                .SetTooltip(tooltip)
                .SetElementSize(ElementSize.Tiny);

        public static FluidButton GetNewButton(IEnumerable<Texture2D> textures, string tooltip = "") =>
            FluidButton.Get()
                .SetIcon(textures)
                .SetAccentColor(SoundyEditorUtils.selectableAccentColor)
                .SetTooltip(tooltip)
                .SetElementSize(ElementSize.Tiny);
    }

    public static class MusicPlayerControlsExtensions
    {
        public static T AddItem<T>(this T target, VisualElement item) where T : MusicPlayerControls
        {
            target.content.AddChild(item);
            return target;
        }

        public static T AddDivider<T>(this T target) where T : MusicPlayerControls =>
            target.AddItem(DesignUtils.dividerVertical);

        public static T AddSpaceBlock<T>(this T target) where T : MusicPlayerControls =>
            target.AddItem(DesignUtils.spaceBlock);

        public static T AddSpaceBlock2X<T>(this T target) where T : MusicPlayerControls =>
            target.AddItem(DesignUtils.spaceBlock2X);

        public static T AddButton<T>(this T target, FluidButton button) where T : MusicPlayerControls =>
            target.AddItem(button);

        public static T AddButton<T>(this T target, IEnumerable<Texture2D> textures, string tooltip, UnityAction callback) where T : MusicPlayerControls
        {
            target.AddButton(MusicPlayerControls.GetNewButton(textures, tooltip).SetOnClick(callback));
            return target;
        }

        public static T Create<T>
        (
            this T target,
            UnityAction previousCallback,
            UnityAction playCallback,
            UnityAction stopCallback,
            UnityAction nextCallback
        ) where T : MusicPlayerControls =>
            target
                .AddPreviousButton(previousCallback)
                .AddSpaceBlock()
                .AddPlayButton(playCallback)
                .AddStopButton(stopCallback)
                .AddSpaceBlock()
                .AddNextButton(nextCallback)
                .AddSpaceBlock()
                .AddDivider()
                .AddSpaceBlock()
                .AddItem(target.icon);

        public static T AddPreviousButton<T>(this T target, UnityAction callback) where T : MusicPlayerControls =>
            target.AddButton(EditorSpriteSheets.EditorUI.Icons.FirstFrame, "Previous", callback);

        public static T AddNextButton<T>(this T target, UnityAction callback) where T : MusicPlayerControls =>
            target.AddButton(EditorSpriteSheets.EditorUI.Icons.LastFrame, "Next", callback);

        public static T AddPlayButton<T>(this T target, UnityAction callback) where T : MusicPlayerControls =>
            target.AddButton(EditorSpriteSheets.EditorUI.Icons.Play, "Play", callback);

        public static T AddStopButton<T>(this T target, UnityAction callback) where T : MusicPlayerControls =>
            target.AddButton(EditorSpriteSheets.EditorUI.Icons.Stop, "Stop", callback);

        /// <summary> Reset the background color of the controls to the default </summary>
        public static T ResetBackgroundColor<T>(this T controls) where T : MusicPlayerControls =>
            controls.SetBackgroundColor(EditorColors.Default.Background);

        /// <summary> Set the background color of the controls </summary>
        /// <param name="controls"> The controls to set the background color of </param>
        /// <param name="color"> The color to set the controls to </param>
        public static T SetBackgroundColor<T>(this T controls, Color color) where T : MusicPlayerControls
        {
            controls.content.SetStyleBackgroundColor(color);
            return controls;
        }

        /// <summary> Set Animated Icon </summary>
        /// <param name="target"> Target Button </param>
        /// <param name="textures"> Icon textures </param>
        public static T SetIcon<T>(this T target, IEnumerable<Texture2D> textures) where T : MusicPlayerControls
        {
            if (target.iconReaction == null)
            {
                target.iconReaction = target.icon.GetTexture2DReaction(textures).SetEditorHeartbeat().SetDuration(0.6f);
            }
            else
            {
                target.iconReaction.SetTextures(textures);
            }
            target.icon.SetStyleDisplay(DisplayStyle.Flex);
            return target;
        }

        /// <summary> Set Static Icon </summary>
        /// <param name="target"> Target Button </param>
        /// <param name="iconTexture2D"> Icon texture </param>
        public static T SetIcon<T>(this T target, Texture2D iconTexture2D) where T : MusicPlayerControls
        {
            target.iconReaction?.Recycle();
            target.iconReaction = null;
            target.icon.SetStyleBackgroundImage(iconTexture2D);
            target.icon.SetStyleDisplay(DisplayStyle.Flex);
            return target;
        }

        /// <summary> Clear the icon. If the icon is animated, its reaction will get recycled </summary>
        /// <param name="target"> Target Button </param>
        public static T ClearIcon<T>(this T target) where T : MusicPlayerControls
        {
            target.iconReaction?.Recycle();
            target.iconReaction = null;
            target.icon.SetStyleBackgroundImage((Texture2D)null);
            target.icon.SetStyleDisplay(DisplayStyle.None);
            return target;
        }
    }
}
