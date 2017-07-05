using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Infrastructure.Helpers;
using System.Collections.Generic;

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
                Assert.AreEqual((object)"456", _testObj1);
                Assert.AreEqual(null, _testString3Local);

                Environment.Exit(0);
            });



        }
    }
}
