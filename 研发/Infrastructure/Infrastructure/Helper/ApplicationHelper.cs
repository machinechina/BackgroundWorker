using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using Infrastructure.Extensions;
using Infrastructure.Workers;

namespace Infrastructure.Helpers
{
    public partial class Helper
    {

        #region Private Fields

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

        #endregion Private Fields

        #region Public Constructors

        static Helper()
        {
            //"If the directory already exists, this method does not create a new directory, but it returns a DirectoryInfo object for the existing directory."
            Directory.CreateDirectory(_localLogPath);

            //fire an event when program closing by user operations while running in console mode.
            Console.CancelKeyPress += (o, e) => ConsoleExiting?.Invoke(ConsoleExitReason.CancelKeyPressed);
            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);
        }

        #endregion Public Constructors

        #region Public Enums

        /// <summary>
        ///
        /// </summary>
        public enum UserRights
        {
            /// <summary>
            ///
            /// </summary>
            NORMAL_USER,

            /// <summary>
            ///
            /// </summary>
            BUILTIN_ADMIN,

            /// <summary>
            ///
            /// </summary>
            RUN_AS_ADMIN
        }

        #endregion Public Enums

        #region Public Properties

        /// <summary>
        /// First time running after update
        /// It's 3-state boolean 
        /// because once called,old version will be set equal to new version 
        /// It cannot be init in static ctor 
        /// because sometimes app will restart to grant admin authorize
        /// </summary>
        public static bool JustUpdated
        {
            get
            {
                if (_justUpdated.HasValue)
                {
                    return _justUpdated.Value;
                }
                else if (ApplicationDeployment.IsNetworkDeployed)
                {
                    _justUpdated = ReadFile(_localVersionPath) != ProductDescription.Version;
                    File.WriteAllText(_localVersionPath, ProductDescription.Version);
                    return _justUpdated.Value;
                }
                return false;
            }
        }
        private static bool? _justUpdated;

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// 检查更新
        /// </summary>
        /// <param name="afterUpdated"></param>
        public static void CheckUpdate(Action afterUpdated)
        {
            UpdateCheckInfo updateInfo = null;

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                try
                {
                    ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
                    updateInfo = ad.CheckForDetailedUpdate();

                    if (updateInfo.UpdateAvailable)
                    {
                        ad.Update();
                        afterUpdated();
                    }
                }
                catch (Exception ex)
                {
                    Log(ex);
                    throw new Exception($"升级到{updateInfo?.AvailableVersion?.ToString() ?? "最新版本"}时出错");
                }
            }
        }

        /// <summary>
        /// 确保单实例运行
        /// </summary>
        /// <param name="mutex"></param>
        public static void EnsureSingleRunning(Mutex mutex)
        {
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                throw new Exception("已有此程序的另一个实例在运行");
            }
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
        /// 流程复杂,程序一共要启动三次
        /// [注意]只有第一次启动时能初始化url参数,因此<see cref="InitDeployQueryString"/>必须在此方法之前调用
        /// </summary>
        public static void EnsureUserRights(UserRights userRights = UserRights.BUILTIN_ADMIN)
        {
            if (userRights == UserRights.BUILTIN_ADMIN)
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
            else if (userRights == UserRights.RUN_AS_ADMIN)
            {
                #region Runas admin

                if (!RunningInAdmin())//首次运行
                {
                    Log($"RUN_AS_ADMIN 1/3 :  {Environment.GetCommandLineArgs().ToJointString()} ");

                    var currentProcessName = Process.GetCurrentProcess().ProcessName + ".exe";

                    Process.Start(
                              new ProcessStartInfo
                              {
                                  FileName = currentProcessName,
                                  Verb = "runas",
                                  Arguments = $"GrantAdmin {ProductDescription.Publisher} {ProductDescription.Product}"
                              });
                    Environment.Exit(0);
                }
                else if (Environment.GetCommandLineArgs().Contains("GrantAdmin"))//以管理员运行
                {
                    Log($"RUN_AS_ADMIN 2/3 :  {Environment.GetCommandLineArgs().ToJointString()} ");

                    var appFromStartMenu = $"\"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).PathCombine(@"Microsoft\Windows\Start Menu\Programs").PathCombine(Environment.GetCommandLineArgs()[2]).PathCombine(Environment.GetCommandLineArgs()[3])}.appref-ms\"";

                    Process.Start(appFromStartMenu, "GrantUpdating");

                    Environment.Exit(0);
                }
                else if (AppDomain.CurrentDomain.SetupInformation.ActivationArguments?.ActivationData?[0] == "GrantUpdating")//以管理员重新运行appref
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

        public static object GetConfigFromDeployThenAppConfig(string key, Type type)
        {
            if (!ApplicationDeployment.IsNetworkDeployed)
            {
                return GetAppConfig(key, type);
            }

            try
            {
                return Convert.ChangeType(GetDeployQueryString(key), type);
            }
            catch (KeyNotFoundException)
            {
                return GetAppConfig(key, type);
            }
        }

        public static T GetConfigFromDeployThenAppConfig<T>(string key)
        {
            return ( T )GetConfigFromDeployThenAppConfig(key, typeof(T));
        }

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

        /// <summary>
        /// 适用于控制台的后台服务框架
        /// </summary>
        /// <param name="configKeyAndFieldNames">需要初始化的配置项字段 key:配置项的key,例:"KeyOfUser",value:配置项的名称,例:nameof(_keyOfUser)</param>
        /// <param name="tryBlock">核心代码</param>
        /// <param name="finallyBlock">清理代码</param>
        /// <param name="userRights">需要的权限</param>
        /// <param name="updateInterval">升级检查时间,默认1小时</param>
        public static void RunAsBackgroundService(Dictionary<string, string> configKeyAndFieldNames, Action tryBlock, Action finallyBlock = null, UserRights userRights = UserRights.NORMAL_USER, int updateInterval = 3600000)
        {
            var mutex = new Mutex(true, ProductDescription.AppName);
            var exitForUpdating = false;

            try
            {
                EnsureSingleRunning(mutex);

                InitDeployQueryString();

                EnsureUserRights(userRights);

                InfoAndLog(ApplicationDeployment.IsNetworkDeployed ? $"{ProductDescription.Product} {ProductDescription.Version}" : ProductDescription.AppName);

                InitConfigurations(configKeyAndFieldNames);

                tryBlock?.Invoke();

                var updateWorker = new UpdateCheckingWorker(updateInterval);
                updateWorker.Start();
                updateWorker.WaitForExit();
                exitForUpdating = true;
                InfoAndLog("找到更新,准备重启...\n");
            }
            catch (Exception ex)
            {
                InfoAndLog(ex);
                Thread.Sleep(5000);
            }
            finally
            {
                finallyBlock?.Invoke();

                WorkerFactory.StopAll();

                mutex.Close();

                if (exitForUpdating)
                {
                    Application.Restart();
                }
            }
        }

        public static bool RunningInAdmin()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);
            return wp.IsInRole(WindowsBuiltInRole.Administrator);
        }

        #endregion Public Methods

        #region Private Methods

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

        /// <summary>
        /// 根据传入的config key 获取config value,根据传入的字段名通过反射赋值
        /// </summary>
        /// <param name="configKeyAndFieldNames"></param>
        private static void InitConfigurations(Dictionary<string, string> configKeyAndFieldNames)
        {
            //获取调用者的类型
            //使用StackFrame是一个危险的行为,这里(2)表示调用堆栈向上两层,找到外部调用者
            //第1层InitConfigurations 第2层RunAsBackgroundService
            //此处重构代码很有可能影响调用的层数!
            var callerType = new StackFrame(2).GetMethod().DeclaringType;
            configKeyAndFieldNames.ForEach(configKeyAndFieldName =>
            {
                var configField = callerType
                  .GetField(configKeyAndFieldName.Value,
                   BindingFlags.Public
                   | BindingFlags.NonPublic
                   | BindingFlags.Static);//获取调用者的字段,必须是static
                var configValue = GetConfigFromDeployThenAppConfig(
                        configKeyAndFieldName.Key,
                        configField?.FieldType);
                configField?.SetValue(null, configValue);

                InfoAndLog($"参数:{configKeyAndFieldName.Key} 值:{configValue}");
            });
        }

        #endregion Private Methods


        #region Console Closing Events
        public enum ConsoleExitReason
        {
            WindowClosed, CancelKeyPressed
        }
        public delegate void ConsoleExitDelegate(ConsoleExitReason exitReason);
        public static event ConsoleExitDelegate ConsoleExiting;

        static bool ConsoleEventCallback(int eventType)
        {

            if (eventType == 2)
            {
                ConsoleExiting?.Invoke(ConsoleExitReason.WindowClosed);
            }
            return false;
        }
        static ConsoleEventDelegate handler;
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        #endregion
    }
}