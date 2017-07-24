using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiskQueue;
using Infrastructure.Extensions;
using Infrastructure.QueueWorker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Infrastructure.UnitTest
{
    [TestClass]
    public class QueueWorkerTest
    {
        private readonly string queryRoot = "d:\\_Queue";
        private readonly string testHistory = "d:\\_TestHistory";

        [TestCleanup]
        public void Cleanup()
        {
            Directory.CreateDirectory(queryRoot);
            Directory.CreateDirectory(testHistory);
            Directory.Move(queryRoot, queryRoot.PathCombine(testHistory).PathCombine(DateTime.Now.ToFileTime().ToString()));
        }

        [TestMethod]
        public void EnqueueAndDequeueByWorkBus()
        {
            string[] data = new[] { "000", "111", "222", "333", "444", "555" };
            using (var queueC = new PersistentQueue(queryRoot.PathCombine("A")))
            using (var session = queueC.OpenSession())
            {
                session.Enqueue(Encoding.UTF8.GetBytes(data[0]));
                session.Flush();
            }

            ConcurrentBag<string> result = new ConcurrentBag<string>();
            QueueWorkerBus.CreateDequeuers(queryRoot, result.Add,
               loopInterval: 1000,
                idleLoopCountBeforeStopping: 3,
                workersCountForEachQueue: 2);

            QueueWorkerBus.Enqueue(queryRoot, "A", data[1]);
            QueueWorkerBus.Enqueue(queryRoot, "A", data[2]);
            QueueWorkerBus.Enqueue(queryRoot, "B", data[3]);

            // 3*1000=3秒后,转入休眠
            Thread.Sleep(10000);

            // 监测任务每10秒工作一次,发现有任务唤醒工作任务
            QueueWorkerBus.Enqueue(queryRoot, "B", data[4]);
            QueueWorkerBus.Enqueue(queryRoot, "C", data[5]);
            Thread.Sleep(15000);

            QueueWorkerBus.StopAllDequeuers();

            Assert.AreEqual(data.Length, result.Count);
            Assert.AreEqual(3, Directory.GetDirectories(queryRoot).Length);
        }


        [TestMethod]
        public void QueueWorkFactoryQueueCountStressTest()
        {
            DoQueueWorkFactoryQueueCountStressTest(100);
        }

        private void DoQueueWorkFactoryQueueCountStressTest(int queueCount)
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
            QueueWorkerBus.CreateDequeuers("d:\\_Queue\\STRESS", result.Enqueue, 1000, 0, 2);

            Parallel.For(0, queueCount, i =>
            {
                QueueWorkerBus.Enqueue(queryRoot, $"{i}", Guid.NewGuid().ToString());
            });

            Thread.Sleep(15000);

            QueueWorkerBus.StopAllDequeuers();

            Assert.AreEqual(queueCount * 1.5, result.Count);
            Assert.AreEqual(queueCount, Directory.GetDirectories("d:\\_Queue\\STRESS").Length);
        }

        [TestMethod]
        public void ConcurrentEnqueue()
        {
            ConcurrentQueue<string> result = new ConcurrentQueue<string>();
            QueueWorkerBus.CreateDequeuers("d:\\_Queue\\ENQUEUE",
                d => Thread.Sleep(1000),
                loopInterval: 1000,
                idleLoopCountBeforeStopping: 0,
                workersCountForEachQueue: 10);
            Parallel.For(0, 100, _ =>
            {
                //检查并发会不会造成一个队列的Workers创建了多次
                QueueWorkerBus.Enqueue(queryRoot, $"A", Guid.NewGuid().ToString());
            });
            Thread.Sleep(5000);

            QueueWorkerBus.StopAllDequeuers();
        }

        [TestMethod]
        public void ConcurrentEnqueueUsingDB()
        {
            using (var db = new TestModel())
            {
                db.Database.ExecuteSqlCommand("delete from TestTables");
            }
            ConcurrentQueue<string> result = new ConcurrentQueue<string>();
            QueueWorkerBus.CreateDequeuers("d:\\_Queue\\ENQUEUE", d =>
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
                QueueWorkerBus.Enqueue(queryRoot, $"A", Guid.NewGuid().ToString());
            });
            Thread.Sleep(5000);

            QueueWorkerBus.StopAllDequeuers();

            using (var db = new TestModel())
            {
                Assert.AreEqual(100, db.TestTables.Count());
            }
        }



        [TestMethod]
        public void EnqueueAndDequeue()
        {
            string[] data = new[] { "123", "456", "789" };
            List<string> result = new List<string>();
            QueueWorker.DequeueWorker worker = new QueueWorker.DequeueWorker(queryRoot, "A", result.Add, 1000);
            worker.Start();

            Enqueue("A", data[0]);
            Enqueue("A", data[1]);
            Enqueue("A", data[2]);

            Thread.Sleep(10000);
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void WorkerShouldStopAfterContinuousIdleLoops()
        {
            string[] data = new[] { "123", "456", "789" };
            List<string> result = new List<string>();
            QueueWorker.DequeueWorker worker = new QueueWorker.DequeueWorker(queryRoot, "A", d => Thread.Sleep(1000), 1000, 3);
            worker.Start();
            Enqueue("A", data[0]);
            Enqueue("A", data[1]);
            Enqueue("A", data[2]);
            Thread.Sleep(3000);  
            Assert.IsTrue(worker.IsRunning);
            Thread.Sleep(10000);
            Assert.IsFalse(worker.IsRunning);
        }

        private void Enqueue(string queueName, string data)
        {
            using (var queue = PersistentQueue.WaitFor(Path.Combine(queryRoot, queueName), TimeSpan.FromSeconds(30)))
            using (var session = queue.OpenSession())
            {
                session.Enqueue(Encoding.UTF8.GetBytes(data));
                session.Flush();
            }
        }
    }
}