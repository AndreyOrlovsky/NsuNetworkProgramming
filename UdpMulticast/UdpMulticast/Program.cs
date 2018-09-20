using System;
using System.Collections.Generic;
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
        private const int port = 41000;
        private static readonly Guid myGuid = Guid.NewGuid();

        private static IPAddress multicastGroup = IPAddress.Parse("235.5.5.11"); //default
        private static IPAddress localIP = GetLocalIP();

        private static readonly UdpClient udpSender = null;
        private static readonly UdpClient udpReceiver = null;
        private static readonly IPEndPoint groupEndPoint = new IPEndPoint(multicastGroup, port);
        private static IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);
        private static IPEndPoint remoteEndPoint = null;

        private static Dictionary<Guid, IPAddress> copies = new Dictionary<Guid, IPAddress>() { [myGuid] = localIP };

        private static string GetJoinMessage(Guid guid) => $"New copy has joined the group {multicastGroup.ToString()}! It's GUID: {guid.ToString()}.";
        private static string GetLeaveMessage(Guid guid) => $"A copy with GUID {guid.ToString()} has left the group {multicastGroup.ToString()}.";
        private static string CopiesCountMessage => $"There are {copies.Count} copies active now! Here are them:";
        private static string CopiesListing => string.Join("\n", copies.Select(copy => $"{copy.Key} {copy.Value}"));
        private const string delimiters = "--------------------------------";
        private static bool trackedSelf = false;

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
            udpSender.JoinMulticastGroup(multicastGroup, timeToLive: 1);

            Thread thread = new Thread(TrackCondition);
            thread.Start();

            SendMessageToGroup("add");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            SendMessageToGroup("remove"); 

            Environment.Exit(0);


        }

        public static void SendMessageToGroup(string request)
        {
            byte[] message = Encoding.ASCII.GetBytes($"{request} {myGuid.ToString()}");
            udpSender.Send(message, message.Length, groupEndPoint);
        }

        public static void TrackCondition()
        {
            udpReceiver.JoinMulticastGroup(multicastGroup, timeToLive: 1);
            while (true)
            {
                string[] message = Encoding.ASCII.GetString(udpReceiver.Receive(ref remoteEndPoint)).Split(' ');
                string request = message[0];
                Guid remoteGuid = Guid.Parse(message[1]);

                if (request == "add" && !(remoteEndPoint.Address.Equals(localIP) && trackedSelf))
                {
                    trackedSelf = true;
                    copies[remoteGuid] = remoteEndPoint.Address;
                    SendMessageToGroup("add"); //извещаем новую копию о себе
                    Console.WriteLine(string.Join("\n", GetJoinMessage(remoteGuid), CopiesCountMessage, CopiesListing, delimiters));

                }
                else if (request == "remove")
                {
                    copies.Remove(remoteGuid);
                    Console.WriteLine(string.Join("\n", GetLeaveMessage(remoteGuid), CopiesCountMessage, CopiesListing, delimiters));

                }
            }
        }

        private static IPAddress GetLocalIP()
        {
            IPAddress localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Unspecified))
            {
                socket.Connect("1.1.1.1", 65535);
                localIP = ((IPEndPoint)socket.LocalEndPoint).Address;
            }

            return localIP;
        }


    }
}
