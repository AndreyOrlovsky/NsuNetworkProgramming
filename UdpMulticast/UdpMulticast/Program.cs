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
        private const int sendPort = 40000;
        private const int receivePort = 40001;
        private static readonly Guid guid = Guid.NewGuid();

        private static IPAddress multicastGroup = IPAddress.Parse("234.212.200.1"); //default
        private static IPAddress joinedCopyAddress = null;
        private static readonly UdpClient udpSender = new UdpClient(sendPort);
        private static readonly UdpClient udpReceiver = new UdpClient(receivePort);
        private static readonly IPEndPoint endPoint = new IPEndPoint(multicastGroup, sendPort);

        private static List<(Guid, IPAddress)> copies = new List<(Guid, IPAddress)> { (guid, GetLocalIP()) };

        private static string GetJoinMessage(Guid guid) => $"New copy has joined the group {multicastGroup.ToString()}! It's GUID: {guid.ToString()}.";
        private static string GetLeaveMessage(Guid guid) =>  $"A copy with GUID {guid.ToString()} has left the group {multicastGroup.ToString()}.";
        private static string CopiesCountMessage => $"There are {copies.Count} copies active now! Here are them:";

        public static void Main(string[] args)
        {


            Console.ReadKey();
            udpSender.JoinMulticastGroup(multicastGroup);

            new Thread(TrackCondition).Start();

            byte[] message = Encoding.ASCII.GetBytes(guid.ToString());
            udpSender.Send(message, message.Length, endPoint);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            
            udpSender.Send(message, message.Length, endPoint); //отправлен guid, который уже отправляли - это сигнал о завершении 


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

        public static void TrackCondition()
        {
            while (true)
            {
                //udpSender.Receive();
            }
        }
    }
}
