using System;
using UnityEngine;

namespace RailsChat
{
    [Serializable]
    public abstract class Packet : ISerializationCallbackReceiver
    {
        public virtual void OnAfterDeserialize()
        {
        }

        public virtual void OnBeforeSerialize()
        {
        }
    }

    [Serializable]
    public class ConfirmSubscriptionPacket : Packet
    {
        private class ChannelData
        {
            public string channel;
        }

        [SerializeField]
        private string identifier;
        [SerializeField]
        private string type = "confirm_subscription";

        private string channel;
        public string Channel { get { return channel; } }
        public string Type { get { return type; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            var channelData = JsonUtility.FromJson<ChannelData>(identifier);
            channel = channelData.channel.Replace("Channel", string.Empty);
        }
    }

    [Serializable]
    public class AuthenticationTokenPacket : Packet
    {
        [SerializeField]
        private string authentication_token;

        public string AuthenticationToken { get { return authentication_token; } }
    }

    [Serializable]
    public class PingPacket : Packet
    {
        [SerializeField]
        private string type;
        [SerializeField]
        private string message;

        public string Message { get { return message; } }
    }

    [SerializeField]
    public class WelcomePacket : Packet
    {
        [SerializeField]
        private string type;

        public string Message { get { return type; } }
    }
}