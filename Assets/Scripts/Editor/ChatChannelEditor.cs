using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ChatChannel))]
public class ChatChannelEditor : Editor
{
    private ChatChannel _chatChannel;
    private string _message;

    private void OnEnable()
    {
        _chatChannel = (ChatChannel)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.BeginHorizontal();

        _message = EditorGUILayout.TextField("Message", _message);

        if (GUILayout.Button("Send"))
        {
            _chatChannel.SendEditorMessage(_message);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Reconnect"))
        {
            _chatChannel.Reconnect();
        }
        EditorGUILayout.EndVertical();
    }
}
