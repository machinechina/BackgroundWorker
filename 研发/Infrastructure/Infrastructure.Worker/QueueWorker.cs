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
    public class QueueWorker : Worker
    {
        private IPersistentQueue _queue;
        private Action<string> _action;
        public string QueueName { get; }

        public QueueWorker(string queueName, Action<string> action, int waitInterval, int stopAfterContinuousIdleLoopCount = 0) : base(waitInterval, stopAfterContinuousIdleLoopCount)
        {
            _queue = QueueWorkerBus.GetOrCreateQueue(queueName);
            _action = action;
            QueueName = queueName;
        }

        public QueueWorker(IPersistentQueue queue, string queueName, Action<string> action, int waitInterval, int stopAfterContinuousIdleLoopCount = 0) : base(waitInterval, stopAfterContinuousIdleLoopCount)
        {
            _queue = queue;
            _action = action;
            QueueName = queueName;
        }

        public void Enqueue(string data)
        {
            using (var session = _queue.OpenSession())
            {
                session.Enqueue(Encoding.UTF8.GetBytes(data));
                session.Flush();
            }
        }

        protected override WorkingState DoWork()
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

            return isWorking ? WorkingState.Busy : WorkingState.Idle;
        }
    }
}