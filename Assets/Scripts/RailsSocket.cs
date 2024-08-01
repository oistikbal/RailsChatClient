using System;
using UnityEngine;
using WebSocketSharp;

public class RailsSocket : IDisposable
{
    private WebSocket _ws;
    public string ChannelName { get; private set; }

    public RailsSocket(string address, string channelName, bool emitOnPing = false)
    {
        _ws = new WebSocket(address);
        ChannelName = channelName;
        _ws.EnableRedirection = true;
        _ws.EmitOnPing = emitOnPing;

        _ws.OnOpen += (sender, e) =>
        {
            var subscription = new SubscribeCommand(this);
            _ws.Send(JsonUtility.ToJson(subscription));
        };

        _ws.OnMessage += (sender, e) =>
        {
            Debug.Log($"[{channelName}Channel]: {e.Data}");
        };

        _ws.OnError += (sender, e) =>
        {
            Debug.Log($"[{channelName}Channel]: {e.Message}");
        };

        _ws.OnClose += (sender, e) =>
        {
            Debug.Log($"[{channelName}Channel]: Closed, Reason: {e.Reason}");
        };

        _ws.ConnectAsync();
    }

    public void Dispose()
    {
        _ws.Close();
    }

    public void Send(Command command)
    {
        _ws.Send(JsonUtility.ToJson(command));
    }

    public void Receive(string message)
    {
    }
}
