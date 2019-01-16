using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace PortForwarder
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var settings = new ForwarderSettings(sourcePort: int.Parse(args[0]),
                    destinationIP: Dns.GetHostAddresses(args[1]).First(), destinationPort: int.Parse(args[2]));


                var forwarder = new Forwarder(settings);
                forwarder.StartHandlingConnections();

                Console.WriteLine("Type 'q' to exit.");

                while (Console.ReadLine() != "q")
                {
                }

                forwarder.Stop();
            }

            catch (Exception e)
            {
                Console.WriteLine("Exception occured: " + e.Message);
                throw;
            }

        }
    }
}

