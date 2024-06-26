﻿using Model.Network;
using Model.Network.Serialized.Impl;
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

            if (!NetworkManager.Instance.onReceiveMessageHandlers.TryAdd(Model.Network.PacketType.Console, HandleReceiveMessage))
                NetworkManager.Instance.onReceiveMessageHandlers[Model.Network.PacketType.Console] += HandleReceiveMessage;
            NetworkManager.Instance.onNewClientConnected += HandleNewNewClientConnected;
        }

        private void OnEnable()
        {
            //TODO: Change to have a client
            if(NetworkManager.LocalClient == null)
            {
                inputMessage.interactable = false;
            }
        }
        private void HandleReceiveMessage(Model.Network.PacketType _, byte[] data)
        {
            var message = new SerializedConsolePacket();
            message.TryDeserializeIntoSelf(data);
            var clientId = Packet.ReadClientId(data);
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
            NetworkManager.Instance.TrySendToClient(new SerializedConsolePacket{Data = messages.text}.GetBytes(new Header(NetworkManager.LocalClient.ID)), clientId);
        }

        private void OnEndEdit(string str)
        {
            if (inputMessage.text != "")
            {
                var messageHeader = new Header(NetworkManager.LocalClient.ID);
                var message = new SerializedConsolePacket {Data = inputMessage.text + System.Environment.NewLine};
                if (NetworkManager.IsServer)
                {
                    NetworkManager.Instance
                        .Broadcast(message.Serialized(NetworkManager.LocalClient.GetNextMessageId(Model.Network.PacketType.Console), messageHeader));
                }
                else
                    NetworkManager.Instance.SendToServer(message.Serialized(NetworkManager.LocalClient.GetNextMessageId(Model.Network.PacketType.Console), messageHeader));

                inputMessage.ActivateInputField();
                inputMessage.Select();
                inputMessage.text = "";
            }

        }

    }
}
