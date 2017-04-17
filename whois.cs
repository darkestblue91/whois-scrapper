using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;

namespace whois_scrapper
{
    public class whois
    {
        private const int port = 43;
        private const string recordType = "domain";

        private const string comServer = "whois.crsnic.net";
        private const string orgServer = "whois.pir.org";
        private const string netServer = "whois.crsnic.net";

        //Regex email match
        private static Regex emailRegex = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", RegexOptions.IgnoreCase);

        public static List<String> Lookup(string domainName)
        {
            using(TcpClient client = new TcpClient())
            {
                client.Connect(orgServer, port);
                string domainQuery = recordType + " " + domainName + "\r\n";
                byte[] domainQueryBytes = Encoding.ASCII.GetBytes(domainQuery.ToCharArray());

                Stream whoisStream = client.GetStream();
                whoisStream.Write(domainQueryBytes, 0, domainQueryBytes.Length);

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

            return emails;
        }

    }
}
