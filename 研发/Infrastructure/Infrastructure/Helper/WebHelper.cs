//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;

//namespace Infrastructure.Helper
//{
//    public class WebHelper
//    {
//        HttpClient http { get; set; }

//        public HttpUtility(String host, Dictionary<String, String> headers = null, int timeout = 3600)
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

//        public T Get<T>(String url)
//        {
//            Console.WriteLine(url);

//            var response = http.GetAsync(url).Result;
//            response.EnsureSuccessStatusCode();

//            var resultTask = response.Content.ReadAsStringAsync().Result;
//            var result = JsonConvert.DeserializeObject<ApiResult<T>>(resultTask);
//            if (result.ErrorCode != ErrorCode.NoError)
//                throw new Exception(result.Message);
//            return result.Result;
//        }

//        public Task<T> GetAsync<T>(String url)
//        {
//            return http.GetAsync(url).ContinueWith(task =>
//            {
//                var response = task.Result;
//                response.EnsureSuccessStatusCode();
//                return response.Content.ReadAsStringAsync().ContinueWith(task2 =>
//                {
//                    var resultTask = task2.Result;
//                    var result = JsonConvert.DeserializeObject<ApiResult<T>>(resultTask);
//                    if (result.ErrorCode != ErrorCode.NoError)
//                        throw new Exception(result.Message);
//                    return result.Result;
//                });
//            }).Unwrap();
//        }

//        public T Post<T>(String url, Dictionary<String, String> args = null, HttpContent content = null)
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
//            var result = JsonConvert.DeserializeObject<ApiResult<T>>(resultTask);
//            if (result.ErrorCode != ErrorCode.NoError)
//                throw new SysException(result.ErrorCode, result.Message);
//            return result.Result;
//        }

//        public void Post(String url, Dictionary<String, String> args, HttpContent content = null)
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
//            var result = JsonConvert.DeserializeObject<ApiResult>(resultTask);
//            if (result.ErrorCode != ErrorCode.NoError)
//                throw new SysException(result.ErrorCode, result.Message);
//        }

//        //public void PostModel<T>(String url, T data, HttpContent content = null)
//        //{
//        //    var response = http.PostAsJsonAsync<T>(url, data).Result;
//        //    response.EnsureSuccessStatusCode();
//        //    var resultTask = response.Content.ReadAsStringAsync().Result;
//        //    var result = JsonConvert.DeserializeObject<ApiResult>(resultTask);
//        //    if (result.ErrorCode != ErrorCode.NoError)
//        //        throw new SysException(result.ErrorCode, result.Message);
//        //}
//        public Task<T> PostAsync<T>(String url, Dictionary<String, String> args, HttpContent content = null)
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
//                    var result = JsonConvert.DeserializeObject<ApiResult<T>>(resultTask);
//                    if (result.ErrorCode != ErrorCode.NoError)
//                        throw new SysException(result.ErrorCode, result.Message);
//                    return result.Result;
//                });
//            }).Unwrap();
//        }

//    }
//}
