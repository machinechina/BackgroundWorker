﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Infrastructure.Helpers;

namespace Infrastructure.UnitTest
{
    [TestClass]
    public class FileHelperTest
    {
        [TestMethod]
        public void TestMD5()
        {
            Assert.AreEqual("73‌​e1‌​9b‌​e0‌​e0‌​ec‌​d8‌​86‌​16‌​b5‌​76‌​2f‌​62‌​1b‌​02‌​26", 
                Helper.GetMD5(@"c:\windows\write.exe"));

            Assert.AreEqual("e3‌​c3‌​27‌​a1‌​58‌​db‌​21‌​85‌​9e‌​da‌​9c‌​8f‌​1a‌​3d‌​e5‌​51",
                Helper.GetMD5(@"..\..\TestFiles\test.png"));

        }
    }
}
