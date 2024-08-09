using System;
using UnityEngine;

namespace RailsChat
{
    [Serializable]
    public abstract class Packet : ISerializationCallbackReceiver
    {
        protected class ChannelData
        {
            [SerializeField]
            private string channel;

            public string Channel { get { return channel; } }
        }

        public virtual void OnAfterDeserialize()
        {
        }

        public virtual void OnBeforeSerialize()
        {
        }
    }

    #region Unordinary Packets

    [Serializable]
    public class ConfirmSubscriptionPacket : Packet
    {
        [SerializeField]
        private string identifier;
        [SerializeField]
        private string type = "confirm_subscription";

        public string Channel { get; private set; }
        public string Type { get { return type; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            Channel = JsonUtility.FromJson<ChannelData>(identifier).Channel;
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
        private string type = "ping";
        [SerializeField]
        private string message;

        public string Message { get { return message; } }
    }

    [SerializeField]
    public class WelcomePacket : Packet
    {
        [SerializeField]
        private string type = "welcome";

        public string Message { get { return type; } }
    }

    #endregion

    public class IdentifierPacket : Packet
    {
        [SerializeField]
        private string identifier;
        [SerializeField]
        private string message;

        public string Identifier { get { return identifier; } }
        public string Message { get { return message; } }

        public string Channel { get; private set; }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            Channel = JsonUtility.FromJson<ChannelData>(identifier).Channel;
        }
    }



    [SerializeField]
    public class MessagePacket : Packet
    {
        [SerializeField]
        private string type = "message";
        [SerializeField]
        private string data;
        [SerializeField]
        private string user;

        public string Data { get { return data; } }
        public string User { get { return user; } }

        public MessagePacket() { }

        public MessagePacket(string data)
        {
            this.data = data;
        }
    }
}