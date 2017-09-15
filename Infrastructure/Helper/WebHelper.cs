using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using Infrastructure.Extensions;
using Infrastructure.Web;

namespace Infrastructure.Helpers
{
    /// <summary>
    /// Web帮助类
    /// 目前httpclient是静态对象实现,参考下面的文章
    /// https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
    /// 1 Make your HttpClient static.
    /// 2 Do not dispose of or wrap your HttpClient in a using unless you explicitly are looking for a particular behaviour
    /// (such as causing your services to fail).
    /// </summary>
    public partial class Helper
    {
        private static HttpClient _client = new HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite)
        };

        public static void SetBaseAddress(string address)
        {
            _client.BaseAddress = new Uri(address);
            _client.DefaultRequestHeaders
             .Accept
             .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TApi"></typeparam>
        /// <param name="url"></param>
        /// <param name="formData"></param>
        /// <param name="content"></param>
        public static void Post<TApi>(string url, Dictionary<string, string> formData = null)
            where TApi : IApiResult
        {
            formData = formData ?? new Dictionary<string, string>();
            InnerHttp<TApi>(url,
                () => _client.PostAsync(url,
                    new FormUrlEncodedContent(formData)).Result);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TApi"></typeparam>
        /// <param name="url"></param>
        /// <param name="jsonData"></param>
        public static void Post<TApi>(string url, string jsonData)
            where TApi : IApiResult
        {
            HttpContent httpContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            InnerHttp<TApi>(url,
                () => _client.PostAsync(url, httpContent).Result);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TApi"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public static TResult Get<TApi, TResult>(string url)
            where TApi : IApiResult<TResult>
        {
            return
                InnerHttp<TApi>(url,
                    () => _client.GetAsync(url).Result)
                .Result;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <typeparam name="TApi"></typeparam>
        /// <param name="url"></param>
        public static void Get<TApi>(string url)
            where TApi : IApiResult
        {
            InnerHttp<TApi>(url,
                () => _client.GetAsync(url).Result);
        }

        private static TApi InnerHttp<TApi>(string url, Func<HttpResponseMessage> httpFunc)
            where TApi : IApiResult
        {
            try
            {
                var result = httpFunc();
                result.EnsureSuccessStatusCode();
                var data = result.Content.ReadAsStringAsync().Result.JsonToObject<TApi>();

                if (data.ErrorCode != "0"
                    && int.Parse(data.ErrorCode.ToString()) != 0)
                    throw new SystemException(data.Message,
                        new SystemException(data.ErrorCode));
                return data;
            }
            catch (Exception ex) when (!(ex is SystemException))
            {
                throw new Exception($"HTTP POST 错误\n{url}", ex);
            }
        }

        public static void DownloadFile(string fileUrl, string filePath = null)
        {
            using (var client = new WebClient())
            {
               
                EnsureFilePathExists(filePath);

                client.DownloadFile(fileUrl, filePath ?? Path.GetFileName(fileUrl));
            }
        }
    }
}