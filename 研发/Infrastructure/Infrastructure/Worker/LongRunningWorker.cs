using System;
using static Infrastructure.Threading.IntervalTask;

namespace Infrastructure.Workers
{
    /// <summary>
    ///
    /// </summary>
    public class LongRunningWorker : Worker
    {
        private Func<bool> _mainFunc;

        /// <summary>
        ///
        /// </summary>
        /// <param name="mainFunc"></param>
        /// <param name="loopInterval"></param>
        /// <param name="stopAfterContinuousIdleLoopCount"></param>
        public LongRunningWorker(
            Func<bool> mainFunc,
            int loopInterval = 1000,
            int stopAfterContinuousIdleLoopCount = 0)
            : base(loopInterval, stopAfterContinuousIdleLoopCount) =>
                _mainFunc = mainFunc;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainAction"></param>
        /// <param name="loopInterval"></param>
        /// <param name="stopAfterContinuousIdleLoopCount"></param>
        public LongRunningWorker(
            Action mainAction,
            int loopInterval = 1000,
            int stopAfterContinuousIdleLoopCount = 0)
            : base(loopInterval, stopAfterContinuousIdleLoopCount) =>
                _mainFunc = () =>
                  {
                      mainAction();
                      return true;
                  };

        protected override WorkingState DoWork() =>
            _mainFunc() ? WorkingState.BUSY : WorkingState.IDLE;
    }
}