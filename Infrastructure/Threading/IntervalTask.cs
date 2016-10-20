using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Helper;

namespace Infrastructure.Threading
{
    public class IntervalTask
    {
        public enum IdleOrWorking { Idle, Working }

        public static Task Start(
             TimeSpan pollInterval,
             Func<IdleOrWorking> func,
             CancellationToken token,
             Int32 stopAfterContinuousIdleLoopCount)
        {
            // We don't use Observable.Interval:
            // If we block, the values start bunching up behind each other.
            return Task.Factory.StartNew(
                () =>
                {
                    var continuousIdleLoopCount = 0;
                    for (;;)
                    {
                        if (token.WaitCancellationRequested(pollInterval))
                            break;
                        try
                        {
                            var workResult = func();
                            if (stopAfterContinuousIdleLoopCount<=0)
                            {
                                //不需要终止
                                continue;
                            }
                            if (workResult==IdleOrWorking.Idle)
                            {
                                //空转计数器累加
                                continuousIdleLoopCount++;
                                if (continuousIdleLoopCount>=stopAfterContinuousIdleLoopCount)
                                {
                                    //空转一定周期后,结束任务
                                    break;
                                }
                            }
                            else if(workResult==IdleOrWorking.Working)
                            {
                                //如果有工作了,空转计数器清零
                                continuousIdleLoopCount = 0;
                            }

                        }
                        catch (Exception ex)
                        {
                            ApplicationHelper.Log(ex);
                        }
                    }
                }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }

    internal static class CancellationTokenExtensions
    {
        public static bool WaitCancellationRequested(
            this CancellationToken token,
            TimeSpan timeout)
        {
            return token.WaitHandle.WaitOne(timeout);
        }
    }
}
