// Copyright (c) 2015 - 2023 Doozy Entertainment. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Doozy.Editor.Common.Extensions;
using Doozy.Editor.Common.ScriptableObjects;
using Doozy.Editor.EditorUI;
using Doozy.Editor.EditorUI.Components;
using Doozy.Editor.EditorUI.Utils;
using Doozy.Runtime.Bindy;
using Doozy.Runtime.UIElements.Extensions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Doozy.Editor.Bindy.Drawers
{
    [CustomPropertyDrawer(typeof(BindyValue), true)]
    public class BindyValueDrawer : PropertyDrawer
    {
        private const string k_EmptyTypeLabel = "Variable not set";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {}

        private static IEnumerable<Texture2D> linkIconTextures => EditorSpriteSheets.EditorUI.Icons.Link;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var bindyValue = property.GetTargetObjectOfProperty() as BindyValue;
            bindyValue?.Initialize();

            var drawer = new VisualElement();

            var propertyTarget = property.FindPropertyRelative("Target");
            var propertyVariableName = property.FindPropertyRelative("VariableName");
            var propertyVariableType = property.FindPropertyRelative("VariableType");

            ObjectField targetObjectField =
                DesignUtils.NewObjectField(propertyTarget, typeof(Object)).SetStyleFlexGrow(1).SetStyleMaxHeight(20);

            Label typeLabel =
                DesignUtils.NewFieldNameLabel("")
                    .SetName("VariableType")
                    .SetStyleAlignSelf(Align.Center)
                    .SetStyleMarginTop(DesignUtils.k_Spacing)
                    .SetStyleMarginLeft(DesignUtils.k_Spacing)
                    .SetStyleMarginRight(DesignUtils.k_Spacing);

            TextField variableNameTextField =
                DesignUtils.NewTextField(propertyVariableName)
                    .SetName("VariableName")
                    .SetStyleFlexGrow(1)
                    .SetStyleMarginTop(DesignUtils.k_Spacing);

            variableNameTextField.SetEnabled(false); //disable the variable name text field, it's only used for display purposes

            EnumField invisibleVariableTypeEnumField =
                DesignUtils.NewEnumField(propertyVariableType, true);

            FluidButton linkButton =
                FluidButton.Get()
                    .SetLabelText("Link")
                    .SetIcon(linkIconTextures)
                    .SetElementSize(ElementSize.Normal)
                    .SetButtonStyle(ButtonStyle.Contained)
                    .SetStyleFlexShrink(0)
                    .SetStyleMarginLeft(DesignUtils.k_Spacing2X)
                    .SetStyleAlignSelf(Align.Center)
                    .SetOnClick(() =>
                    {
                        var searchWindowContext = new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition));
                        DynamicSearchProvider dsp =
                            ScriptableObject.CreateInstance<DynamicSearchProvider>()
                                .AddItems(GetSearchMenuItems(property, bindyValue));
                        SearchWindow.Open(searchWindowContext, dsp);
                    });

            FluidField targetFluidField =
                FluidField.Get()
                    .AddFieldContent
                    (
                        DesignUtils.row
                            .AddChild
                            (
                                DesignUtils.column
                                    .SetStyleJustifyContent(Justify.Center)
                                    .AddChild(targetObjectField)
                                    .AddChild
                                    (
                                        DesignUtils.row
                                            .AddChild(typeLabel)
                                            .AddChild(variableNameTextField)
                                    )
                            )
                            .AddChild(linkButton)
                    );

            targetObjectField.RegisterValueChangedCallback(evt =>
            {
                if (evt == null) return;
                UpdateLinkButton();
                if (bindyValue == null) return;
                bindyValue.Initialize();
                if (bindyValue.IsValid())
                {
                    object value = bindyValue.GetValue();
                    typeLabel.text = value != null ? value.GetType().Name : k_EmptyTypeLabel;
                    return;
                }
                typeLabel.text = k_EmptyTypeLabel;
                propertyVariableName.stringValue = string.Empty;
                propertyVariableType.enumValueIndex = (int)VariableType.None;
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.UpdateIfRequiredOrScript();
            });

            UpdateLinkButton();

            invisibleVariableTypeEnumField.RegisterValueChangedCallback(evt =>
            {
                if (evt?.newValue == null) return;
                UpdateLinkButton();
            });

            drawer.schedule.Execute(() =>
            {
                if (bindyValue != null)
                {
                    bindyValue.Initialize();

                    if (bindyValue.IsValid())
                    {
                        object value = bindyValue.GetValue();
                        typeLabel.text = value != null ? value.GetType().Name : k_EmptyTypeLabel;
                    }
                    else
                    {
                        typeLabel.text = k_EmptyTypeLabel;
                    }
                } //end of target is not null

                variableNameTextField.RegisterValueChangedCallback(evt =>
                {
                    if (evt?.newValue == null) return;
                    bindyValue?.Initialize();
                    if (evt.previousValue == evt.newValue)
                        return;
                    if (bindyValue != null)
                    {
                        if (bindyValue.IsValid())
                        {
                            object value = bindyValue.GetValue();
                            typeLabel.text = value != null ? value.GetType().Name : string.Empty;
                        }
                        else
                        {
                            typeLabel.text = k_EmptyTypeLabel;
                        }
                    }
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }); //end of variable name text field

            }); //end of schedule

            return
                drawer
                    .AddChild(targetFluidField)
                    .AddChild(invisibleVariableTypeEnumField);

            void UpdateLinkButton()
            {
                linkButton.SetEnabled(propertyTarget.objectReferenceValue != null);
            }
        }

        public static List<KeyValuePair<string, UnityAction>> GetSearchMenuItems(SerializedProperty property, BindyValue value)
        {
            var searchItems = new HashSet<SearchItem>();

            var keyValuePairsList = new List<KeyValuePair<string, UnityAction>>();

            if (value.target == null)
                return keyValuePairsList;

            void SetTarget(Object target)
            {
                Undo.RecordObject(property.serializedObject.targetObject, "Set Target");
                value.SetTarget(target);
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                property.serializedObject.UpdateIfRequiredOrScript();
                value.Initialize();
            }

            void SetField(string nameOfField)
            {
                Undo.RecordObject(property.serializedObject.targetObject, "Set Field");
                value.SetField(nameOfField);
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                property.serializedObject.UpdateIfRequiredOrScript();
                value.Initialize();
            }

            void SetProperty(string nameOfProperty)
            {
                Undo.RecordObject(property.serializedObject.targetObject, "Set Property");
                value.SetProperty(nameOfProperty);
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                property.serializedObject.UpdateIfRequiredOrScript();
                value.Initialize();
            }

            //check if value.target is a ScriptableObject
            if (value.target is ScriptableObject so)
            {
                searchItems.Add
                (
                    new SearchItem
                    (
                        so,
                        BindyValue.GetFieldInfos(so),    //
                        BindyValue.GetPropertyInfos(so), //
                        SetTarget,                       //value.SetTarget,
                        SetField,                        //value.SetField,
                        SetProperty                      //value.SetProperty
                    )
                );

            }
            //check if value.target is a GameObject
            else
            {
                //get the GameObject for the target UnityEvent.Object
                GameObject go = value.GetGameObject();

                //add the GameObject to the search items
                searchItems.Add
                (
                    new SearchItem
                    (
                        go,
                        BindyValue.GetFieldInfos(go),    //
                        BindyValue.GetPropertyInfos(go), //
                        SetTarget,                       //value.SetTarget,
                        SetField,                        //value.SetField,
                        SetProperty                      //value.SetProperty
                    )
                );

                //add all attached components to search items
                foreach (Component co in go.GetComponents(typeof(Component)))
                    searchItems.Add
                    (
                        new SearchItem
                        (
                            co,
                            BindyValue.GetFieldInfos(co),    //
                            BindyValue.GetPropertyInfos(co), //
                            SetTarget,                       //value.SetTarget,
                            SetField,                        //value.SetField,
                            SetProperty                      //value.SetProperty
                        )
                    );
            }

            //create the search list
            foreach (SearchItem item in searchItems)
                foreach (KeyValuePair<string, UnityAction> searchAction in item.GetSearchActions())
                    keyValuePairsList.Add(new KeyValuePair<string, UnityAction>(searchAction.Key, searchAction.Value));

            //42
            return keyValuePairsList;
        }

        [Serializable]
        public struct SearchItem
        {
            public Object target { get; }
            public List<string> fields { get; }
            public List<string> properties { get; }

            public UnityAction<Object> TargetSetter;
            public UnityAction<string> FieldSetter;
            public UnityAction<string> PropertySetter;

            private string typeName => target.GetType().Name;
            private string GetPath(string s) => $"{typeName}/{s}";

            public List<KeyValuePair<string, UnityAction>> GetSearchActions()
            {
                //why are you here?

                var list = new List<KeyValuePair<string, UnityAction>>();

                // ReSharper disable once ForCanBeConvertedToForeach
                // ReSharper disable once LoopCanBeConvertedToQuery
                for (int i = 0; i < fields.Count; i++)
                {
                    SearchItem tmpThis = this;        //this is a struct, so we need to copy it
                    string f = fields[i];             //field name
                    string path = tmpThis.GetPath(f); //field path
                    list.Add
                    (
                        new KeyValuePair<string, UnityAction>
                        (
                            path,
                            () =>
                            {
                                tmpThis.TargetSetter.Invoke(tmpThis.target);
                                tmpThis.FieldSetter.Invoke(f);
                            }
                        )
                    );
                }

                // ReSharper disable once ForCanBeConvertedToForeach
                // ReSharper disable once LoopCanBeConvertedToQuery
                for (int i = 0; i < properties.Count; i++)
                {
                    SearchItem tmpThis = this;        //this is a struct, so we need to copy it
                    string p = properties[i];         //property name
                    string path = tmpThis.GetPath(p); //property path
                    
                    //get property to check if it canRead or canWrite
                    PropertyInfo propertyInfo = tmpThis.target.GetType().GetProperty(p);
                    
                    //if property is read only add (read only) to the path
                    if (propertyInfo != null && !propertyInfo.CanWrite)
                        path += " (read only)";
                    
                    //if property is write only add (write only) to the path
                    if (propertyInfo != null && !propertyInfo.CanRead)
                        path += " (write only)";
                    
                    list.Add
                    (
                        new KeyValuePair<string, UnityAction>
                        (
                            path,
                            () =>
                            {
                                tmpThis.TargetSetter.Invoke(tmpThis.target);
                                tmpThis.PropertySetter.Invoke(p);
                            }
                        )
                    );
                }

                return list;
            }

            public SearchItem
            (
                Object target,
                IEnumerable<FieldInfo> fields,
                IEnumerable<PropertyInfo> properties,
                UnityAction<Object> targetSetter,
                UnityAction<string> fieldSetter,
                UnityAction<string> propertySetter
            )
            {
                //what are you searching for?
                this.target = target;
                this.fields = fields.Select(f => f.Name).ToList();
                this.properties = properties.Select(p => p.Name).ToList();
                TargetSetter = targetSetter;
                FieldSetter = fieldSetter;
                PropertySetter = propertySetter;
            }
        }
    }
}
