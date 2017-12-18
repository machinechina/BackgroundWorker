using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiskQueue;
using Infrastructure.Workers;

namespace Infrastructure.QueueWorker
{
    /// <summary>
    /// 维护Worker实例,保证启动或队列创建时,有一个Worker负责Dequeue
    /// 同时创建和维护队列实例,同一名称的队列应该只被创建一次,并且始终存活
    /// </summary>
    public static class QueueWorkerBus
    {
        private static Action<string> _dequeueAction;
        private static ConcurrentDictionary<string, DequeueWorker[]> _workers
            = new ConcurrentDictionary<string, DequeueWorker[]>();
        private static QueueWatchWorker _watchWorker;

        private static string _queueRootFolder;
        private static int _loopInterval;
        private static short _workersCountForEachQueue;
        private static int _stopAfterContinuousIdleLoopCount;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="queueRootFolder">队列物理存储目录(必须是空目录)</param>
        /// <param name="dequeueAction">出列后,需要执行的工作</param>
        /// <param name="loopInterval">空闲时的自旋等待周期</param>
        /// <param name="idleLoopCountBeforeStopping">空闲时关闭Worker所需的连续自旋次数,0为不关闭,建议设置以减少开销</param>
        /// <param name="workersCountForEachQueue">每个队列的Worker数</param>
        public static void CreateDequeuers(string queueRootFolder,
            Action<string> dequeueAction,
            int loopInterval,
            int idleLoopCountBeforeStopping = 0,
            short workersCountForEachQueue = 1)
        {
            _queueRootFolder = queueRootFolder;

            _dequeueAction = dequeueAction
                ?? throw new Exception("必须指定操作");

            _loopInterval = loopInterval >= 0 ? loopInterval
                : throw new Exception("等待周期必须>=0");

            _stopAfterContinuousIdleLoopCount = idleLoopCountBeforeStopping >= 0
                ? idleLoopCountBeforeStopping
                : throw new Exception("空转周期必须>=0");

            _workersCountForEachQueue = workersCountForEachQueue >= 0
                ? workersCountForEachQueue
                : throw new Exception("Worker数必须>=0");

            //启用监控Worker,当队列不为空时启用/唤醒Worker
            _watchWorker = new QueueWatchWorker(_queueRootFolder);
            _watchWorker.Start();
        }

        /// <summary>
        /// 停止所有队列的所有Workers
        /// 由于清理顺序要要求,暂时不能通过WorkerFactory.StopAll的方式清理
        /// </summary>
        public static void StopAllDequeuers()
        {
            _watchWorker?.Stop();//首先停止watcher,确保其他worker不会再次被唤醒
            Parallel.ForEach(_workers.Values.SelectMany(_ => _), w => w.Stop());
        }

        /// <summary>
        /// 将一个对象放入指定的队列
        /// 有可能在不同进程中入列和出列
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="data">(序列化的)对象</param>
        public static void Enqueue(string queueRootFolder, string queueName, string data)
        {
            if (_queueRootFolder != null && _queueRootFolder != queueRootFolder)
            {
                throw new Exception("队列根目录必须与CreateDequeuers指定的一致");
            }

            using (var queue = PersistentQueue.WaitFor(Path.Combine(queueRootFolder, queueName), TimeSpan.FromSeconds(60)))
            using (var session = queue.OpenSession())
            {
                session.Enqueue(Encoding.UTF8.GetBytes(data));
                session.Flush();
            }
        }

        /// <summary>
        /// 为指定队列开启Workers
        /// 如果没有则创建
        /// </summary>
        /// <param name="queueName"></param>
        internal static void StartWorkersForQueue(string queueName)
        {
            lock (_workers)//确保并发时不会创建额外Workers
            {
                _workers.GetOrAdd(queueName, _ =>
                {
                    var workers = new DequeueWorker[_workersCountForEachQueue];
                    for (int i = 0; i < _workersCountForEachQueue; i++)
                    {
                        workers[i] = new DequeueWorker(_queueRootFolder, queueName, _dequeueAction, _loopInterval, _stopAfterContinuousIdleLoopCount);
                    }
                    return workers;
                })
                .AsParallel().ForAll(w => w.Start());
            }
        }
    }
}