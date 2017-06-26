﻿using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Infrastructure.Extension;
using Infrastructure.Extensions;

namespace Infrastructure.Helpers
{
    public partial class Helper
    {
        public static string AppName
        {
            get
            {
                return System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            }
        }

        public enum AdminRights
        {
            BUILTIN_ADMIN,
            RUN_AS_ADMIN
        }

        public static bool RunningInAdmin()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);
            return wp.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// 验证是否已管理员权限运行程序
        /// 仅在clickonce发布且程序需要管理员权限时使用
        /// 策略1:BuiltInAdmin (建议后台服务程序使用)
        /// clickonce本身无法设置admin权限,这里尝试使用关闭UAC的方式启用内置管理员账号(built-in administrator)
        /// 优点:
        /// 直接改系统配置,不对程序本身有任何运行时要求
        /// 缺点:
        /// MetroApp无法正常使用;
        /// [严重]Win10的某个版本在关闭内置管理员后,第二次安装会无法打开程序,该程序会处于锁定状态(需要在文件属性中手动解锁才能使用),如果发生这种情况要考虑使用RunAsAdmin策略
        /// 策略2:RunAsAdmin (建议一般程序使用)
        /// 程序运行时发现非管理员权限时,尝试以管理员权限重启(调用自身进程)
        /// 此时的进程虽然管理员权限,但是不属于网络部署,无法自动升级
        /// 因此再次关闭自身并调用快捷方式(appref-ms文件,startmenu文件夹),第三次启动后的程序将同时拥有管理员权限和自动升级功能
        /// 优点:
        /// 不依赖于系统
        /// 缺点:
        /// 流程复制,程序一共要启动三次
        /// [注意]只有第一次启动时能初始化url参数,因此<see cref="InitDeployQueryString"/>必须在此方法之前调用
        /// </summary>
        public static void EnsureAdminRights(AdminRights adminRights = AdminRights.BUILTIN_ADMIN)
        {
            if (adminRights == AdminRights.BUILTIN_ADMIN)
            {
                #region Built-in admin

                if (!RunningInAdmin())
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
                else
                {
                    Log($"Native Run as Admin : {Environment.GetCommandLineArgs().ToJointString()}");
                }

                #endregion Built-in admin
            }
            else if (adminRights == AdminRights.RUN_AS_ADMIN)
            {
                #region Runas admin

                if (!RunningInAdmin())//首次运行
                {
                    Log($"RUN_AS_ADMIN 1/3 :  {Environment.GetCommandLineArgs().ToJointString()} ");

                    var currentProcessName = Process.GetCurrentProcess().ProcessName + ".exe";

                    using (MemoryStream memoryStream = new MemoryStream(AppDomain.CurrentDomain.ActivationContext.DeploymentManifestBytes))
                    using (XmlTextReader xmlTextReader = new XmlTextReader(memoryStream))
                    {
                        var xDocument = XDocument.Load(xmlTextReader);
                        var description = xDocument.Root.Elements().Where(e => e.Name.LocalName == "description").First();
                        var publisher = description.Attributes().Where(a => a.Name.LocalName == "publisher").First().Value;
                        var product = description.Attributes().Where(a => a.Name.LocalName == "product").First().Value;

                        Process.Start(
                              new ProcessStartInfo
                              {
                                  FileName = currentProcessName,
                                  Verb = "runas",
                                  Arguments = $"GrantAdmin {publisher} {product}"
                              });
                    }

                    Environment.Exit(0);
                }
                else if (Environment.GetCommandLineArgs().Contains("GrantAdmin"))//以管理员运行
                {
                    Log($"RUN_AS_ADMIN 2/3 :  {Environment.GetCommandLineArgs().ToJointString()} ");

                    var appFromStartMenu = $"\"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).PathCombine(@"Microsoft\Windows\Start Menu\Programs").PathCombine(Environment.GetCommandLineArgs()[2]).PathCombine(Environment.GetCommandLineArgs()[3])}.appref-ms\"";

                    Process.Start(appFromStartMenu, "GrantUpdating");

                    Environment.Exit(0);
                }
                else if (AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData?[0] == "GrantUpdating")//以管理员重新运行appref
                {
                    Log($"RUN_AS_ADMIN 3/3 :  {Environment.GetCommandLineArgs().ToJointString()} {AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData.ToJointString()} ");
                }
                else
                {
                    Log($"Native Run as Admin : {Environment.GetCommandLineArgs().ToJointString()}");
                }

                #endregion Runas admin
            }
        }

        public static void EnsureSingleRunning(Mutex mutex)
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

        public static string Version
        {
            get
            {
                return ApplicationDeployment.IsNetworkDeployed ? ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() : "";
            }
        }

        /// <summary>
        /// 通过网络部署时可以在安装链接后传递querystring(需要在项目属性中开启)
        /// 如http://....../CDN.ConsoleApp.application?SyncApiParam=123123
        /// 这样可以实现一次部署,不同配置
        /// 但程序在本地再次打开时,将取不到这个参数,所以第一次安装后需要持久化
        /// 尽可能早的调用InitDeployQueryString已持久化参数
        /// 并在需要的时候用GetDeployQueryString获取
        /// </summary>
        /// <returns></returns>
        private static IDictionary<string, string> _deployQuerys;

        private static string _localStorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "_deployQuerys_" + AppName);

        public static void InitDeployQueryString(params string[] requiredKeys)
        {
            if (ApplicationDeployment.IsNetworkDeployed)//只有网络部署会用到Url Params
            {
                try
                {
                    //从url安装时获取到,之后的自动/手动升级都不会再访问到,Query是get访问器会直接抛异常
                    _deployQuerys = HttpUtility.ParseQueryString(ApplicationDeployment.CurrentDeployment.ActivationUri.Query)
                        .ToDictionary();

                    File.WriteAllText(_localStorePath, _deployQuerys.ToJson());//一旦获取到url params就持久化
                }
                catch { _deployQuerys = new Dictionary<string, string>(); }

                //必选参数,如果没找到则抛异常
                foreach (var key in requiredKeys)
                {
                    GetDeployQueryString(key);
                }
            }
        }

        public static T GetConfigFromDeployThenAppConfig<T>(string key)
        {
            if (!ApplicationDeployment.IsNetworkDeployed)
            {
                return GetAppConfig<T>(key);
            }

            try
            {
                return ( T )Convert.ChangeType(GetDeployQueryString(key), typeof(T));
            }
            catch (KeyNotFoundException)
            {
                return GetAppConfig<T>(key);
            }
        }

        private static string GetDeployQueryString(string key)
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
                    var valueFromLocal = File.ReadAllText(_localStorePath).JsonToObject<Dictionary<string, string>>()[key];
                    _deployQuerys.Add(key, valueFromLocal);
                    return valueFromLocal;
                }
            }
            catch
            {
                throw new KeyNotFoundException($"Missing param from url [{key}]");
            }
        }
    }
}