using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Helpers;

namespace Infrastructure.Helper
{
    public class ApplicationHelper
    {
        public static void CheckAdminRights()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);
            if (!wp.IsInRole(WindowsBuiltInRole.Administrator))
            {
                //try run bat for close UAC
                var uacAutoClosed = false;
                try
                {
                    Process p = Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = "closeUAC.bat",
                            Verb = "runas"
                        });
                    p.WaitForExit();
                    uacAutoClosed = p.ExitCode == 0;
                }
                catch (Exception ex)
                {
                    Log(ex);
                }

                if (uacAutoClosed)
                {
                    throw new Exception("系统已关闭UAC,请重新启动计算机");
                }
                else
                {
                    throw new Exception("此程序必须用管理员权限运行,请确保:\n" +
                   "当前用户拥有管理员权限 \n" +
                   "Win7用户请关闭UAC \n" +
                   @"Win8以上请开启LUA并重启:<PowerShell> Set-ItemProperty -Path ""HKLM:\Software\Microsoft\Windows\CurrentVersion\Policies\System"" -Name ""EnableLUA"" -Value ""0""" + " \n" +
                   "强制管理员权限运行程序可能导致无法自动升级 \n");
                }
            }
        }

        public static void CheckSingleRunning(Mutex mutex)
        {
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                throw new Exception("已有此程序的另一个实例在运行");
            }
        }

        public static void CheckUpdate(Action afterUpdated)
        {
            UpdateCheckInfo info = null;

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                try
                {
                    ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
                    info = ad.CheckForDetailedUpdate();
                    if (info.UpdateAvailable)
                    {
                        ad.Update();
                        afterUpdated();
                    }
                }
                catch (Exception ex)
                {
                    Log(ex);
                    throw new Exception($"升级到{info?.AvailableVersion?.ToString() ?? "最新版本"}时出错");
                }
            }
        }

        public static void Info(String msg)
        {
            Console.WriteLine($"{DateTime.Now.ToString()} -- {msg}");
        }

        public static void Log(Exception ex, String msg = "")
        {
            Log(msg + ex.ToString());
        }

        public static void Log(String msg)
        {
            try
            {
                File.AppendAllText($"{Environment.CurrentDirectory}\\{DateTime.Now.ToString("yyyyMMdd")}.log", DateTime.Now.ToString() + "---" + msg + "\n\n\n");
            }
            catch
            {
            }
        }

        public static String Version
        {
            get
            {
                return ApplicationDeployment.IsNetworkDeployed ? ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() : "";
            }
        }

        /// <summary>
        /// 通过网络部署时可以在安装链接后传递querystring
        /// 如http://....../CDN.ConsoleApp.application?SyncApiParam=123123
        /// 这样可以实现一次部署,不同配置
        /// 但程序在本地再次打开时,将取不到这个参数,所以第一次安装后需要持久化
        /// 尽可能早的调用InitDeployQueryString已持久化参数
        /// 并在需要的时候用GetDeployQueryString获取
        /// </summary>
        /// <returns></returns>
        private static Dictionary<String, String> _deployQuerys;

        private static String _localStorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "_deployQuerys");

        public static void InitDeployQueryString()
        {
            try
            {
                var query = HttpUtility.ParseQueryString(ApplicationDeployment.CurrentDeployment.ActivationUri.Query);
                _deployQuerys = query.Keys.Cast<string>()
                    .ToDictionary(k => k, v => query[v]);
                File.WriteAllText(_localStorePath, _deployQuerys.ToJson());
            }
            catch
            {
                _deployQuerys = new Dictionary<string, string>();
            }
        }

        public static String GetDeployQueryString(String key)
        {
            try
            {
                //from cache
                if (_deployQuerys.ContainsKey(key))
                {
                    return _deployQuerys[key];
                }
                else
                {
                    //from local store
                    var valueFromLocal = File.ReadAllText(_localStorePath).FromJsonToObject<Dictionary<String, String>>()[key];
                    _deployQuerys.Add(key, valueFromLocal);
                    return valueFromLocal;
                }
            }
            catch (Exception ex)
            {
                Log(ex, "No such query string from deploy");
                return String.Empty;
            }
        }
    }
}
