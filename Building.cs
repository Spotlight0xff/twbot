using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Newtonsoft.Json;


namespace twbot
{
    class Building
    {
        private Browser _mbuild;
        private bool _build;
        private List<VillageData> _data;
        private string _hkey;
        private string _host;

        public Building(string host)
        {
            _data = null;
            _build = false;
            _mbuild = new Browser();
            _hkey = null;
            _host = host;
        }

        public void updateData(ref List<VillageData> data)
        {
            _data = data;
            Console.WriteLine("Data got updated!");
        }


        public void setCookies(CookieContainer cookies)
        {
            _mbuild.setCookies(cookies);
            Console.WriteLine("Cookies got set.");
        }

        public void setHKey(string hkey)
        {
            Console.WriteLine("hkey got set/updated");
            _hkey = hkey;
        }

        public void Start()
        {
            Thread buildThread = new Thread(doBuild);
            buildThread.Start();
        }

        // should be started as a thread
        // does the building of the villages
        // use pause_build() to pause building and continue_build() to continue
        public void doBuild()
        {
            string build = "";
            bool queue = false;

            _build = true;
            while (_build)
            {
                foreach(VillageData village in _data)
                {
                    int id = village.id;
                    _mbuild.get(Parse.viewUrl(_host, id, "overview")); // get the overview to watch buildings & resources
                    Parse.parseOverview(_mbuild.getContent(), ref village.buildings, ref queue);
                    if (queue == true)
                        continue;
                    // decide which building should be built
                    lock (village.buildings)
                    {
                        build = whichBuilding(ref village.buildings);
                    }

                    
                    if (build != null)
                    {
                        string url = Parse.actionBuild(_host, build, id, _hkey);
                        _mbuild.get(url);
                        Console.WriteLine("[build: {0} @ stage {3}] build {1} -> {2}", id, build, village.buildings.get(build)+1, village.buildings.level);
                    }
                    //Thread.Sleep(_buildingspeed);
                }
            }
        }

        private string whichBuilding(ref BuildingData buildings)
        {
            using (StreamReader sr = new StreamReader("build.json"))
            {
                int level = 1;
                String json = sr.ReadToEnd();
//                Console.WriteLine(json);

                List<Dictionary<string,short>> values = JsonConvert.DeserializeObject<List<Dictionary<string, short>>>(json);
                foreach (Dictionary<string, short> val in values)
                {
                    foreach (KeyValuePair<string, short> pair in val)
                    {
                        if (buildings.get(pair.Key) < pair.Value)
                        {
                            buildings.level = level;
 //                           Console.WriteLine("[{0}] is: {1}, should: {2}", pair.Key, buildings.get(pair.Key), pair.Value);
 //                           Console.WriteLine("Village is in stage "+level.ToString());
                            return pair.Key;
                        }
//                        Console.WriteLine("{0}, {1}", pair.Key, pair.Value);
                    }
                    level ++;
//                    Console.WriteLine();
                }
            }
            return null;
        }

        // pauses the building process
        public void pauseBuild()
        {
            _build = false;
        }

        // continues the building process
        public void continueBuild()
        {
            _build = true;
        }
    }
}
