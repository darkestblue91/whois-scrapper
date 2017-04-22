using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

        //Object to lock proxies list for only one thread
        static Object locker = new Object();

        //Get one random proxy from the list to connect to a Whois server
        public static Proxy getRandomProxy()
        {
            Proxy proxy;

            lock(locker)
            {
                //Look for a proxy alive and not used
                if(proxies.Any(p => p.isAlive == true && p.beingUsed == false))
                {
                    proxy = proxies.Find(x => x.isAlive == true && x.beingUsed == false);
                    proxy.beingUsed = true;
                }
                else
                {
                    proxy = proxies.ElementAt(0);
                }
            }

            return proxy;
        }
    }
}
