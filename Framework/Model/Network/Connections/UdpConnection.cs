using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Model.Network.Connections
{
    public class UdpConnection
    {
        public struct ReceivedData
        {
            public byte[] Data;
            public IPEndPoint IPEndPoint;
        }

        private readonly UdpClient _udpClient;
        private readonly Queue<ReceivedData> _dataReceivedQueue = new();

        private readonly object _handler = new object();
        [Obsolete]
        private readonly IReceiveNetData _receiver;

        public event Action<Exception> OnException = delegate { };
        public event Action<ReceivedData> onReceiveData = delegate { };
    
        public EndPoint LocalEndPoint => _udpClient.Client.LocalEndPoint;
    
        public UdpConnection(int port, IReceiveNetData receiver = null)
        {
            _udpClient = new UdpClient(port);
            
            this._receiver = receiver;

            _udpClient.BeginReceive(OnReceive, null);
        }

        public UdpConnection(IPAddress ip, int port, IReceiveNetData receiver = null)
        {
            _udpClient = new UdpClient();
            _udpClient.Connect(ip, port);

            this._receiver = receiver;

            _udpClient.BeginReceive(OnReceive, null);
        }

        public void Close()
        {
            _udpClient.Close();
        }

        public void FlushReceiveData()
        {
            lock (_handler)
            {
                while (_dataReceivedQueue.Count > 0)
                {
                    ReceivedData receivedData = _dataReceivedQueue.Dequeue();
                    if (_receiver != null)
                        _receiver.OnReceiveData(receivedData.Data, receivedData.IPEndPoint);
                    onReceiveData?.Invoke(receivedData);
                }
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                ReceivedData receivedData = new ReceivedData();
                receivedData.Data = _udpClient.EndReceive(ar, ref receivedData.IPEndPoint);

                lock (_handler)
                {
                    _dataReceivedQueue.Enqueue(receivedData);
                }
            }
            catch(SocketException e)
            {
                // This happens when a client disconnects, as we fail to send to that port.
                OnException(e);
            }

            _udpClient.BeginReceive(OnReceive, null);
        }

        public void Send(byte[] data)
        {
            _udpClient.Send(data, data.Length);
        }

        public void Send(byte[] data, IPEndPoint ipEndpoint)
        {
            _udpClient.Send(data, data.Length, ipEndpoint);
        }
    }
}