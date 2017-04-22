using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
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

namespace whois_scrapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public String proxyFile;
        public String domainFile;
        public bool onlyWhois;
        static Object locker = new Object();
        private int domainsQuantity;

        public MainWindow()
        {
            InitializeComponent();
        }

        //Start scrapping process
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Check if the proxy file is set
            if(proxyFile != null)
            {
                //Load proxies file into List<Proxy>
                List<Proxy> proxyList = new List<Proxy>();
                StreamReader reader = new StreamReader(proxyFile);
                String proxyLine;

                //Iterate proxy files line by line, add to the List<Proxy>
                while ((proxyLine = reader.ReadLine()) != null) {
                    Proxy proxy = new Proxy()
                    {
                        proxyUrl = proxyLine,
                        beingUsed = false,
                        isAlive = true
                    };
                    proxyList.Add(proxy);
                }

                Proxies.proxies = proxyList;

                //Start Scrapping process
                runProcess(domainFile);
            }
            else
            {
                MessageBox.Show("You have to choose the proxies file before scrapping.", "Proxy file needed", MessageBoxButton.OK , MessageBoxImage.Exclamation);
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
                btnScrap.IsEnabled = true;
            }
        }

        //Change only Whois or Whois + Webscrap
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (chkMode.IsChecked == true)
            {
                onlyWhois = true;
            } else
            {
                onlyWhois = false;
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
            }
        }

        //Create separate thread for the scrap process
        public void runProcess(String domainsFile)
        {
            List<string> domains = new List<string>(File.ReadAllLines(domainsFile));

            int scrappedDomains = 0;
            domainsQuantity = domains.Count;
            labelDomains.Content = "0 of " + domainsQuantity;
            progressBar.Maximum = domainsQuantity;

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;

            worker.DoWork += delegate (object s, DoWorkEventArgs args)
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
                        String newLine = domain + ":" + email;
                        FilesControl.writeToFile(newLine);
                    }

                    lock(locker)
                    {
                        scrappedDomains++;
                        worker.ReportProgress(scrappedDomains);
                    }

                   
                });

            };

            worker.ProgressChanged += delegate (object s, ProgressChangedEventArgs args)
            {
                labelDomains.Content = args.ProgressPercentage + " of " + domainsQuantity;
                progressBar.Value = args.ProgressPercentage;
            };

            worker.RunWorkerAsync(domainsFile);
        }

    }
}
