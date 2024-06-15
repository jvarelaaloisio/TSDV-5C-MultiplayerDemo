using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Model.Network;
using Model.Network.Connections;

namespace MatchMaker
{
    internal class HandshakeHandler
    {
        public async Task ProcessReceivedData(float frequency, CancellationToken token, UdpConnection connection)
        {
            connection.onReceiveData += HandleData;
            while (!token.IsCancellationRequested)
            {
                Console.WriteLine("Flushing received data");
                connection.FlushReceiveData();
                const int millisecondsInASecond = 1000;
                await Task.Delay((int)(millisecondsInASecond / frequency), token);
            }
            connection.onReceiveData -= HandleData;
        }

        private void HandleData(UdpConnection.ReceivedData receivedData)
        {
        }
    }
}