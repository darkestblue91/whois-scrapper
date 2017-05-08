using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*

Whois Servers

1. Get Top Level Domain of a domain (getDomainTld)
2. Get the correct WHOIS server for a domain(getWhoisServer)
3. Clean HTTP address to only domains domain.com (cleanDomain)

 */

namespace whois_scrapper
{
    public class WhoisServers
    {
        //Domain servers to extract
        private static String[] tlds = { ".com", ".net", ".org" };

        //Get TLD from a domain
        private static String getDomainTld(String domain)
        {
            //Compare all tld with the domain tld and return the correct tld
            foreach (string tld in tlds)
            {
                if(domain.EndsWith(tld, StringComparison.InvariantCultureIgnoreCase))
                {
                    return tld;
                }
            }

            return tlds[0];
        }

        //Get the correct WHOIS server for a domain
        public static String getWhoisServer(String domain)
        {
            //Get domain TLD
            String domainTld = getDomainTld(domain);
            String whoisServer;

            //Get correct WHOIS server
            switch(domainTld)
            {
                case ".com":
                    whoisServer = "whois.crsnic.net";
                    break;
                case ".net":
                    whoisServer = "whois.crsnic.net";
                    break;
                case ".org":
                    whoisServer = "whois.pir.org";
                    break;
                default:
                    whoisServer = "whois.name.com";
                    break;
            }

            return whoisServer;
        }

        //Clear URL to get a domain
        public static String cleanDomain(String uncleanDomain)
        {
            try
            {
                //If is a basic URL
                if (Uri.IsWellFormedUriString(uncleanDomain, UriKind.Absolute))
                {
                    Uri uri = new Uri(uncleanDomain);
                    return getTld(uri);
                }
                else  //If it is just a DNS domain
                {
                    UriHostNameType hostType = Uri.CheckHostName(uncleanDomain);
                    if(hostType == UriHostNameType.Dns)
                    {
                        return uncleanDomain;
                    }
                    
                    //If it is not a domain of any kind
                    else
                    {
                        return null;
                    }
                    
                }                            
            }
            catch (UriFormatException e)
            {
                return null;
            }      
        }

        private static String getTld(Uri uri)
        {
            //Part the Uri.Host in parts by .
            var parts = uri.Host.Split('.');

            if(parts == null || parts.Length == 2)
                return parts[0] + "." + parts[1];

            //If it is a domain of three parts
            if (parts == null || parts.Length != 3)
                return null;

            return parts[1] + "." + parts[2];
        }
    }
}
