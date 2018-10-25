using System;
using System.Net;

namespace TcpFileTransfer
{
    class ClientProgram
    {
        public static void Main(string[] args)
        {
            try
            {
                if (args.Length != 3)
                {
                    Console.WriteLine("Usage: Client.exe {filename} {ip} {port}");
                    return;
                }

                using (var client = new Client(serverAddress: args[1],
                    port: int.Parse(args[2]), pathToFile: args[0]))
                {
                    client.SendInfo();
                    switch (client.ReceiveServerResponse())
                    {
                        case ResponseCodes.SendingFile:
                            Console.WriteLine("Starting sending file...");
                            break;
                        case ResponseCodes.FileAlreadyExists:
                            Console.WriteLine("File with this name already exists." +
                                              "Choose different name.");
                            break;
                        case ResponseCodes.ServerTooBusy:
                            Console.WriteLine("Server is too busy.");
                            break;
                    }

                    client.SendFile();

                    switch (client.ReceiveServerResponse())
                    {
                        case 20:
                            Console.WriteLine("File successfully sent.");
                            break;
                        case 21:
                            Console.WriteLine("Error while sending file.");
                            break;

                    }

                }
            }


            catch (Exception exp)
            {
                Console.WriteLine("Exception occurred: " + exp);
            }
        }
    }
}
