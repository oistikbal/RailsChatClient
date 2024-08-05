using System.Collections;
using System.Collections.Generic;
using Doozy.Runtime.Signals;
using TMPro;
using UnityEngine;

public class LoginButton : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _email;
    [SerializeField]
    private TextMeshProUGUI _password;

    private SignalStream _loginSuccesfullStream;

    private void Awake()
    {
        _loginSuccesfullStream = SignalsService.GetStream(StreamId.UI.LoginSuccesfull);
    }

    public void OnLoginButtonClicked()
    {
        _loginSuccesfullStream.SendSignal();
    }
}
