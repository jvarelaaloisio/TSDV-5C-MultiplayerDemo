using System;
using System.Threading;
using System.Threading.Tasks;
using MatchMakerModel;
using Model.Network;
using Model.Network.Connections;

namespace MatchMaker
{
    internal static class Program
    {
        private static UdpConnection _connection;

        public static async Task Main(string[] args)
        {
            var response = new MatchMakerHandshakeResponsePacket();
            Console.WriteLine(response.Payload);
            var packetListController = new PacketProvider();
            packetListController.Initialize();
            return;
            var port = 12345;
            _connection = new UdpConnection(port, null);
            Console.WriteLine($"Server started at {_connection.LocalEndPoint}");
            var cancelReceiveData = new CancellationTokenSource();
            var handshakeHandler = new HandshakeHandler();
            
            _ = handshakeHandler.ProcessReceivedData(60, cancelReceiveData.Token, _connection);
            _ = CallbackOnKeyPress.Run(ConsoleKey.Escape, () => cancelReceiveData.Cancel());
            try
            {
                while (!cancelReceiveData.Token.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancelReceiveData.Token);
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Match");
            }
            cancelReceiveData.Dispose();
            Console.WriteLine("Matchmaker has stopped\nHave a nice day :)");
            Console.ReadKey();
        }
    }
}