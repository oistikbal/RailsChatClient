using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TypeReferences;
using UnityEngine;
using WebSocketSharp;

namespace RailsChat
{
    public class RailsSocket : IDisposable
    {
        private WebSocket _ws;

        private Dictionary<string, AbstractChannel> _channels;

        public Dictionary<string, AbstractChannel> Channels { get { return _channels; } }


        public RailsSocket(string url, string token, List<TypeReference> typeReferences)
        {
            _channels = new Dictionary<string, AbstractChannel>();
            _ws = new WebSocket($"{url}?token={token}");

            _ws.EnableRedirection = true;


            _ws.OnOpen += (sender, e) =>
            {
                foreach (var typeReference in typeReferences) 
                {
                    SubscribeChannel(typeReference.Type);
                }
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

        private void SubscribeChannel(Type channelType)
        {
            if (_channels.ContainsKey(channelType.Name))
            {
                Debug.LogError($"Channel {channelType.Name} already exists!");
                return;
            }
            var channel = Activator.CreateInstance(channelType, this);
            _channels.Add(channelType.Name, (AbstractChannel)channel);
        }

        public T GetChannel<T>() where T : AbstractChannel
        {
            if (_channels.TryGetValue(typeof(T).Name, out var channel))
                return (T)channel;
            else return null;
        }

        void HandleWebSocketMessage(string json)
        {
            try
            {
                if (json.Contains("\"type\":\"ping\""))
                {
                    //PingPacket packet = JsonUtility.FromJson<PingPacket>(json);
                }
                else if (json.Contains("\"type\":\"confirm_subscription\""))
                {
                    ConfirmSubscriptionPacket packet = JsonUtility.FromJson<ConfirmSubscriptionPacket>(json);
                    _channels[packet.Channel].OnPacketReceived(packet);
                }
                else if (json.Contains("\"type\":\"welcome\""))
                {
                    WelcomePacket packet = JsonUtility.FromJson<WelcomePacket>(json);
                }
                else if (!string.IsNullOrEmpty(json))
                {
                    IdentifierPacket identifierPacket = JsonUtility.FromJson<IdentifierPacket>(json);
                    if (identifierPacket.Message.Contains("\"type\":\"message\""))
                    {
                        _channels[identifierPacket.Channel].OnPacketReceived(JsonUtility.FromJson<MessagePacket>(identifierPacket.Message));
                    }

                }
                else
                {
                    Debug.LogWarning("Unknown packet type: " + json);
                }
            }
            catch (Exception ex) 
            {
                Debug.LogException(ex); 
            }

        }
    }
}
