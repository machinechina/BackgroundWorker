using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Native
{
    public class UncConnection : IDisposable
    {
        string _networkName;

        /// <summary>
        /// 获得UNC路径的访问权
        /// 用法: using (new UncConnection(@"\\UNC", new NetworkCredential("user", "password")))
        /// </summary>
        /// <param name="networkName"></param>
        /// <param name="credentials"></param>
        public UncConnection(string networkName,
            NetworkCredential credentials)
        {
            _networkName = networkName;

            var netResource = new NetResource()
            {
                Scope = ResourceScope.GlobalNetwork,
                ResourceType = ResourceType.Disk,
                DisplayType = ResourceDisplaytype.Share,
                RemoteName = networkName
            };

            var userName = string.IsNullOrEmpty(credentials.Domain)
                ? credentials.UserName
                : string.Format(@"{0}\{1}", credentials.Domain, credentials.UserName);

            var result = WNetAddConnection2(
                netResource,
                credentials.Password,
                userName,
                0);

            if (result != 0)
            {
                string strErrMsg = "";
                if (result == 53)
                {
                    strErrMsg = "未找到网络路径.(网络路径不能以'/'结尾)";
                }
                else if (result == 67)
                {
                    strErrMsg = "未找到网络名称";
                }
                if (result == 86)
                {
                    strErrMsg = "错误的用户名或密码";
                }
                else if (result == 1219)
                {
                    strErrMsg = "无法使用同一账户对服务器或共享目录进行多重连接.(确保[应用程序池-进程模型-标识]使用了正确的账户";
                }

                throw new Win32Exception(result, "Error connecting to " + networkName + " remote share.Error Code:" + result.ToString() + "." + strErrMsg);
            }
        }

        ~UncConnection()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            WNetCancelConnection2(_networkName, 0, true);
        }

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(NetResource netResource,
            string password, string username, int flags);

        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2(string name, int flags,
            bool force);
    }

    [StructLayout(LayoutKind.Sequential)]
    public class NetResource
    {
        public ResourceScope Scope;
        public ResourceType ResourceType;
        public ResourceDisplaytype DisplayType;
        public int Usage;
        public string LocalName;
        public string RemoteName;
        public string Comment;
        public string Provider;
    }

    public enum ResourceScope : int
    {
        Connected = 1,
        GlobalNetwork,
        Remembered,
        Recent,
        Context
    };

    public enum ResourceType : int
    {
        Any = 0,
        Disk = 1,
        Print = 2,
        Reserved = 8,
    }

    public enum ResourceDisplaytype : int
    {
        Generic = 0x0,
        Domain = 0x01,
        Server = 0x02,
        Share = 0x03,
        File = 0x04,
        Group = 0x05,
        Network = 0x06,
        Root = 0x07,
        Shareadmin = 0x08,
        Directory = 0x09,
        Tree = 0x0a,
        Ndscontainer = 0x0b
    }
}
