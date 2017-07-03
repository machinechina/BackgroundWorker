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
        public static void RunProcess(string fileName, string args, string output = "", string error = "")
        {
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
                proc.OutputDataReceived += (sender, e) =>
                output += e.Data;
                proc.ErrorDataReceived += (sender, e) =>
                error += e.Data;
                proc.Start();

                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
                if (!proc.ExitCode.Equals(0))
                {
                    throw new Exception($"执行进程错误,错误码:{proc.ExitCode}\n进程名称:{fileName}\n参数:{args}\n支持输出:{output}\n错误输出:{error}");
                }
            }
        }
    }
}
