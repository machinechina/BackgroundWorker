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
        /// <param name="outputEvent"></param>
        /// <param name="errorEvent"></param>
        /// <returns>Process Id</returns>
        public static Process RunProcessAsync(string fileName, string args = "",
            DataReceivedEventHandler outputEvent = null,
            DataReceivedEventHandler errorEvent = null)
        {
            return ( Process )RunProcess(fileName, args, false, outputEvent);
        }

        private static object RunProcess(string fileName, string args = "", bool waitForExit = false,
            DataReceivedEventHandler outputEvent = null,
            DataReceivedEventHandler errorEvent = null)
        {
            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();
            using (var proc = new Process
            {
                StartInfo =
                            {
                                FileName =fileName,
                                Arguments =args,
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                RedirectStandardOutput=true,
                                RedirectStandardError=true
                            }
            })
            {

                proc.OutputDataReceived += outputEvent ??
                    new DataReceivedEventHandler((sender, e) =>
                     output.AppendLine(e.Data ?? ""));
                proc.ErrorDataReceived += errorEvent ??
                    new DataReceivedEventHandler((sender, e) =>
                     error.AppendLine(e.Data ?? ""));

                if (!proc.Start())
                    throw new InvalidOperationException($"启动进程错误:{proc}");

                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                if (waitForExit)
                {
                    proc.WaitForExit();
                    if (!proc.ExitCode.Equals(0))
                        throw new Exception(
                            $"执行进程错误,错误码:{proc.ExitCode}\n" +
                            $"进程:{fileName}\n" +
                            $"参数:{args}\n" +
                            $"输出:{output}\n" +
                            $"错误:{error}");
                    return output;
                }
                else
                    return proc;
            }
        }


    }
}
