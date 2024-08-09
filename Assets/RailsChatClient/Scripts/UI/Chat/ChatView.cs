using System.Collections.Generic;
using Doozy.Runtime.Signals;
using TMPro;
using UnityEngine;

namespace RailsChat
{
    public class ChatView : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _textScroll;

        private ChatChannel _chatChannel;

        private Queue<string> _messages = new Queue<string>();

        private void Start()
        {
            SignalsService.GetStream(StreamId.UI.ChannelSubscribed).OnSignal += OnChannelSubscribed;
            SignalsService.GetStream(StreamId.UI.SendButton).OnSignal += OnChatSendButton;
        }

        private void OnChannelSubscribed(Signal signal)
        {
            if (signal.TryGetValue(out AbstractChannel channel))
            {
                if (channel is ChatChannel)
                {
                    _chatChannel = (ChatChannel)channel;
                    _chatChannel.PacketReceived += OnPacketReceived;
                }
            }
        }

        private void OnChatSendButton(Signal signal)
        {
            if (signal.TryGetValue(out string input) && !string.IsNullOrEmpty(input))
            {
                _chatChannel.SendCommand(new MessageCommand(_chatChannel, input));
            }
        }

        private void OnPacketReceived(Packet packet) 
        {
            if(packet is MessagePacket messagePacket)
            {
                _messages.Enqueue($"{messagePacket.User}: {messagePacket.Data}\n");
            }
        }

        private void Update()
        {
            while (_messages.Count != 0) 
            {
                var message = _messages.Dequeue();
                _textScroll.text += message;
            }
        }
    }
}
