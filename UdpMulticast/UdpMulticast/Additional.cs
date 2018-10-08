using System;
using System.Net;
using System.Net.Sockets;

namespace UdpMulticast
{
    static class Parameters
    {
        public static readonly int Port = 42000;
        public static IPAddress MulticastGroup = IPAddress.Parse("235.5.5.11"); //default
        public static readonly IPEndPoint LocalEndPoint = new IPEndPoint(IPAddress.Any, Port);
        public static readonly int IntervalMillis = 1000;
        public static readonly int CriticalNumberOfChecks = 3;
        public const string Delimiters = "--------------------------------";
    }

    static class Utilities
    {
        public static void AllowReusePort(this UdpClient udpClient)
        {
            udpClient.ExclusiveAddressUse = false;
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(Parameters.LocalEndPoint);
        }
    }
}

