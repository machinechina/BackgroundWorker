using System;
using System.Threading.Tasks;
using Infrastructure.Helpers;
using Infrastructure.Threading;
using Infrastructure.Web;
using static Infrastructure.Threading.IntervalTask;

namespace Infrastructure.Workers
{
    /// <summary>
    /// 长轮询Worker
    /// </summary>
    public class LongPollingWorker<TApi, TResult> : Worker
            where TApi : IApiResult<TResult>
    {
        private string _url;
        private Func<bool> _breakCondition;
        private Action<TResult> _mainAction;
        private bool _runReturnActionAsync;

        /// <summary>
        /// 长轮询Worker
        /// </summary>
        /// <param name="url">请求地址(GET)</param>
        /// <param name="breakCondition"></param>
        /// <param name="mainAction"></param>
        /// <param name="loopInterval"></param>
        /// <param name="stopAfterContinuousIdleLoopCount"></param>
        public LongPollingWorker(string url,
            Func<bool> breakCondition = null,
            Action<TResult> mainAction = null,
            int loopInterval = 1000,
            int stopAfterContinuousIdleLoopCount = 0)
            : base(loopInterval, stopAfterContinuousIdleLoopCount)
        {
            _url = url;
            _breakCondition = breakCondition ?? (() => false);
            _mainAction = mainAction;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected override WorkingState DoWork()
        {
            TResult result = default(TResult);

            if (!_breakCondition())//校验轮询条件
                result = Helper.Get<TApi, TResult>(_url);
            else
                return WorkingState.IDLE;

            if (!_breakCondition())//长轮询耗时较长,轮询条件可能失效
                Task.Run(() => _mainAction(result));
            else
                return WorkingState.IDLE;

            return WorkingState.BUSY;
        }
    }
}