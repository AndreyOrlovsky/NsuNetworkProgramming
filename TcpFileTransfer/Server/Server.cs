using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpFileTransfer
{
    class Server 
    {
        public int Port { get; }
        public TcpListener ClientsAwaiter { get; }

        public static readonly int BufferSizeBytes = 4096;

        public Server(int port)
        {
            Port = port;
            ClientsAwaiter = new TcpListener(IPAddress.Any, port);
        }

        public List<ClientHandler> Clients { get; }

        
    }
}
