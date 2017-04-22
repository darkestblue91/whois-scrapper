using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Net;
using Starksoft.Aspen.Proxy;

namespace whois_scrapper
{
    public class whois
    {
        private const int port = 43;
        private const string recordType = "domain";

        //Regex email match
        private static Regex emailRegex = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", RegexOptions.IgnoreCase);

        //Ask to one Whois server about the domain
        public static List<String> Lookup(string domainName)
        {

            Proxy proxyToUse = Proxies.getRandomProxy();
            String[] result = proxyToUse.proxyUrl.Split(new char[] { ':' });

            Socks5ProxyClient proxy = new Socks5ProxyClient();
            proxy.ProxyHost = result[0];
            proxy.ProxyPort = Int32.Parse(result[1]);
            proxy.ProxyUserName = "darkestblue91";
            proxy.ProxyPassword = "Np5@t6FE3dghqNc_";

            //Clean domain URL
            String cleanedDomain = WhoisServers.cleanDomain(domainName);

            TcpClient client = proxy.CreateConnection(WhoisServers.getWhoisServer(domainName), 43);

            //string domainQuery = recordType + " " + domainName + " \r\n";
            string domainQuery = domainName + " \r\n";
            byte[] sendBuf = System.Text.ASCIIEncoding.ASCII.GetBytes(domainQuery.ToCharArray());
            client.GetStream().Write(sendBuf, 0, sendBuf.Length);
            StreamReader whoisStreamReader = new StreamReader(client.GetStream(), Encoding.ASCII);

            String streamOutputContent = "";

            List<string> whoisData = new List<string>();

            while (null != (streamOutputContent = whoisStreamReader.ReadLine()))
            {
                whoisData.Add(streamOutputContent);
            }

            client.Close();

            String dataAsString = String.Join(Environment.NewLine, whoisData);
            List<String> emailList = ExtractEmails(dataAsString);

            return emailList;
        }

        //Receives a string and return a List<String> with the emails
        private static List<String> ExtractEmails(string data)
        {
            //Look for all emails in the string
            MatchCollection emailMatches = emailRegex.Matches(data);

            //Add all the emails on a List<String>
            List<String> emails = new List<String>();
            foreach(Match emailMatch in emailMatches)
            {
                emails.Add(emailMatch.Value);
            }

            emails = emails.Distinct().ToList();

            return emails;
        }
    }
}
