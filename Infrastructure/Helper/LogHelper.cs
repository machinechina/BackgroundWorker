using System;
using System.IO;

namespace Infrastructure.Helpers
{
    public partial class Helper
    {
        #region Private Fields

        private static readonly object logLocker = new object();
        private static readonly string InfoPrefix = "Info";
        private static readonly string ErrorPrefix = "Error";

        #endregion Private Fields

        #region Private Properties

        private static string BuildPrefix(string prefix) => $"------ {prefix} {DateTime.Now.ToString()} ------";

        #endregion Private Properties

        #region Public Methods

        public static void Error(Exception ex)
        {
            DoInfo(BuildText(ErrorPrefix, ex.Message));
            DoLog(BuildText(ErrorPrefix, ex.ToString()));
        }

        public static void Error(Exception ex, string msg)
        {
            DoInfo(BuildText(ErrorPrefix, msg, ex.Message));
            DoLog(BuildText(ErrorPrefix, msg, ex.ToString()));
        }

        public static void Info(string msg)
            => DoInfo(BuildText(InfoPrefix, msg));

        public static void Log(string msg)
            => DoLog(BuildText(InfoPrefix, msg));

        #endregion Public Methods

        #region Private Methods

        private static string BuildText(string prefix, string text) => $"{BuildPrefix(prefix)}{Environment.NewLine}{text}{Environment.NewLine}";

        private static string BuildText(string prefix, string text1, string text2) => $"{BuildPrefix(prefix)}{Environment.NewLine}{text1}{Environment.NewLine}{text2}{Environment.NewLine}";

        private static void DoInfo(string text) => Console.WriteLine(text);
        private static void DoLog(string text)
        {
            StreamWriter sw = null;
            try
            {
                lock (logLocker)
                {
                    sw = File.AppendText($"{_localLogPath}\\{DateTime.Now.ToString("yyyyMMdd")}.log");
                    sw.WriteLine(text);
                }
            }
            catch { }
            finally
            {
                sw?.Close();
            }
        }

        #endregion Private Methods
    }
}