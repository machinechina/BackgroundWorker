using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Infrastructure.Extensions;

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
        public static int RunProcessAsync(string fileName, string args = "",
            DataReceivedEventHandler outputEvent = null,
            DataReceivedEventHandler errorEvent = null)
        {
            return ( int )RunProcess(fileName, args, false, outputEvent);
        }

        private static object RunProcess(string fileName, string args = "", bool waitForExit = false,
            DataReceivedEventHandler outputEvent = null,
            DataReceivedEventHandler errorEvent = null)
        {
            //true:ShellExecute,false:openProcess
            //true相当于在run里面执行,可以运行非exe程序,但不可获取输出
            //同步模式下只能运行exe程序
            var useShellExecute = !Path.GetExtension(fileName)
                                        .ToLower()
                                        .IncludeIn(".exe", "");
            //非exe程序不可获取输出
            var redirectable = !useShellExecute;
            //非exe程序必须创建window
            var noWindow = !useShellExecute;
            if (waitForExit && useShellExecute)
                throw new Exception("同步模式下只能运行exe程序");

            var output = new StringBuilder();
            var error = new StringBuilder();

            using (var proc = new Process
            {
                StartInfo =
                            {
                                FileName =fileName,
                                Arguments =args,
                                UseShellExecute =useShellExecute,
                                CreateNoWindow = noWindow,
                                RedirectStandardOutput=redirectable,
                                RedirectStandardError=redirectable
                            }
            })
            {
                if (redirectable)
                {
                    proc.OutputDataReceived += outputEvent ??
                    new DataReceivedEventHandler((sender, e) =>
                     output.AppendLine(e.Data ?? ""));
                    proc.ErrorDataReceived += errorEvent ??
                        new DataReceivedEventHandler((sender, e) =>
                         error.AppendLine(e.Data ?? ""));
                }

                if (!proc.Start())
                    throw new InvalidOperationException($"启动进程错误:{proc}");

                if (redirectable)
                {
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                }

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
                    return proc.Id;

            }
        }
    }
}