using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Net;

namespace whois_scrapper
{
    public class whois
    {
        private const int port = 43;
        private const string recordType = "domain";
        public static bool doWhois = true; //Do whois process
        public static bool doWeb = true; //Do web scrapping process
        //Regex email match
        private static Regex emailRegex = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", RegexOptions.IgnoreCase);
        private static Regex whoisRegex = new Regex(@"(whois).(([a-zA-Z0-9-_]{2,})\.){0,4}([a-zA-Z0-9-]{2,}\.[a-zA-Z0-9-]{2,})", RegexOptions.IgnoreCase);

        public static String askToWhois(String cleanedDomain, String whoisServer)
        {
            int index = -1;
            String data2 = String.Empty;

            //Max tries 5 times
            int i = 0;
            while (i < 5)
            {

                //Get random proxy IP:PORT
                Proxy proxyToUse = Proxies.getRandomProxy();

                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.SendTimeout = 5000;
                client.ReceiveTimeout = 6000;

                try
                {

                    client.Connect(proxyToUse.proxyUrl, proxyToUse.port);
                    //Create establish connection message
                    string proxyMsg = "CONNECT " + whoisServer + ":" + 43 + " HTTP/1.1 \n\n";
                    byte[] establishSend= Encoding.ASCII.GetBytes(proxyMsg);
                    byte[] establishRev = new byte[500];

                    //Send establish message
                    client.Send(establishSend, establishSend.Length, 0);

                    //Receive data
                    int intMessage = client.Receive(establishRev, establishRev.Length, SocketFlags.None);
                    string data = Encoding.ASCII.GetString(establishRev);

                    //If GET 200 OK ask for domain whois info
                    index = data.IndexOf("200");

                    if (index != -1)
                    {
                        //Get domain Whois data
                        string domainQuery = cleanedDomain + "\r\n";
                        byte[] whoisSend = Encoding.ASCII.GetBytes(domainQuery);
                        byte[] whoisRev = new byte[256];
                        client.Send(whoisSend, whoisSend.Length, 0);

                        bool x = true;
                        int j;
                        int bytesReceived = 0;

                        while (x)
                        {
                            j = client.Receive(whoisRev, whoisRev.Length, SocketFlags.None);

                            if(j > 0)
                            {
                                bytesReceived += j;
                                data2 += Encoding.ASCII.GetString(whoisRev);
                                i = 5;
                            }
                            else
                            {
                                x = false;
                            }        
                        }
                    }
                    else
                    {
                        i++;
                    }

                }
                catch (SocketException e)
                {
                    i++;
                }
                finally
                {
                    if(client.Connected)
                    {
                        client.Shutdown(SocketShutdown.Both);
                        client.Close();
                    }              
                }

            }

            return data2;
        }

        public static List<String> Lookup(String cleanedDomain)
        {
            List<String> emailList = new List<String>();

            if(cleanedDomain != null)
            {
                //Do Whois process
                if(doWhois)
                {
                    //Get appropiate WHOIS server for the domain
                    String whoisServer = WhoisServers.getWhoisServer(cleanedDomain);

                    //Ask to the WHOIS server about domain
                    String data = askToWhois(cleanedDomain, whoisServer);

                    //If the domains are .com or net
                    if (whoisServer == "whois.crsnic.net")
                    {
                        String comNetWhoisServer = extractWhoisServer(data);
                        data = askToWhois(cleanedDomain, comNetWhoisServer);
                        //Extact emails from the data 
                        emailList = ExtractEmails(data);
                    }
                }

                //Do web scrap process
                if (doWeb)
                {
                    Crawler crawler = new Crawler(cleanedDomain);
                    List<String> crawEmails = crawler.scrapWeb();
                    emailList = emailList.Concat(crawEmails).Distinct().ToList();
                }
            }

            return emailList;
        }

        //Extact WHOIS server from a List<String> of a WHOIS Response
        private static String extractWhoisServer(string data)
        {
            //Look for WHOIS servers in the string
            Match match = whoisRegex.Match(data);

            return match.Value;
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
