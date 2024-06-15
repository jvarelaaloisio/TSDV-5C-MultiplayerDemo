using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Model.Network;
using Model.Network.Connections;
using Model.Network.Impl;
using Model.Network.Serialized;
using Model.Network.Serialized.Impl;
using UnityEngine;

namespace Network
{
    public delegate void HandleMessageDelegate(Model.Network.PacketType packetTypeObs, byte[] data);
    public class NetworkManager : MonoBehaviourSingleton<NetworkManager>, IReceiveNetData
    {
        public static IPEndPoint LocalEndPoint { get; private set; }
        //TODO: To client
        public static Client LocalClient { get; private set; }
        //TODO: To client
        public IPAddress serverIp { get; private set; }

        public int port { get; private set; }

        public static bool IsServer{ get; private set; }

        public int TimeOut = 30;
    
        private UdpConnection _connection;

        public Dictionary<int, Client> ClientsById { get; } = new();
        private readonly Dictionary<int, IPEndPoint> ipsById = new();

        public readonly Dictionary<Model.Network.PacketType, HandleMessageDelegate> onReceiveMessageHandlers = new();
        public Action<byte[], IPEndPoint> OnReceiveEvent;

        /// <summary>
        /// When a new client connects to the server
        /// </summary>
        public event Action<int> onNewClientConnected = delegate { };

        public event Action<int> onClientDisconnected = delegate { };

        /// <summary>
        /// When started client is accepted by the server
        /// </summary>
        public event Action<int> onConnectionSuccessful = delegate { };
        public event Action<Model.Network.Impl.HandshakeResponseCodes> onConnectionError = delegate { };

        private void Update()
        {
            // Flush the data in main thread
            if (_connection != null)
                _connection.FlushReceiveData();
        }

        public void StartServer(int port)
        {
            IsServer = true;
            this.port = port;
            _connection = new UdpConnection(port, this);
            _connection.OnException += HandleUpdConnectionException;
            LocalEndPoint = (IPEndPoint)_connection.LocalEndPoint;
        }

        public void StartHost(int port, string nickname)
        {
            StartServer(port);
            TryAddClientAsServer(LocalEndPoint, out var localClient, nickname);
            LocalClient = localClient;
        }

        public void StartClient(IPAddress ip, int port, string nickName)
        {
            IsServer = false;

            this.port = port;
            this.serverIp = ip;

            _connection = new UdpConnection(ip, port, this);
            _connection.OnException += HandleUpdConnectionException;
            LocalEndPoint = (IPEndPoint)_connection.LocalEndPoint;

            var handShake = new SerializedHandshakeRequestPacket {Payload = nickName };
            LocalClient = new Client(Client.InvalidId, Time.time, nickName);
            SendToServer(handShake.GetBytes(new Model.Network.Header(Client.InvalidId)));
        }

        //TODO: Check if this should be calling TryAddNewClient or do something to merge them both
        public bool TryAddClient(int clientId, string nickname)
        {
            var newClient = new Client(clientId, Time.realtimeSinceStartup, nickname);
            return ClientsById.TryAdd(clientId, newClient);
        }

        /// <summary>
        /// This method should only be called as server
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="newClient"></param>
        /// <param name="nickname"></param>
        /// <returns></returns>
        public bool TryAddClientAsServer(IPEndPoint ip, out Client newClient, string nickname)
        {
            var alreadyHadClient = ipsById.ContainsValue(ip);
            if (alreadyHadClient)
            {
                Debug.LogWarning($"{name}: Client {ip.Address} ({ip.Port}) was already added!");

                newClient = default;
                return false;
            }
        
            var newId = ClientsById.Any()
                            ? ClientsById.Keys.Max() + 1
                            : 0;

            newClient = new Client(newId, Time.realtimeSinceStartup, nickname);
            ClientsById.Add(newId, newClient);
            ipsById.TryAdd(newId, ip);
            Debug.Log($"Added client: {ip.Address} ({ip.Port}) → (<color=#00f009>{newClient.ID}</color>): <color=#00f0f0>{newClient.Nickname}</color>");
            return true;
        }

        private void RemoveClient(int id)
        {
            if (ClientsById.ContainsKey(id))
            {
                Debug.Log("Removing client: " + id);
                ClientsById.Remove(id);
            }
        }

