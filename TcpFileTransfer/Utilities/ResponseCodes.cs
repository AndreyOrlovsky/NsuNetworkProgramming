using System;

namespace TcpFileTransfer
{
    public static class ResponseCodes
    {
        public const int SendingFile = 10;
        public const int FileAlreadyExists = 11;
        public const int ServerTooBusy = 12;

        public const int FileSent = 20;
        public const int ErrorOccurred = 21;
    }
}
