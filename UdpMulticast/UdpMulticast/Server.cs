using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UdpMulticast
{
    class Server : IDisposable
    {
        public UdpClient UdpReceiver { get; }
        private IPEndPoint remoteEndPoint;
        private readonly ConcurrentDictionary<Guid, IPAddress> copies;
        private readonly ConcurrentDictionary<Guid, int> timeouts;

        public Server()
        {
            copies = new ConcurrentDictionary<Guid, IPAddress>();
            timeouts = new ConcurrentDictionary<Guid, int>();

            UdpReceiver = new UdpClient();
            UdpReceiver.AllowReusePort();
        }

        private string CopiesCountMessage
            => $"There are {copies.Count} copies active now! Here are them:";

        private string CopiesListing
            => string.Join(Environment.NewLine, copies.Select(copy => $"{copy.Key} {copy.Value}"));

        private string GetJoinMessage(Guid guid)
            => $"New copy has joined the group {Parameters.MulticastGroup}! It's GUID: {guid.ToString()}.";

        private string GetLeaveMessage(Guid guid)
            => $"A copy with GUID {guid.ToString()} has left the group {Parameters.MulticastGroup}.";

        public void ReceiveNotifications()
        {
            new Thread(() =>
            {
                while (true)
                {
                    string[] message = Encoding.ASCII.GetString(UdpReceiver.Receive(ref remoteEndPoint)).Split(' ');
                    string request = message[0];
                    Guid remoteGuid = Guid.Parse(message[1]);

                    if (request == "add")
                    {
                        if (!copies.ContainsKey(remoteGuid))
                        {
                            copies[remoteGuid] = remoteEndPoint.Address;
                            Console.WriteLine(string.Join("\n",
                                GetJoinMessage(remoteGuid), CopiesCountMessage, CopiesListing, Parameters.Delimiters));
                        }

                        timeouts[remoteGuid] = 0;

                    }
                }
            }).Start();
        }

        public void TrackCondition()
        {
            int timeout;
            IPAddress address;

            new Thread(() =>
            {
                while (true)
                {
                    foreach (var kvp in timeouts)
                    {
                        Guid guid = kvp.Key;

                        if (++timeouts[guid] >= Parameters.CriticalTimeout)
                        {
                            copies.TryRemove(guid, out address);
                            timeouts.TryRemove(guid, out timeout);
                            Console.WriteLine(string.Join("\n",
                                GetLeaveMessage(guid), CopiesCountMessage, CopiesListing, Parameters.Delimiters));
                        }
                    }

                    Thread.Sleep(Parameters.IntervalMillis);

                }
            }).Start();
        }

        public void Dispose()
        {
            UdpReceiver.Dispose();
        }
    }

}