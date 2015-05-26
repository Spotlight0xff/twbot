using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;


namespace twbot
{
    /// <summary>Parses HTML content</summary>
    public class Parse
    {

        /// <summary>
        /// Parses the H-Key on a HTML page.
        /// Should work on any logged-in page as we parse the logout link.
        /// </summary>
        /// <param name="html">HTML content to parse</param>
        ///	<returns>hkey if found, else null</returns>
        public static string parseHkey(string html)
        {
            string hkey = null;
            HtmlAgilityPack.HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            if (!handleError(doc, "parseHkey"))
                return null;


            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//a"))
            {
                string href = node.GetAttributeValue("href", null);
                string action = retrieveParam(href, "action");
                if (action != null)
                {
                    if (action.Equals("logout"))
                    {
                        hkey = retrieveParam(href, "h");
                    }
                }
            }
            return hkey;
        }


        /// <summary>
        /// parses the production overview to get all village IDs
        /// </summary>
        /// <param name="html">HTML content to parse</param>
        ///	<returns>List of Village IDs</returns>
        public static List<short> parseVillagesOverview(string html)
        {
            List<short> list = new List<short>();
            HtmlAgilityPack.HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            if (!handleError(doc, "parseVillagesOverview"))
                return null;
            foreach (var node in doc.DocumentNode.SelectNodes("//table[@class='vis']/tr"))
            {
                var link = node.Descendants("a").Where(x => x.Attributes.Contains("href"));
                foreach (var a in link)
                {
                    string url = a.Attributes["href"].Value;
                    string village = retrieveParam(url, "village");
                    list.Add(short.Parse(village));
                }
            }

            return list;
        }

        /// <summary>
        /// Parses a HTML page for resource information (wood/stone/iron) and storage capacity.
        /// Saves the resources in the provided Resources variable.
        /// </summary>
        /// <param name="html">HTML page to parse</param>
        /// <param name="res">Resource to save to</param>
        ///	<returns>false on error, true on success</returns>
        public static bool parseResources(string html, ref Resources res)
        {
            Match match = null;
            res.resources = new Dictionary<string, int>();
            string[] types = new string[] {"wood", "stone", "iron"};
            
            foreach (string type in types)
            {
                // Match regexp from "<td><span id="wood" title="21600000" class="warn">400000</span>"
                match = Regex.Match(html, "<td><span id=\"" + type + "\" [^>.]*>([0-9]*)</span>", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    int val = int.Parse(match.Groups[1].Value);
                    res.resources.Add(type, val);
                }else
                {
                    File.WriteAllText("debug/resource_fail.html", html);
                    Console.WriteLine("Failed to read resources! check debug/ ("+res.resources.Count().ToString()+")");
                    return false;
                }
            }

            match = Regex.Match(html, "<td id=\"storage\">([0-9]*)</td>", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                int val = int.Parse(match.Groups[1].Value);
                res.storage_max = val;
            }else
            {
                File.WriteAllText("debug/resource_fail_storage.html", html);
                Console.WriteLine("Failed to read storage! check debug/");
                return false;
            }

            return true;
       }


        // parse the village overview
        // cur_building is true when the 
        /// <summary>
        /// Parse the village overview to get building levels.
        /// Saves the information in the provided `buildings` variable.
        /// </summary>
        /// <param name="html">HTML page to parse</param>
        /// <param name="buildings">variable to save the found levels</param>
        /// <param name="queue">writes true to this if there is a queue</param>
        ///	<returns>false on error, true on success</returns>
        public static bool parseOverview(string html, ref BuildingData buildings, ref bool queue)
        {

            if (buildings == null)
                buildings = new BuildingData();

            HtmlAgilityPack.HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            if (!handleError(doc, "parseOverview"))
                    return false;


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
                // supports Umlaute
                Match match = Regex.Match(row.InnerText, @" [\u00C0-\u017Fa-zA-Z]* \([a-zA-Z]* ([0-9]{1,2})\)$", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    level = short.Parse(match.Groups[1].Value);
                }else
                {
                    continue;
                }

                if (building.Equals("main"))
                { // check if the village is building something
                    var node = row.ParentNode;
                  //  Console.WriteLine("\"" + node.InnerHtml +  "\"");

                    var secondNode = row.ParentNode.SelectNodes("td")[1];
                    if (String.IsNullOrWhiteSpace(secondNode.InnerHtml))
                    {
                        queue = false;
                    }else
                    {
                        queue = true;
                    }
/*                    foreach (var child in row.ParentNode.SelectNodes("td"))
                    {
                        Console.WriteLine(child.InnerHtml);
                        Console.WriteLine("---");
                    }
  */              }

                buildings.set(building, level);
            }
            return true;
        }
 


