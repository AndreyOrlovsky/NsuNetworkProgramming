using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocksProxy
{
    internal class ProxyPair
    {
        public byte[] Buffer { get; }
        public Socket Client { get; }
        public Socket Remote { get; }

        public ProxyPair(Socket client, Socket remote, byte[] buffer)
        {
            this.Buffer = buffer;
            this.Client = client;
            this.Remote = remote;
        }
    }

    internal class Proxy
    {
        public int Port { get; }

        private IPEndPoint remotePoint;

        private const int BufferSize = 2048;
        private const int MaxClients = 100;

        public Proxy(int port)
        {
            this.Port = port;
        }

        public void Start()
        {
            Socket clientsAwaiter = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientsAwaiter.Bind(new IPEndPoint(IPAddress.Any, Port));
            clientsAwaiter.Listen(MaxClients);
            clientsAwaiter.BeginAccept(AcceptCallback, clientsAwaiter);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            byte[] request = new byte[BufferSize];
            ;
            Socket clientsAwaiter = (Socket)ar.AsyncState;

            // слушаем новых клиентов
            clientsAwaiter.BeginAccept(AcceptCallback, clientsAwaiter);

            Socket client = clientsAwaiter.EndAccept(ar);
            Socket remote = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            client.Receive(request, 0, request.Length, SocketFlags.None);

            /* проверили версию и то что авторизация не требуется
            VER	NMETHODS METHODS
             5     1        0... */
            if (request[0] != 5)
            {
                throw new NotImplementedException("Only SOCKS5 is supported");
            }

            if (request[2] != 0)
            {
                throw new NotImplementedException("Authentication is not supported");
            }

            byte[] response = { 5, 0 };
            client.Send(response, 0, 2, SocketFlags.None);
            client.Receive(request, 0, BufferSize, SocketFlags.None);

            IPEndPoint remoteEndPoint = GetEndPoint(in request);
            switch (request[1])
            {
                case 1:
                    remote.Connect(remoteEndPoint);
                    Console.WriteLine("Socket connected to " + remoteEndPoint);
                    break;

                case 2:
                    remote.Bind(remoteEndPoint);
                    Console.WriteLine("Socket binded to " + remoteEndPoint);
                    break;
            }

            response = new byte[BufferSize];
            Array.Copy(request, response, BufferSize);

            response[1] = 0;
            client.Send(response, 0, BufferSize, SocketFlags.None);


            byte[] buffer = new byte[BufferSize];
            ProxyPair pair = new ProxyPair(client, remote, buffer);
            remote.BeginReceive(buffer, 0, request.Length, SocketFlags.None, RemoteCallback, pair);
            client.BeginReceive(buffer, 0, request.Length, SocketFlags.None, ClientCallback, pair);
        }

        private IPEndPoint GetEndPoint(in byte[] request)
        {
            switch (request[3])
            {
                //пришёл ip
                case 1:
                    string ip = request[4] + "." + request[5] + "." + request[6] + "." + request[7];
                    int port = request[8] * 256 + request[9];
                    remotePoint = new IPEndPoint(IPAddress.Parse(ip), port);
                    break;
                //пришёл url
                case 3:

                 /* VER	    CMD	     RSV	    ATYP	          DST.ADDR	               DST.PORT
                     5        1       0          3      [length (1 byte)][hostname]      0 80 (2 bytes) */
                    byte hostnameLength = request[4];

                    string host = Encoding.ASCII.GetString(request, 5, hostnameLength);

                    int portIndex = 5 + hostnameLength;
                    ResolveHostname(host, port: request[portIndex] * 256 + request[portIndex + 1]);
                    break;

                default:
                    throw new NotImplementedException("Only IPv4 and hostnames are allowed");
            }

            return remotePoint;
        }

        private void ResolveHostname(string hostname, int port)
        {
             Dns.BeginGetHostEntry(hostname, ResolveCallback, port);
        }

        private void ResolveCallback(IAsyncResult ar)
        {
            var ip = Dns.EndGetHostAddresses(ar)
                .First(addr => addr.AddressFamily == AddressFamily.InterNetwork);

            remotePoint = new IPEndPoint(ip, port: (int)ar.AsyncState);
        }

        private void ClientCallback(IAsyncResult ar)
        {
            ProxyPair pair = (ProxyPair)ar.AsyncState;
            int received = pair.Client.EndReceive(ar);

            Console.WriteLine($"Forwarding: {pair.Client.RemoteEndPoint} -> {pair.Remote.RemoteEndPoint}");


            pair.Remote.Send(pair.Buffer, 0, received, SocketFlags.None);
            pair.Client.BeginReceive(pair.Buffer, 0, pair.Buffer.Length, SocketFlags.None, ClientCallback, pair);

        }

        private void RemoteCallback(IAsyncResult ar)
        {
            ProxyPair pair = (ProxyPair)ar.AsyncState;
            int received = pair.Remote.EndReceive(ar);

            Console.WriteLine($"Forwarding: {pair.Client.RemoteEndPoint} <- {pair.Remote.RemoteEndPoint}");

            pair.Client.Send(pair.Buffer, 0, received, SocketFlags.None);
            pair.Remote.BeginReceive(pair.Buffer, 0, pair.Buffer.Length, SocketFlags.None, RemoteCallback, pair);

        }
    }
}
