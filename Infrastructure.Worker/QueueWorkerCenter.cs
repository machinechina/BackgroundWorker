using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiskQueue;

namespace Infrastructure.QueueWorker
{
    /// <summary>
    /// 维护Worker实例,保证启动或队列创建时,有一个Worker负责Dequeue
    /// 同时创建和维护队列实例,同一名称的队列应该只被创建一次,并且始终存活
    /// </summary>
    public static class QueueWorkerCenter
    {
        private static Action<String> _action;
        private static ConcurrentDictionary<String, IPersistentQueue> _queues = new ConcurrentDictionary<string, IPersistentQueue>();
        private static ConcurrentBag<DeQueueWorker> _workers = new ConcurrentBag<DeQueueWorker>();
        private static String _queueRootFolder = "";
        private static Int32 _waitInterval;
        private static Int16 _workersCountForEachQueue;
        private static Int32 _stopAfterContinuousIdleLoopCount;
        private static Boolean _initialized = false;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="queueRootFolder">队列物理存储目录(必须是空目录)</param>
        /// <param name="dequeueAction">出列后,需要执行的工作</param>
        /// <param name="waitInterval">空闲时的自旋等待周期</param>
        /// <param name="stopAfterContinuousIdleLoopCount">空闲时关闭Worker所需的连续自旋次数,0为不关闭,建议设置以减少开销</param>
        /// <param name="workersCountForEachQueue">每个队列的Worker数</param>
        /// <param name="startWorkImmediately">是否寻找现有队列,并启动相关的所有Workers</param>
        public static void Initialize(String queueRootFolder, Action<String> dequeueAction, Int32 waitInterval, Int32 stopAfterContinuousIdleLoopCount = 0, Int16 workersCountForEachQueue = 1, Boolean startWorkImmediately = false)
        {
            Directory.CreateDirectory(queueRootFolder);
            _queueRootFolder = queueRootFolder;

            _action = dequeueAction
                ?? throw new Exception("必须指定操作");

            _waitInterval = waitInterval >= 0 ? waitInterval
                : throw new Exception("等待周期必须>=0");

            _stopAfterContinuousIdleLoopCount = stopAfterContinuousIdleLoopCount >= 0 ? stopAfterContinuousIdleLoopCount
                : throw new Exception("空转周期必须>=0");

            _workersCountForEachQueue = workersCountForEachQueue >= 0 ? workersCountForEachQueue
                : throw new Exception("Worker数必须>=0");

            //找到所有的队列,每一个启动一个Worker处理
            //尽量不要使用,而是在入列的时候LazyLoad
            if (startWorkImmediately)
            {
                //加载队列
                Directory.GetDirectories(queueRootFolder, "*", SearchOption.TopDirectoryOnly)
                    .AsParallel().ForAll(queuePath =>
                    {
                        var queueName = Path.GetFileName(queuePath);
                        GetOrCreateQueueWithWorkers(queueName);
                        //激活队列的所有Workers(如果队列不为空)
                        StartWorkerForQueue(queueName);
                    });
            }

            _initialized = true;
        }

        /// <summary>
        /// 停止所有队列的所有Workers
        /// </summary>
        public static void StopAll()
        {
            Parallel.ForEach(_workers, w => w.Stop());
        }

        /// <summary>
        /// 将一个对象放入指定的队列
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="data">(序列化的)对象</param>
        public static void Enqueue(String queueName, String data)
        {
            if (!_initialized)
                throw new Exception("初始化尚未完成");

            //获取队列并入列,如果不存在则创建(同时分配队列的Workers)
            using (var session = GetOrCreateQueueWithWorkers(queueName).OpenSession())
            {
                session.Enqueue(Encoding.UTF8.GetBytes(data));
                session.Flush();
            }
            //入列时启动队列的workers
            StartWorkerForQueue(queueName);
        }

        internal static void StartWorkerForQueue(String queueName)
        {
            //仅在队列有任务的时候才会开启Workers
            var queue = _queues[queueName];
            if (_queues[queueName].EstimatedCountOfItemsInQueue > 0)
            {
                _workers.Where(w => w.QueueName == queueName).AsParallel().ForAll(w => w.Start());
            }
        }

        internal static IPersistentQueue GetOrCreateQueueWithWorkers(String name)
        {
            IPersistentQueue queue;
            if (_queues.TryGetValue(name, out queue))
                return queue;

            lock (_queues)
            {
                //创建或获取队列
                return _queues.GetOrAdd(name,
                     _ =>
                     {
                         //新建或加载队列,并且分配Workers来处理
                         var queuePath = Path.Combine(_queueRootFolder, name);
                         queue = new PersistentQueue(queuePath);
                         Parallel.For(0, _workersCountForEachQueue, i =>
                         {
                             var worker = new DeQueueWorker(queue, name, _action, _waitInterval, _stopAfterContinuousIdleLoopCount);
                             _workers.Add(worker);
                         });

                         return queue;
                     });
            }
        }

        internal static IPersistentQueue GetOrCreateQueue(String name)
        {
            return _queues.GetOrAdd(name, new PersistentQueue(Path.Combine(_queueRootFolder, name)));
        }
    }
}