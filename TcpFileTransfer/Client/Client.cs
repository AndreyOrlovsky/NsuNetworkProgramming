using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpFileTransfer
{
    class Client : IDisposable
    {
        public int Port { get; }
        public FileInfo FileToSend { get; }
        public TcpClient Sender { get; }

        private NetworkStream remoteStream;
        private FileStream fileStream;
        private static readonly int BufferSizeBytes = 1024;

        public Client(string serverAddress, int port, string pathToFile)
        {
            Port = port;
            FileToSend = new FileInfo(pathToFile);

            Sender = new TcpClient(serverAddress, port); //connected here
            new StreamReader(Sender.GetStream());
            remoteStream = Sender.GetStream();
        }

        public void SendInfo()
        {
            string info =
                $"{FileToSend.Name}\n" +
                $"{FileToSend.Length}";

            byte[] infoBytes = Encoding.UTF8.GetBytes(info);
            remoteStream.Write(infoBytes, 0, infoBytes.Length);
        }

        public void SendFile()
        {
            byte[] dataBytes = new byte[BufferSizeBytes];

            while (true)
            {
                if (fileStream.Read(dataBytes, 0, BufferSizeBytes) == 0)
                    break;
                remoteStream.Write(dataBytes, 0, BufferSizeBytes);
            }

        }

        public byte ReceiveServerResponse()
        {
            byte[] response = new byte[1];
            remoteStream.Read(response, offset: 0, size: 1);
            return response[0];
        }

        public void Dispose()
        {
            remoteStream?.Dispose();
            fileStream?.Dispose();
            Sender?.Dispose();
        }
    }
}