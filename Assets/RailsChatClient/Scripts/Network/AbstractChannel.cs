using System;
using System.Collections;
using System.Collections.Generic;
using TypeReferences;
using UnityEngine;

namespace RailsChat
{
    public abstract class AbstractChannel
    {
        public enum ChannelStatus
        {
            Default,
            UnConfirmed,
            Open,
            Closed,
            Failed
        }

        protected RailsSocket _socket;
        protected ChannelStatus _status;

        public ChannelStatus Status { get { return _status; } }

        public AbstractChannel(RailsSocket socket)
        {
            _socket = socket;
            Subscribe();
        }

        protected void Subscribe()
        {
            _status = ChannelStatus.UnConfirmed;
            _socket.Send(new SubscribeCommand(this));
        }

        public virtual void PacketReceived(Packet packet)
        {
            if (packet is ConfirmSubscriptionPacket) 
            {
                _status = ChannelStatus.Open;
            }
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
