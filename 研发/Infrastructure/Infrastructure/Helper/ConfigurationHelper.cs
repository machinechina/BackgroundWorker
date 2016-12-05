using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Infrastructure.Helpers
{
    public partial class Helper
    {
        public static T GetAppConfig<T>(string key)
        {
            T val = default(T);
            try
            {
                var value = GetAppConfig(key);
                if (typeof(T).IsEquivalentTo(typeof(Guid)))
                {
                    val = (T)Convert.ChangeType(new Guid(value), typeof(T));
                }
                else
                {
                    val = (T)Convert.ChangeType(value, typeof(T));
                }
            }
            catch (Exception)
            {

            }
            return val;
        }

        public static string GetAppConfig(string keyname, string configPath = "Config")
        {
            var config = System.Configuration.ConfigurationManager.AppSettings[keyname];
            try
            {
                if (string.IsNullOrWhiteSpace(config))
                {
                    string filePath = Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, configPath);
                    if (File.Exists(filePath))
                    {
                        using (TextReader reader = new StreamReader(filePath))
                        {
                            XElement xml = XElement.Load(filePath);
                            if (xml != null)
                            {
                                var element = xml.Elements().SingleOrDefault(e => e.Attribute("key") != null && e.Attribute("key").Value.Equals(keyname));
                                if (element != null)
                                {
                                    config = element.Attribute("value").Value;
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                config = string.Empty;
            }
            return config;
        }
    }
}
