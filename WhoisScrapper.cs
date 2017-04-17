using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace whois_scrapper
{
    public class WhoisScrapper
    {
        public static String StartScrap(String filename)
        {
            //Load all domains into array
            String[] domains = File.ReadAllLines(filename);

            List<String> finalLines = new List<string>();

            whois o = new whois();

            foreach (String domain in domains)
            {
               List<String> domainEmails = whois.Lookup(domain);
                foreach(String email in domainEmails)
                {
                    String newLine = domain + ":" + email;
                    FilesControl.writeToFile(newLine);
                    finalLines.Add(newLine);
                }
            }

            return String.Join(Environment.NewLine, finalLines);
        }
    }
}
