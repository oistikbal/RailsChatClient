using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using WebSocketSharp;

namespace RailsChat
{
    public class RailsSocket : IDisposable
    {
        private WebSocket _ws;

        private Dictionary<Type, AbstractChannel> _channels;
        private Dictionary<string, Type> _channelsMap;

        public Dictionary<Type, AbstractChannel> Channels { get { return _channels; } }


        public RailsSocket(string url, string token)
        {
            _channels = new Dictionary<Type, AbstractChannel>();
            _channelsMap = new Dictionary<string, Type>();
            _ws = new WebSocket(url);

            _ws.EnableRedirection = true;

            _ws.SetCookie(new WebSocketSharp.Net.Cookie("token", token));

            _ws.OnOpen += (sender, e) =>
            {
                Debug.Log($"[Socket Open]");
            };

            _ws.OnMessage += (sender, e) =>
            {
                HandleWebSocketMessage(e.Data);
            };

            _ws.OnError += (sender, e) =>
            {
                Debug.LogError($"[{e.Message}");
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

        public void SubscribeChannel(Type channelType)
        {
            if (_channels.ContainsKey(channelType))
            {
                Debug.LogError($"Channel {channelType.Name} already exists!");
                return;
            }
            var channel = Activator.CreateInstance(channelType, this);
            _channels.Add(channelType, (AbstractChannel)channel);
            _channelsMap.Add(channelType.Name, channelType);
        }

        void HandleWebSocketMessage(string json)
        {
            if (json.Contains("\"type\":\"ping\""))
            {
                PingPacket packet = JsonUtility.FromJson<PingPacket>(json);
                HandlePingPacket(packet);
            }
            else if (json.Contains("\"type\":\"confirm_subscription\""))
            {
                ConfirmSubscriptionPacket packet = JsonUtility.FromJson<ConfirmSubscriptionPacket>(json);
                HandleConfirmSubscriptionPacket(packet);
            }
            else if (json.Contains("\"authentication_token\""))
            {
                AuthenticationTokenPacket packet = JsonUtility.FromJson<AuthenticationTokenPacket>(json);
                HandleAuthenticationTokenPacket(packet);
            }
            else if (json.Contains("\"type\":\"welcome\""))
            {
                WelcomePacket packet = JsonUtility.FromJson<WelcomePacket>(json);
                HandleWelcomePacket(packet);
            }
            else
            {
                Debug.LogWarning("Unknown packet type." + json);
            }
        }

        private void HandleConfirmSubscriptionPacket(ConfirmSubscriptionPacket packet)
        {
            Debug.Log($"ConfirmSubscriptionPacket received. Channel: {packet.Channel}");
            var type = _channelsMap[$"{packet.Channel}Channel"];
            _channels[type].PacketReceived(packet);
        }

        private void HandleAuthenticationTokenPacket(AuthenticationTokenPacket packet)
        {
            Debug.Log($"AuthenticationTokenPacket received. Token: {packet.AuthenticationToken}");
        }

        private void HandleWelcomePacket(WelcomePacket packet)
        {
            Debug.Log($"WelcomePacket received. Message: {packet.Message}");
        }

        private void HandlePingPacket(PingPacket packet)
        {
            Debug.Log($"PingPacket received. Message: {packet.Message}");
        }
    }
}
