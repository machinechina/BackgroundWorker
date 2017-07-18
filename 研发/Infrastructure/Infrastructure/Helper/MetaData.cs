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
        static readonly string _localPathTemplate = $" {Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "_{0}_" + ProductDescription.AppName)}";

        private static readonly string _localStorePath = string.Format(_localPathTemplate, "deployQuerys");

        private static readonly string _localLogPath = string.Format(_localPathTemplate, "logs");

        private static readonly string _localVersionPath = string.Format(_localPathTemplate, "version");
    }
}
