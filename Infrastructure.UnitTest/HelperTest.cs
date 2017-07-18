using System;
using System.Collections.Generic;
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
            Assert.IsNotNull(Helper.RunProcess("ipconfig"));
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
    }
}