using System;
using System.Net;
using UnityEngine.UI;

public class ChatScreen : MonoBehaviourSingleton<ChatScreen>
{
    public Text messages;
    public InputField inputMessage;

    protected override void Initialize()
    {
        inputMessage.onEndEdit.AddListener(OnEndEdit);

        this.gameObject.SetActive(false);

        NetworkManager.Instance.OnReceiveEvent += OnReceiveDataEvent;
        NetworkManager.Instance.onNewClientConnected += HandleNewNewClientConnected;
    }

    private void HandleNewNewClientConnected(int clientId)
    {
        //Broadcast "new client added"
        CatchUpClient(clientId);
    }

    private void CatchUpClient(int clientId)
    {
        NetworkManager.Instance.SendToClient(new NetConsoleMessage(messages.text).Serialize(), clientId);
    }

    void OnReceiveDataEvent(byte[] data, IPEndPoint ep)
    {
        var messageType = (MessageType)BitConverter.ToInt32(data, 0);
        switch (messageType)
        {
            case MessageType.Console:
                var message = new NetConsoleMessage("");
                messages.text += message.Deserialize(data);
                break;
        }

        if (NetworkManager.IsServer)
        {
            NetworkManager.Instance.Broadcast(data);
        }
    }

    void OnEndEdit(string str)
    {
        if (inputMessage.text != "")
        {
            var message = new NetConsoleMessage(inputMessage.text + System.Environment.NewLine);
            if (NetworkManager.IsServer)
            {
                NetworkManager.Instance.Broadcast(message.Serialize());
                messages.text += message.data;
            }
            else
            {
                NetworkManager.Instance.SendToServer(message.Serialize());
            }

            inputMessage.ActivateInputField();
            inputMessage.Select();
            inputMessage.text = "";
        }

    }

}
