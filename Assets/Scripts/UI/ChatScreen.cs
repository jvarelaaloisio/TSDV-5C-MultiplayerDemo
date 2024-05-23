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
            var message = new SerializedNetConsoleMessage();
            message.TryDeserializeIntoSelf(data);
            var clientId = NetMessage.ReadClientId(data);
            if (!NetworkManager.Instance.ClientsById.TryGetValue(clientId, out var client))
                Debug.LogError($"{name}: client id ({clientId}) was not found!" +
                               $"\nMessage content was {message.Data}");
            else
                messages.text += $"{client.Nickname}: {message.Data}";
        }

        private void HandleNewNewClientConnected(int clientId)
        {
            //Broadcast "new client added"
            CatchUpClient(clientId);
        }

        private void CatchUpClient(int clientId)
        {
            NetworkManager.Instance.TrySendToClient(new SerializedNetConsoleMessage{Data = messages.text}.GetBytes(new MessageHeader(NetworkManager.LocalClient.ID)), clientId);
        }

        private void OnEndEdit(string str)
        {
            if (inputMessage.text != "")
            {
                var messageHeader = new MessageHeader(NetworkManager.LocalClient.ID);
                var message = new SerializedNetConsoleMessage {Data = inputMessage.text + System.Environment.NewLine};
                if (NetworkManager.IsServer)
                {
                    NetworkManager.Instance
                        .Broadcast(message.Serialized(NetworkManager.LocalClient.GetNextMessageId(MessageType.Console), messageHeader));
                }
                else
                    NetworkManager.Instance.SendToServer(message.Serialized(NetworkManager.LocalClient.GetNextMessageId(MessageType.Console), messageHeader));

                inputMessage.ActivateInputField();
                inputMessage.Select();
                inputMessage.text = "";
            }

        }

    }
}
