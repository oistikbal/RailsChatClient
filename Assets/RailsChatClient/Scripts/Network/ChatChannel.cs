using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class ChatChannel : MonoBehaviour
{
    [SerializeField]
    private string _webSocketUrl = "ws://localhost:3000/cable";
    [SerializeField]
    private string _loginUrl = "http://localhost:3000/api/users/sign_in";
    [SerializeField]
    private string _email = "a@a.com";
    [SerializeField]
    private string _password = "aaaaaa";
    private RailsSocket _railsSocket;
    private string _token;

    private static readonly HttpClient client = new HttpClient();

    private async void Awake()
    {
        _token = await Authenticate(_email, _password);

        _railsSocket = new RailsSocket(_webSocketUrl, "Chat", _token);
    }

    private async Task<string> Authenticate(string email, string password)
    {
        var user = new User(email, password);
        var content = new StringContent($"{{\"user_login\": {JsonUtility.ToJson(user)}}}", Encoding.UTF8, "application/json");
        var response = await client.PostAsync(_loginUrl, content);
        response.EnsureSuccessStatusCode(); 

        var responseString = await response.Content.ReadAsStringAsync();
        var responseObject = JsonUtility.FromJson<string>(responseString);
        return responseObject;
    }

    private void OnDestroy()
    {
        _railsSocket.Dispose();
    }

#if UNITY_EDITOR
    public void SendEditorMessage(string message)
    {
        if (EditorApplication.isPlaying)
        {
            _railsSocket.Send(new MessageCommand(_railsSocket, message));
        }
    }

    public void Reconnect()
    {
        if (EditorApplication.isPlaying)
        {
            _railsSocket.Dispose();
            _token = Authenticate(_email, _password).Result;
            _railsSocket = new RailsSocket(_webSocketUrl, "Chat", _token);
        }
    }
#endif

}
