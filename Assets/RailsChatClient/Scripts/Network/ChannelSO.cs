using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewChannel", menuName = "ScriptableObjects/Channel", order = 1)]
public class ChannelSO : ScriptableObject
{
    public enum ChannelStatus
    {
        None,
        Closed,
        Open
    }

    public string ChannelName;

    private ChannelStatus _status;
    public ChannelStatus Status { get {  return _status; } }

    public void PacketReceived(BasePacket packet) { }
}