        public void OnReceiveData(byte[] data, IPEndPoint ip)
        {
            var messageType_OBS = Packet.ReadType(data);
            var flags_OBS = Packet.ReadFlags(data);
            if (!Header.TryParse(data, out var header))
            {
                Debug.LogError($"{name}: Couldn't parse {nameof(header)}!");
                return;
            }

            //TODO: Move to Initialize
            var packetProvider = new PacketProvider();
            packetProvider.Initialize();
            if (!packetProvider.TryGetType(header.TypeGroup, header.TypeId, out var type))
            {
                Debug.LogError($"{name}: Couldn't retrieve {nameof(type)} from {nameof(packetProvider)}!" +
                               $"\nHeader was: {header.TypeGroup} ({header.TypeId})");
            }
            
            switch (messageType_OBS)
            {
                case Model.Network.PacketType.Invalid:
                {
                    Debug.LogError($"{name}: Message received was invalid!");
                    break;
                }
                case Model.Network.PacketType.HandshakeRequest when IsServer:
                {
                    HandleHandshakeRequestAsServer(ip, data);
                    break;
                }
                case Model.Network.PacketType.HandshakeResponse:
                {
                    //TODO: To server
                    if (IsServer)
                    {
                        Debug.LogError($"{name}: Server received a {nameof(Model.Network.PacketType.HandshakeResponse)} from {ip.Address}({ip.Port})." +
                                       $"\nThis is not allowed!");
                        break;
                    }
                    HandleHandshakeResponseAsClient(data, ip);
                    break;
                }
                //TODO: To client
                case Model.Network.PacketType.ClientListUpdate when !IsServer:
                {
                    var message = new ClientListUpdatePacket();
                    message.TryDeserializeIntoSelf(data);
                    foreach (var (clientId, nickname)
                             in message
                                .Payload
                                .Where(clientData => !ClientsById.ContainsKey(clientData.clientId)))
                    {
                        TryAddClient(clientId, nickname);
                    }
                    break;
                }
                default:
                {
                    //TODO: To server
                    if (IsServer && !ipsById.ContainsValue(ip))
                    {
                        Debug.LogError($"{ip.Address}({ip.Port}) is not a subscribed client");
                        break;
                    }

                    if (flags_OBS.HasFlag(Model.Network.PacketFlags.IsSerialized))
                    {
                        var clientId = Packet.ReadClientId(data);
                        if(clientId == Client.InvalidId)
                        {
                            Debug.LogError($"{name}: {nameof(clientId)} is invalid!");
                            break;
                        }
                        if(!ClientsById.TryGetValue(clientId, out var client))
                        {
                            Debug.LogError($"{name}: Client was not added!");
                            break;
                        }
                        var messageCount = client.GetMessageId(messageType_OBS);
                        var messageId = SerializedPacket.ReadMessageId(data);
                        var isNewerThanLatestProcessedMessage = messageCount < messageId;
                        if(isNewerThanLatestProcessedMessage)
                        {
                            client.SetMessageId(messageType_OBS, messageCount);
                            //TODO: To Server
                            if (IsServer)
                                Broadcast(data);
                            else
                                ProcessMessage(data, messageType_OBS);
                            OnReceiveEvent?.Invoke(data, ip);
                        }
                        else
                            Debug.Log($"{name}: Message id ({messageId}) is older than the last one processed ({messageCount})");
                    }
                    else
                    {
                        //TODO: To Server
                        if (IsServer)
                            Broadcast(data);
                        else
                            ProcessMessage(data, messageType_OBS);
                        OnReceiveEvent?.Invoke(data, ip);
                    }
                    break;
                }
            }
        }

        private void ProcessMessage(byte[] data, Model.Network.PacketType packetTypeObs)
        {
            if(onReceiveMessageHandlers.TryGetValue(packetTypeObs, out var handler))
                handler?.Invoke(packetTypeObs, data);
        }

        private void HandleHandshakeRequestAsServer(IPEndPoint ip, byte[] data)
        {
            var request = new SerializedHandshakeRequestPacket();
            if (!request.TryDeserializeIntoSelf(data))
            {
                Debug.LogError($"{name}: Couldn't serialize handshake request from ip {ip.Address}({ip.Port})");
                return;
            }

            if (ClientsById.Values.Any(client => client.Nickname == request.Payload))
            {
                Debug.LogWarning($"{name}: Rejecting client with ip {ip.Address}({ip.Port}).\tReason: {Model.Network.Impl.HandshakeResponseCodes.DuplicatedNickname}");
                var response = new HandshakeResponsePacket {Payload = (Model.Network.Impl.HandshakeResponseCodes.DuplicatedNickname, Client.InvalidId)};
                var refusedConnectionData = response.GetBytes(new Model.Network.Header(LocalClient.ID));
                _connection.Send(refusedConnectionData, ip);
            }
            else if(TryAddClientAsServer(ip, out var client, request.Payload))
            {
                var response = new HandshakeResponsePacket {Payload = (Model.Network.Impl.HandshakeResponseCodes.Success, client.ID)};
                var acceptedConnectionData = response.GetBytes(new Model.Network.Header(LocalClient.ID));
                _connection.Send(acceptedConnectionData, ip);
                SendUpdatedClientList();
                onNewClientConnected(client.ID);
            }
            else
                Debug.LogError($"{name}: Couldn't add new client!" +
                               $"\nip: {ip.Address} ({ip.Port})");
        }

        private void SendUpdatedClientList()
        {
            var message = new ClientListUpdatePacket { Payload = ClientsById.Select(pair => (pair.Value.ID, pair.Value.Nickname)).ToArray() };
            Broadcast(message.GetBytes(new Model.Network.Header(LocalClient.ID)));
        }

        private void HandleHandshakeResponseAsClient(byte[] data, IPEndPoint ip)
        {
            var handshakeResponse = new HandshakeResponsePacket();
            if (handshakeResponse.TryDeserializeIntoSelf(data))
            {
                if (handshakeResponse.Payload.result is Model.Network.Impl.HandshakeResponseCodes.Success)
                {
                    var myClientId = handshakeResponse.Payload.clientId;
                    if (LocalClient == null)
                        Debug.LogError($"{name}: {nameof(LocalClient)} was not initialized!");
                    else
                        LocalClient.ID = myClientId;
                    onConnectionSuccessful(myClientId);
                }
                else
                    onConnectionError(handshakeResponse.Payload.result);
            }
        }

        public bool TrySendToClient(byte[] data, int clientId)
        {
            if (!ipsById.TryGetValue(clientId, out var ip))
                return false;
            _connection.Send(data, ip);
            return true;
        }

        public void SendToServer(byte[] data)
        {
            _connection.Send(data);
        }

        public void Broadcast(byte[] data)
        {
            foreach (var ip in ipsById.Values)
            {
                var currentIpIsMine = Equals(ip, LocalEndPoint);
                if (currentIpIsMine)
                {
                    var messageType = Packet.ReadType(data);
                    ProcessMessage(data, messageType);
                }
                else
                    _connection.Send(data, ip);
            }
        }

        private void HandleUpdConnectionException(Exception e)
        {
            Debug.LogError($"{name}Exception thrown: {e.Message}");
        }
    }
}