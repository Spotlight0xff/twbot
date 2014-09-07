using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using HtmlAgilityPack;
using System.Linq;
using System.Text.RegularExpressions;


namespace twbot
{
    class TribalWars
    {

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        [Serializable]
        private struct VillageData
        {
            short Id;
            string Name;
            short coord_x;
            short coord_y;
            BuildingData buildings;
            UnitsData units;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        [Serializable]
        private struct BuildingData
        {
            short building_main;
            short building_barracks;
            short building_stable;
            short building_garage;
            short building_snob;
            short building_smith;
            short building_place;
            short building_market;
            short building_wood;
            short building_stone;
            short building_iron;
            short building_farm;
            short building_storage;
            short building_hide;
            short building_wall;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        [Serializable]
        private struct UnitsData
        {
            short unit_spear;
            short unit_sword;
            short unit_axe;
            short unit_archer;
            short unit_spy;
            short unit_light;
            short unit_marcher;
            short unit_heavy;
            short unit_ram;
            short unit_catapult;
            short unit_snob;
        }


        private Browser _m;
        private string _host;
        private string _user;
        private string _password;
        private bool _loggedIn;
        private List<VillageData> _data;
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
        }


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
                    _data.Add(village);
                }
            }           
        }

        // queries a single village and returns its data in the
        // VillageData structure.
        private VillageData parseVillage(short id)
        {
            VillageData village = new VillageData();

            _m.get(viewUrl(id, "overview"));
            HtmlAgilityPack.HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(_m.getContent());
            if (doc.ParseErrors != null && doc.ParseErrors.Count() > 0 )
            {
                Console.WriteLine("Parse error occured @ parseOverview");
                doc.Save("failed.html");
                return village;
            }

            foreach (var row in doc.DocumentNode.SelectNodes("//table[@class='vis']/tr/td[@width='50%']"))
            {

                
                var links = row.Descendants("a");
                string building = "";
                int level = 0;
                foreach (var link in links)
                { // get href-links from <a>-Tags
                    string url = link.Attributes["href"].Value;
                    building = retrieveParam(url, "screen");
                }

                // Match regexp from "Marktplatz (Stufe 5) to 5"
                Match match = Regex.Match(row.InnerText, @" [\u00C0-\u017Fa-zA-Z]* \([a-zA-Z]* ([0-9]{1,2})\)$", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    level = int.Parse(match.Groups[1].Value);
                }else
                {
                    continue;
                }

                Console.WriteLine(building + " => " + level);
            }

            return village;
        }

        // tries to retrieve the parameter specified in 'param' from an URL
        // example: url = "http://192.168.2.100/game.php?village=42&screen=overview"
        // retrieveParam(url, "village");
        // ^ returns "42"
        public static string retrieveParam(string url, string param)
        {
            url = url.Replace("&amp;", "&");
//            Console.WriteLine("Taking apart: "+url);
            string query = url.Split(new Char[] {'?'})[1];
//            Console.WriteLine("Query-part: " + query);
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
