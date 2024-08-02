// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System.Linq;
using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.Components;
using Doozy.Editor.EditorUI.Utils;
using Doozy.Runtime.Common.Extensions;
using Doozy.Runtime.Soundy;
using Doozy.Runtime.Soundy.ScriptableObjects;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Doozy.Editor.Soundy.Editors
{
    [CustomEditor(typeof(MusicObject), true)]
    public class MusicObjectEditor : AudioObjectEditor
    {
        private MusicObject castedTarget => (MusicObject)target;

        private SerializedProperty propertyData { get; set; }

        protected override void FindSerializedProperties()
        {
            base.FindSerializedProperties();

            propertyData = serializedObject.FindProperty("Data");
        }

        protected override void Initialize()
        {
            base.Initialize();

            componentHeader
                .SetIcon(EditorSpriteSheets.Soundy.Icons.Music)
                .SetComponentNameText("Music");

            playerElement =
                new SoundyEditorPlayer.PlayerElement()
                    .SetAudioClipGetter(() => castedTarget.data?.Clip)
                    .SetVolumeGetter(castedTarget.GetVolume)
                    .SetPitchGetter(castedTarget.GetPitch)
                    .SetPriorityGetter(() => castedTarget.priority)
                    .SetPanStereoGetter(() => castedTarget.panStereo)
                    .SetSpatialBlendGetter(() => castedTarget.spatialBlend)
                    .SetReverbZoneMixGetter(() => castedTarget.reverbZoneMix)
                    .SetDopplerLevelGetter(() => castedTarget.dopplerLevel)
                    .SetSpreadGetter(() => castedTarget.spread)
                    .SetMinDistanceGetter(() => castedTarget.minDistance)
                    .SetMaxDistanceGetter(() => castedTarget.maxDistance)
                    .SetLoopGetter(() => castedTarget.loop)
                    .SetIgnoreListenerPauseGetter(() => castedTarget.ignoreListenerPause);

            InitializeData();
            InitializeDataToolbar();
        }

        protected override void OnDragAndDropAudioClip()
        {
            castedTarget.SetClip(dataFluidDragAndDrop.references.ToArray().First());
            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();
            // UpdateData();
        }

        protected override void UpdateData()
        {
            if (propertyData == null)
                return;

            dataContainer.Clear();

            var row =
                new VisualElement()
                    .ResetLayout()
                    .SetName("Row")
                    .SetStyleFlexDirection(FlexDirection.Row);

            var optionsContainer = SoundyEditorUtils.Elements.GetSideContainer().SetName("OptionsContainer");

            FluidButton updateNameButton =
                SoundyEditorUtils.Elements.GetUpdateNameButton()
                    .SetTooltip("Set the Music name to match the AudioClip name (in a pretty way)")
                    .SetOnClick(() =>
                    {
                        var clip = propertyData.FindPropertyRelative(nameof(SoundData.Clip)).objectReferenceValue as AudioClip;
                        if (clip == null) return;
                        propertyAudioName.stringValue = clip.name.CleanName();
                        serializedObject.ApplyModifiedProperties();
                        serializedObject.Update();
                        UpdateData();
                        root.schedule.Execute(() => castedTarget.OnUpdate?.Invoke());
                    });

            row
                .AddChild(DesignUtils.NewPropertyField(propertyData))
                .AddSpaceBlock()
                .AddChild
                (
                    optionsContainer
                        .AddChild(updateNameButton)
                        .AddFlexibleSpace()
                );

            dataContainer
                .AddChild(row)
                .Bind(serializedObject);
        }

        protected override void Compose()
        {
            base.Compose();

            root
                .AddSpaceBlock()
                .AddChild(DesignUtils.dividerHorizontal)
                .AddSpaceBlock()
                .AddChild(playerElement)
                .AddSpaceBlock()
                .AddChild(dataContainer)
                .AddEndOfLineSpace();
        }
    }
}
