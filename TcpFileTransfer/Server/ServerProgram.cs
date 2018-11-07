using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpFileTransfer
{
    static class Locker
    {
        public static readonly object Lock = new object();
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
                var server = new Server(port: int.Parse(args[0]));

                new Thread(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(SleepTimeout);
                        server.PrintCurrentReceivingStatistics();
                    }

                }).Start();

                server.ClientsAwaiter.Start();

                while (true)
                {
                    if (server.ClientsAwaiter.Pending())
                    {
                        new Thread(server.HandleConnection).Start();
                    }
                }
            }

            catch (Exception e)
            {
                Console.WriteLine("Exception caught: " + e.Message);
            }
        }

    }
}
