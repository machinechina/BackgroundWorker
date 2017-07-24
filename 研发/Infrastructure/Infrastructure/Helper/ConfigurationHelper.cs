using System;
using System.Configuration;
using System.Linq;
using Infrastructure.Extensions;
using static System.AppDomain;

namespace Infrastructure.Helpers
{
    public partial class Helper
    {
        public static T GetAppConfig<T>(string key, string configName = null)
        {
            return ( T )GetAppConfig(key, typeof(T), configName);
        }

        public static object GetAppConfig(string key, Type type, string configName = null)
        {
            try
            {
                var value = GetAppConfig(key, configName);
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

        public static void SetAppConfig(string key, string value, string configName = null)
        {
            var config = configName != null ?
                ConfigurationManager.OpenMappedExeConfiguration(
                    new ExeConfigurationFileMap
                    {
                        ExeConfigFilename = CurrentDomain.SetupInformation.ApplicationBase
                    .PathCombine(configName)
                    }, ConfigurationUserLevel.None) :
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.AppSettings.Settings.AllKeys.Contains(key))
                config.AppSettings.Settings[key].Value = value;
            else
                config.AppSettings.Settings.Add(key, value);

            config.Save(ConfigurationSaveMode.Minimal);

            if (configName == null)
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
        }

        private static string GetAppConfig(string key, string configName = null)
        {
            return configName != null ?
               ConfigurationManager.OpenMappedExeConfiguration(
                    new ExeConfigurationFileMap
                    {
                        ExeConfigFilename = CurrentDomain.SetupInformation.ApplicationBase
                    .PathCombine(configName)
                    }, ConfigurationUserLevel.None)
                    .AppSettings.Settings[key].Value :
                ConfigurationManager.AppSettings[key];
        }
    }
}