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

public abstract class Command
{

}

[Serializable]
public class SubscribeCommand : Command
{
    [SerializeField]
    private string command;
    public string identifier;

    public SubscribeCommand(string channel)
    {
        command = "subscribe";
        identifier = JsonUtility.ToJson(new Channel(channel + "Channel"));
    }
}

[Serializable]
public class MessageCommand : Command
{
    [SerializeField]
    private string command;
    public string identifier;

    public MessageCommand(string channel)
    {
        command = "message";
        identifier = JsonUtility.ToJson(new Channel(channel + "Channel"));
    }
}