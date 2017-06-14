using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    public partial class Helper
    {
        public static void Info(string msg)
        {
            Console.WriteLine($"{DateTime.Now.ToString()} -- {msg}");
        }


        public static void Log(Exception ex, string msg = "")
        {
            Log(msg + "\n" + ex.ToString());
        }

        public static void InfoAndLog(Exception ex, string msg = "")
        {
            Info(msg + "\n" + ex.Message);
            Log(ex, msg);
        }

        private static readonly object logLocker = new object();
        public static void Log(string msg)
        {
            try
            {
                lock (logLocker)
                {
                    StreamWriter sw;
                    sw = File.AppendText($"{Environment.CurrentDirectory}\\{DateTime.Now.ToString("yyyyMMdd")}.log");
                    sw.WriteLine(DateTime.Now.ToString() + "---" + msg + "\n\n\n");
                    sw.Close();
                }
            }
            catch
            {
            }
        }
    }
}
