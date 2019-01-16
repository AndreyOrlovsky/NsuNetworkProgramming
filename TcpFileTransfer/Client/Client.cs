using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpFileTransfer
{
    class Client : IDisposable
    {
        public const int BufferSizeBytes = 2048;

        public int Port { get; }
        public FileInfo FileToSend { get; }
        public TcpClient Sender { get; }

        private NetworkStream remoteStream;
        public Client(string serverAddress, int port, string pathToFile)
        {
            Port = port;
            FileToSend = new FileInfo(pathToFile);

            Sender = new TcpClient(serverAddress, port); //connected here
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
            byte[] buffer = new byte[BufferSizeBytes];

            using (FileStream reader = new FileStream(FileToSend.FullName,
                FileMode.Open, FileAccess.Read))
            {
                while (true)
                {
                    int bytesRead = reader.Read(buffer, offset: 0, count: Client.BufferSizeBytes);
                    if (bytesRead == 0)
                        break;
                    remoteStream.Write(buffer, offset: 0, size: bytesRead);
                }
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
            Sender?.Dispose();
        }
    }
}