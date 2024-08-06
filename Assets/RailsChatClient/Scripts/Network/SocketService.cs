using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WebSocketSharp;

namespace RailsChat
{
    public class SocketService : MonoSingleton<SocketService>
    {
        [SerializeField]
        private string _webSocketUrl = "ws://localhost:3000/cable";
        [SerializeField]
        private string _loginUrl = "http://localhost:3000/api/users/sign_in";
        [SerializeField]
        private List<ChannelSO> _channels;


        private HttpClient _httpClient;
        private RailsSocket _railsSocket;

        private void Awake()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> LogIn(string email, string password)
        {
            string token = await Authenticate(email, password);

            if (token.IsNullOrEmpty())
                return false;

            _railsSocket = new RailsSocket(_webSocketUrl, token);


            if (!await _railsSocket.Connect())
                return false;


            foreach (var channel in _channels)
            {
                _railsSocket.SubscribeChannel(channel);
            }

            return true;
        }

        private async Task<string> Authenticate(string email, string password)
        {
            var user = new User(email, password);
            var content = new StringContent(JsonUtility.ToJson(new UserLogin(new User(email, password))), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_loginUrl, content);

            string responseString;
            AuthenticationTokenPacket responseObject;
            try
            {
                responseString = await response.Content.ReadAsStringAsync();
                responseObject = JsonUtility.FromJson<AuthenticationTokenPacket>(responseString);
            }
            catch
            {
                return null;
            }

            return responseObject.AuthenticationToken;
        }
    }
}