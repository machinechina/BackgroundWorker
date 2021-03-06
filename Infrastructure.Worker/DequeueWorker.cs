﻿using System;
using System.IO;
using System.Text;
using DiskQueue;
using Infrastructure.Helpers;
using Infrastructure.Workers;
using static Infrastructure.Threading.IntervalTask;

namespace Infrastructure.QueueWorker
{
    /// <summary>
    /// 根据输入的队列名创建队列,指定出列操作
    /// Start后会循环取出队列中的数据,并执行指定的操作
    /// 对于多队列多任务等复杂场景建议从QueueWorkerBus使用,不建议直接使用
    /// </summary>
    public class DequeueWorker : Worker
    {
        private Action<string> _dequeueAction;
        private string _queueRootFolder { get; }
        private string _queueName { get; }

        /// <summary>
        /// 循环取出队列中的数据,并执行指定的操作
        /// </summary>
        /// <param name="queueRootFolder"></param>
        /// <param name="queueName"></param>
        /// <param name="dequeueAction"></param>
        /// <param name="loopInterval"></param>
        /// <param name="stopAfterContinuousIdleLoopCount">触发设置唤醒机制,否则不建议设置</param>
        public DequeueWorker(string queueRootFolder, string queueName, Action<string> dequeueAction, int loopInterval, int stopAfterContinuousIdleLoopCount = 0)
            : base(loopInterval, stopAfterContinuousIdleLoopCount)
        {
            _dequeueAction = dequeueAction;
            _queueRootFolder = queueRootFolder;
            _queueName = queueName;
        }

        /// <summary>
        /// 每次从队列取出一个并处理
        /// </summary>
        /// <returns></returns>
        protected override WorkingState DoWork()
        {
            var isWorking = false;
            string dataString = null;
            using (var queue = PersistentQueue.WaitFor(Path.Combine(_queueRootFolder, _queueName), TimeSpan.FromSeconds(60)))
            using (var session = queue.OpenSession())
            {
                var data = session.Dequeue();
                if (data != null)
                {
                    dataString = Encoding.UTF8.GetString(data);
                    Helper.Log($"Get Dequeue Data : {dataString}");
                }

                session.Flush();
            }
            if (dataString != null)
            {
                isWorking = true;
                try
                {
                    Helper.Log($"Start Processing Dequeue Data : {dataString}");
                    _dequeueAction(dataString);
                }
                catch (Exception ex)
                {
                    var msg = $"Error Processing Dequeue Data : {dataString}";
                    Helper.Error(ex, msg);
                }
                finally
                {
                    //防止在处理过程中断电
                    Helper.Log($"End Processing Dequeue Data : {dataString}");
                }
            }
            return isWorking ? WorkingState.BUSY : WorkingState.IDLE;
        }

    }
}