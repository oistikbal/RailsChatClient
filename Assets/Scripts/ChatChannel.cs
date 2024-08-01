using UnityEngine;

public class ChatChannel : MonoBehaviour
{
    [SerializeField]
    private string _address = "ws://localhost:3000/cable";
    private RailsSocket _railsSocket;

    private void Awake()
    {
        _railsSocket = new RailsSocket(_address, "Chat", false);
    }

    private void OnDestroy()
    {
        _railsSocket.Dispose();
    }
}
