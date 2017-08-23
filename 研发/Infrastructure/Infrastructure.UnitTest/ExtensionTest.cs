using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
            Assert.AreEqual("a".ToJson(), "a".JsonToObject<string>());
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

        [TestMethod]
        public void Contains()
        {
            Assert.IsTrue("AA".Contains("A", false));
            Assert.IsFalse("AA".Contains("a", false));
            Assert.IsTrue("AA".Contains("a", true));
        }

        [TestMethod]
        public void Lines()
        {
            string nullString = null;
            Assert.AreEqual(nullString.Lines(), 0);
            Assert.AreEqual(string.Empty.Lines(), 1);
            Assert.AreEqual("".Lines(), 1);
            Assert.AreEqual("AA".Lines(), 1);
            Assert.AreEqual("AA\n".Lines(), 2);
            Assert.AreEqual("AA\nBB".Lines(), 2);
            Assert.AreEqual("AA\nBB\nCC".Lines(), 3);
        }

        [TestMethod]
        public void Join()
        {
            var a = new string[] { "aaa", "bbb" };
            Assert.AreEqual("aaa,bbb", a.ToJointString());
            var b = new char[] { 'a', 'b' };
            Assert.AreEqual("a,b", b.ToJointString());
            var c = "abc";
            Assert.AreEqual("a,b,c", c.ToArray().ToJointString());
        }

        [TestMethod]
        public void Sort()
        {
            var a = "bcdaea";
            Assert.AreEqual("aabcde", a.Sort());
        }

        #region Relection Test

        [TestMethod]
        public void FindMembers()
        {
            var allMembers = typeof(TestMembersA).FindMembers("test", true, true);
            Assert.AreEqual(11, allMembers.Count());

            var membersA = typeof(TestMembersA).FindMembers("A", true, true);
            Assert.AreEqual(7, allMembers.Count());
        }

        private class TestMembersA
        {
            private string testFieldOfA;
            private string TestPropertyOfA { get; set; }

            private void TestFunctionOfA()
            { }

            private TestMembersB testMembersA;
            private TestMembersB testMembersB;
        }

        private class TestMembersB
        {
            private string testFieldOfB;
            private string TestPropertyOfB { get; set; }

            private void TestFunctionOfB()
            { }
        }

        #endregion Relection Test

        #region Expression Test

        [TestMethod]
        public void Then()
        {
            var value = 1;
            var result2 = F(out value).Then(result =>
              {
                  result = false;
                  value++;
              });

            Assert.IsTrue(result2);
            Assert.AreEqual(3, value);
        }

        [TestMethod]
        public void WhenTrue()
        {
            var value = 1;
            var result2 = F(out value).WhenTrue(() =>
            {
                value++;
            });

            Assert.IsTrue(result2);
            Assert.AreEqual(3, value);
        }

        private bool F(out int value)
        {
            value = 2;
            return true;
        }

        #endregion Expression Test

        #region Convert

        [TestMethod]
        public void Convert()
        {
            string a = null;
            Assert.AreEqual(0, "0".ConvertTo<int>());
            Assert.AreEqual(null, a.ConvertTo<string>());
        }

        #endregion Convert
    }
}