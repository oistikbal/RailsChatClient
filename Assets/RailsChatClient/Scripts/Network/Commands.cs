using System;
using UnityEngine;

[Serializable]
public class StringData
{
    public string data;

    public StringData(string data)
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
    public SubscribeCommand(ChannelSO channel) : base("subscribe", JsonUtility.ToJson(new StringData(channel.ChannelName + "Channel")))
    {
    }
}

[Serializable]
public abstract class AbstractMessageCommand : Command
{
    public AbstractMessageCommand(string channelName) : base("message", JsonUtility.ToJson(new StringData(channelName + "Channel")))
    {
    }
}


[Serializable]
public class MessageCommand : AbstractMessageCommand
{
    public MessageCommand(ChannelSO channel, string message) : base(channel.ChannelName)
    {
        data = JsonUtility.ToJson(new StringData(message));
    }
}