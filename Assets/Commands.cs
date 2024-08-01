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
public class SubscribeCommand
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