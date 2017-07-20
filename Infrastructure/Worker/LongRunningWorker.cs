using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Threading;
using static Infrastructure.Threading.IntervalTask;

namespace Infrastructure.Workers
{
    public class LongRunningWorker : Worker
    {
        private Action _action;

        public LongRunningWorker(Action action, int loopInterval) : base(loopInterval, stopAfterContinuousIdleLoopCount: -1)
        {
            _action = action;
        }

        protected override IntervalTask.WorkingState DoWork()
        {
            _action();
            return WorkingState.BUSY;
        }
    }
}



