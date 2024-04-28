using Nglib.APP.CODE;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.TESTS.MODELS;
using System;
using System.Linq;

namespace Nglib.APP.CODE
{

    [TestClass]
    public class AttributesToolsTests
    {

        [TestMethod(), TestCategory("Unit")]
        public void GetAttributeTest()
        {
            //Obtenir un attribut depuis le type d'une classe
            DemoAttribute demoatr = Nglib.APP.CODE.AttributesTools.GetAttribute<DemoAttribute>(typeof(TESTS.MODELS.DemoClass));
            Assert.AreEqual(demoatr.Name, "test");

            //Obtenir un attribut depuis une instance d'une classe
            var demoobj = new TESTS.MODELS.DemoClass();
            demoatr = Nglib.APP.CODE.AttributesTools.GetAttribute<DemoAttribute>(demoobj);
            Assert.AreEqual(demoatr.Name, "test");
        }

        [TestMethod(), TestCategory("Unit")]
        public void GetMembersAttributesTest()
        {
            var demoatrs = Nglib.APP.CODE.AttributesTools.GetMembersWithAttribute<DemoAttribute>(typeof(TESTS.MODELS.DemoClass));
            Assert.AreEqual(demoatrs.Count, 3);

            var demoatrs2 = Nglib.APP.CODE.AttributesTools.GetMethodsWithAttribute<DemoAttribute>(typeof(TESTS.MODELS.DemoClass));
            Assert.AreEqual(demoatrs2.Count, 1);
            Assert.AreEqual(demoatrs2.FirstOrDefault().Key.Name, "Calculate");
            Assert.AreEqual(demoatrs2.FirstOrDefault().Value.Name, "calculatemethod");

        }


        [TestMethod(), TestCategory("Unit")]
        public void GetTypesWithAttributeTest()
        {
            var dtypes = Nglib.APP.CODE.AttributesTools.GetTypesWithAttribute<DemoAttribute>(null);
            Assert.IsTrue(dtypes.Count > 0);

            var dtypes2 = Nglib.APP.CODE.AttributesTools.GetTypesWithAttribute<ObsoleteAttribute>(null);
            Assert.IsTrue(dtypes2.Count > 0);
        }


 


    }
}
