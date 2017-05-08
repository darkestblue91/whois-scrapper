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
        private static String filePath;
        public static String fileName = "\\whoisOutput.txt";

        //Write line to output file
        public static bool writeToFile(string line)
        {
            //Use actual executable path
            filePath = AppDomain.CurrentDomain.BaseDirectory + fileName;
            bool notSaved = true;

            while(notSaved)
            {
                try
                {
                    lock (fileLock)
                    {
                        //Create of use actual file
                        if (!File.Exists(filePath))
                        {
                            StreamWriter sw = File.CreateText(filePath);
                        }

                        //Write lines on the file
                        using (StreamWriter sw = File.AppendText(filePath))
                        {
                            sw.WriteLine(line);
                            notSaved = false;
                        }
                    }
                }
                catch (IOException e)
                {
                    notSaved = true;
                }
            }

            return true;
        }

        public static bool writeText(String text)
        {
            filePath = AppDomain.CurrentDomain.BaseDirectory + "\\whoisOutput.txt";
            bool notSaved = true;

            while (notSaved)
            {
                try
                {
                    lock (fileLock)
                    {
                        //Create of use actual file
                        if (!File.Exists(filePath))
                        {
                            StreamWriter sw = File.CreateText(filePath);
                        }

                        //Write lines on the file
                        using (StreamWriter sw = File.AppendText(filePath))
                        {
                            sw.Write(text);
                            notSaved = false;
                        }
                    }
                }
                catch (IOException e)
                {
                    notSaved = true;
                }
            }

            return true;
        }
    }
}
