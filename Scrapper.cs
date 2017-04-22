using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Threading;
using System.ComponentModel;


/* 

Main Scrapper file

Create the thread for the scrapping process (runProcess)
Scrapping function (startScrap)

*/

namespace whois_scrapper
{
    public class Scrapper
    {
        //Create separate thread for the scrap process
        public static void runProcess(String domainsFile)
        {
            List<string> domains = new List<string>(File.ReadAllLines(domainsFile));

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;

            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                String file = (string)args.Argument;

                domains.AsParallel().ForAllInApproximateOrder(domain =>
                {
                    //Scrape domain WHOIS
                    List<String> domainEmails = whois.Lookup(WhoisServers.cleanDomain(domain));

                    //Iterate all WHOIS emails and save it
                    foreach (String email in domainEmails)
                    {
                        //Create and save emails in the output file "domain.com:email"
                        String newLine = WhoisServers.cleanDomain(domain) + ":" + email;
                        FilesControl.writeToFile(newLine);
                    }

                    worker.ReportProgress(1);
                });

            };

            worker.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {
                int percentage = args.ProgressPercentage;
            };

            worker.RunWorkerAsync(domainsFile);
        }

        //public static void runProcess(String domainsFile)
        //{
        //    Thread thread = new Thread(() => startScrap(domainsFile));
        //    thread.Start();
        //}

        //Main scrapping procces function
        public static void startScrap(String domainsFile)
        {
            //Load all domains into array
            List<string> domains = new List<string>(File.ReadAllLines(domainsFile));

            domains.AsParallel().ForAllInApproximateOrder(domain =>
            {
                //Scrape domain WHOIS
                List<String> domainEmails = whois.Lookup(WhoisServers.cleanDomain(domain));

                //Iterate all WHOIS emails and save it
                foreach (String email in domainEmails)
                {
                    //Create and save emails in the output file "domain.com:email"
                    String newLine = domain + ":" + email;
                    FilesControl.writeToFile(newLine);
                }

                ////Scrap HTML pages

                //if (true)
                //{
                //    List<String> htmlEmails = Crawler.scrapePage(domain);
                //    foreach (String email in htmlEmails)
                //    {
                //        String newLine = domain + ":" + email;
                //        FilesControl.writeToFile(newLine);
                //        finalLines.Add(newLine);
                //    }
                //}

            });
        }
    }
}
