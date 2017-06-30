using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiskQueue;
using Infrastructure.Helpers;

namespace Infrastructure.UnitTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Helpers.Helper.RunAsBackgroundService(() =>
            //{
            //}, null);

            if (args.Length > 0)
            {
                var total = 1000;
                for (int i = 0; i < 10; i++)
                {
                    Task.Run(() =>
                    {
                        while (total>= 0)
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

            Console.ReadLine();
        }
    }
}