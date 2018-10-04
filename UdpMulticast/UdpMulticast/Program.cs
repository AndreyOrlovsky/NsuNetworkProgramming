using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;

namespace UdpMulticast
{
    class Program
    {
        private const int port = 42000;
        private static readonly Guid myGuid = Guid.NewGuid();

        private static IPAddress multicastGroup = IPAddress.Parse("235.5.5.11"); //default

        private static readonly UdpClient udpSender;
        private static readonly UdpClient udpReceiver;
        private static IPEndPoint groupEndPoint;
        private static IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);
        private static IPEndPoint remoteEndPoint;
        private static ConcurrentDictionary<Guid, IPAddress> copies = new ConcurrentDictionary<Guid, IPAddress>();
        private static ConcurrentDictionary<Guid, int> timeouts = new ConcurrentDictionary<Guid, int>();


        private static string GetJoinMessage(Guid guid)
            => $"New copy has joined the group {multicastGroup.ToString()}! It's GUID: {guid.ToString()}.";
        private static string GetLeaveMessage(Guid guid)
            => $"A copy with GUID {guid.ToString()} has left the group {multicastGroup.ToString()}.";
        private static string CopiesCountMessage
            => $"There are {copies.Count} copies active now! Here are them:";
        private static string CopiesListing
            => string.Join("\n", copies.Select(copy => $"{copy.Key} {copy.Value}"));

        private static readonly int intervalMillis = 1000;

        private static readonly int criticalTimeout = 2;
        private const string delimiters = "--------------------------------";

        static Program()
        {
            udpSender = new UdpClient();
            udpSender.ExclusiveAddressUse = false;
            udpSender.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpSender.Client.Bind(localEndPoint);

            udpReceiver = new UdpClient();
            udpReceiver.ExclusiveAddressUse = false;
            udpReceiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpReceiver.Client.Bind(localEndPoint);
        }

        public static void Main(string[] args)
        {

            try
            {
                if (args.Length > 0)
                {
                    multicastGroup = IPAddress.Parse(args[0]);

                }

                groupEndPoint = new IPEndPoint(multicastGroup, port);
                udpSender.JoinMulticastGroup(multicastGroup, timeToLive: 1);
                udpReceiver.JoinMulticastGroup(multicastGroup, timeToLive: 1);

                new Thread(TrackCondition).Start();
                new Thread(ReceiveNotifications).Start();
                new Thread(NotifyAboutMe).Start();




                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();

                Environment.Exit(0); 
            }

            catch (FormatException e)
            {
                Console.WriteLine("Exception occurred: " + e.Message);
                Console.WriteLine($"{args[0]} is not even an IP address. " +
                                  $"1st parameter to program should be in range from 224.0.0.0 to 239.255.255.255.");
            }

            catch (SocketException e)
            {
                Console.WriteLine("Exception occurred: " + e.Message);
                Console.WriteLine($"{args[0]} should be in range from 224.0.0.0 to 239.255.255.255.");
            }

            finally
            {
                udpSender.Dispose();
                udpReceiver.Dispose();
            }


        }

        public static void SendMessagesToGroup(string request)
        {
            while (true)
            {
                byte[] message = Encoding.ASCII.GetBytes($"{request} {myGuid.ToString()}");
                udpSender.Send(message, message.Length, groupEndPoint);
                Thread.Sleep(intervalMillis);
            }
        }

        public static void NotifyAboutMe() => SendMessagesToGroup("add");

        public static void ReceiveNotifications()
        {
            while (true)
            {
                string[] message = Encoding.ASCII.GetString(udpReceiver.Receive(ref remoteEndPoint)).Split(' ');
                string request = message[0];
                Guid remoteGuid = Guid.Parse(message[1]);

                if (request == "add")
                {
                    if (!copies.ContainsKey(remoteGuid))
                    {
                        copies[remoteGuid] = remoteEndPoint.Address;
                        Console.WriteLine(string.Join("\n",
                            GetJoinMessage(remoteGuid), CopiesCountMessage, CopiesListing, delimiters));
                    }

                    timeouts[remoteGuid] = 0;

                }
            }
        }

        public static void TrackCondition()
        {
            int timeout;
            IPAddress address;

            while (true)
            {
                foreach (var kvp in timeouts)
                {
                    Guid guid = kvp.Key;

                    if (++timeouts[guid] >= criticalTimeout)
                    {
                        copies.TryRemove(guid, out address);
                        timeouts.TryRemove(guid, out timeout);
                        Console.WriteLine(string.Join("\n",
                            GetLeaveMessage(guid), CopiesCountMessage, CopiesListing, delimiters));
                    }
                }
                Thread.Sleep(intervalMillis);

            }
        }
    }
}
