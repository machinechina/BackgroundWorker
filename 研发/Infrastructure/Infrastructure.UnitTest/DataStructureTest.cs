using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Infrastructure.UnitTest
{
    [TestClass]
    public class DataStructureTest
    {
        [TestMethod]
        public void ConcurrentBagAddDistinctItems()
        {
            List<string> data = new List<string>();
            var random = new Random();
            for (int i = 0; i < 1000; i++)
            {
                data.Add(random.Next(1, 4).ToString());
            }

            Assert.AreEqual(1000, data.Count);

            for (int i = 0; i < 10; i++)
            {
                ConcurrentBag<string> bag = new ConcurrentBag<string>();
                data.AsParallel().ForAll(d =>
                {
                    lock (data)
                    {
                        if (!bag.Contains(d))
                        {
                            Thread.Sleep(100);
                            bag.Add(d);
                            Thread.Sleep(100);
                        }
                    }
                });

                Assert.AreEqual(3, bag.Count);
            }
        }

        [TestMethod]
        public void IfParallelWaiting()
        {
            var list = new[] { "a", "b", "c" };
            Parallel.ForEach(list, i =>
            {
                Thread.Sleep(5000);
                Console.WriteLine(i);
            });

            list.AsParallel().ForAll(i =>
            {
                Thread.Sleep(5000);
                Console.WriteLine(i);
            });
        }

        [TestMethod]
        public void SyncnInsertDBWithNoQueue()
        {
            using (var db = new TestModel())
            {
                db.Database.ExecuteSqlCommand("delete from TestTables");

                for (int i = 0; i < 100; i++)
                {
                    db.TestTables.Add(new TestTable { TestColumn1 = Guid.NewGuid().ToString() });
                    db.SaveChanges();
                }

                Assert.AreEqual(100, db.TestTables.Count());
            }
        }
    }
}
