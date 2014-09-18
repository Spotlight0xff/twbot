using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace twbot
{
    public class Parse
    {

        // Parses the villages (production) overview to get the village Ids
        // returns them in a List
        public static List<short> parseVillagesOverview(string html)
        {
            List<short> list = new List<short>();
            HtmlAgilityPack.HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            if (doc.ParseErrors != null && doc.ParseErrors.Count() > 0 )
            {
                Console.WriteLine("Parse error occured @ parseOverview");
                doc.Save("failed.html");
                return null;
            }
            foreach (var node in doc.DocumentNode.SelectNodes("//table[@class='vis']/tr"))
            {
                var link = node.Descendants("a").Where(x => x.Attributes.Contains("href"));
                foreach (var a in link)
                {
                    string url = a.Attributes["href"].Value;
                    string village = TribalWars.retrieveParam(url, "village");
                    list.Add(short.Parse(village));
                    Console.WriteLine("Village ID: " + village);
                }
            }

            return list;
        }


        public static bool parseOverview(string html, ref BuildingData buildings)
        {
            if (buildings == null)
                buildings = new BuildingData();

            HtmlAgilityPack.HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            if (doc.ParseErrors != null && doc.ParseErrors.Count() > 0 )
            { // parser error
                Console.WriteLine("[parseOverview] Parse error occured, check");
                doc.Save("failed_parseOverview_"+DateTime.Now.ToFileTime()+".html");
                return false;
            }

            foreach (var row in doc.DocumentNode.SelectNodes("//table[@class='vis']/tr/td[@width='50%']"))
            {   
                var links = row.Descendants("a");
                string building = "";
                short level = 0;
                foreach (var link in links)
                { // get href-links from <a>-Tags
                    string url = link.Attributes["href"].Value;
                    building = TribalWars.retrieveParam(url, "screen");
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

                buildings.set(building, level);
                Console.WriteLine(building + " => " + buildings.get(building) );
            }
            return true;
        }
 


        // searches the html for a link with the specific match (in work)
        public static string searchLink(string html, string query)
        { // TODO: TEST!!!
            throw new System.NotImplementedException();
            HtmlAgilityPack.HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            if (doc.ParseErrors != null && doc.ParseErrors.Count() > 0)
            {
                Console.WriteLine("Parse error occured @ parseOverview");
                doc.Save("failed.html");
                return null;
            }

            IEnumerable<HtmlNode> links = doc.DocumentNode.Descendants("a").Where(x => x.Attributes.Contains("href"));
            foreach (var link in links)
            {
                if(link.Attributes["href"].Value.LastIndexOf(query) != -1)
                    return link.Attributes["href"].Value;
            }


            return null;
        }

    }
}
