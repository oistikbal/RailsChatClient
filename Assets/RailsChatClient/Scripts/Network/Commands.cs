using System;
using UnityEngine;

namespace RailsChat 
{ 
    [Serializable]
    public class Channel
    {
        public string channel;

        public Channel(string channel)
        {
            this.channel = channel;
        }
    }

    [Serializable]
    public class Data
    {
        public string data;

        public Data(string data)
        {
            this.data = data;
        }
    }

    [Serializable]
    public abstract class Command
    {
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
        public SubscribeCommand(ChannelSO channel) : base("subscribe", JsonUtility.ToJson(new Channel(channel.ChannelName + "Channel")))
        {
        }
    }

    [Serializable]
    public abstract class AbstractMessageCommand : Command
    {
        public string data;
        public AbstractMessageCommand(string channelName) : base("message", JsonUtility.ToJson(new Channel(channelName + "Channel")))
        {
        }
    }


    [Serializable]
    public class MessageCommand : AbstractMessageCommand
    {
        public MessageCommand(ChannelSO channel, string message) : base(channel.ChannelName)
        {
            data = JsonUtility.ToJson(new Data(message));
        }
    }
}