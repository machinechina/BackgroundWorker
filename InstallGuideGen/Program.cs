using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace InstallGuideGen
{
    public class Program
    {
        const string ROW_TEMPLATE_INPUT =
            "<tr class=\"tr\"><td class=\"tdleft\">@@desc</td>\n" +
                "<td class=\"tdright\">\n" +
                    "<input class=\"customValue\" id=\"@@key\" value=\"@@value\" />\n" +
                "</td>\n" +
            "</tr>\n";

        const string ROW_TEMPLATE_SELECT =
            "<tr class=\"tr\"><td class=\"tdleft\">@@desc</td>\n" +
                "<td class=\"tdright\">\n" +
                    "<select class=\"customValue\" id=\"@@key\" value=\"@@value\" >\n" +
                        "@@options" +
                    "</select>" +
                "</td>\n" +
            "</tr>\n";

        const string ROW_TEMPLATE_OPTION = "<option> @@value</option> \n";
        //注释的格式为: xxxxxx[a,b,c] 其中a,b,c为下拉选项
        const string COMMENT_FORMET = @"(\[.*\])";
        static void Main(string[] args)
        {
            try
            {
                StringBuilder rows = new StringBuilder();

                XmlDocument xml = new XmlDocument();

                var appFilePath = Directory.GetFiles(".", "*.application", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault();

                Console.WriteLine($"应用路径:{appFilePath}");

                var appName = Path.GetFileNameWithoutExtension(appFilePath);

                Console.WriteLine($"应用标识:{appName}");

                xml.Load(appFilePath);

                var appTitle = xml.GetElementsByTagName("description")[0].Attributes["asmv2:product"].Value;

                Console.WriteLine($"应用名称:{appTitle}");

                var configFilePath = xml.GetElementsByTagName("dependentAssembly")[0].Attributes["codebase"].Value.Replace("exe.manifest", "exe.config.deploy");

                Console.WriteLine($"配置文件:{configFilePath}");

                xml.Load(configFilePath);

                foreach (XmlNode node in xml.GetElementsByTagName("appSettings")[0].ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Element && node.Name == "add")
                    {
                        var key = node.Attributes["key"].Value;
                        var value = node.Attributes["value"].Value;
                        var desc = key;
                        var prevNode = node.PreviousSibling;
                        var selectable = false;
                        if (prevNode?.NodeType == XmlNodeType.Comment)
                        {
                            selectable = Regex.IsMatch(prevNode.Value, COMMENT_FORMET);
                            desc = prevNode.Value;
                        }
                        if (selectable)
                        {
                            rows.AppendLine(
                                ROW_TEMPLATE_SELECT
                                    .Replace("@@desc",
                                        Regex.Replace(desc, COMMENT_FORMET, ""))
                                    .Replace("@@key", key)
                                    .Replace("@@value", value)
                                    .Replace("@@options",
                                       string.Join("",
                                            Regex.Match(desc, COMMENT_FORMET)
                                                .Value.Trim('[', ']').Split(',')
                                                .Select(v => ROW_TEMPLATE_OPTION
                                                    .Replace("@@value", v)))));
                        }
                        else
                        {
                            rows.AppendLine(
                                  ROW_TEMPLATE_INPUT
                                      .Replace("@@desc", desc)
                                      .Replace("@@key", key)
                                      .Replace("@@value", value));
                        }

                        Console.WriteLine($"替换配置{key}");
                    }
                }


                var template = File.OpenText("InstallGuideTemplate.html").ReadToEnd();

                File.WriteAllText("default.aspx",
                    template
                        .Replace("@@Rows", rows.ToString())
                        .Replace("@@Title", appTitle)
                        .Replace("@@AppName", appName),
                    Encoding.UTF8);

                Console.WriteLine($"生成完毕");

            }
            catch (Exception ex)
            {
                Console.WriteLine("错误:请将此程序和InstallGuideTemplate.html一起置于Clickonce发布目录(和.application文件同一目录)后运行\n");

                Console.WriteLine(ex.Message);

            }


        }
    }
}
