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
        private Timer _timer;

        public ScheduleWorker(Action action, DateTime runTime, TimeSpan period, bool startImmediately = false) : base(action)
        {
            _startImmediately = startImmediately;

            //避免Threading.Timer被回收的方法
            //1 实例放在函数作用域之外(这里的_timer)
            //2 保持state的引用不被回收(第二个参数)
            _timer = new Timer(_ =>
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