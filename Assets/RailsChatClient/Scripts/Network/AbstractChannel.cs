using System;
using Doozy.Runtime.Signals;
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

        private SignalStream _channelSubscribed;
        public Action<Packet> PacketReceived;

        public AbstractChannel(RailsSocket socket)
        {
            _socket = socket;
            _channelSubscribed = SignalsService.GetStream(StreamId.UI.ChannelSubscribed);
            Subscribe();
        }

        protected virtual void Subscribe()
        {
            _status = ChannelStatus.UnConfirmed;
            _socket.Send(new SubscribeCommand(this));
            _channelSubscribed.SendSignal(this);
        }

        public virtual void SendCommand(Command command)
        {
            _socket.Send(command);
        }

        public virtual void OnPacketReceived(Packet packet)
        {
            PacketReceived?.Invoke(packet);
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
