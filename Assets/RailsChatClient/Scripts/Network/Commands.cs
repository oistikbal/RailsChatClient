using System;
using UnityEngine;

namespace RailsChat
{
    [Serializable]
    public abstract class Command
    {
        [Serializable]
        protected class Channel
        {
            public string channel;

            public Channel(string channel)
            {
                this.channel = channel;
            }
        }

        [Serializable]
        protected class Data
        {
            public string data;

            public Data(string data)
            {
                this.data = data;
            }
        }


        [SerializeField]
        private string command;
        public string identifier;

        public Command(string command, string identifier)
        {
            this.command = command;
            this.identifier = identifier;
        }
    }

    [Serializable]
    public class SubscribeCommand : Command
    {
        public SubscribeCommand(AbstractChannel channel) : base("subscribe", JsonUtility.ToJson(new Channel(channel.ToString())))
        {
        }
    }

    [Serializable]
    public abstract class AbstractMessageCommand : Command
    {
        public string data;
        public AbstractMessageCommand(string channelName) : base("message", JsonUtility.ToJson(new Channel(channelName)))
        {
        }
    }


    [Serializable]
    public class MessageCommand : AbstractMessageCommand
    {
        public MessageCommand(AbstractChannel channel, string message) : base(channel.ToString())
        {
            data = JsonUtility.ToJson(new MessagePacket(message));
        }
    }
}