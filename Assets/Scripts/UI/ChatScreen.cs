using Network;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ChatScreen : MonoBehaviourSingleton<ChatScreen>
    {
        public Text messages;
        public InputField inputMessage;

        protected override void Initialize()
        {
            inputMessage.onEndEdit.AddListener(OnEndEdit);

            this.gameObject.SetActive(false);

            if (!NetworkManager.Instance.onReceiveMessageHandlers.TryAdd(MessageType.Console, HandleReceiveMessage))
                NetworkManager.Instance.onReceiveMessageHandlers[MessageType.Console] += HandleReceiveMessage;
            NetworkManager.Instance.onNewClientConnected += HandleNewNewClientConnected;
        }

        private void HandleReceiveMessage(MessageType _, byte[] data)
        {
            var message = new NetConsoleMessage();
            message.TryDeserializeIntoSelf(data);
            messages.text += message.Data;
        }

        private void HandleNewNewClientConnected(int clientId)
        {
            //Broadcast "new client added"
            CatchUpClient(clientId);
        }

        private void CatchUpClient(int clientId)
        {
            NetworkManager.Instance.TrySendToClient(new NetConsoleMessage{Data = messages.text}.GetBytes(), clientId);
        }

        private void OnEndEdit(string str)
        {
            if (inputMessage.text != "")
            {
                var message = new NetConsoleMessage {Data = inputMessage.text + System.Environment.NewLine};
                if (!NetworkManager.TryGetLocalClient(out var localClient))
                {
                    Debug.LogError($"{name}: {nameof(localClient)} not found!");
                    return;
                }
                if (NetworkManager.IsServer)
                    NetworkManager.Instance
                                  .Broadcast(message.Serialized(localClient.GetNextMessageId(MessageType.Console)));
                else
                    NetworkManager.Instance.SendToServer(message.Serialized(localClient.GetNextMessageId(MessageType.Console)));

                inputMessage.ActivateInputField();
                inputMessage.Select();
                inputMessage.text = "";
            }

        }

    }
}
