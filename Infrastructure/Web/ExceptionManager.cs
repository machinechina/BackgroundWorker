using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Web
{
    public static class ExceptionManager
    {
        public static ApiResult<TResult> Process<TResult>(Func<TResult> func, string unErrorMessage = "")
        {
            ApiResult<TResult> apiResult = null;
            try
            {
                apiResult = new ApiResult<TResult>(func.Invoke());
            }
            catch (Exception e)
            {
                var baseException = e.GetBaseException();

                if (baseException is SysException)
                {
                    var sysException = baseException as SysException;
                    apiResult = new ApiResult<TResult> { ErrorCode = sysException.ErrorCode, Message = sysException.Message };
                }
                else
                {
                    apiResult = new ApiResult<TResult>
                    {
                        ErrorCode = ErrorCode.UnknownError,
                        Message = string.IsNullOrEmpty(unErrorMessage) ?
                                  baseException.Message : unErrorMessage
                    };
                }
            }
            return apiResult;
        }

        public static ApiResult Process(Action action, string unErrorMessage = "")
        {
            ApiResult apiResult = null;
            try
            {
                action.Invoke();
                apiResult = new ApiResult();
            }
            catch (Exception e)
            {
                var baseException = e.GetBaseException();
                if (baseException is SysException)
                {
                    var sysException = baseException as SysException;
                    apiResult = new ApiResult { ErrorCode = sysException.ErrorCode, Message = sysException.Message };
                }
                else
                {
                    apiResult = new ApiResult
                    {
                        ErrorCode = ErrorCode.UnknownError,
                        Message = string.IsNullOrEmpty(unErrorMessage) ?
                                  baseException.Message : unErrorMessage
                    };
                }
            }
            return apiResult;
        }
        public static Task<ApiResult<TResult>> Process<TResult>(Func<Task<TResult>> func, string unErrorMessage = "")
        {
            var apiResult =
            func.Invoke().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    ApiResult<TResult> resultIfError = null;

                    var baseException = task.Exception.GetBaseException();

                    if (baseException is SysException)
                    {
                        var sysException = baseException as SysException;
                        resultIfError = new ApiResult<TResult>
                        {
                            ErrorCode = sysException.ErrorCode,
                            Message = sysException.Message
                        };
                    }
                    else
                    {
                        resultIfError = new ApiResult<TResult>
                        {
                            ErrorCode = ErrorCode.UnknownError,
                            Message = string.IsNullOrEmpty(unErrorMessage) ?
                                      baseException.Message : unErrorMessage
                        };
                    }
                    return resultIfError;
                }
                else
                {
                    return new ApiResult<TResult>(task.Result);
                }
            });

            return apiResult;
            //another solution
            // For error handling.
            //         task.ContinueWith(t => { /* error handling */ }, context, 
            // TaskContinuationOptions.OnlyOnFaulted);

            // If it succeeded.
            // task.ContinueWith(t => { /* on success */ }, context,
            //    TaskContinuationOptions.OnlyOnRanToCompletion);

        }
    }

}
