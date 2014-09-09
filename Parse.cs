using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace twbot
{
    public class Parse
    {
        public static List<short> parseOverview(string html)
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

        public static string searchLink(string html, string query)
        { // TODO: TEST!!!
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
