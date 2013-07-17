using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TestStartProcess
{
    class Program
    {
        public static void Log(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.Write("{0} {1}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString());
            w.Write(" :");
            w.WriteLine(" {0}", logMessage);
        }

        public static void DumpLog(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
        }

        static public string GetProcessOwner(int processId)
        {
            string query = "Select * From Win32_Process Where ProcessID = " + processId;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();

            foreach (ManagementObject obj in processList)
            {
                string[] argList = new string[] { string.Empty, string.Empty };
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                {
                    return argList[1] + "\\" + argList[0];
                }
            }

            return "NO OWNER";
        }

        static void StartQBIfNotRunningAsAdmin()
        {
            using (StreamWriter w = File.AppendText("StartQBlog.txt"))
            {
                Process[] processes = System.Diagnostics.Process.GetProcessesByName("qbw32");

                bool found = false;
                foreach (Process proc in processes)
                {
                    string userName = GetProcessOwner(proc.Id);                   
                    if (userName.ToUpper().Contains("ADMINISTRATOR"))
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    try
                    {
                        Log("QB not running as Administrator.  Starting it...", w);                        
                        ProcessStartInfo startInfo = new ProcessStartInfo("C:\\Program Files (x86)\\Intuit13.0\\QuickBooks Enterprise Solutions 13.0\\qbw32.exe");                        
                        Process.Start(startInfo);
                    }
                    catch (Win32Exception e)
                    {
                        Log(e.Message, w);
                    }
                    catch (Exception e)
                    {
                        Log(e.Message, w);
                    }
                }             
            }
        }

        static void Main(string[] args)
        {
            while (true)
            {
                System.Threading.Thread.Sleep(5000);
                StartQBIfNotRunningAsAdmin();                
            }
        }
    }
}
