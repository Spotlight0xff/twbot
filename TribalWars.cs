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
        private bool _debug;
        private Browser _m;
        private string _host;
        private string _user;
        private string _password;
        private string _hkey; // hkey is required to perform actions
        private bool _loggedIn;
        private List<VillageData> _data; // contains data of all villages in a list
        private Research _research;
        private Building _building;


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
            _building = new Building();
            _research = new Research();

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

        public void startBuilding()
        {
            _building.setCookies(_m.getCookies());
            _building.setHKey(_hkey);
            _building.updateData(ref _data);
            _building.setHost(_host);
            _building.Start();
        }

        public void startResearch()
        {
            _research.setCookies(_m.getCookies());
            _research.setHKey(_hkey);
            _research.updateData(ref _data);
            _research.setHost(_host);
            _research.Start();
        }

/*
 *
 *
 * RESEARCH PROCESS
 *
 *
 */



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
                
                _m.get(Parse.viewUrl(_host, village_id, "overview_villages", "&mode=prod"));
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
            _m.get(Parse.viewUrl(_host, id, "overview"));
            string content = _m.getContent();
            
            // parse the overview and save it in the struct.
            bool queue = false;
            Parse.parseOverview(content, ref buildings, ref queue);
            village.buildings = buildings;



            return village;
        }

    }
}
