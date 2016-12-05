using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiskQueue;
using Infrastructure.Extensions;
using Infrastructure.QueueWorker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Infrastructure.UnitTest
{
    [TestClass]
   public class ExtensionTest
    {
        [TestMethod]
        public void JsonTest()
        {
            var a = "a".ToJson();
          //  Assert.AreEqual("a".ToJson(), "a".JsonToObject<String>());
        }
    }
}
