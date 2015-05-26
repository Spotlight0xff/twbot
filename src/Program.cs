using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace twbot
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                run();
            } catch (Exception e)
            {
                Console.WriteLine("{0}{1}", e.GetType(), e.StackTrace);
            }
            // only on windows:
            // Console.ReadLine();
        }

        private static void run()
        {
            TribalWars tw = new TribalWars("127.0.0.1");
            bool login = tw.login("bot0", "bot0");
            if (!login)
            {
                Console.WriteLine("Unable to login");
                return;
            }
            tw.initScan();
            tw.startBuilding();
            tw.startResearch();
            tw.startMonitoring();
        }
    }
}
