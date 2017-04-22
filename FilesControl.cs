using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*

File IO - Write to .txt output file all the scrapped emails

1. Write lines to file (writeToFile)


*/

namespace whois_scrapper
{
    public class FilesControl
    {
        //Variable to lock access to only one thread
        private static Object fileLock = new Object();

        //Write line to output file
        public static bool writeToFile(string line)
        {
            //Use actual executable path
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\whoisOutput.txt";

            lock (fileLock)
            {
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
            }

            return true;
        }
    }
}
