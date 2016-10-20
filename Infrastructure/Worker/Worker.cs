using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Threading;
using static Infrastructure.Threading.IntervalTask;

namespace Infrastructure.Workers
{
    public abstract class Worker : IWorker
    {
        private CancellationTokenSource _cancellationTokenSource;
        private Task _messageDispatcherWorker;
        private Int32 _loopInterval;
        private Int32 _stopAfterContinuousIdleLoopCount;

        public Worker(Int32 loopInterval, Int32 stopAfterContinuousIdleLoopCount = 0)
        {
            _loopInterval = loopInterval;
            _stopAfterContinuousIdleLoopCount = stopAfterContinuousIdleLoopCount;
        }

        public virtual void Start()
        {
            lock (this)
            {
                if (IsRunning) return;
                IsRunning = true;
            }

            _cancellationTokenSource = new CancellationTokenSource();

            if (_loopInterval >= 0)//loop
            {
                _messageDispatcherWorker = IntervalTask.Start(TimeSpan.FromMilliseconds(_loopInterval),
                         DoWork, _cancellationTokenSource.Token, _stopAfterContinuousIdleLoopCount);
                _messageDispatcherWorker.ContinueWith(t => IsRunning = false);
            }
            else//NO loop
            {
                _messageDispatcherWorker = new Task<IdleOrWorking>(DoWork);
                _messageDispatcherWorker.Start();
            }
        }

        public virtual void Stop()
        {
            lock (this)
            {
                if (!IsRunning) return;

                _cancellationTokenSource.Cancel();
                _messageDispatcherWorker.Wait();

                IsRunning = false;
            }
        }

        protected void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        protected abstract IdleOrWorking DoWork();
        public void Wait()
        {
            _messageDispatcherWorker.Wait();
            IsRunning = false;
        }

        protected Boolean IsCancellationRequested
        {
            get
            {
                return _cancellationTokenSource.IsCancellationRequested;
            }
        }

        public bool IsRunning
        {
            get; protected set;
        } = false;

    }

}
