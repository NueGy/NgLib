using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.DATA.COLLECTIONS;

namespace Nglib.MANIPULATE
{
    [TestClass]
    public class CollectionsTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            Dictionary<string, object> ins = new Dictionary<string, object>();
            ins.Add("tEsT", "testsetset");
            bool re = ins.ContainsKey("test", true);
            Assert.IsTrue(re);

            ins["re"] = "toto";
            Assert.IsNotNull(ins["re"]);

        }
    }
}
