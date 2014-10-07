using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
namespace twbot
{
    public class Building
    {
        private Browser _mbuild;
        private int _buildingspeed;
        private bool _build;

        public Building()
        {
            _build = false;
            _buildingspeed = 0; // the higher the slower the process
            _mbuild = new Browser();
        }


        // should be started as a thread
        // does the building of the villages
        // use pause_build() to pause building and continue_build() to continue
        public void doBuild(CookieContainer cookies)
        {
            _mbuild.setCookies(cookies);
            string build = "";
            bool queue = false;
            _build = true;
            List<VillageData> _data = TribalWars._data;
            while (_build)
            {
                foreach(VillageData village in _data)
                {
                    int id = village.id;
                    _mbuild.get(viewUrl(id, "overview")); // get the overview to watch buildings & resources
                    Parse.parseOverview(_mbuild.getContent(), ref village.buildings, ref queue);
                    if (queue == true)
                        continue;
                    // decide which building should be built
                    lock (village.buildings)
                    {
                        build = whichBuilding(ref village.buildings);
                    }

                    
                    if (build != null)
                    { // do we have something to do?
                        string url = actionBuild(build, id, _hkey);
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
                    }
                    level ++;
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
