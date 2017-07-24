using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiskQueue;
using Infrastructure.Extensions;
using Infrastructure.Helpers;
using Infrastructure.Native;
using Infrastructure.Web;

namespace Infrastructure.UnitTest
{
    public class A : IApiResult
    {
        public string ErrorCode => a;

        public string Message => a + "m";

        public string a { get; set; }
    }

    public static class B
    {
        public static bool b { get; private set; }
        static B()
        {
            b = true;
        }
    }

    public class Program
    {

        public static void Main(string[] args)
        {
            DeviceChangeNotifier.DeviceNotify += 
                msg => {
                    Helper.Info(msg.ToString());
            };
            DeviceChangeNotifier.Start();

            Console.ReadLine();
        }



        private static void ConcurrentEnqueDequeue(string[] args)
        {

            if (args.Length > 0)
            {
                var total = 1000;
                for (int i = 0; i < 10; i++)
                {
                    Task.Run(() =>
                    {
                        while (total >= 0)
                        {
                            try
                            {
                                using (var queue = PersistentQueue.WaitFor("d:\\_QUEUE", TimeSpan.FromSeconds(30)))
                                using (var session = queue.OpenSession())
                                {
                                    var data = session.Dequeue(); ;
                                    session.Flush();
                                    if (data != null)
                                    {
                                        Console.WriteLine($"Dequeue { Encoding.UTF8.GetString(data)}");
                                        Interlocked.Decrement(ref total);
                                    }
                                }
                                Thread.Sleep(100);
                            }
                            catch (Exception ex)
                            {
                                Helper.Log(ex);
                            }
                        }
                    });
                }
            }
            else
            {
                var total = 1000;

                for (int i = 0; i < 10; i++)
                {
                    Task.Run(() =>
                    {
                        var data = i.ToString();
                        while (total >= 0)
                        {
                            try
                            {
                                using (var queue = PersistentQueue.WaitFor("d:\\_QUEUE", TimeSpan.FromSeconds(30)))
                                using (var session = queue.OpenSession())
                                {
                                    session.Enqueue(Encoding.UTF8.GetBytes(data));
                                    session.Flush();
                                    Console.WriteLine($"Enqueue {data}");
                                    Interlocked.Decrement(ref total);
                                }
                                Thread.Sleep(100);
                            }
                            catch (Exception ex)
                            {
                                Helper.Log(ex);
                            }
                        }
                    });
                }
            }
        }
    }
}