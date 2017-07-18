namespace Infrastructure.Web
{
    public class ApiResult : IApiResult
    {
        public ErrorCode ErrorCode { get; set; }
        public string Message { get; set; }

        string IApiResult.ErrorCode => ErrorCode.ToString();
    }

    public class ApiResult<TResult> : IApiResult<TResult>
    {
        public ErrorCode ErrorCode { get; set; }
        public string Message { get; set; }
        public TResult Result { get; set; }
        string IApiResult.ErrorCode => ErrorCode.ToString();

        public ApiResult()
        {
        }

        public ApiResult(TResult result)
        {
            Result = result;
        }
    }

    public interface IApiResult
    {
        string ErrorCode { get; }
        string Message { get; }
    }

    public interface IApiResult<TResult> : IApiResult
    {
        TResult Result { get; }
    }
}