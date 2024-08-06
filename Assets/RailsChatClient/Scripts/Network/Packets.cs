using System;
using UnityEngine;

namespace RailsChat 
{ 
    [Serializable]
    public class User
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
    public class UserLogin
    {
        [SerializeField]
        private User user_login;

        public UserLogin(User user)
        {
            user_login = user;
        }
    }

    [Serializable]
    public abstract class Packet
    {
    }

    [Serializable]
    public class ConfirmSubcriptionPacket : Packet
    {
        private string message;

        public string Message { get { return message; } }
    }

    [Serializable]
    public class AuthenticationTokenPacket : Packet
    {
        [SerializeField]
        private string authentication_token;

        public string AuthenticationToken { get { return authentication_token; } }

        public AuthenticationTokenPacket(string authenticationToken)
        {
            authentication_token = authenticationToken;
        }
    }
}