using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Helpers;
using static Infrastructure.Threading.IntervalTask;

namespace Infrastructure.Workers
{
    public class UpdateCheckingWorker : Worker
    {
        public UpdateCheckingWorker(int loopInterval) : base(loopInterval)
        {
        }

        protected override WorkingState DoWork()
        {
            Helper.CheckUpdate(() =>
             {
                 Stop();
             });
            return WorkingState.BUSY;
        }
    }
}
