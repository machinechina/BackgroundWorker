using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Threading;

namespace Infrastructure.Workers
{
    public class ShortRunningWorker : Worker
    {
        Action _action;
        public ShortRunningWorker(Action action) : base(-1)
        {
            _action = action;
        }
        protected override IntervalTask.WorkingState DoWork()
        {
            _action?.Invoke();
            return IntervalTask.WorkingState.IDLE;
        }
    }
}
