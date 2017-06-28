using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Extensions;

namespace Infrastructure.UnitTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Helpers.Helper.RunAsBackgroundService(() =>
            {

            }, null);


        }
    }
 
}
