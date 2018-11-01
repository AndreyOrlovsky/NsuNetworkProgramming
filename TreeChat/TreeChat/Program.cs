using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TreeChat
{
    class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                IPEndPoint endPoint = null;

                if (args.Length == 5)
                {
                    endPoint = new IPEndPoint(IPAddress.Parse(args[3]), int.Parse(args[4]));
                }

                else if (args.Length != 3)
                {
                    Console.WriteLine("Usage: TreeChat.exe {username} {losspercent} {port} [optional: {parentip} {parentport}]");
                    return;
                }

                TreeChat chat = new TreeChat(userName: args[0], lossPercent: int.Parse(args[1]), 
                    parentNode: endPoint, localPort: int.Parse(args[2]));

                chat.Launch();

                // Process.GetCurrentProcess().Exited += (sender, eventArgs) => chat.InformAboutExit();

                string text;
                while ((text = Console.ReadLine()) != @"\quit")
                {
                     chat.SendMessage(text);
                }

                chat.Stop();
            }

            catch (Exception e)
            {
                Console.WriteLine($"[Error]: Something went wrong: {e.Message}");
            }
        }
    }
}
