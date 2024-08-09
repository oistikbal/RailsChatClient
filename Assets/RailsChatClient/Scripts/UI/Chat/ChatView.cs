using Doozy.Runtime.Signals;
using UnityEngine;

namespace RailsChat
{
    public class ChatView : MonoBehaviour
    {
        private ChatChannel _chatChannel;

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
                }
            }
        }

        private void OnChatSendButton(Signal signal)
        {
            if (signal.TryGetValue(out string input))
            {
                Debug.Log(input);
            }
        }
    }
}
