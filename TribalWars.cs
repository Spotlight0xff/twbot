using System;
using System.Collections.Generic;
using System.Text;
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
        private bool _loggedIn;
        private List<VillageData> _data; // contains data of all villages in a list
        private volatile bool _build; // controls the building process
        private volatile int _buildingspeed; // the higher this value is the slower is the building process


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
            _loggedIn = false;
            _data = null;
            _build = true;
            _buildingspeed = 200;
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

        private string viewUrl(int village, string screen, string addition=null)
        {
            return Browser.construct(_host, "game.php", "village="+village+"&screen="+screen)+ (addition ?? "");
        }
/*
        public bool getView(View view, int village)
        {
            Console.WriteLine("Change view to {0}", view);
            string url;
            switch (view)
            {
                case View.MAIN:
                    url = viewUrl(village, "main");
                    break;

                case View.OVERVIEW_COMBINED:
                    url = viewUrl(village, "overview_combined");
                    break;

                case View.MESSAGES:
                    url = viewUrl(village, "mail");
                    break;

                default:
                    return false;
            }

            _m.get(url);
            return true;
        }


        */
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
        //
        public void doBuild()
        {
            Browser mbuild = new Browser();
            while (!_loggedIn)
            {
                Console.WriteLine("[build] Waiting for login...");
                Thread.Sleep(500);
            }

            mbuild.setCookies(_m.getCookies());
            _build = true;
            while (_build)
            {
                foreach(VillageData village in _data)
                {
                    int id = village.Id;
                    Console.WriteLine("[build:{0}] GET overview", village.Id);
                    mbuild.get(viewUrl(id, "overview")); // get the overview to watch buildings & resources
                    Parse.parseOverview(mbuild.getContent(), ref village.buildings);

                    Thread.Sleep(_buildingspeed);
                }
            }
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
            // * get session token
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
                int village_id = int.Parse(retrieveParam(url, "village"));
                Console.WriteLine("[initScan] Current village is: " + village_id); 
                _m.get(viewUrl(village_id, "overview_villages", "&mode=prod"));
                List<short> village_ids = Parse.parseVillagesOverview(_m.getContent());
                foreach (var id in village_ids)
                {
                    VillageData village = parseVillage(id);
                    village.Id = id;
                    Console.WriteLine(village.ToString());
                    _data.Add(village);
                }
            }           
        }

        // queries a single village and returns its data in the
        // VillageData structure.
        private VillageData parseVillage(short id)
        {
            VillageData village = new VillageData();
            BuildingData buildings = new BuildingData();

            // query the village overview
            _m.get(viewUrl(id, "overview"));
            string content = _m.getContent();
            
            // parse the overview and save it in the struct.
            Parse.parseOverview(content, ref buildings);
            village.buildings = buildings;



            return village;
        }

        // tries to retrieve the parameter specified in 'param' from an URL
        // example: url = "http://192.168.2.100/game.php?village=42&screen=overview"
        // retrieveParam(url, "village");
        // ^ returns "42"
        public static string retrieveParam(string url, string param)
        {
            url = url.Replace("&amp;", "&");

            string query = url.Split(new Char[] {'?'})[1];
            string[] queries = query.Split(new Char[] {'&'});
            foreach (string arg in queries)
            {
                string[] parts = arg.Split(new Char[] {'='});
                if (parts[0].Equals(param))
                    return parts[1];
            }

            return null;
        }
    }
}
