// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using Doozy.Runtime.Soundy;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEngine.UIElements;

namespace Doozy.Editor.Soundy.Drawers
{
    [CustomPropertyDrawer(typeof(MusicData), true)]
    public class MusicDataDrawer : AudioDataDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement drawer = SoundyEditorUtils.Elements.GetPropertyContainer();
            
            var clipProperty = property.FindPropertyRelative(nameof(AudioData.Clip));
            // var volumeProperty = property.FindPropertyRelative(nameof(AudioData.Volume));
            // var pitchProperty = property.FindPropertyRelative(nameof(AudioData.Pitch));
            // var weightProperty = property.FindPropertyRelative(nameof(AudioData.Weight));
            //
            // var volumeAnimatedContainer = new FluidAnimatedContainer("Volume", true).Hide(false);
            // var pitchAnimatedContainer = new FluidAnimatedContainer("Pitch", true).Hide(false);
            // var weightAnimatedContainer = new FluidAnimatedContainer("Weight", true).Hide(false);
            //
            // var tabGroup =
            //     new FluidToggleGroup()
            //         .SetControlMode(FluidToggleGroup.ControlMode.OneToggleOn);
            //
            // var volumeTab =
            //     SoundyEditorUtils.Elements
            //         .GetTab()
            //         .SetTooltip("The volume of the audio clip that will be played when this audio data is used")
            //         .SetTabPosition(TabPosition.TabOnLeft)
            //         .SetIcon(EditorSpriteSheets.EditorUI.Icons.Sound)
            //         .SetOnValueChanged(evt => volumeAnimatedContainer.Toggle(evt.newValue));
            //
            // var pitchTab =
            //     SoundyEditorUtils.Elements
            //         .GetTab()
            //         .SetTooltip("The pitch of the audio clip that will be played when this audio data is used")
            //         .SetTabPosition(TabPosition.TabInCenter)
            //         .SetIcon(EditorSpriteSheets.EditorUI.Icons.Pitch)
            //         .SetOnValueChanged(evt => pitchAnimatedContainer.Toggle(evt.newValue));
            //
            // var weightTab =
            //     SoundyEditorUtils.Elements
            //         .GetTab()
            //         .SetTooltip("Weight used for randomization. The higher the weight, the more chances this sound has to be picked when using randomization.")
            //         .SetTabPosition(TabPosition.TabOnRight)
            //         .SetIcon(EditorSpriteSheets.EditorUI.Icons.Dice)
            //         .SetOnValueChanged(evt => weightAnimatedContainer.Toggle(evt.newValue));
            //
            // volumeTab.AddToToggleGroup(tabGroup);
            // pitchTab.AddToToggleGroup(tabGroup);
            // weightTab.AddToToggleGroup(tabGroup);
            //
            // volumeAnimatedContainer.SetOnShowCallback(() =>
            // {
            //     volumeAnimatedContainer
            //         .AddContent(SoundyEditorUtils.Elements.GetVolumeFluidField(volumeProperty).SetStyleMarginTop(DesignUtils.k_Spacing))
            //         .Bind(property.serializedObject);
            // });
            //
            // pitchAnimatedContainer.SetOnShowCallback(() =>
            // {
            //     pitchAnimatedContainer
            //         .AddContent(SoundyEditorUtils.Elements.GetPitchFluidField(pitchProperty).SetStyleMarginTop(DesignUtils.k_Spacing))
            //         .Bind(property.serializedObject);
            // });
            //
            // weightAnimatedContainer.SetOnShowCallback(() =>
            // {
            //     weightAnimatedContainer
            //         .AddContent(SoundyEditorUtils.Elements.GetWeightFluidField(weightProperty).SetStyleMarginTop(DesignUtils.k_Spacing))
            //         .Bind(property.serializedObject);
            // });
            //
            // float volumeValue = volumeProperty.floatValue + 1;
            // float pitchValue = pitchProperty.floatValue + 1;
            // int weightValue = weightProperty.intValue + 1;
            //
            // IVisualElementScheduledItem refresher = drawer.schedule.Execute(() =>
            // {
            //     if (!(Math.Abs(volumeValue - volumeProperty.floatValue) < Mathf.Epsilon))
            //     {
            //         volumeValue = volumeProperty.floatValue;
            //         volumeTab.SetLabelText($"Volume: {volumeValue * 100:0}%");
            //     }
            //
            //     if (!(Math.Abs(pitchValue - pitchProperty.floatValue) < Mathf.Epsilon))
            //     {
            //         pitchValue = pitchProperty.floatValue;
            //         //show + - signs for the pitch value
            //         pitchTab.SetLabelText($"Pitch: {pitchValue:0.00}");
            //     }
            //
            //     if (!(Math.Abs(weightValue - weightProperty.intValue) < Mathf.Epsilon))
            //     {
            //         weightValue = weightProperty.intValue;
            //         weightTab.SetLabelText($"Weight: {weightValue}");
            //     }
            //
            // }).Every(100);
            //
            // drawer.RegisterCallback<DetachFromPanelEvent>(_ => refresher?.Pause());
            // drawer.RegisterCallback<AttachToPanelEvent>(_ => refresher?.Resume());
            
            drawer
                .AddChild(SoundyEditorUtils.Elements.GetClipFluidField(clipProperty))
                // .AddSpaceBlock()
                // .AddChild
                // (
                //     DesignUtils.row
                //         .AddChild(volumeTab)
                //         .AddSpace(1)
                //         .AddChild(pitchTab)
                //         .AddSpace(1)
                //         .AddChild(weightTab)
                // )
                // .AddChild(volumeAnimatedContainer)
                // .AddChild(pitchAnimatedContainer)
                // .AddChild(weightAnimatedContainer)
                ;

            // drawer.RegisterCallback<DetachFromPanelEvent>(_ =>
            // {
            //     volumeAnimatedContainer.Dispose();
            //     pitchAnimatedContainer.Dispose();
            //     weightAnimatedContainer.Dispose();
            //
            //     volumeTab.Recycle();
            //     pitchTab.Recycle();
            //     weightTab.Recycle();
            //     
            //     refresher?.Pause();
            // });

            return drawer;
        }
    }
}
