using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Infrastructure.Extensions;
using Infrastructure.Helpers;
using Infrastructure.Web;
using Infrastructure.Workers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Infrastructure.UnitTest
{
    [TestClass]
    public class HelperTest
    {
        private static string _testString1;
        public static int _testInt1;
        private static string _testString2;
        public static object _testObj1;

        [TestMethod]
        public void RunAsBackgroundService_InitConfigs()
        {
            string _testString3Local = null;

            Helper.RunAsBackgroundService(new Dictionary<string, string> {
                {"TestString",nameof(_testString1)},
                {"TestInt",nameof(_testInt1)},
                {"TestString2_NotExist",nameof(_testString2)},
                {"TestObject",nameof(_testObj1)},
                {"TestString3",nameof(_testString3Local)},
            }, () =>
            {
                Assert.AreEqual("abc", _testString1);
                Assert.AreEqual(123, _testInt1);
                Assert.AreEqual(null, _testString2);
                Assert.AreEqual(( object )"456", _testObj1);
                Assert.AreEqual(null, _testString3Local);

                Environment.Exit(0);
            });
        }

        [TestMethod]
        public void RunAsBackgroundService_Update()
        {
            Helper.RunAsBackgroundService(null, tryBlock: () =>
            {
            }, updateInterval: 10000);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void RunProcessErrorTest()
        {
            Helper.RunProcess("ipconfig", "error");
        }

        [TestMethod]
        public void RunProcessTest()
        {
            //Sync
            var resultOfPing1 = Helper.RunProcess("ping", "127.0.0.1");
            Assert.AreEqual(13, resultOfPing1.Lines());

            //Async
            StringBuilder resultOfPing2 = new StringBuilder();
            var p = Helper.RunProcessAsync("ping", "127.0.0.1",
            outputEvent: (o, e) =>
                resultOfPing2.AppendLine(e.Data ?? "")
            );
            Thread.Sleep(5000);
            Assert.AreEqual(13, resultOfPing2.ToString().Lines());

            //Async and Shell
            var procId = Helper.RunProcessAsync("win.ini");
            Thread.Sleep(5000);
            var proc = Process.GetProcessById(procId);
            proc.Kill();
            proc.WaitForExit();
        }

        [TestMethod]
        public void ConfigurationTest()
        {
            {
                //Read
                Assert.AreEqual("abc",
                    Helper.GetAppConfig<string>("TestString"));
                Assert.AreEqual(null,
                    Helper.GetAppConfig<string>("NotExist"));

                //Write
                Helper.SetAppConfig("newKey", "newValue");
                Assert.AreEqual("newValue",
                    Helper.GetAppConfig<string>("newKey"));

                Helper.SetAppConfig("newKey", "newValue2");
                Assert.AreEqual("newValue2",
                    Helper.GetAppConfig<string>("newKey"));

                //Already existed before write
                Assert.AreEqual("abc",
                    Helper.GetAppConfig<string>("TestString"));
            }

            {
                //Read Remote
                Assert.AreEqual("abc",
                   Helper.GetAppConfig<string>("TestString", "app2.config"));
                Assert.AreEqual(null,
                    Helper.GetAppConfig<string>("NotExist", "app2.config"));

                //Write Remote
                Helper.SetAppConfig("newKey", "newValue", "app2.config");
                Assert.AreEqual("newValue",
                    Helper.GetAppConfig<string>("newKey", "app2.config"));

                Helper.SetAppConfig("newKey", "newValue2", "app2.config");
                Assert.AreEqual("newValue2",
                    Helper.GetAppConfig<string>("newKey", "app2.config"));

                //Already existed before write
                Assert.AreEqual("abc",
                   Helper.GetAppConfig<string>("TestString", "app2.config"));
            }
        }

        [TestMethod]
        public void RandomStringAlphanumeric()
        {
            var str1 = Helper.RandomString(10);
        }

        #region Web Test

        [TestMethod]
        public void HttpPostTest()
        {
            //Helper.Post("http://ctest.yilesi.cn/ppt/convert/callback", new
            //{
            //    inputFilePath ="c:\\a.ppt",
            //    outputFilePath ="c:\\a",
            //    success = 1,
            //    errorInfo = ""
            //}.ToJson());
        }

        [TestMethod]
        public void HttpGetTest()
        {

            Helper.SetBaseAddress("http://banpai.istudy.sh.cn/TaiXue.Api/api/");
            var result1 = Helper.Get<ApiResult1, DateTime>("basic/gettime");
            Assert.AreEqual(result1.Date, DateTime.Now.Date);

            //使用全路径,覆盖BaseAddress
            //{"code":-1,"msg":"\u9519\u8bef","datetime":"0.1650390625 ms"}
            try
            {
                var url2 = "https://m.campus.qq.com/api/open/getObjectInfo";
                var result2 = Helper.Get<ApiResult2, string>(url2);
            }
            catch (Exception ex)
            {
                Assert.AreEqual("错误", ex.Message);
            }

            //无返回Get,Code="0000"
            //请在10秒之内按两次卡
            try
            {
                Helper.Get<CardApiResult>("http://192.168.99.112:10240/StopDT?DelayTime=0");
            }
            catch { }
            Helper.Get<CardApiResult>("http://192.168.99.112:10240/StartDT?QUESTION_TYPE=2&optionNum=6&resultNum=2");
            Thread.Sleep(10000);
            var keys = Helper.Get<CardApiResult, CardKeyData[]>("http://192.168.99.112:10240/GetAnsWerKey?StartNo=0");
            Assert.AreEqual(2, keys.Length);

            for (int i = 0; i < keys.Length; i++)
            {
                Assert.AreEqual(i, keys[i].No);
            }

            Helper.Get<CardApiResult>("http://192.168.99.112:10240/StopDT?DelayTime=0");

        }

        private class ApiResult1 : IApiResult<DateTime>
        {
            public DateTime Result { get; set; }

            public string res_code { get; set; }

            public string res_msg { get; set; }

            public string ErrorCode => res_code;

            public string ErrorMessage => res_msg;

            public string Message => ErrorMessage;
        }

        private class ApiResult2 : IApiResult<string>
        {
            public string Result => "";
            public string code { get; set; }
            public string msg { get; set; }

            public string ErrorCode => code;
            public string Message => msg;
        }

        public class CardApiResult : IApiResult<CardKeyData[]>
        {
            public string ResultCode { get; set; }

            public string ResultMsg { get; set; }
            public CardKeyData[] KeyData { get; set; }

            public string ErrorCode => ResultCode;

            public string Message => ResultMsg;

            public CardKeyData[] Result => KeyData;
        }

        public class CardKeyData
        {
            public int No { get; set; }
            public string CardNo { get; set; }
            public string Keyinfo { get; set; }
            public DateTime KeyTime { get; set; }
        }

        #endregion Web Test


    }
}