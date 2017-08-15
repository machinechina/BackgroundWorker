using System;
using System.Threading;
using Infrastructure.Threading;

namespace Infrastructure.Workers
{
    /// <summary>
    /// 按照指定的时间间隔调用Worker
    /// TODO:长间隔勿用,应使用Quartz来定时
    /// </summary>
    public class ScheduleWorker : ShortRunningWorker
    {
        private bool _startImmediately;
        private bool _timerElapsed;

        public ScheduleWorker(Action action, DateTime runTime, TimeSpan period, bool startImmediately = false) : base(action)
        {
            _startImmediately = startImmediately;
            
            new Timer(_ =>
            {
                _timerElapsed = true;
                DoWork();
            }, null, runTime - DateTime.Now, period);
            
            
        }

        protected override IntervalTask.WorkingState DoWork()
        {
            if (!_startImmediately && !_timerElapsed)
            {
                return IntervalTask.WorkingState.IDLE;
            }

            return base.DoWork();
        }
    }
}