using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

/*

Proxies List<Proxy>

Store proxies list
Get one random proxy (getRandomProxy)
 
*/

namespace whois_scrapper
{
    public class Proxies
    {
        //Store a list of proxies
        public static List<Proxy> proxies;

        //Get one random proxy from the list to connect to a Whois server
        public static Proxy getRandomProxy()
        {
            Random random = new Random();
            int pos = random.Next(proxies.Count -1);
            return proxies[pos];
        }

        public static void loadProxies(String proxyFile)
        {
            proxies = new List<Proxy>();
            StreamReader reader = new StreamReader(proxyFile);
            String proxyLine;

            //Iterate proxy files line by line, add to the List<Proxy>
            while ((proxyLine = reader.ReadLine()) != null)
            {
                String[] result = proxyLine.Split(new char[] { ':' });
                Proxy proxy = new Proxy()
                {
                    proxyUrl = result[0],
                    port = Int32.Parse(result[1])
                };
                proxies.Add(proxy);
            }
        } 
    }
}
