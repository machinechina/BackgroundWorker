using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    public partial class Helper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="args"></param>
        /// <returns>Process output</returns>
        public static string RunProcess(string fileName, string args = "")
        {
            return RunProcess(fileName, args, true).ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="args"></param>
        /// <returns>Process Id</returns>
        public static int RunProcessAsync(string fileName, string args = "")
        {
            return ( int )RunProcess(fileName, args, false);
        }

        private static object RunProcess(string fileName, string args = "", bool waitForExit = false)
        {
            var output = "";
            var error = "";
            using (var proc = new Process
            {
                StartInfo =
                            {
                                FileName =fileName,
                                Arguments =args,
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                RedirectStandardOutput=waitForExit,
                                RedirectStandardError=waitForExit
                            }
            })
            {
                if (waitForExit)
                {
                    proc.OutputDataReceived += (sender, e) =>
               output += e.Data;
                    proc.ErrorDataReceived += (sender, e) =>
                    error += e.Data;
                }

                if (!proc.Start())
                    throw new InvalidOperationException($"启动进程错误:{proc}");

                if (waitForExit)
                {
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                    proc.WaitForExit();
                    if (!proc.ExitCode.Equals(0))
                    {
                        throw new Exception($"执行进程错误,错误码:{proc.ExitCode}\n进程:{fileName}\n参数:{args}\n输出:{output}\n错误:{error}");
                    }
                    return output;
                }

                return proc.Id;
            }
        }


    }
}
