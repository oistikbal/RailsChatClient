using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using TypeReferences.Editor.Drawers;
using TypeReferences;
using RailsChat;

namespace RailsChat
{

    [CustomEditor(typeof(SocketService)), CanEditMultipleObjects]
    public class SocketServiceEditor : Editor
    {
        private SocketService _socketService;
        private ReorderableList reorderableList;

        void OnEnable()
        {
            _socketService = (SocketService)target;
        }

        public override void OnInspectorGUI()
        {

            if (EditorApplication.isPlaying)
            {
                GUI.enabled = false;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Web Socket Url");
                EditorGUILayout.TextField(serializedObject.FindProperty("_webSocketUrl").stringValue);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Login Url");
                EditorGUILayout.TextField(serializedObject.FindProperty("_loginUrl").stringValue);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();

                GUI.enabled = true;
                if (_socketService.Channels.Count == 0)
                {
                    GUIStyle centeredLabelStyle = new GUIStyle(GUI.skin.label);
                    centeredLabelStyle.alignment = TextAnchor.MiddleCenter;
                    EditorGUILayout.LabelField("No Socket or Channels are initialized", centeredLabelStyle);
                    return;
                }

                if (reorderableList == null)
                {
                    // Find the serialized property representing the list
                    reorderableList = new ReorderableList(_socketService.Channels, typeof(AbstractChannel), false, true, false, false);
                    reorderableList.elementHeight = EditorGUIUtility.singleLineHeight * 2 + 8;

                    reorderableList.drawHeaderCallback = (Rect rect) =>
                    {
                        EditorGUI.LabelField(rect,"Channels");
                    };

                    reorderableList.drawNoneElementCallback = (Rect rect) =>
                    {
                    };

                    reorderableList.drawElementBackgroundCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                    {
                        float padding = 5f;
                        float labelWidth = 50f;
                        float fieldWidth = rect.width - labelWidth - padding * 2;
                        float lineHeight = EditorGUIUtility.singleLineHeight;

                        // Adjust the rect for the "Name" label
                        var nameLabelRect = new Rect(rect.x + padding, rect.y + padding, labelWidth, lineHeight);
                        EditorGUI.LabelField(nameLabelRect, "Name:");

                        // Adjust the rect for the "Name" field
                        var nameFieldRect = new Rect(nameLabelRect.x + labelWidth, rect.y + padding, fieldWidth, lineHeight);
                        EditorGUI.LabelField(nameFieldRect, _socketService.Channels[index].ToString());

                        // Adjust the rect for the "Status" label
                        var statusLabelRect = new Rect(rect.x + padding, rect.y + padding + lineHeight + padding, labelWidth, lineHeight);
                        EditorGUI.LabelField(statusLabelRect, "Status:");

                        // Adjust the rect for the "Status" field
                        var statusFieldRect = new Rect(statusLabelRect.x + labelWidth, rect.y + padding + lineHeight + padding, fieldWidth, lineHeight);
                        GUI.enabled = false;
                        EditorGUI.EnumPopup(statusFieldRect, _socketService.Channels[index].Status);
                        GUI.enabled = true;
                    };

                    reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                    {

                    };
                }

                // Draw the ReorderableList
                reorderableList.DoLayoutList();
            }
            else
            {
                base.DrawDefaultInspector();
            }
        }
    }
}