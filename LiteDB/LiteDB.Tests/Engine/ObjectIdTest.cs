﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LiteDB.Tests
{
    [TestClass]
    public class ObjectIdTest
    {
        [TestMethod]
        public void ObjectId_Test()
        {
            var oid0 = ObjectId.Empty;
            var oid1 = ObjectId.NewObjectId();
            var oid2 = ObjectId.NewObjectId();
            var oid3 = ObjectId.NewObjectId();

            var c1 = new ObjectId(oid1);
            var c2 = new ObjectId(oid2.ToString());
            var c3 = new ObjectId(oid3.ToByteArray());

            Assert.AreEqual(oid0, ObjectId.Empty);
            Assert.AreEqual(oid1, c1);
            Assert.AreEqual(oid2, c2);
            Assert.AreEqual(oid3, c3);

            Assert.AreEqual(c1.CompareTo(c2), -1); // 1 < 2
            Assert.AreEqual(c2.CompareTo(c3), -1); // 2 < 3

            // serializations
            var joid = JsonSerializer.Serialize(c1, true);
            var jc1 = JsonSerializer.Deserialize(joid).AsObjectId;

            Assert.AreEqual(c1, jc1);
        }
    }
}