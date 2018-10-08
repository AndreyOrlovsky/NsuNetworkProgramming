using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UdpMulticast;

namespace Main
{
    static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    Parameters.MulticastGroup = IPAddress.Parse(args[0]);
                }
                
                Parameters.MulticastGroup = IPAddress.Parse("235.5.5.11");

                using (var client = new Client(groupEndPoint: new IPEndPoint(Parameters.MulticastGroup, Parameters.Port)))
                using (var server = new Server())
                {
                    client.UdpSender.JoinMulticastGroup(Parameters.MulticastGroup, timeToLive: 1);
                    server.UdpReceiver.JoinMulticastGroup(Parameters.MulticastGroup, timeToLive: 1);

                    server.TrackCondition();
                    server.ReceiveNotifications();
                    client.NotifyAboutMe();

                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();

                    Environment.Exit(0);
                }
            }

            catch (FormatException e)
            {
                Console.WriteLine("Exception occurred: " + e.Message);
                Console.WriteLine($"{args[0]} is not even an IP address. " +
                                  "1st parameter to program should be in range from 224.0.0.0 to 239.255.255.255.");
            }

            catch (SocketException e)
            {
                Console.WriteLine("Exception occurred: " + e.Message);
                Console.WriteLine($"IP {args[0]} is not in range from 224.0.0.0 to 239.255.255.255.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred: " + e.Message + e.TargetSite);
            }
        }
    }
}
