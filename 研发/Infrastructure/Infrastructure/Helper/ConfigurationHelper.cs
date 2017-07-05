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
            return ( T )GetAppConfig(key, typeof(T));
        }

        public static object GetAppConfig(string key, Type type)
        {
            try
            {
                var value = GetAppConfig(key);
                if (type.IsEquivalentTo(typeof(Guid)))
                {
                    return Convert.ChangeType(new Guid(value), type);
                }
                else
                {
                    return Convert.ChangeType(value, type);
                }
            }
            catch (Exception)
            {

            }
            return default(object);
        }

        private static string GetAppConfig(string key, string configPath = "Config")
        {
            var config = System.Configuration.ConfigurationManager.AppSettings[key];
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
                                var element = xml.Elements().SingleOrDefault(e => e.Attribute("key") != null && e.Attribute("key").Value.Equals(key));
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
