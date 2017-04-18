using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace whois_scrapper
{
    public class Crawler
    {

        private static Regex anchorRegex = new Regex("(?<=<a\\s*?href=(?:'|\"))[^'\"]*?(?=(?:'|\"))");
        private static Regex emailRegex = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", RegexOptions.IgnoreCase);

        //Scrape one Url 
        public static List<String> scrapePage(String pageUrl)
        {
            WebRequest webRequest;
            WebResponse webResponse;
            String Rstring;

            webRequest = WebRequest.Create("http://" + pageUrl);
            webResponse = webRequest.GetResponse();

            Stream streamResponse = webResponse.GetResponseStream();
            StreamReader sreader = new StreamReader(streamResponse);
            Rstring = sreader.ReadToEnd();

            List<String> links = getPageAnchors(Rstring);
            List<String> emails = getPageEmails(Rstring);

            streamResponse.Close();
            sreader.Close();
            webResponse.Close();

            return emails;
        }

        //Get links of a HTML page
        private static List<String> getPageAnchors(String htmlContent)
        {
            List<String> newLinks = new List<String>();

            //Iterate all links of a page

            MatchCollection anchorMatches = anchorRegex.Matches(htmlContent);
            //Iterate all anchors of a page
            foreach (Match anchorMatch in anchorMatches)
            {
                newLinks.Add(anchorMatch.Value);
            }

            newLinks = newLinks.Distinct().ToList();

            return newLinks;
        }

        //Get emails of a HTML page
        private static List<String> getPageEmails(String htmlContent)
        {
            List<String> newEmails = new List<String>();

            //Iterate all emails of a page
            MatchCollection emailMatches = emailRegex.Matches(htmlContent);

            foreach (Match emailMatch in emailMatches)
            {
                newEmails.Add(emailMatch.Value);
            }

            newEmails = newEmails.Distinct().ToList();

            return newEmails;

        }
    }
}
