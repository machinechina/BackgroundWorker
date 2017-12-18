using System;
using Infrastructure.Helpers;
using Infrastructure.Web;

namespace Infrastructure.UnitTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var a =  ErrorCode.NoError;

            Helper.Log("aaa");
            Helper.Log("bbb");

            Console.ReadLine();
        }
 
    }
}