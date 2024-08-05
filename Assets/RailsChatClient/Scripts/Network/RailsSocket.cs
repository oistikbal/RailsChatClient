using System;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WebSocketSharp;

public class RailsSocket : IDisposable
{
    private WebSocket _ws;

    public RailsSocket(string url, string token)
    {
        _ws = new WebSocket(url);
        _ws.EnableRedirection = true;

        _ws.SetCookie(new WebSocketSharp.Net.Cookie("token", token));

        _ws.OnOpen += (sender, e) =>
        {
            Debug.Log($"[{e}");
        };

        _ws.OnMessage += (sender, e) =>
        {
            Debug.Log($"[{e.Data}");
        };

        _ws.OnError += (sender, e) =>
        {
            Debug.Log($"[{e.Message}");
        };

        _ws.OnClose += (sender, e) =>
        {
            Debug.Log($"[Closed, Reason: {e.Reason}");
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

    public void SubscribeChannel(ChannelSO channelSO)
    {
        var subscription = new SubscribeCommand(channelSO);
        _ws.Send(JsonUtility.ToJson(subscription));
    }
}
