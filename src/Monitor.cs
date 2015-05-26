using System;
using System.Net;
using System.Threading;
using System.IO;
using System.Timers;
using System.Diagnostics;

namespace twbot
{
    class Monitor : Module
    {
        // should be started as a thread
        // handles the monitoring, writes data to file 
        public override void doWork()
        {
            bool queue = false;
            _active = true;
            System.Timers.Timer timer = new System.Timers.Timer(500);
            timer.Elapsed += new ElapsedEventHandler(elapsed);
            timer.Enabled = true;
            while (_active)
            {
                Thread.Sleep(20000);
            }
        }

        private void elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (VillageData village in _data)
            {
                if (village.buildings.get("hide") >= 10)
                    continue;
                TimeSpan now = DateTime.Now - Process.GetCurrentProcess().StartTime;
                File.AppendAllText("monitor/monitor_"+village.id, now.TotalMilliseconds.ToString() + "\n");
                File.AppendAllText("monitor/monitor_"+village.id, village.ToString() + "\n");
            }
        }
    }
}
