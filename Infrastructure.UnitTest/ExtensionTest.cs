using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Extensions;
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

            dic.AddOrUpdate(1, v => v.Add("c"));

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

            try//并发调用,不安全
            {
                var dic3 = new Dictionary<int, string>();
                Parallel.For(0, 100, i => dic3.AddOrUpdate(1, i.ToString()));
                Assert.AreEqual(1, dic3.Count);
            }
            catch (AggregateException)
            {
            }

            //ConcurrentDictionary 覆盖了AddOrUpdate,所以实现时要判断类型
            var dic4 = new ConcurrentDictionary<int, string>();
            Parallel.For(0, 100, i => dic4.AddOrUpdate(1, i.ToString()));
            Assert.AreEqual(1, dic4.Count);

            try//显示调用Add,不安全
            {
                var dic5 = new ConcurrentDictionary<int, string>();
                Parallel.For(0, 100, i => (( IDictionary<int, string> )dic5).Add(1, i.ToString()));
                Assert.AreEqual(1, dic5.Count);
            }
            catch (AggregateException)
            {
            }

            var dic6 = new ConcurrentDictionary<int, List<string>>();
            Parallel.For(0, 100, i =>
            dic6.AddOrUpdate(1, v =>
            {
                lock (dic6)
                {
                    v.Add("a");
                }
            })
            );
            Assert.AreEqual(1, dic6.Count);
            Assert.AreEqual(100, dic6[1].Count);
            Assert.AreEqual("a", dic6[1][0]);

        }
    }
}