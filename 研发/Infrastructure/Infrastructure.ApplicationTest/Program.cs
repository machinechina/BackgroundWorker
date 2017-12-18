using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Infrastructure.Helpers;

namespace Infrastructure.ApplicationTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Helper.InitDeployQueryString();
            Helper.EnsureUserRights(Helper.UserRights.RUN_AS_ADMIN);
          
            Console.WriteLine($"{nameof(Helper.JustUpdatedOrReinstalled)}：{Helper.JustUpdatedOrReinstalled}");
            Console.WriteLine($"value of a：{Helper.GetConfigFromDeployThenAppConfig<string>("a")}");
            Console.ReadLine();
            //隐藏模式
            Application.Run();

        }
    }
}
