using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

/*

Web Crawler

Takes a domain and look in all pages for emails.
Scrap emails of that pages.

*/

namespace whois_scrapper
{
    public class Crawler
    {
        private String domain; //Domain to scrap
        private List<String> links; //Scrapped links to avoid repeated links
        private List<String> emails; //Scrapped emails to avoid repeated emails
        private static Regex anchorRegex = new Regex("(?<=<a\\s*?href=(?:'|\"))(([\\w\\.\\-\\+]+:)\\/{2}(([\\w\\d\\.]+):([\\w\\d\\.]+))?@?(([a-zA-Z0-9\\.\\-_]+)(?::(\\d{1,5}))?))?(\\/(?:[a-zA-Z0-9\\.\\-\\/\\+\\%]+)?)(?:\\?([a-zA-Z0-9=%\\-_\\.\\*&;]+))?(?:#([a-zA-Z0-9\\-=,&%;\\/\\\"'\\?]+)?)?", RegexOptions.IgnoreCase);
        private static Regex emailRegex = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", RegexOptions.IgnoreCase);
        private static Regex fileRegex = new Regex(@"^.*\.(jpg|jpeg|png|gif|doc|pdf|avi|css|exe|midi|mid|mp3|raw|mpeg|mpg|ram|rar|tiff|txt|wav|zip|7zip|iso|dmg|js|swf|svg|rss|xml|atom|webm|mp4|ogg|wav|flac)$", RegexOptions.IgnoreCase);
        public static int levels = 3; //Default levels to scrap
        public static int pages = 100; //Default pages to scrap

        //Crawler constructor
        public Crawler(String domain)
        {
            this.domain = domain;
            links = new List<String>();
            emails = new List<String>();
        }

        //Main function
        //Start the scrap process of a domain
        public List<String> scrapWeb()
        {
            //Recursive page scraping, starting in root domain, returning all unique emails
            scrapePage("http://" + domain, domain);
            return emails;
        }

        //Scrape one page with its url
        public void scrapePage(String pageUrl, String domain)
        {
            //constructUrl(pageUrl);

            //Check if the url is a file
            if (!isAFile(pageUrl) && (links.Count <= pages))
            {
                //Attempt to get HTML content from url
                try
                {
                    WebRequest webRequest;
                    WebResponse webResponse;
                    String htmlContent = "";

                    webRequest = WebRequest.Create(pageUrl);
                    webResponse = webRequest.GetResponse();

                    Stream streamResponse = webResponse.GetResponseStream();
                    StreamReader sreader = new StreamReader(streamResponse);
                    htmlContent = sreader.ReadToEnd();
                    streamResponse.Close();
                    sreader.Close();
                    webResponse.Close();

                    //Get emails of the page
                    getPageEmails(htmlContent, domain);

                    //Get anchors of the page
                    List<String> temporalLinks = getPageAnchors(htmlContent, domain);
                   
                    //Scrap all the unique url finded
                    foreach (String link in temporalLinks)
                    {
                        scrapePage(link, domain);
                    }
                }
                catch (WebException e) {}    
            }
        }

        //Get links of a HTML page
        private List<String> getPageAnchors(String htmlContent, String domain)
        {
            List<String> newLinks = new List<String>();

            //Iterate all anchors of a page
            MatchCollection anchorMatches = anchorRegex.Matches(htmlContent);

            foreach (Match anchorMatch in anchorMatches)
            {
                //Check if is the same domain
                String urlDomain = WhoisServers.cleanDomain(anchorMatch.Value);
                if(urlDomain == domain)
                {
                    String matchedlink = links.Where(link => link.Contains(anchorMatch.Value)).FirstOrDefault();
                    if (matchedlink == null)
                    {
                        //Check if it is under or equal the established level to scrap
                        Uri uri = new Uri(anchorMatch.Value);
                        if (uri.Segments.Length <= levels)
                        {
                            links.Add(anchorMatch.Value);
                            newLinks.Add(anchorMatch.Value);
                        }
                        
                    }
                }
            }
            return newLinks.Distinct().ToList();
        }

        //Get emails of a HTML page
        private void getPageEmails(String htmlContent, String domain)
        {
            //Get all emails of the html content
            MatchCollection emailMatches = emailRegex.Matches(htmlContent);
 
            //Save emails on the list
            foreach (Match emailMatch in emailMatches)
            {
                //If the email already on the list?
                String addedEmail = emails.Where(email => email.Contains(emailMatch.Value)).FirstOrDefault();
                if (addedEmail == null)
                {
                    emails.Add(emailMatch.Value);
                }
            }
        }

        //Check if the URL is a file or not
        private bool isAFile(String url)
        {
            Match isFile = fileRegex.Match(url);
            if (isFile.Success) return true;
            return false;
        }
    }
}
