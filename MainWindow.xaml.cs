using System;
using System.Collections.Generic;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            String domain = txtDomain.Text;
            String finalResult = WhoisScrapper.StartScrap(domain);
            txtWhois.Text = finalResult;
        }

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
            }
        }
    }
}
