using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace twbot
{
    public class Parse
    {

        // tries to parse the hkey on a html side
        // should work everywhere, as we scan for the logout-link which contains it
        // and is on every page
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


        // Parses the villages (production) overview to get the village Ids
        // returns them in a List
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

        // parse the village overview
        // cur_building is true when the 
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
 


        // searches the html for a link with the specific match (in work)
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



        // tries to retrieve the parameter specified in 'param' from an URL
        // example: url = "http://192.168.2.100/game.php?village=42&screen=overview"
        // retrieveParam(url, "village");
        // ^ returns "42"
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

        // handles parsing errors and saves the affected htmlcode in a file
        // returns false, when an error has occured
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

    }
}
