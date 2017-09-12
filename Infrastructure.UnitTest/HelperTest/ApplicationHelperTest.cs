using System;
using System.IO;
using Infrastructure.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Infrastructure.UnitTest
{
    [TestClass]
    public class ApplicationHelperTest
    {
        [TestMethod]
        public void TestInitializeRefFiles()
        {

            var md5 = "e3‌​c3‌​27‌​a1‌​58‌​db‌​21‌​85‌​9e‌​da‌​9c‌​8f‌​1a‌​3d‌​e5‌​51";
            var targetFileName = "test.file.png";
            var sourceFilePath = $" {Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "_refFiles_" + ProductDescription.AppName)}\\{md5}\\{targetFileName}";

            DeleteFiles();

            //传入不兼容的系统(32位)
            Helper.InitializeRefFile(targetFileName, "http://taixue.istudy.sh.cn/client/test.png", md5, false);
            Assert.IsFalse(File.Exists(sourceFilePath));
            Assert.IsFalse(File.Exists(targetFileName));

            //第一次调用
            Helper.InitializeRefFile(targetFileName, "http://taixue.istudy.sh.cn/client/test.png", md5);
            Assert.IsTrue(File.Exists(sourceFilePath));
            Assert.IsTrue(File.Exists(targetFileName));

            //重复调用
            Helper.InitializeRefFile(targetFileName, "http://taixue.istudy.sh.cn/client/test.png", md5);
            Assert.IsTrue(File.Exists(sourceFilePath));
            Assert.IsTrue(File.Exists(targetFileName));

            DeleteFiles();

            void DeleteFiles()
            {
                try
                {
                    File.Delete(sourceFilePath);
                }
                catch { }
                try
                {
                    File.Delete(targetFileName);
                }
                catch { }

            }
        }
    }
}
