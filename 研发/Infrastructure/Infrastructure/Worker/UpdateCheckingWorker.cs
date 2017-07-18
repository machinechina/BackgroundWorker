using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Helpers;
using static Infrastructure.Threading.IntervalTask;

namespace Infrastructure.Workers
{
    /// <summary>
    /// 检查更新的Worker
    /// </summary>
    public class UpdateCheckingWorker : Worker
    {
        /// <summary>
        /// 检查更新的Worker
        /// </summary>
        /// <param name="loopInterval">检查时间周期(毫秒)</param>
        public UpdateCheckingWorker(int loopInterval) : base(loopInterval)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override WorkingState DoWork()
        {
            Helper.CheckUpdate(() => Task.Run(() => Stop()));
            return WorkingState.BUSY;
        }
    }
}
