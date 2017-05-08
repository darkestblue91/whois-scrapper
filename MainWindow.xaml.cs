using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace whois_scrapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public String proxyFile;
        public String domainFile;
        public bool running;
        public bool cleanUrl = false;
        static Object locker = new Object();
        private int domainsQuantity;

        BackgroundWorker worker = new BackgroundWorker();

        public static int threadCount = 0;

        public MainWindow()
        {
            InitializeComponent();
            running = false;
        }

        //Start scrapping process
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Start or stop
            if (!running)
            {
                
                //Check if the proxy file is set
                if (proxyFile != null)
                {
                    //Load proxies
                    Proxies.loadProxies(proxyFile);

                    //Set output file name
                    if(chkWHOIS.IsChecked == true && chkWeb.IsChecked == false)
                    {
                        FilesControl.fileName = "\\whoisOutput.txt";
                        whois.doWhois = true;
                        whois.doWeb = false;
                    }
                    else if (chkWHOIS.IsChecked == false && chkWeb.IsChecked == true)
                    {
                        FilesControl.fileName = "\\layersOutput.txt";
                        whois.doWhois = false;
                        whois.doWeb = true;
                    }
                    else if (chkWHOIS.IsChecked == true && chkWeb.IsChecked == true)
                    {
                        FilesControl.fileName = "\\whoislayersOutput.txt";
                        whois.doWhois = true;
                        whois.doWeb = true;
                    }

                    //Start Scrapping process
                    progressBar.Value = 0;
                    runProcess(domainFile);
                }
                else
                {
                    MessageBox.Show("You have to choose the proxies file before scrapping.", "Proxy file needed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            } else
            {
                worker.CancelAsync();
                running = false;
                btnScrap.Content = "Stopping!";
                btnScrap.IsEnabled = false;
            }
        }

        //Select domains to scrap in .txt
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //File dialog
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();

            //.txt as default file ext
            dialog.DefaultExt = ".txt";

            Nullable<bool> result = dialog.ShowDialog();

            if(result == true)
            {
                txtDomain.Text = dialog.FileName;
                domainFile = dialog.FileName;

                if(domainFile != null && proxyFile != null)
                {
                    btnScrap.IsEnabled = true;
                }            
            }
        }

        //Select proxies .txt file
        private void btnSetProxy_Click(object sender, RoutedEventArgs e)
        {
            //File dialog
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();

            //.txt as default file ext
            dialog.DefaultExt = ".txt";

            Nullable<bool> result = dialog.ShowDialog();

            if (result == true)
            {
                proxyFile = dialog.FileName;
                txtProxy.Text = dialog.FileName;

                if (domainFile != null && proxyFile != null)
                {
                    btnScrap.IsEnabled = true;
                }
            }
        }

        //Create separate thread for the scrap process
        public void runProcess(String domainsFile)
        {
            List<string> domains = new List<string>(File.ReadAllLines(domainsFile));

            Crawler.levels = Int32.Parse(txtLevels.Text);
            Crawler.pages = Int32.Parse(txtPages.Text);
            int scrappedDomains = 0;
            domainsQuantity = domains.Count;
            labelDomains.Content = "0 of " + domainsQuantity + " domains scrapped!";
            progressBar.Maximum = domainsQuantity;

            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;

            running = true;
            btnScrap.Content = "Stop";

            int threadNumber = Int32.Parse(txtThreads.Text);

            ThreadPool.SetMinThreads(threadNumber, threadNumber);
            ThreadPool.SetMaxThreads(threadNumber + 5, threadNumber + 9);

            worker.DoWork += delegate (object s, DoWorkEventArgs args)
            {
                String file = (string)args.Argument;
                
                    domains.AsParallel().ForAllInApproximateOrder(threadNumber, domain =>
                    {
                        if(worker.CancellationPending == false)
                        {
                            lock(locker)
                            {
                                threadCount++;
                            }
                            

                            //Scrape domain WHOIS
                            String cleanedDomain = WhoisServers.cleanDomain(domain);
                            List<String> domainEmails = whois.Lookup(cleanedDomain);

                            //Iterate all WHOIS emails and save it
                            foreach (String email in domainEmails)
                            {
                                String newLine;
                                if(cleanUrl == true)
                                {
                                    newLine = email + "|http://" + cleanedDomain;
                                } else
                                {
                                    newLine = email + "|" + domain;
                                }

                                //Create and save emails in the output file "domain.com:email"
                                FilesControl.writeToFile(newLine);
                            }

                            lock(locker)
                            {
                                scrappedDomains++;
                            }
                           
                            worker.ReportProgress(scrappedDomains);

                            lock(locker)
                            {
                                threadCount--;
                            }
                        }
                        else {
                            worker.ReportProgress(scrappedDomains);
                        }
                    });
            };

            worker.ProgressChanged += delegate (object s, ProgressChangedEventArgs args)
            {
                //if(threadCount >= 0)
                //{
                    lblT.Content = threadCount + " threads running";
                //}

                labelDomains.Content = args.ProgressPercentage + " of " + domainsQuantity + " domains";
                progressBar.Value = args.ProgressPercentage;

                //Scrap cancelled
                if(running == false && threadCount == 0)
                {
                    btnScrap.Content = "Scrap";
                    btnScrap.IsEnabled = true;
                }

                //Scrap finished
                if(args.ProgressPercentage == domainsQuantity)
                {
                    running = false;
                    btnScrap.IsEnabled = true;
                    btnScrap.Content = "Scrap";
                    labelDomains.Content = "Finished! " + domainsQuantity + " domains scrapped!"; 
                }
            };

            worker.RunWorkerAsync(domainsFile);
        }

        //Only accepts numbers on threads TextBox
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void chkCleaned_Checked(object sender, RoutedEventArgs e)
        {
            cleanUrl = true;
        }

        private void chkCleaned_Unchecked(object sender, RoutedEventArgs e)
        {
            cleanUrl = false;
        }
    }
}
