using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

public struct Client
{
    public float timeStamp;
    public int id;
    public IPEndPoint ipEndPoint;

    public Client(IPEndPoint ipEndPoint, int id, float timeStamp)
    {
        this.timeStamp = timeStamp;
        this.id = id;
        this.ipEndPoint = ipEndPoint;
    }
}

public class NetworkManager : MonoBehaviourSingleton<NetworkManager>, IReceiveData
{
    public IPAddress ipAddress
    {
        get; private set;
    }

    public int port
    {
        get; private set;
    }

    public static bool IsServer
    {
        get; private set;
    }

    public int TimeOut = 30;

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

    private UdpConnection connection;

    private readonly Dictionary<IPEndPoint, Client> clients = new ();
    private readonly Dictionary<int, IPEndPoint> ipsById = new();

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
    }

    public void StartClient(IPAddress ip, int port)
    {
        IsServer = false;

        this.port = port;
        this.ipAddress = ip;

        connection = new UdpConnection(ip, port, this);
        var handShake = new NetHandShake();
        handShake.data = (-1, -1);
        SendToServer(handShake.Serialize());
    }

    public bool TryAddClient(IPEndPoint ip, int clientId)
    {
        var newClient = new Client(ip, clientId, Time.realtimeSinceStartup);
        if (clients.TryAdd(ip, newClient))
        {
            ipsById.TryAdd(clientId, ip);
            return true;
        }
        return false;
    }

    public bool TryAddNewClient(IPEndPoint ip, out int newId)
    {
        var wasClientAlreadyAdded = clients.Any(kvp
                                              => Equals(kvp.Value.ipEndPoint, ip));
        if (!wasClientAlreadyAdded)
        {
            Debug.Log($"Adding client: {ip.Address} ({ip.Port})");

            newId = clients.Count > 0 ? clients.Last().Value.id + 1 : 0;

            var newClient = new Client(ip, newId, Time.realtimeSinceStartup);
            clients.Add(ip, newClient);
            ipsById.TryAdd(newId, ip);
            return true;
        }
        Debug.LogWarning($"{name}: Client {ip.Address} ({ip.Port}) was already added!");

        newId = -1;
        return false;
    }

    private void RemoveClient(IPEndPoint ip)
    {
        if (clients.ContainsKey(ip))
        {
            Debug.Log("Removing client: " + ip.Address);
            clients.Remove(ip);
        }
    }

    public void OnReceiveData(byte[] data, IPEndPoint ip)
    {
        var messageType = (MessageType)BitConverter.ToInt32(data, 0);
        switch (messageType)
        {
            //A new client wants to connect and I'm server
            case MessageType.HandShake when IsServer:
            {
                if(TryAddNewClient(ip, out var clientId))
                {
                    var acceptedConnectionMessage = new NetHandShake();
                    var acceptedConnectionData = acceptedConnectionMessage.Serialize();
                    connection.Send(acceptedConnectionData, ip);
                    onNewClientConnected(clientId);
                }
                else
                    Debug.LogError($"{name}: Couldn't add new client!" +
                                   $"\nip: {ip.Address} ({ip.Port})");
                break;
            }
            //I connected as client and server accepted
            case MessageType.HandShake:
            {
                var newClientMessage = new NetHandShake();
                newClientMessage.data = newClientMessage.Deserialize(data);
                var myClientId = newClientMessage.data.clientId;
                TryAddClient(ip, myClientId);
                onConnectionSuccessful(myClientId);
                break;
            }
            default:
                OnReceiveEvent?.Invoke(data, ip);
                break;
        }
    }

    public bool SendToClient(byte[] data, int clientId)
    {
        if (TryGetClient(clientId, out var client))
        {
            connection.Send(data, client.ipEndPoint);
            return true;
        }
        return false;
    }

    public void SendToServer(byte[] data)
    {
        connection.Send(data);
    }

    public void Broadcast(byte[] data)
    {
        using (var iterator = clients.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                connection.Send(data, iterator.Current.Value.ipEndPoint);
            }
        }
    }

    private bool TryGetClient(int clientId, out Client client)
    {
        if (ipsById.TryGetValue(clientId, out var ip))
        {
            if (clients.TryGetValue(ip, out client))
                return true;

            Debug.LogError($"{name}: IP ({ip}) was not found!");
            return false;
        }

        client = default;
        Debug.LogError($"{name}: ID ({clientId}) was not found!");
        return false;
    }
}
