using System;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Helpers;
using Infrastructure.Threading;
using static Infrastructure.Threading.IntervalTask;

namespace Infrastructure.Workers
{
    public abstract class Worker : IWorker
    {
        private CancellationTokenSource _mainTaskCancellationToken;
        private Task _mainTask;
        private Task _mainTaskContinueWithTask;

        private int _loopInterval;
        private int _stopAfterContinuousIdleLoopCount;

        public Worker(int loopInterval, int stopAfterContinuousIdleLoopCount = 0)
        {
            _loopInterval = loopInterval;
            _stopAfterContinuousIdleLoopCount = stopAfterContinuousIdleLoopCount;
        }

        /// <summary>
        /// 启动任务
        ///
        /// </summary>
        public virtual void Start()
        {
            lock (this)
            {
                if (IsRunning) return;
                IsRunning = true;
            }

            if (_loopInterval >= 0)//LongRunning(轮询任务)
            {
                _mainTaskCancellationToken = new CancellationTokenSource();
                _mainTask = IntervalTask.Start(TimeSpan.FromMilliseconds(_loopInterval),
                         DoWork, _mainTaskCancellationToken.Token, _stopAfterContinuousIdleLoopCount);
            }
            else//ShortRunning(单次任务)
            {
                _mainTask = new Task<WorkingState>(DoWork);
                _mainTask.Start();
            }

            //到达条件:轮询任务被Stop,轮询任务达到空闲阈值,单次任务执行完毕
            //无论哪种情况,task退出后都将状态置为 IsRunning=false 表示任务结束
            //结束后可以调用Start重新启动任务
            _mainTaskContinueWithTask = _mainTask
                .ContinueWith(t =>
                {
                    IsRunning = false;
                    Helper.Log($"Worker {_mainTask.Id} Stopped.");
                });

            Helper.Log($"Worker {_mainTask.Id} Started.");
        }

        /// <summary>
        /// 设置取消标识并等待任务结束
        /// 结束后可以调用Start重新启动任务(新的task)
        /// </summary>
        public virtual void Stop()
        {
            if (!IsRunning) return;

            _mainTaskCancellationToken?.Cancel();
            _mainTaskContinueWithTask?.Wait();
        }

        protected abstract WorkingState DoWork();

        /// <summary>
        /// 在运行的状态下等待,直到有特殊事件导致任务终止(如升级)
        /// </summary>
        public void WaitForExit()
        {
            if (!IsRunning) return;

            _mainTaskContinueWithTask?.Wait();
        }

        protected bool IsCancellationRequested
        {
            get
            {
                return _mainTaskCancellationToken.IsCancellationRequested;
            }
        }

        public bool IsRunning
        {
            get; protected set;
        } = false;
    }
}