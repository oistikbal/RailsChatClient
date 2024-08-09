using Doozy.Runtime.Signals;
using TMPro;
using UnityEngine;

namespace RailsChat
{
    public class SendButton : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField _text;

        private SignalStream _sendButtonClicked;

        private void Awake()
        {
            _sendButtonClicked = SignalsService.GetStream(StreamId.UI.SendButton);
        }

        public void OnSendButtonClicked()
        {
            _sendButtonClicked.SendSignal<string>(_text.text);
            _text.text = string.Empty;
        }
    }
}
