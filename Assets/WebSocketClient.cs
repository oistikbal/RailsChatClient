using System;
using System.Threading;
using UnityEngine;
using WebSocketSharp;


public class WebSocketClient : MonoBehaviour
{

    private WebSocketSharp.WebSocket ws;

    private void Start()
    {
        ws = new WebSocket("ws://localhost:3000/cable");
        ws.EnableRedirection = true;

        ws.OnOpen += (sender, e) => 
        {
            var subscription = new SubscribeCommand("Chat");
            ws.Send(JsonUtility.ToJson(subscription));
        };

        ws.OnMessage += (sender, e) => 
        {
            Debug.Log($"[WebSocket Message]: {e.Data}");
        };

        ws.OnError += (sender, e) => {
            Debug.Log($"[WebSocket Error]: {e.Message}");
        };

        ws.OnClose += (sender, e) => {
            Debug.Log($"[WebSocket Close]: {e.Reason}");
        };

        ws.ConnectAsync();
    }

    private void OnDestroy()
    {
        ws.Close();
    }
}
