using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace AutomatedTesting
{
    class Program
    {
        private static readonly string pathToNode
            = @"C:\Users\Андрей\source\repos\NsuNetworkProgramming\TreeChat\TreeChat\bin\Debug\TreeChat.exe";

        private static IEnumerable<string> NextNodeArgumentLine()
        {
            yield return "Root 0 11000";
            yield return "Left 0 12000 127.0.0.1 11000";
            yield return "Right 0 13000 127.0.0.1 11000";
            yield return "Bottom 0 14000 127.0.0.1 12000";
        }


        static void Main()
        {
            /*
                          (Root) 11k
                         /      \
                        /        \
               12k   (left)     (right) 13k
                    /
                   /
           14k  (bottom)
             
             */
            foreach (var arguments in NextNodeArgumentLine())
            {
                Process.Start(pathToNode, arguments);
                Wait();
            }

        }

        private static void Wait()
        {
            Thread.Sleep(TimeSpan.FromSeconds(new Random().Next(3, 5)));
        }
    }
}
