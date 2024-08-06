using Doozy.Runtime.Signals;
using TMPro;
using UnityEngine;

namespace RailsChat
{
    public struct LoginData
    {
        public string Email;
        public string Password;

        public LoginData(string email, string password)
        {
            Email = email;
            Password = password;
        }
    }

    public class LoginButton : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField _email;
        [SerializeField]
        private TMP_InputField _password;

        private SignalStream _loginButtonClicked;

        private void Awake()
        {
            _loginButtonClicked = SignalsService.GetStream(StreamId.UI.LoginButtonClicked);
        }

        public void OnLoginButtonClicked()
        {
            _loginButtonClicked.SendSignal<LoginData>(new LoginData(_email.text, _password.text));
        }
    }
}