using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpFileTransfer
{
    static class Locker
    {
        public static object Lock;
    }

    class ServerProgram
    {
        public static readonly TimeSpan SleepTimeout = TimeSpan.FromSeconds(3);

        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 1)
                {
                    Console.WriteLine("Usage: Server.exe {port}");
                    return;
                }

                Console.WriteLine("Listening for clients...");

                var server = new Server(int.Parse(args[0]));
                new Thread(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(SleepTimeout);
                        if (server.Clients.Any())
                        {
                            for (var i = 0; i < server.Clients.Count; i++)
                            {
                                var client = server.Clients[i];
                                var averageSpeed =
                                    client.TotalReceived / (DateTime.Now - client.ReceiveStartedMoment)
                                    .TotalMilliseconds;
                                var instantSpeed =
                                    client.TotalReceived / (DateTime.Now - client.InstantCheckMoment)
                                    .TotalMilliseconds;
                                var sb = new StringBuilder();
                                sb.Append($"{i})")
                                    .Append($" File {client.FileToReceive.Name}")
                                    .Append($" with size {client.FileSize}")
                                    .Append($" from {client.FileSender.Client.RemoteEndPoint}")
                                    .Append($" has average speed {averageSpeed}")
                                    .Append($" and instant speed {instantSpeed}")
                                    .Append($" (received by {client.FileSize / client.TotalReceived}");
                                Console.WriteLine(sb.ToString() + '\n');
                                lock (Locker.Lock)
                                {
                                    client.InstantCheckMoment = DateTime.Now;
                                    client.InstantReceived = 0;
                                }

                            }
                        }

                    }

                }).Start();
                while (true)
                {
                    if (server.ClientsAwaiter.Pending())
                    {
                        new Thread(() =>
                        {
                            ClientHandler clientHandler = null;
                            try
                            {
                                clientHandler
                                   = new ClientHandler(server.ClientsAwaiter.AcceptTcpClient());
                                server.Clients.Add(clientHandler);

                                clientHandler.ReceiveInfo();

                                if (File.Exists(clientHandler.PathToFile))
                                    clientHandler.SendResponse(ResponseCodes.FileAlreadyExists);

                                clientHandler.SendResponse(ResponseCodes.SendingFile);


                                clientHandler.ReceiveFile();

                                clientHandler.SendResponse(ResponseCodes.FileSent);
                            }
                            catch (Exception exp)
                            {
                                Console.WriteLine("Exception caught: " + exp.Message);
                                clientHandler.SendResponse(ResponseCodes.ErrorOccurred);
                            }

                        }).Start();
                    }
                }
            }

            catch (Exception exp)
            {
                Console.WriteLine("Exception caught: " + exp.Message);
            }
        }
    }
}
