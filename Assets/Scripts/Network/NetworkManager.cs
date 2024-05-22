using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Network;
using UnityEngine;

public delegate void HandleMessageDelegate(MessageType messageType, byte[] data);
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
    
    private UdpConnection connection;

    [Obsolete]
    private readonly Dictionary<IPEndPoint, Client> clients_OLD = new ();
    private readonly Dictionary<int, Client> clientsById = new ();
    private readonly Dictionary<int, IPEndPoint> ipsById = new();

    public readonly Dictionary<MessageType, HandleMessageDelegate> onReceiveMessageHandlers = new();
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
    public event Action<HandshakeResponseCodes> onConnectionError = delegate { };

    private void Update()
    {
        // Flush the data in main thread
        if (connection != null)
            connection.FlushReceiveData();
    }

    public void StartServer(int port)
    {
        IsServer = true;
        this.port = port;
        connection = new UdpConnection(port, this);
        LocalEndPoint = (IPEndPoint)connection.LocalEndPoint;
    }
    
    public void StartHost(int port, string nickname)
    {
        StartServer(port);
        TryAddClientAsServer(LocalEndPoint, out _, nickname);
    }

    public void StartClient(IPAddress ip, int port, string nickName)
    {
        IsServer = false;

        this.port = port;
        this.serverIp = ip;

        connection = new UdpConnection(ip, port, this);
        LocalEndPoint = (IPEndPoint)connection.LocalEndPoint;

        var handShake = new NetHandshakeRequest{Data = nickName };
        LocalClient = new Client(-1, Time.time, nickName);
        SendToServer(handShake.GetBytes());
    }

    //TODO: Check if this should be calling TryAddNewClient or do something to merge them both
    public bool TryAddClient(int clientId, string nickname)
    {
        var newClient = new Client(clientId, Time.realtimeSinceStartup, nickname);
        return clientsById.TryAdd(clientId, newClient);
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

        Debug.Log($"Adding client: {ip.Address} ({ip.Port})");

        var newId = clientsById.Any()
                        ? clientsById.Keys.Max() + 1
                        : 0;

        newClient = new Client(newId, Time.realtimeSinceStartup, nickname);
        clientsById.Add(newId, newClient);
        ipsById.TryAdd(newId, ip);
        return true;
    }

    private void RemoveClient(int id)
    {
        if (clientsById.ContainsKey(id))
        {
            Debug.Log("Removing client: " + id);
            clientsById.Remove(id);
        }
    }

    public void OnReceiveData(byte[] data, IPEndPoint ip)
    {
        var messageType = NetMessage.ReadType(data);
        var flags = NetMessage.ReadFlags(data);
        switch (messageType)
        {
            case MessageType.Invalid:
            {
                Debug.LogError($"{name}: Message received was invalid!");
                break;
            }
            case MessageType.HandshakeRequest when IsServer:
            {
                HandleHandshakeRequestAsServer(ip, data);
                break;
            }
            case MessageType.HandshakeResponse:
            {
                //TODO: To server
                if (IsServer)
                {
                    Debug.LogError($"{name}: Server received a {nameof(MessageType.HandshakeResponse)} from {ip.Address}({ip.Port})." +
                                   $"\nThis is not allowed!");
                    break;
                }
                HandleHandshakeResponseAsClient(data, ip);
                break;
            }
            //TODO: To client
            case MessageType.ClientListUpdate when !IsServer:
            {
                var message = new NetClientListUpdate();
                message.TryDeserializeIntoSelf(data);
                foreach (var (clientId, nickname)
                         in message
                            .Data
                            .Where(clientData => !clientsById.ContainsKey(clientData.clientId)))
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

                if (flags.HasFlag(MessageFlags.IsSerialized))
                {
                    var clientId = NetMessage.ReadClientId(data);
                    if(clientId == Client.InvalidId)
                    {
                        Debug.LogError($"{name}: {nameof(clientId)} is invalid!");
                        break;
                    }
                    if(!clientsById.TryGetValue(clientId, out var client))
                    {
                        Debug.LogError($"{name}: Client was not added!");
                        break;
                    }
                    var messageCount = client.GetMessageId(messageType);
                        var messageId = SerializedNetMessage.ReadMessageId(data);
                        var isNewerThanLatestProcessedMessage = messageCount < messageId;
                    if(isNewerThanLatestProcessedMessage)
                    {
                        client.SetMessageId(messageType, messageCount);
                        ProcessMessage(data, messageType);
                        OnReceiveEvent?.Invoke(data, ip);
                    }
                    else
                        Debug.Log($"{name}: Message id ({messageId}) is older than the last one processed ({messageCount})");
                }
                else
                {
                    ProcessMessage(data, messageType);
                    OnReceiveEvent?.Invoke(data, ip);
                }
                break;
            }
        }
    }

    private void ProcessMessage(byte[] data, MessageType messageType)
    {
        if(onReceiveMessageHandlers.TryGetValue(messageType, out var handler))
            handler?.Invoke(messageType, data);
        if(IsServer)
            Broadcast(data);
    }

    private void HandleHandshakeRequestAsServer(IPEndPoint ip, byte[] data)
    {
        var request = new NetHandshakeRequest();
        if (!request.TryDeserializeIntoSelf(data))
        {
            Debug.LogError($"{name}: Couldn't serialize handshake request from ip {ip.Address}({ip.Port})");
            return;
        }

        if (clientsById.Values.Any(client => client.Nickname == request.Data))
        {
            Debug.LogWarning($"{name}: Rejecting client with ip {ip.Address}({ip.Port}).\tReason: {HandshakeResponseCodes.DuplicatedNickname}");
            var response = new NetHandshakeResponse{Data = (HandshakeResponseCodes.DuplicatedNickname, -1)};
            var refusedConnectionData = response.GetBytes();
            connection.Send(refusedConnectionData, ip);
        }
        else if(TryAddClientAsServer(ip, out var client, request.Data))
        {
            var response = new NetHandshakeResponse{Data = (HandshakeResponseCodes.Success, client.ID)};
            var acceptedConnectionData = response.GetBytes();
            connection.Send(acceptedConnectionData, ip);
            onNewClientConnected(client.ID);
            SendUpdatedClientList();
        }
        else
            Debug.LogError($"{name}: Couldn't add new client!" +
                           $"\nip: {ip.Address} ({ip.Port})");
    }

    private void SendUpdatedClientList()
    {
        var message = new NetClientListUpdate
                      { Data = clientsById.Select(pair => (pair.Value.ID, pair.Value.Nickname)).ToArray() };
        Broadcast(message.GetBytes());
    }

    private void HandleHandshakeResponseAsClient(byte[] data, IPEndPoint ip)
    {
        var handshakeResponse = new NetHandshakeResponse();
        if (handshakeResponse.TryDeserializeIntoSelf(data))
        {
            if (handshakeResponse.Data.result is HandshakeResponseCodes.Success)
            {
                var myClientId = handshakeResponse.Data.clientId;
                LocalClient = new Client(myClientId, LocalClient.timeStamp, LocalClient.Nickname);
                onConnectionSuccessful(myClientId);
            }
            else
                onConnectionError(handshakeResponse.Data.result);
        }
    }

    public bool TrySendToClient(byte[] data, int clientId)
    {
        if (!ipsById.TryGetValue(clientId, out var ip))
            return false;
        connection.Send(data, ip);
        return true;
    }

    public void SendToServer(byte[] data)
    {
        connection.Send(data);
    }

    public void Broadcast(byte[] data)
    {
        foreach (var ip in ipsById.Values)
        {
            var currentIpIsMine = Equals(ip, LocalEndPoint);
            if (currentIpIsMine)
            {
                var messageType = NetMessage.ReadType(data);
            }
            else
                connection.Send(data, ip);
        }
    }
}
