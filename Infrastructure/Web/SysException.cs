using System;
using System.Runtime.Serialization;

namespace Infrastructure.Web
{
    public class SysException : Exception
    {
        public ErrorCode ErrorCode { get; set; }
        public SysException() { }
        protected SysException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ErrorCode = (ErrorCode)info.GetValue("ErrorCode", typeof(ErrorCode));
        }
        public SysException(ErrorCode errorCode, string message = null)
            : base(message ?? errorCode.ToString())
        {
            ErrorCode = errorCode;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ErrorCode", this.ErrorCode);
            base.GetObjectData(info, context);
        }
    }
}