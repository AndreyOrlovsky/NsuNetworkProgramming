using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocksProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            var proxy = new Proxy(17005);

            proxy.Start();

            while (true)
            {

            }
        }
    }
}
