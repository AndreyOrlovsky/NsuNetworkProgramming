using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UdpMulticast
{
    class Client : IDisposable
    {
        public UdpClient UdpSender { get; }
        private readonly Guid myGuid;
        private readonly IPEndPoint groupEndPoint;

        public Client(IPEndPoint groupEndPoint)
        {
            myGuid = Guid.NewGuid();
            UdpSender = new UdpClient();
            UdpSender.AllowReusePort();
            this.groupEndPoint = groupEndPoint;
        }

        public void SendMessagesToGroup(string request)
        {
            new Thread(() =>
            {
                while (true)
                {
                    byte[] message = Encoding.ASCII.GetBytes($"{request} {myGuid.ToString()}");
                    UdpSender.Send(message, message.Length, groupEndPoint);
                    Thread.Sleep(Parameters.IntervalMillis);
                }
            }).Start();

        }

        public void NotifyAboutMe() => SendMessagesToGroup("add");

        public void Dispose()
        {
            UdpSender.Dispose();
        }
    }
}

