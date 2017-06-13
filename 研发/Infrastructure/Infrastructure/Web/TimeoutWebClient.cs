using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Web
{
    public class TimeoutWebClient : WebClient
    {
        private int _timeout;

        protected override WebRequest GetWebRequest(Uri address)
        {
            var req = base.GetWebRequest(address);
            req.Timeout = _timeout;
            //有些server对于没有UA的请求会500(多数是他们验证UA直接抛异常了)
            (( HttpWebRequest )req).UserAgent =
            "Mozilla/5.0 (Windows NT 6.3; Win64; x64) " +
                "AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/56.0.2924.87 Safari/537.36";

            return req;
        }

        public TimeoutWebClient(int timeout = 0)
        {
            _timeout = timeout;
        }
    }
}
