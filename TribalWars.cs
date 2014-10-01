using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using HtmlAgilityPack;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace twbot
{
    class TribalWars
    {       
        private Browser _m;
        private string _host;
        private string _user;
        private string _password;
        private string _hkey; // hkey is required to perform actions
        private bool _loggedIn;
        private List<VillageData> _data; // contains data of all villages in a list
        private volatile bool _build; // controls the building process
        private volatile int _buildingspeed; // the higher this value is the slower is the building process
        private volatile bool _research;


        // TODO: expand and complete
        public enum View
        {
            MAIN,
            OVERVIEW_COMBINED,
            MAP,
            MESSAGES,
        };

        public TribalWars(string ip, Browser m = null)
        {
            _m = m ?? new Browser();
            _host = ip;
            _user = "";
            _password = "";
            _hkey = "";
            _loggedIn = false;
            _data = null;
            _build = true;
            _buildingspeed = 200;
            _research = true;
        }

        // logs into the tribalwars server using the provided credentials
        // host has to be provided to the constructor
        // TODO: check webserver if its twlan
        public bool login(string name, string password)
        {
            // TODO: urlencode
            _user = name;
            _password = password;

            

            // login using POST
            int status = _m.post(Browser.construct(_host, "index.php", "action=login"), "user="+_user+"&password="+_password);
            if (status == 302)
            { // expected redirection , login failure otherwise
                string location = _m.getRedirect();

                Console.WriteLine("[login] Redirect to "+location);
                
                string url = Browser.construct(_host, location);
                status = _m.get(url); // should be ok, some error otherwise
                if (status == 200)
                    _loggedIn = true;
                else
                {
                    Console.WriteLine("[login] Request to "+url+" returned "+status);
                    Console.WriteLine("[login] Therefore not logged in.");
                }
            }else
            {
                Console.WriteLine("[login] Login request returned " + status + " ( and not 302 as expected)");
                if (status == 200)
                {
                    Console.WriteLine("[login] Check login credentials.");
                }
            }
            return _loggedIn;
        }

        // constructs an URL to build a building (hkey is required to work)
        private string actionBuild(string building, int village, string hkey)
        {
            return Browser.construct(_host, "game.php", "village="+village+"&screen=main&action=build&id="+building+"&h="+hkey);
        }


        private string viewUrl(int village, string screen, string addition=null)
        {
            return Browser.construct(_host, "game.php", "village="+village+"&screen="+screen)+ (addition ?? "");
        }
/*
 *
 *
 *  BUILDING PROCESS
 *
 *
 *
 */
        // should be started as a thread
        // does the building of the villages
        // use pause_build() to pause building and continue_build() to continue
        public void doBuild()
        {
            string build = "";
            bool queue = false;
            Browser mbuild = new Browser();
            while (!_loggedIn)
            {
                Console.WriteLine("[build] Waiting for login...");
                Thread.Sleep(500);
            }

            mbuild.setCookies(_m.getCookies());
            _build = true;
            // Stopwatch stopwatch = new Stopwatch();
            // stopwatch.Start();
            while (_build)
            {
                foreach(VillageData village in _data)
                {
                    int id = village.id;
//                    Console.WriteLine("[build:{0}] GET overview", id);
                    mbuild.get(viewUrl(id, "overview")); // get the overview to watch buildings & resources
                    Parse.parseOverview(mbuild.getContent(), ref village.buildings, ref queue);
                    if (queue == true)
                        continue;
                    // decide which building should be built
                    lock (village.buildings)
                    {
                        build = whichBuilding(ref village.buildings);
                    }

                    
                    if (build != null)
                    {
                        string url = actionBuild(build, id, _hkey);
                        mbuild.get(url);
                        Console.WriteLine("[build: {0} @ stage {3}] build {1} -> {2}", id, build, village.buildings.get(build)+1, village.buildings.level);
                    }else
                    {
                        /*
                        stopwatch.Stop();
                        Console.WriteLine("Time elapsed: {0}",
                                        stopwatch.Elapsed);
                        Console.WriteLine("done!");
                        Console.ReadKey();
                        */
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


/*
 *
 *
 * RESEARCH PROCESS
 *
 *
 */

        // should be started as a thread
        // handles the research in every village
        // use pause_research() to pause researching and continue_research() to continue
        public void doResearch()
        {
            string research = "";
            bool queue = false;
            string content;
            Browser mres = new Browser();
            while (!_loggedIn)
            {
                Console.WriteLine("[research] Waiting for login...");
                Thread.Sleep(500);
            }

            mres.setCookies(_m.getCookies());
            _research = true;
            // Stopwatch stopwatch = new Stopwatch();
            // stopwatch.Start();
            while (_research)
            {
                foreach(VillageData village in _data)
                {
                    int id = village.id;
                    mres.get(viewUrl(id, "smith")); // get the overview to get research levels
                    content = mres.getContent();
                    // TODO!
                    //Parse.parseSmithOverview(mbuild.getContent(), ref village.research, ref queue);
                    //if (queue == true)
                    //    continue; // skip if there is already something being researched
                    
                    // dirty method: search for links to research
                    string path = Parse.searchLink(content, "action=research&");
                    if (path == null)
                    { // no link found
                        continue;
                    }
                    research = Parse.retrieveParam(path, "id");
                    Console.WriteLine("[research:{0}] do  "+research, id);
                    string url = Browser.construct(_host, path);
                    int status = mres.get(url);
                    if (status != 302)
                    { // some error must've occured
                        Console.WriteLine("[research:{0}] Could not research {1} (see id_{0}_research_error.html)", id, research);
                        mres.save("id_"+id+"_research_error.html");
                    }
                    
                    //Thread.Sleep(_researchspeed); // not implemented yet!
                }
            }
        }

       // pauses the researching process
        public void pauseResearch()
        {
            _research = false;
        }

        // continues the researching process
        public void continueResearch()
        {
            _research = true;
        }



/*
 *
 *
 *
 * INIT SCAN
 *
 *
 *
 */
       // initiates an initial scan to fill the structures at startup.
        public void initScan()
        {
            // TODO:
            // * get session token (hkey)
            // * get all villages info (buildings, troops, resources?)
            // * mails (unimportant)
            // * get point + rank
            // *
            if (File.Exists("data.bin"))
            { // earlier data exists. load it and proceed with it
                using (var file = File.OpenRead("data.bin"))
                {
                    var reader = new BinaryFormatter();
                    // read entire village data
                    _data = (List<VillageData>) reader.Deserialize(file);
                }
            }else
            {
                _data = new List<VillageData>();
                string url = _m.getUrl();
                Console.WriteLine("[initScan] Current URL: "+url);
                int village_id = int.Parse(Parse.retrieveParam(url, "village"));
                Console.WriteLine("[initScan] Current village is: " + village_id); 
                
                _m.get(viewUrl(village_id, "overview_villages", "&mode=prod"));
                string content = _m.getContent();

                _hkey = Parse.parseHkey(content); // get current hkey and save globally
                if (_hkey == null)
                    throw new Exception();
                Console.WriteLine("hkey: "+_hkey);
                
                List<short> village_ids = Parse.parseVillagesOverview(content);
                foreach (var id in village_ids)
                {
                    VillageData village = parseVillage(id);
                    village.id = id;
                    Console.WriteLine(village.ToString());
                    _data.Add(village);
                }
            }           
        }

        // queries a single village and returns its data in the
        // VillageData structure.
        // TODO: resources, units
        private VillageData parseVillage(short id)
        {
            VillageData village = new VillageData();
            BuildingData buildings = new BuildingData();

            // query the village overview
            _m.get(viewUrl(id, "overview"));
            string content = _m.getContent();
            
            // parse the overview and save it in the struct.
            bool queue = false;
            Parse.parseOverview(content, ref buildings, ref queue);
            village.buildings = buildings;



            return village;
        }

    }
}
