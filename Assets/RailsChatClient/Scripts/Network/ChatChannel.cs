namespace RailsChat
{
    public class ChatChannel : AbstractChannel
    {
        public ChatChannel(RailsSocket socket) : base(socket)
        {
        }

        public override string ToString()
        {
            return "ChatChannel";
        }

        public override void OnPacketReceived(Packet packet)
        {
            base.OnPacketReceived(packet);
        }
    }
}
