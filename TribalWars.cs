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
        public class VillageData
        {
            public short Id;
            public string Name;
            public short coord_x;
            public short coord_y;
            public BuildingData buildings;
            public UnitsData units;

            public string ToString()
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(this);
            }

        }

        public class BuildingData
        {
            public short building_main;
            public short building_barracks;
            public short building_stable;
            public short building_garage;
            public short building_snob;
            public short building_smith;
            public short building_place;
            public short building_market;
            public short building_wood;
            public short building_stone;
            public short building_iron;
            public short building_farm;
            public short building_storage;
            public short building_hide;
            public short building_wall;



            public void set(string building, short level)
            {
                query(building, false, level);
            }

            public short get(string building)
            {
                return query(building, true);
            }

            public string ToString()
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(this);
            }

            /*
             * Method to set or get building levels.
             * set op to true to get building level and false to set it
             *  returns the requested building level
             */
            private short query(string building, bool op, short level = 0)
            {
                switch (building)
                {
                    case "main":
                        return (op ? building_main : building_main=level);

                    case "barracks":
                        return (op ? building_barracks : building_barracks=level);

                    case "stable":
                        return (op ? building_stable : building_stable=level);

                    case "garage":
                        return (op ? building_garage : building_garage=level);

                    case "snob":
                        return (op ? building_snob : building_snob=level);

                    case "smith":
                        return (op ? building_smith : building_smith=level);

                    case "place":
                        return (op ? building_place : building_place=level);

                    case "market":
                        return (op ? building_market : building_market=level);

                    case "wood":
                        return (op ? building_wood : building_wood=level);

                    case "stone":
                        return (op ? building_stone : building_stone=level);

                    case "iron":
                        return (op ? building_iron : building_iron=level);

                    case "farm":
                        return (op ? building_farm : building_farm=level);

                    case "storage":
                        return (op ? building_storage : building_storage=level);
 
                    case "hide":
                        return (op ? building_hide : building_hide=level);

                    case "wall":
                        return (op ? building_wall : building_wall=level);

 

                }

                return 0;
            }

        }

        public class UnitsData
        {
            public short unit_spear;
            public short unit_sword;
            public short unit_axe;
            public short unit_archer;
            public short unit_spy;
            public short unit_light;
            public short unit_marcher;
            public short unit_heavy;
            public short unit_ram;
            public short unit_catapult;
            public short unit_snob;

            public string ToString()
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(this);
            }   

        }


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

                Console.WriteLine("Redirect to "+location);
                
                string url = Browser.construct(_host, location);
                status = _m.get(url); // should be ok, some error otherwise
                if (status == 200)
                    _loggedIn = true;
                else
                {
                    Console.WriteLine("Request to "+url+" returned "+status);
                    Console.WriteLine("Therefore not logged in.");
                }
            }else
            {
                Console.WriteLine("Login request returned " + status + " ( and not 302 as expected)");
                if (status == 200)
                {
                    Console.WriteLine("Check login credentials.");
                }
            }
            return _loggedIn;
        }

        private string viewUrl(int village, string screen, string addition=null)
        {
            return Browser.construct(_host, "game.php", "village="+village+"&screen="+screen)+ (addition ?? "");
        }

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

        // initiates an initial scan to fill the structures at startup.
        public void init_scan()
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
                Console.WriteLine("Current URL: "+url);
                int village_id = int.Parse(retrieveParam(url, "village"));
                Console.WriteLine("Current village is: " + village_id); 
                _m.get(viewUrl(village_id, "overview_villages", "&mode=prod"));
                List<short> village_ids = Parse.parseOverview(_m.getContent());
                foreach (var id in village_ids)
                {
                    VillageData village = parseVillage(id);
                    village.Id = id;
                    Console.WriteLine(village.ToString());
                    _data.Add(village);
                }
            }           
        }

        // should be started as a thread
        // does the building of the villages
        // use pause_build() to pause building and continue_build() to continue
        //
        public void do_build()
        {
            _build = true;
            while (_build)
            {

                Console.WriteLine("building...");
                Thread.Sleep(200);
            }
        }

        // pauses the building process
        public void pause_build()
        {
            _build = false;
        }

        // continues the building process
        public void continue_build()
        {
            _build = true;
        }

        // queries a single village and returns its data in the
        // VillageData structure.
        private VillageData parseVillage(short id)
        {
            VillageData village = new VillageData();
            BuildingData buildings = new BuildingData();

            // query the village overview
            _m.get(viewUrl(id, "overview"));

            HtmlAgilityPack.HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(_m.getContent());
            if (doc.ParseErrors != null && doc.ParseErrors.Count() > 0 )
            { // parser error
                Console.WriteLine("Parse error occured @ parseOverview");
                doc.Save("failed.html");
                return village;
            }

            foreach (var row in doc.DocumentNode.SelectNodes("//table[@class='vis']/tr/td[@width='50%']"))
            {   
                var links = row.Descendants("a");
                string building = "";
                short level = 0;
                foreach (var link in links)
                { // get href-links from <a>-Tags
                    string url = link.Attributes["href"].Value;
                    building = retrieveParam(url, "screen");
                }

                // Match regexp from "Marktplatz (Stufe 5) to 5"
                // support Umlaute
                Match match = Regex.Match(row.InnerText, @" [\u00C0-\u017Fa-zA-Z]* \([a-zA-Z]* ([0-9]{1,2})\)$", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    level = short.Parse(match.Groups[1].Value);
                }else
                {
                    continue;
                }

                buildings.set(building, level);
                Console.WriteLine(building + " => " + buildings.get(building) );
            }
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
