using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Threading;


namespace twbot
{
    class Module
    {
        protected Browser _browser;
        protected List<VillageData> _data;
        protected string _hkey;
        protected string _host;
        protected bool _active;
        protected bool _debug;

        public Module()
        {
            _debug = false;
            _data = null;
            _active = false;
            _browser = new Browser();
            _hkey = null;
            _host = null;
        }

        public void setHost(string host)
        {
            _host = host;
            Console.WriteLine("Host got set!");
        }

        public void updateData(ref List<VillageData> data)
        {
            _data = data;
            Console.WriteLine("Data got updated!");
        }


        public void setCookies(CookieContainer cookies)
        {
            _browser.setCookies(cookies);
            Console.WriteLine("Cookies got set.");
        }

        public void setHKey(string hkey)
        {
            Console.WriteLine("hkey got set/updated");
            _hkey = hkey;
        }

        public virtual void doWork()
        {}

        // pauses the process
        public void pauseWork()
        {
            _active = false;
        }

        // resumes the process
        public void resumeWork()
        {
            _active = true;
        }
 
        public void Start()
        {
            Thread workThread = new Thread(doWork);
            workThread.Start();
        }

    }
}
