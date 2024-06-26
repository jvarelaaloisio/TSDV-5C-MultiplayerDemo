using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Model.Network;
using Model.Network.Connections;
using static System.Int32;

namespace MatchMaker
{
    internal static class Program
    {
        private static UdpConnection _connection;

        public static async Task Main(string[] args)
        {
            var packetListController = new PacketProvider();
            packetListController.Initialize();
            Console.WriteLine("Initializing matchmaker, please select a port.");
            var port = -1;
            while (port is < IPEndPoint.MinPort or > IPEndPoint.MaxPort)
            {
                Console.Write("Port: ");
                var portInput = Console.ReadLine();
                if (!TryParse(portInput, out port))
                {
                    Console.WriteLine($"Port value ({portInput}) is not a valid input." +
                                      $"\nExpected values are integers between {IPEndPoint.MinPort} and {IPEndPoint.MaxPort}.");
                    port = -1;
                }
            }

            _connection = new UdpConnection(port, null);
            Console.WriteLine($"Server started at {_connection.LocalEndPoint}");
            var cancelReceiveData = new CancellationTokenSource();
            var handshakeHandler = new DataFlusher(_connection, Console.Out);
            _ = handshakeHandler.ProcessReceivedData(60, cancelReceiveData.Token);

            Console.CancelKeyPress += HandleConsoleOnCancelKeyPress;
            try
            {
                while (!cancelReceiveData.Token.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancelReceiveData.Token);
                }
            }
            catch (TaskCanceledException)
            {
            }
            Console.CancelKeyPress -= HandleConsoleOnCancelKeyPress;

            cancelReceiveData.Dispose();
            Console.WriteLine("Matchmaker has stopped\nHave a nice day :)");
            Console.ReadKey();
            
            void HandleConsoleOnCancelKeyPress(object? o, ConsoleCancelEventArgs consoleCancelEventArgs)
            {
                consoleCancelEventArgs.Cancel = true;
                cancelReceiveData.Cancel();
            }
        }
    }
}