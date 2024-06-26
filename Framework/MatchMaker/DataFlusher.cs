using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Model.Network;
using Model.Network.Connections;

namespace MatchMaker
{
    internal class DataFlusher
    {
        private readonly UdpConnection _connection;
        private readonly TextWriter _output;

        public DataFlusher(UdpConnection connection, TextWriter output)
        {
            _connection = connection;
            _output = output;
        }

        public async Task ProcessReceivedData(float frequency, CancellationToken token)
        {
            await _output.WriteLineAsync($"{nameof(DataFlusher)} is receiving data.");
            _connection.onReceiveData += HandleData;
            token.Register(HandleCancel);
            while (!token.IsCancellationRequested)
            {
                _connection.FlushReceiveData();
                const int millisecondsInASecond = 1000;
                await Task.Delay((int)(millisecondsInASecond / frequency), token);
            }
        }

        private void HandleCancel()
        {
            _output.WriteLine($"{nameof(DataFlusher)} reception was canceled.");
            _connection.onReceiveData -= HandleData;
        }

        //TODO: Make this an async task
        private void HandleData(UdpConnection.ReceivedData receivedData)
        {
            _output.WriteLine($"Received new data. Trying to parse.");
            if (Header.TryParse(receivedData.Data, out var header))
            {
                _output.WriteLine($"{nameof(Header)} data: " +
                                  $"\nType: {header.TypeGroup} ({header.TypeId})" +
                                  $"\nClient ID: {header.ClientId}" +
                                  $"\nFlags: {header.Flags}");
            }
        }
    }
}