using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Infrastructure.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public class ProductDescription
    {

        #region Public Constructors

        static ProductDescription()
        {
            AppName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                Version = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                using (MemoryStream memoryStream = new MemoryStream(AppDomain.CurrentDomain.ActivationContext.DeploymentManifestBytes))
                using (XmlTextReader xmlTextReader = new XmlTextReader(memoryStream))
                {
                    var xDocument = XDocument.Load(xmlTextReader);
                    var description = xDocument.Root.Elements().Where(e => e.Name.LocalName == "description").First();

                    Product = description.Attributes().Where(a => a.Name.LocalName == "product").First().Value;
                    Publisher = description.Attributes().Where(a => a.Name.LocalName == "publisher").First().Value;
                }
            }
        }

        #endregion Public Constructors

        #region Public Properties

        public static string AppName { get; set; } = "";
        public static string Product { get; set; } = "";
        public static string Publisher { get; set; } = "";
        public static string Version { get; set; } = "";

        #endregion Public Properties

    }

}
