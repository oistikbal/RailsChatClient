using UnityEditor;
using UnityEditor.VersionControl;
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
            _railsSocket = new RailsSocket(_address, "Chat", false);
        }
    }
#endif

}
