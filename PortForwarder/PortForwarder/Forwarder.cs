using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace PortForwarder
{
    public class ForwarderSettings
    {
        public ForwarderSettings(int sourcePort, IPAddress destinationIP, int destinationPort)
        {
            SourcePort = sourcePort;
            DestinationIP = destinationIP;
            DestinationPort = destinationPort;
        }

        public int SourcePort { get; }
        public IPAddress DestinationIP { get; }
        public int DestinationPort { get; }
    }

    public class ForwarderPair
    {
        public const int BufferSize = 1024;

        public TcpClient LocalClient { get; set; }
        public EndPoint LocalEndPoint { get; set; }
        public byte[] LocalBuffer { get; } = new byte[BufferSize];

        public TcpClient RemoteClient { get; set; }
        public EndPoint RemoteEndpoint { get; set; }
        public byte[] RemoteBuffer { get; } = new byte[BufferSize];
    }

    public class Forwarder
    {
        private const int remoteConnectTimeoutMillis = 5000;
        private ForwarderSettings settings;
        private TcpListener clientsAwaiter;
        List<ForwarderPair> clients = new List<ForwarderPair>();


        public Forwarder(ForwarderSettings settings)
        {
            this.settings = settings;
        }

        public void StartHandlingConnections()
        {
            clientsAwaiter = new TcpListener(IPAddress.Any, settings.SourcePort);
            clientsAwaiter.Start();
            clientsAwaiter.BeginAcceptTcpClient(AcceptNewConnection, state: null);
        }

        public void Stop()
        {
            clientsAwaiter.Stop();
        }

        private void AcceptNewConnection(IAsyncResult asyncResult)
        {
            TcpClient newClient = clientsAwaiter.EndAcceptTcpClient(asyncResult);
            clientsAwaiter.BeginAcceptTcpClient(AcceptNewConnection, state: null); //заново ждём новых соединений
            Console.WriteLine("New connection from " + newClient.Client.RemoteEndPoint);
            ConnectToRemote(newClient);
        }

        private void ConnectToRemote(TcpClient client)
        {
            ForwarderPair pair = new ForwarderPair();
            pair.LocalClient = client;
            pair.RemoteClient = new TcpClient();
            pair.RemoteClient.BeginConnect(settings.DestinationIP, settings.DestinationPort, ConnectToRemoteCallback, state: pair);
        }

        private void ConnectToRemoteCallback(IAsyncResult asyncResult)
        {
            ForwarderPair pair = (ForwarderPair)asyncResult.AsyncState;
            if (!asyncResult.AsyncWaitHandle.WaitOne(remoteConnectTimeoutMillis))
            {
                Console.WriteLine("Failed to connect to remote host, closing connections");
                pair.LocalClient.Close();
                return;
            }
            try
            {
                pair.RemoteClient.EndConnect(asyncResult);
                if (pair.LocalClient.Connected && pair.RemoteClient.Connected)
                {
                    pair.LocalEndPoint = pair.RemoteClient.Client.LocalEndPoint;
                    pair.RemoteEndpoint = pair.RemoteClient.Client.RemoteEndPoint;
                    Console.WriteLine("Connected client " + pair.LocalEndPoint + " to " + pair.RemoteEndpoint);
                    lock (clients)
                    {
                        clients.Add(pair);
                    }
                    StartForwarding(pair);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error connecting to remote host, : " + e.Message);
            }
        }

        private void StartForwarding(ForwarderPair pair)
        {
            try
            {
                BeginLocalReceive(pair);
                BeginRemoteReceive(pair);
                Console.WriteLine($"Forwarding: {pair.LocalEndPoint} <----> {pair.RemoteEndpoint}");
            }
            catch
            {
                DisconnectPair(pair);
            }
        }

        private void BeginLocalReceive(ForwarderPair pair)
        {
            pair.LocalClient.GetStream().BeginRead(pair.LocalBuffer, 0, pair.LocalBuffer.Length, LocalReceiveCallback, pair);
        }

        private void LocalReceiveCallback(IAsyncResult asyncResult)
        {
            ForwarderPair pair = (ForwarderPair)asyncResult.AsyncState;

            int bytesRead = pair.LocalClient.GetStream().EndRead(asyncResult);
            if (bytesRead > 0)
            {
                pair.RemoteClient.GetStream().Write(pair.LocalBuffer, 0, bytesRead);
            }

            //продолжаем принимать 
            BeginLocalReceive(pair);
        }

        private void BeginRemoteReceive(ForwarderPair pair)
        {
            pair.RemoteClient.GetStream().BeginRead(pair.RemoteBuffer, 0, pair.RemoteBuffer.Length, RemoteReceiveCallback, pair);

        }

        private void RemoteReceiveCallback(IAsyncResult asyncResult)
        {
            ForwarderPair pair = (ForwarderPair)asyncResult.AsyncState;

            int readBytes = pair.RemoteClient.GetStream().EndRead(asyncResult);
            if (readBytes > 0)
            {
                pair.LocalClient.GetStream().Write(pair.RemoteBuffer, 0, readBytes);
            }

            //продолжаем принимать 
            BeginRemoteReceive(pair);

        }

        private void DisconnectPair(ForwarderPair pair)
        {
            lock (clients)
            {
                clients.Remove(pair);
            }

            Console.WriteLine("Forwarding terminated: " + pair.LocalEndPoint + " <--X--> " + pair.RemoteEndpoint);

            pair.LocalClient.GetStream().Close();
            pair.LocalClient.Close();

            pair.RemoteClient.GetStream().Close();
            pair.RemoteClient.Close();


        }


    }
}

