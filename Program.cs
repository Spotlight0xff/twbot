using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            TribalWars tw = new TribalWars("192.168.2.100");
            bool login = tw.login("spotlight", "3726");
            if (!login)
            {
                Console.WriteLine("Unable to login. Check login credentials.");
                return;
            }
            tw.init_scan();
        }
    }
}
