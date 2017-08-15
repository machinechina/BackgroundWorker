using System;
using System.Collections.Generic;
using System.Threading;
using Infrastructure.Workers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Infrastructure.UnitTest
{
    [TestClass]
    public class WorkerTest
    {
        [TestMethod]
        public void ShortRunningWorkerTest()
        {
            Worker w = new ShortRunningWorker(() =>
              {
                  Thread.Sleep(5000);
              });
            w.Start();
            Assert.IsTrue(w.IsRunning);

            w.Stop();
            Assert.IsFalse(w.IsRunning);

            w.Start();
            Assert.IsTrue(w.IsRunning);

            w.WaitForExit();
            Assert.IsFalse(w.IsRunning);
        }

        [TestMethod]
        public void LongRunningWorkerTest()
        {
        }

        [TestMethod]
        public void ScheduleWorkerTest()
        {
            var startTime = DateTime.Now;
            var timeStamps = new List<DateTime>();
            var w = new ScheduleWorker(() =>
              {
                  timeStamps.Add(DateTime.Now);
              }, DateTime.Now.AddSeconds(5), TimeSpan.FromSeconds(5));
            w.Start();
            Thread.Sleep(TimeSpan.FromSeconds(16));

            w.Stop();
            Assert.AreEqual(3, timeStamps.Count);
            for (int i = 0; i < 3; i++)
            {
                var expectTime = startTime.AddSeconds((i + 1) * 5);
                var actualTime = timeStamps[i];
                Assert.IsTrue(1 >
                    Math.Abs((expectTime - actualTime).TotalSeconds));
            }
        }

        [TestMethod]
        public void TimerTest()
        {
            var timeStamps = new List<DateTime>();
            var runTime = DateTime.Now.AddSeconds(5);
            new Timer(_ =>
            {
                timeStamps.Add(DateTime.Now);
            }, null, runTime - DateTime.Now, TimeSpan.FromSeconds(5));
            Thread.Sleep(TimeSpan.FromSeconds(16));
            Assert.AreEqual(3, timeStamps.Count);
          
        }
    }
}
