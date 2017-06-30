using System;
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
    }
}
