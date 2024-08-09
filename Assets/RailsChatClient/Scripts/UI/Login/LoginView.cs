using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Containers;
using UnityEngine;

namespace RailsChat
{

    public class LoginView : MonoBehaviour
    {

        private SignalStream _loginSuccesfull;

        private void Awake()
        {
            SignalsService.GetStream(StreamId.UI.LoginButton).OnSignal += OnLoginButtonClicked;
            _loginSuccesfull = SignalsService.GetStream(StreamId.UI.LoginSuccesfull);
        }


        private async void OnLoginButtonClicked(Signal signal)
        {

            if (signal.TryGetValue(out LoginData loginData))
            {
                UIPopup.ClearQueue();
                var popup = UIPopup.Get("PopupBlock");
                popup.SetTexts("Logging In");
                popup.Show();

                var status = await SocketService.instance.LogIn(loginData.Email, loginData.Password);
                if (status)
                {
                    popup.Hide();
                    _loginSuccesfull.SendSignal();
                }

            }
        }

    }
}
