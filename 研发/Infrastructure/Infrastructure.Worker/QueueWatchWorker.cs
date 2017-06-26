using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiskQueue;
using Infrastructure.Helpers;
using Infrastructure.Threading;
using Infrastructure.Workers;

namespace Infrastructure.QueueWorker
{
    /// <summary>
    /// 监控Bus中的队列有没有新加入元素,有则激活相关workers
    /// 此worker不会空转休眠
    /// </summary>
    internal class QueueWatchWorker : Worker
    {
        private List<string> _watchList = new List<string>();
        private string _queueRootFolder;
        public QueueWatchWorker(string queueRootFolder, int loopInterval = 10000) : base(loopInterval, 0)
        {
            _queueRootFolder = queueRootFolder;
        }

        /// <summary>
        /// 尝试获取Bus提供的文件夹下所以队列,并检查是否有数据
        /// </summary>
        /// <returns></returns>
        protected override IntervalTask.WorkingState DoWork()
        {
            Directory.GetDirectories(_queueRootFolder, "*", SearchOption.TopDirectoryOnly)
                   .AsParallel().ForAll(queuePath =>
                   {
                       if (File.Exists(Path.Combine(queuePath, "meta.state")))//检验是否是queue的文件夹
                       {
                           var queueName = Path.GetFileName(queuePath);
                           using (var queue = PersistentQueue.WaitFor(Path.Combine(_queueRootFolder, queueName), TimeSpan.FromSeconds(30)))
                               if (queue?.EstimatedCountOfItemsInQueue > 0)
                               {
                                   Helper.Log($"Queue {queueName} ready to awaken");
                                   QueueWorkerBus.StartWorkersForQueue(queueName);
                               }
                       }
                   });

            return IntervalTask.WorkingState.BUSY;
        }
    }
}
