using System;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Helpers;

namespace Infrastructure.Threading
{
    public class IntervalTask
    {
        public enum WorkingState { IDLE, BUSY }

        public static Task Start(
             TimeSpan pollInterval,
             Func<WorkingState> func,
             CancellationToken token,
             int stopAfterContinuousIdleLoopCount)
        {
            // We don't use Observable.Interval:
            // If we block, the values start bunching up behind each other.
            return Task.Factory.StartNew(
                () =>
                {
                    var continuousIdleLoopCount = 0;

                    for (;;)
                    {
                        var workingState = WorkingState.IDLE;
                        try
                        {
                            workingState = func();
                        }
                        catch (Exception ex)
                        {
                            Helper.Log(ex);
                        }

                        //等待一个周期,接收终止命令
                        if (token.WaitCancellationRequested(pollInterval))
                            break;

                        #region 结算空转计数

                        if (stopAfterContinuousIdleLoopCount <= 0)
                        {
                            //不需要终止
                            continue;
                        }
                        if (workingState == WorkingState.IDLE)
                        {
                            //空转计数器累加
                            continuousIdleLoopCount++;
                            if (continuousIdleLoopCount >= stopAfterContinuousIdleLoopCount)
                            {
                                //空转一定周期后,结束任务
                                break;
                            }
                        }
                        else if (workingState == WorkingState.BUSY)
                        {
                            //如果有工作了,空转计数器清零
                            continuousIdleLoopCount = 0;
                        }

                        #endregion 结算空转计数
                    }
                }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }

    internal static class CancellationTokenExtensions
    {
        public static bool WaitCancellationRequested(
            this CancellationToken token,
            TimeSpan timeout) =>
            token.WaitHandle.WaitOne(timeout);
    }
}