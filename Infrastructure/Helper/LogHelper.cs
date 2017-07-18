using System;
using System.IO;

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

        public static void InfoAndLog(string msg)
        {
            Info(msg);
            Log(msg);
        }

        private static readonly object logLocker = new object();

        public static void Log(string msg)
        {
            StreamWriter sw = null;
            try
            {
                lock (logLocker)
                {
                    sw = File.AppendText($"{_localLogPath}\\{DateTime.Now.ToString("yyyyMMdd")}.log");
                    sw.WriteLine(DateTime.Now.ToString() + "---" + msg + "\n\n\n");
                }
            }
            catch { }
            finally
            {
                sw?.Close();
            }
        }
    }
}