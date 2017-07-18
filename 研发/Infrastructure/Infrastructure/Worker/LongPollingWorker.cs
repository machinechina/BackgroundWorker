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
        private Action<TResult> _returnAction;

        /// <summary>
        /// 长轮询Worker
        /// </summary>
        /// <param name="url">请求地址(GET)</param>
        /// <param name="breakCondition"></param>
        /// <param name="returnAction"></param>
        public LongPollingWorker(string url,
            Func<bool> breakCondition = null,
            Action<TResult> returnAction = null)
            : base(1000)
        {
            _url = url;
            _breakCondition = breakCondition;
            _returnAction = returnAction;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected override IntervalTask.WorkingState DoWork()
        {
            TResult result = default(TResult);

            if (!_breakCondition())//校验轮询条件
                result = Helper.Get<TApi, TResult>(_url);
            if (!_breakCondition())//长轮询耗时较长,轮询条件可能失效
                Task.Factory.StartNew(() => _returnAction(result));

            return WorkingState.BUSY;
        }
    }
}