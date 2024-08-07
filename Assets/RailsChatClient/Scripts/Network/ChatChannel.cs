using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RailsChat
{
    public class ChatChannel : AbstractChannel
    {
        public ChatChannel(RailsSocket socket) : base(socket)
        {
        }

        public override string ToString()
        {
            return "Chat";
        }

        public override void PacketReceived(Packet packet)
        {
            base.PacketReceived(packet);
        }
    }
}
