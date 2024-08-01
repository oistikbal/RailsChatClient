using System;

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
public abstract class BasePacket
{
    public string Type { get; set; }
}

[Serializable]
public class ConfirmSubcriptionPacket : BasePacket
{
    public string message;
}