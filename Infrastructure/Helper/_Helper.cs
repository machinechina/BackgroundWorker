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
        static readonly string _localPathTemplate = $"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "_{0}_" + ProductDescription.AppName)}";

        private static readonly string _localStorePath = string.Format(_localPathTemplate, "deployQuerys");

        private static readonly string _localLogPath = string.Format(_localPathTemplate, "logs");

        private static readonly string _localVersionPath = string.Format(_localPathTemplate, "version");

        private static readonly string _localRefFilePath = string.Format(_localPathTemplate, "refFiles");

        #region Public Constructors

        static Helper()
        {
            RunProcessAsync(@"rundll32", @"c:\windows\system32\dfshim.dll CleanOnlineAppCache");

            //"If the directory already exists, this method does not create a new directory, but it returns a DirectoryInfo object for the existing directory."
            Directory.CreateDirectory(_localLogPath);
            Directory.CreateDirectory(_localRefFilePath);

            //fire an event when program closing by user operations while running in console mode.
            Console.CancelKeyPress += (o, e) => ConsoleExiting?.Invoke(ConsoleExitReason.CancelKeyPressed);
            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);
        }

        #endregion Public Constructors
    }
}
