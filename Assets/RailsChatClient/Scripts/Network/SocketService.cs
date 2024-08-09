using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TypeReferences;
using UnityEditor;
using UnityEngine;
using WebSocketSharp;

namespace RailsChat
{
    public class SocketService : MonoSingleton<SocketService>
    {
        [Serializable]
        private class User
        {
            public string email;
            public string password;

            public User(string email, string password)
            {
                this.email = email;
                this.password = password;
            }
        }

        [Serializable]
        private class UserLogin
        {
            [SerializeField]
            private User user_login;

            public UserLogin(User user)
            {
                user_login = user;
            }
        }


        [SerializeField]
        private string _webSocketUrl = "ws://localhost:3000/cable";
        [SerializeField]
        private string _loginUrl = "http://localhost:3000/api/v1/users/sign_in";

        [Inherits(typeof(AbstractChannel))]
        [SerializeField]
        private List<TypeReference> _channelTypes;
        
        private HttpClient _httpClient;
        [SerializeField]
        private RailsSocket _railsSocket;

        public List<AbstractChannel> Channels 
        { 
            get 
            { 
                if(_railsSocket != null)
                {
                    return _railsSocket.Channels.Values.ToList<AbstractChannel>();
                }
                else
                {
                    return new List<AbstractChannel>();
                }

            } 
        }

        private void Awake()
        {
            _httpClient = new HttpClient();
        }

        private void OnDestroy()
        {
            if (_railsSocket != null)
                _railsSocket.Dispose();
        }

        public async Task<bool> LogIn(string email, string password)
        {
            string token = await Authenticate(email, password);

            if (token.IsNullOrEmpty())
                return false;

            _railsSocket = new RailsSocket(_webSocketUrl, token);


            if (!await _railsSocket.Connect())
                return false;


            foreach (var channelType in _channelTypes)
            {
                _railsSocket.SubscribeChannel(channelType);
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