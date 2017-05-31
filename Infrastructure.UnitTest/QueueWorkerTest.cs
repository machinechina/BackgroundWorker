using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiskQueue;
using Infrastructure.QueueWorker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Infrastructure.UnitTest
{
    [TestClass]
    public class QueueWorkerTest
    {
        [TestMethod]
        public void EnqueueAndDequeueByWorkFactory()
        {
            string[] data = new[] { "123", "456", "789", "000" };
            using (var queueC = new PersistentQueue("d:\\_Queue\\BASIC\\C"))
            using (var session = queueC.OpenSession())
            {
                session.Enqueue(Encoding.UTF8.GetBytes(data[3]));
                session.Flush();
            }

            ConcurrentBag<string> result = new ConcurrentBag<string>();
            QueueWorkerBus.Initialize("d:\\_Queue\\BASIC", result.Add, 1000, 1, 2, true);

            QueueWorkerBus.Enqueue("A", data[0]);
            QueueWorkerBus.Enqueue("A", data[1]);
            QueueWorkerBus.Enqueue("B", data[2]);

            Thread.Sleep(5000);
            QueueWorkerBus.StopAll();

            Assert.AreEqual(data.Length, result.Count);
            Assert.AreEqual(3, Directory.GetDirectories("d:\\_Queue\\BASIC").Length);
        }

        [TestMethod]
        public void EnqueueAndDequeueByWorkFactoryLazy()
        {
            string[] data = new[] { "123", "456", "789", "000" };

            ConcurrentBag<string> result = new ConcurrentBag<string>();
            QueueWorkerBus.Initialize("d:\\_Queue\\BASIC", result.Add, 1000);

            QueueWorkerBus.Enqueue("A", data[0]);
            QueueWorkerBus.Enqueue("A", data[1]);
            QueueWorkerBus.Enqueue("B", data[2]);
            QueueWorkerBus.Enqueue("C", data[3]);

            Thread.Sleep(5000);
            QueueWorkerBus.StopAll();

            Assert.AreEqual(data.Length, result.Count);
            Assert.AreEqual(3, Directory.GetDirectories("d:\\_Queue\\BASIC").Length);
        }

        [TestMethod]
        public void QueueWorkFactoryQueueCountStressTest()
        {
            DoQueueWorkFactoryQueueCountStressTest(100);
        }

        private static void DoQueueWorkFactoryQueueCountStressTest(int queueCount)
        {
            //this create 1000 queue files
            Parallel.For(0, queueCount, i =>
            {
                using (var queueC = new PersistentQueue($"d:\\_Queue\\STRESS\\{i}"))
                using (var session = queueC.OpenSession())
                {
                    if (i % 2 == 0)
                    {
                        //仅有一半的队列初始化有数据,这样剩下一半会Lazy加载
                        session.Enqueue(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
                        session.Flush();
                    }
                }
            });

            //Load 1000 queues and start 1000 workers

            ConcurrentQueue<string> result = new ConcurrentQueue<string>();
            //修改这几个参数测试性能与线程数量
            QueueWorkerBus.Initialize("d:\\_Queue\\STRESS", result.Enqueue, 1000, 0, 2, true);

            Parallel.For(0, queueCount, i =>
            {
                QueueWorkerBus.Enqueue($"{i}", Guid.NewGuid().ToString());
            });

            Thread.Sleep(5000);

            QueueWorkerBus.StopAll();

            Assert.AreEqual(queueCount * 1.5, result.Count);
            Assert.AreEqual(queueCount, Directory.GetDirectories("d:\\_Queue\\STRESS").Length);
        }

        [TestMethod]
        public void ConcurrentEnqueue()
        {
            ConcurrentQueue<string> result = new ConcurrentQueue<string>();
            QueueWorkerBus.Initialize("d:\\_Queue\\ENQUEUE", d => Thread.Sleep(1000), 1000, 0, 10);
            Parallel.For(0, 100, _ =>
            {
                //检查并发会不会造成一个队列的Workers创建了多次
                QueueWorkerBus.Enqueue($"A", Guid.NewGuid().ToString());
            });
            Thread.Sleep(5000);

            QueueWorkerBus.StopAll();
        }

        [TestMethod]
        public void ConcurrentEnqueueUsingDB()
        {
            using (var db = new TestModel())
            {
                db.Database.ExecuteSqlCommand("delete from TestTables");
            }
            ConcurrentQueue<string> result = new ConcurrentQueue<string>();
            QueueWorkerBus.Initialize("d:\\_Queue\\ENQUEUE", d =>
            {
                using (var db = new TestModel())
                {
                    //模拟一些读写开销
                    var data = db.TestTables.FirstOrDefault(row => row.TestColumn1 == d);
                    if (data == null)
                    {
                        db.TestTables.Add(new TestTable { TestColumn1 = d });
                    }
                    db.SaveChanges();
                }
            }, 1000, 0, 10);
            Parallel.For(0, 100, _ =>
            {
                //检查并发会不会造成一个队列的Workers创建了多次
                QueueWorkerBus.Enqueue($"A", Guid.NewGuid().ToString());
            });
            Thread.Sleep(5000);

            QueueWorkerBus.StopAll();

            using (var db = new TestModel())
            {
                Assert.AreEqual(100, db.TestTables.Count());
            }
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

        [TestMethod]
        public void EnqueueAndDequeue()
        {
            string[] data = new[] { "123", "456", "789" };
            List<string> result = new List<string>();
            QueueWorker.QueueWorker worker = new QueueWorker.QueueWorker("d:\\_Queue\\A", result.Add, 1000);
            worker.Start();
            worker.Enqueue(data[0]);
            worker.Enqueue(data[1]);
            worker.Enqueue(data[2]);

            Thread.Sleep(10000);
            Assert.AreEqual(3, result.Count);
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
        public void WorkerShouldStopAfterContinuousIdleLoops()
        {
            string[] data = new[] { "123", "456", "789" };
            List<string> result = new List<string>();
            QueueWorker.QueueWorker worker = new QueueWorker.QueueWorker("d:\\_Queue\\A", d => Thread.Sleep(1000), 1000, 3);
            worker.Start();
            worker.Enqueue(data[0]);
            worker.Enqueue(data[1]);
            worker.Enqueue(data[2]);
            Thread.Sleep(3000);
            Assert.IsTrue(worker.IsRunning);
            Thread.Sleep(5000);
            Assert.IsFalse(worker.IsRunning);
        }

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
    }
}