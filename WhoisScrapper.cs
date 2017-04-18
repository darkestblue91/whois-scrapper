using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace whois_scrapper
{
    public class WhoisScrapper
    {
        private static int threadNumber = 4;

        public static void StartScrap(String filename)
        {
            //Load all domains into array
            List<string> domains = new List<string>(File.ReadAllLines(filename));

            List<String> finalLines = new List<string>();

            domains.AsParallel().ForAllInApproximateOrder(threadNumber, domain =>
            {
                List<String> domainEmails = whois.Lookup(domain);
                foreach (String email in domainEmails)
                {
                    String newLine = domain + ":" + email;
                    FilesControl.writeToFile(newLine);
                    finalLines.Add(newLine);
                }
            });
        }
    }
}
