using UnityEngine;

namespace RailsChat 
{ 
    [CreateAssetMenu(fileName = "NewChannel", menuName = "ScriptableObjects/Channel", order = 1)]
    public class ChannelSO : ScriptableObject
    {
        public enum ChannelStatus
        {
            Default,
            Open,
            Closed,
            Failed
        }

        public string ChannelName;

        private ChannelStatus _status;

        public ChannelStatus Status { get { return _status; } }

        public void PacketReceived(Packet packet) { }

        public void Subscribe(RailsSocket socket)
        {
            socket.Send(new SubscribeCommand(this));
            _status = ChannelStatus.Open;
        }

        private void OnEnable()
        {
            _status = ChannelStatus.Default;
        }
    }
}