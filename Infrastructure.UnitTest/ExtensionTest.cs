using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiskQueue;
using Infrastructure.Extension;
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

        [TestMethod]
        public void AddOrUpdateDictionary()
        {
            var dic = new Dictionary<int, List<string>>();
            dic.AddOrUpdate(1, new List<string> { "a" });

            Assert.AreEqual(1, dic.Count);
            Assert.AreEqual(1, dic[1].Count);
            Assert.AreEqual("a", dic[1][0]);

            dic.AddOrUpdate(1, new List<string> { "b" });

            Assert.AreEqual(1, dic.Count);
            Assert.AreEqual(1, dic[1].Count);
            Assert.AreEqual("b", dic[1][0]);

            dic.AddOrUpdate(1, v=>v.Add("c"));

            Assert.AreEqual(1, dic.Count);
            Assert.AreEqual(2, dic[1].Count);
            Assert.AreEqual("b", dic[1][0]);
            Assert.AreEqual("c", dic[1][1]);

            var dic2 = new Dictionary<int, List<string>>();
            dic2.AddOrUpdate(1, v => v.Add("a"));

            Assert.AreEqual(1, dic2.Count);
            Assert.AreEqual(1, dic2[1].Count);
            Assert.AreEqual("a", dic2[1][0]);

            dic2.AddOrUpdate(1, v => v.Remove("a"));

            Assert.AreEqual(1, dic2.Count);
            Assert.AreEqual(0, dic2[1].Count);

        }
    }
}
