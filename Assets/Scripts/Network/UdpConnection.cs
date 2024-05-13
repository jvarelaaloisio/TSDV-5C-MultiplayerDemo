using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Network
{
    public class UdpConnection
    {
        private struct DataReceived
        {
            public byte[] data;
            public IPEndPoint ipEndPoint;
        }

        private readonly UdpClient udpClient;
        private readonly Queue<DataReceived> _dataReceivedQueue = new();

        object handler = new object();
        private readonly IReceiveNetData _receiver;

        public event Action<(byte[] data, IPEndPoint ipEndpoint)> onReceiveData = delegate { };
    
        public EndPoint LocalEndPoint => udpClient.Client.LocalEndPoint;
    
        public UdpConnection(int port, IReceiveNetData receiver = null)
        {
            udpClient = new UdpClient(port);
            
            this._receiver = receiver;

            udpClient.BeginReceive(OnReceive, null);
        }

        public UdpConnection(IPAddress ip, int port, IReceiveNetData receiver = null)
        {
            udpClient = new UdpClient();
            udpClient.Connect(ip, port);

            this._receiver = receiver;

            udpClient.BeginReceive(OnReceive, null);
        }

        public void Close()
        {
            udpClient.Close();
        }

        public void FlushReceiveData()
        {
            lock (handler)
            {
                while (_dataReceivedQueue.Count > 0)
                {
                    DataReceived dataReceived = _dataReceivedQueue.Dequeue();
                    if (_receiver != null)
                        _receiver.OnReceiveData(dataReceived.data, dataReceived.ipEndPoint);
                }
            }
        }

        void OnReceive(IAsyncResult ar)
        {
            try
            {
                DataReceived dataReceived = new DataReceived();
                dataReceived.data = udpClient.EndReceive(ar, ref dataReceived.ipEndPoint);

                lock (handler)
                {
                    _dataReceivedQueue.Enqueue(dataReceived);
                }
            }
            catch(SocketException e)
            {
                // This happens when a client disconnects, as we fail to send to that port.
                UnityEngine.Debug.LogError("[UdpConnection] " + e.Message);
            }

            udpClient.BeginReceive(OnReceive, null);
        }

        public void Send(byte[] data)
        {
            udpClient.Send(data, data.Length);
        }

        public void Send(byte[] data, IPEndPoint ipEndpoint)
        {
            udpClient.Send(data, data.Length, ipEndpoint);
        }
    }
}