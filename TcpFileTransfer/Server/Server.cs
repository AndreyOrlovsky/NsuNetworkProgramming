using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpFileTransfer
{
    class Server
    {
        public const int BufferSizeBytes = 2048;

        public int Port { get; }
        public TcpListener ClientsAwaiter { get; }

        private readonly List<ClientHandler> clients = new List<ClientHandler>();

        public Server(int port)
        {
            Port = port;
            ClientsAwaiter = new TcpListener(IPAddress.Any, port);
        }

        public void PrintCurrentReceivingStatistics()
        {
            lock (clients)
            {
                if (clients.Any())
                {
                    for (var i = 0; i < this.clients.Count; i++)
                    {
                        var client = this.clients[i];

                        var averageSpeed =
                            client.TotalReceived / (DateTime.Now - client.ReceiveStartedMoment)
                            .TotalMilliseconds;

                        var instantSpeed =
                            client.InstantReceived / (DateTime.Now - client.InstantCheckMoment)
                            .TotalMilliseconds;

                        var sb = new StringBuilder();
                        sb.Append($"{i + 1})")
                            .Append($" File {client.FileToReceive.Name}")
                            .Append($" with size {client.FileSize} bytes")
                            .Append($" from {client.FileSender.Client.RemoteEndPoint}")
                            .Append($" has average speed {averageSpeed:F5} b/s")
                            .Append($" and instant speed {instantSpeed:F5} b/s")
                            .Append($" (received by {client.TotalReceived / (double)client.FileSize:P})");

                        Console.WriteLine(sb.ToString() + '\n');

                        lock (Lockers.InstantStatistics)
                        {
                            client.InstantCheckMoment = DateTime.Now;
                            client.InstantReceived = 0;
                        }
                    }

                }
            }
        }

        public void HandleConnection()
        {
            ClientHandler clientHandler = null;
            try
            {
                clientHandler = new ClientHandler(this.ClientsAwaiter.AcceptTcpClient());


                clientHandler.ReceiveInfo();

                if (File.Exists(clientHandler.PathToFile))
                {
                    clientHandler.SendResponse(ResponseCodes.FileAlreadyExists);
                    return;
                }

                lock (clients)
                {
                    clients.Add(clientHandler);
                }

                clientHandler.SendResponse(ResponseCodes.SendingFile);

                clientHandler.ReceiveFile();

                lock (clients)
                {
                    clients.Remove(clientHandler);
                }

                clientHandler.SendResponse(ResponseCodes.FileSent);

            }

            catch (Exception e)
            {
                Console.WriteLine("Exception caught: " + e);
                clientHandler.SendResponse(ResponseCodes.ErrorOccurred);
            }
        }

    }

}
