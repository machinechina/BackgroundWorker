//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;
//using Infrastructure.Extensions;
//using Infrastructure.Web;

//namespace Infrastructure.Helper
//{
//    public class WebHelper
//    {
//        HttpClient http { get; set; }

//        public HttpUtility(string host, Dictionary<string, string> headers = null, int timeout = 3600)
//        {
//            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
//            http = new HttpClient(handler);
//            http.Timeout = new TimeSpan(0, 0, timeout);
//            http.BaseAddress = new Uri(host);
//            if (headers != null)
//            {
//                headers.ToList().ForEach(h => http.DefaultRequestHeaders.Add(h.Key, h.Value));
//            }
//        }

//        public T Get<T>(string url)
//        {
//            var response = http.GetAsync(url).Result;
//            response.EnsureSuccessStatusCode();

//            var resultTask = response.Content.ReadAsStringAsync().Result;
//            var result = resultTask.JsonToObject<ApiResult<T>>();
//            if (result.ErrorCode != ErrorCode.NoError)
//                throw new Exception(result.Message);
//            return result.Result;
//        }

//        public Task<T> GetAsync<T>(string url)
//        {
//            return http.GetAsync(url).ContinueWith(task =>
//            {
//                var response = task.Result;
//                response.EnsureSuccessStatusCode();
//                return response.Content.ReadAsStringAsync().ContinueWith(task2 =>
//                {
//                    var resultTask = task2.Result;
//                    var result = resultTask.JsonToObject<ApiResult<T>>();
//                    if (result.ErrorCode != ErrorCode.NoError)
//                        throw new Exception(result.Message);
//                    return result.Result;
//                });
//            }).Unwrap();
//        }

//        public T Post<T>(string url, Dictionary<string, string> args = null, HttpContent content = null)
//        {
//            if (args == null)
//            {
//                args = new Dictionary<string, string>();
//            }
//            if (content == null)
//                content = new FormUrlEncodedContent(args);
//            var response = http.PostAsync(url, content).Result;
//            response.EnsureSuccessStatusCode();

//            var resultTask = response.Content.ReadAsStringAsync().Result;
//            var result = resultTask.JsonToObject<ApiResult<T>>();
//            if (result.ErrorCode != ErrorCode.NoError)
//                throw new SysException(result.ErrorCode, result.Message);
//            return result.Result;
//        }

//        public void Post(string url, Dictionary<string, string> args, HttpContent content = null)
//        {
//            if (args == null)
//            {
//                args = new Dictionary<string, string>();
//            }
//            if (content == null)
//                content = new FormUrlEncodedContent(args);
//            var response = http.PostAsync(url, content).Result;
//            response.EnsureSuccessStatusCode();

//            var resultTask = response.Content.ReadAsStringAsync().Result;
//            var result = resultTask.JsonToObject<ApiResult>();
//            if (result.ErrorCode != ErrorCode.NoError)
//                throw new SysException(result.ErrorCode, result.Message);
//        }

 
//        public Task<T> PostAsync<T>(string url, Dictionary<string, string> args, HttpContent content = null)
//        {
//            if (content == null)
//                content = new FormUrlEncodedContent(args);

//            return http.PostAsync(url, content).ContinueWith(task =>
//            {
//                var response = task.Result;
//                response.EnsureSuccessStatusCode();
//                return response.Content.ReadAsStringAsync().ContinueWith(task2 =>
//                {
//                    var resultTask = task2.Result;
//                    var result = resultTask.JsonToObject<ApiResult<T>>();
//                    if (result.ErrorCode != ErrorCode.NoError)
//                        throw new SysException(result.ErrorCode, result.Message);
//                    return result.Result;
//                });
//            }).Unwrap();
//        }

//    }
//}
