using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace whois_scrapper
{
    public class FilesControl
    {
        public static bool writeToFile(string line)
        {
            //Use actual executable path
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\whoisOutput.txt";

            //Create of use actual file
            if (!File.Exists(path))
            {
                StreamWriter sw = File.CreateText(path);
            }

            //Write lines on the file
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(line);
            }

            return true;
        }
    }
}
