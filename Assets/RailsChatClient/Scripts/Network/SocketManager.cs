using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;

namespace RailsChat
{
    public class SocketService : MonoBehaviour
    {
        [SerializeField]
        private string _webSocketUrl = "ws://localhost:3000/cable";
        [SerializeField]
        private string _loginUrl = "http://localhost:3000/api/users/sign_in";
        private readonly HttpClient client = new HttpClient();

        public List<ChannelSO> channels;

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
    }
}