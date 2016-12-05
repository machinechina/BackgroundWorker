using System;
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
    /// </summary>
    public class DeQueueWorker : Worker
    {
        private IPersistentQueue _queue;
        private Action<String> _action;
        public String QueueName { get; }

        public DeQueueWorker(String queueName, Action<String> action, Int32 waitInterval, Int32 stopAfterContinuousIdleLoopCount = 0) : base(waitInterval, stopAfterContinuousIdleLoopCount)
        {
            _queue = QueueWorkerCenter.GetOrCreateQueue(queueName);
            _action = action;
            QueueName = queueName;
        }

        public DeQueueWorker(IPersistentQueue queue, String queueName, Action<String> action, Int32 waitInterval, Int32 stopAfterContinuousIdleLoopCount = 0) : base(waitInterval, stopAfterContinuousIdleLoopCount)
        {
            _queue = queue;
            _action = action;
            QueueName = queueName;
        }

        public void Enqueue(String data)
        {
            using (var session = _queue.OpenSession())
            {
                session.Enqueue(Encoding.UTF8.GetBytes(data));
                session.Flush();
            }
        }

        protected override IdleOrWorking DoWork()
        {
            var isWorking = false;
            using (var session = _queue.OpenSession())
            {
                byte[] data;
                while ((data = session.Dequeue()) != null)
                {
                    isWorking = true;

                    var dataString = Encoding.UTF8.GetString(data);
                    try
                    {
                        _action(dataString);
                    }
                    catch (Exception ex)
                    {
                        var msg = $"Error Dequeue: {dataString}\n";
                        Helper.Log(ex, msg);
                    }
                    finally
                    {
                        session.Flush();
                    }
                }
            }

            return isWorking ? IdleOrWorking.Working : IdleOrWorking.Idle;
        }
    }
}