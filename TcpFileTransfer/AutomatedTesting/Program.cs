using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutomatedTesting
{
    class Program
    {
        private static readonly string pathToClient
            = @"C:\Users\Андрей\source\repos\NsuNetworkProgramming\TcpFileTransfer\Client\bin\Debug\Client.exe";


        private static readonly string pathToServer
            = @"C:\Users\Андрей\source\repos\NsuNetworkProgramming\TcpFileTransfer\Server\bin\Debug\Server.exe";

        private static readonly IEnumerable<string> filesToTestTransfering = new[]
        {
            "SourceTreeSetup-3.0.8.exe",
            "apktool_2.3.4.exe",
            "APK Easy Tool 1.541 Portable.zip",
            "VSCodeUserSetup-x64-1.28.2.exe",
            "Git.pptx",
            "astah-uml-7_2_0-1ff236-jre-64bit-setup.exe",
            "Bytecode-Viewer-2.9.11.exe",
        };


        static void Main()
        {
            var random = new Random();
            var clientInfo = new ProcessStartInfo
            {
                FileName = pathToClient,
                //CreateNoWindow = true
            };

            Process.Start(pathToServer, "19000");

            foreach (string fileName in filesToTestTransfering)
            {
                Thread.Sleep(TimeSpan.FromSeconds(random.Next(1, 10)));

                clientInfo.Arguments =
                    $@"""C:\Users\Андрей\OneDrive\{fileName}"" 127.0.0.1 19000";

                Process.Start(clientInfo);
            }

        }
    }
}
