using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TcpFileTransfer
{
    class ClientHandler
    {
        public TcpClient FileSender { get; }
        public FileInfo FileToReceive { get; private set; }
        public int FileSize { get; private set; }
        public string Folder { get; }
        public int TotalReceived { get; private set; }
        public int InstantReceived { get; set; }
        public DateTime ReceiveStartedMoment { get; private set; }
        public DateTime InstantCheckMoment { get; set; }

        private NetworkStream remoteStream;
        private readonly IPEndPoint remoteEndPoint;
        private byte[] buffer = new byte[Server.BufferSizeBytes];
        public string PathToFile => $"{Folder}\\{FileToReceive.Name}";


        public ClientHandler(TcpClient fileSender)
        {
            this.FileSender = fileSender;
            remoteStream = fileSender.GetStream();
            remoteEndPoint = (IPEndPoint) fileSender.Client.RemoteEndPoint;
            Folder = $"uploads\\{remoteEndPoint.Address}";
            Directory.CreateDirectory(Folder);

        }

        public void ReceiveInfo()
        {
            buffer = new byte[Server.BufferSizeBytes];
            int bytesReceived = remoteStream.Read(buffer, offset: 0, size: Server.BufferSizeBytes);
            string[] fileInfo = Encoding.UTF8
                .GetString(buffer, 0, bytesReceived)
                .Split('\n');

            FileToReceive = new FileInfo(fileInfo[0]);
            FileSize = int.Parse(fileInfo[1]);



        }

        public void ReceiveFile()
        {
            ReceiveStartedMoment = DateTime.Now;
            InstantCheckMoment = DateTime.Now;

            buffer = new byte[Server.BufferSizeBytes];

            using (FileStream writer = new FileStream(PathToFile,
                FileMode.Create, FileAccess.Write))
            {

                while (true)
                {
                    int bytesReceived = remoteStream.Read(buffer, offset: 0, size: Server.BufferSizeBytes);

                    if (bytesReceived == 0)
                    {
                        break;
                    }

                    writer.Write(buffer, offset: 0, count: buffer.Length);

                    TotalReceived += bytesReceived;
                    lock (Locker.Lock)
                    {
                        InstantReceived += bytesReceived;
                    }

                }


            }
        }

        public void SendResponse(byte code)
        {
            byte[] buffer = new byte[1] {code};
            remoteStream.Write(buffer, offset: 0, size: buffer.Length);
        }
    }
}
