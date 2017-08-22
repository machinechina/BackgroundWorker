using System;
using Infrastructure.Workers;

namespace Infrastructure.UnitTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ScheduleWorker w = new ScheduleWorker(() => Console.WriteLine(DateTime.Now), DateTime.Now.AddSeconds(5), TimeSpan.FromMinutes(1));
            w.Start();
            Console.ReadLine();
        }
    }
}