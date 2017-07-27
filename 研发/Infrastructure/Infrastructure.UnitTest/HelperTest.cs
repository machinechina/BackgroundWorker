using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Infrastructure.Extensions;
using Infrastructure.Helpers;
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
    }
}