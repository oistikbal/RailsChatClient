using System;
using UnityEngine;
using WebSocketSharp;

public class RailsSocket : IDisposable
{
    private WebSocket _ws;

    public RailsSocket(string address, string channel, bool emitOnPing = false)
    {
        _ws = new WebSocket(address);

        _ws.EnableRedirection = true;
        _ws.EmitOnPing = emitOnPing;

        _ws.OnOpen += (sender, e) =>
        {
            var subscription = new SubscribeCommand(channel);
            _ws.Send(JsonUtility.ToJson(subscription));
        };

        _ws.OnMessage += (sender, e) =>
        {
            Debug.Log($"[{channel}Channel]: {e.Data}");
        };

        _ws.OnError += (sender, e) =>
        {
            Debug.Log($"[{channel}Channel]: {e.Message}");
        };

        _ws.OnClose += (sender, e) =>
        {
            Debug.Log($"[{channel}Channel]: {e.Reason}");
        };

        _ws.ConnectAsync();
    }

    public void Dispose()
    {
        _ws.Close();
    }

    public void Send(Command command)
    {

    }

    public void Receive(string message)
    {
    }
}