        /// <summary>
        /// Search a HTML page for a link with a specific match
        /// </summary>
        /// <param name="html">HTML page to parse</param>
        /// <param name="query">Search query</param>
        ///	<returns>Link if found or null otherwise</returns>
        public static string searchLink(string html, string query)
        { 
            HtmlAgilityPack.HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            if (!handleError(doc, "searchLink"))
                return null;

            IEnumerable<HtmlNode> links = doc.DocumentNode.Descendants("a").Where(x => x.Attributes.Contains("href"));
            foreach (var link in links)
            {
                if(link.Attributes["href"].Value.LastIndexOf(query) != -1)
                    return link.Attributes["href"].Value;
            }


            return null;
        }



        /// <summary>
        /// Tries to retrieve a parameter in the provided URL.
        /// For example:
        /// url = "http://127.0.0.1/game.php?village=42&amp;screen=overview"
        /// retrieveParam(url, "village");
        /// ^ returns "42"."
        /// </summary>
        /// <param name="url">URL to parse</param>
        /// <param name="param">Parameter</param>
        ///	<returns>Value of the parameter as a String</returns>
        public static string retrieveParam(string url, string param)
        {
            // Console.WriteLine("Process: "+url);
            url = url.Replace("&amp;", "&");
            string[] split_query = url.Split(new Char[] {'?'});
            if (split_query.Length < 2)
                return null;
            string query = split_query[1];
            // Console.WriteLine("query: "+query);
            string[] queries = query.Split(new Char[] {'&'});
            foreach (string arg in queries)
            {
                string[] parts = arg.Split(new Char[] {'='});
                if (parts[0].Equals(param))
                    return parts[1];
            }

            return null;
        }

        /// <summary>
        /// handles any parsing errors and saves the affected
        /// HTML content in a file to debug the error 
        /// (if a parsing error occured)
        /// </summary>
        /// <param name="doc">HtmlDocument to parse</param>
        /// <param name="func">Function name (to save it with an appropiate filename)</param>
        ///	<returns>true on success and false on parse error</returns>
        private static bool handleError(HtmlDocument doc, string func)
        {
            if (doc.ParseErrors != null && doc.ParseErrors.Count() > 0)
            {
                Console.WriteLine("Parse error occured @ parseOverview");
                doc.Save("failed_"+func+".html");
                return false;
            }
            return true;
        }

        /// <summary>
        /// construct an URL to build a building (hkey required to work).
        /// </summary>
        /// <param name="host">Host (Domain)</param>
        /// <param name="building">building to build</param>
        /// <param name="village">village ID</param>
        /// <param name="hkey">hkey to include into the URL</param>
        ///	<returns>constructed URL</returns>
        public static string actionBuild(string host, string building, int village, string hkey)
        {
            return Browser.construct(host, "game.php", "village="+village+"&screen=main&action=build&id="+building+"&h="+hkey);
        }

        /// <summary>
        /// construct an URL to a specified view.
        /// </summary>
        /// <param name="host">Host (Domain)</param>
        /// <param name="village">Village ID</param>
        /// <param name="screen">specified screen-view</param> 
        /// <param name="addition">any addition to the URL</param>
        ///	<returns>constructed URL</returns>
        public static string viewUrl(string host, int village, string screen, string addition=null)
        {
            return Browser.construct(host, "game.php", "village="+village+"&screen="+screen)+ (addition ?? "");
        }



    }
}
