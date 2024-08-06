using System;
using System.Threading.Tasks;
using UnityEngine;
using WebSocketSharp;

namespace RailsChat
{
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
                Debug.Log($"[Socket Open]");
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
        }


        public async Task<bool> Connect()
        {
            try
            {
                //Need a fix, can't await tasks on another thread.
                await UnityMainThreadDispatcher.Instance().EnqueueAsync(() => { _ws.Connect(); });
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception in ConnectAsync: {ex.Message}");
                return false;
            }
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
            channelSO.Subscribe(this);
        }
    }
}
