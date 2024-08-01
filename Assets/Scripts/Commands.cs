using System;
using UnityEngine;

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
    public string data;

    public Command(string command, string identifier)
    {
        this.command = command;
        this.identifier = identifier;
    }
}

[Serializable]
public class SubscribeCommand : Command
{
    public SubscribeCommand(RailsSocket socket) : base("subscribe", JsonUtility.ToJson(new Channel(socket.ChannelName + "Channel")))
    {
    }
}

[Serializable]
public abstract class AbstractMessageCommand : Command
{
    public AbstractMessageCommand(string channelName) : base("message", JsonUtility.ToJson(new Channel(channelName + "Channel")))
    {
    }
}


[Serializable]
public class MessageCommand : AbstractMessageCommand
{
    public MessageCommand(RailsSocket socket, string message) : base(socket.ChannelName)
    {
        data = JsonUtility.ToJson(new Data(message));
    }
}