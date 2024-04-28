using Nglib.APP.CODE;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nglib.APP.CODE
{

    [TestClass]
    public class PropertiesToolsTests
    {
        [TestMethod]
        public void TestMethod1()
        {
        }


        [TestMethod(), TestCategory("Unit")]
        public void GetValueTest()
        {
            TESTS.MODELS.DemoClass demoobj = new TESTS.MODELS.DemoClass();
            demoobj.Name = "test";
            var value = Nglib.APP.CODE.PropertiesTools.GetValue(demoobj, "Name");
            Assert.AreEqual(value, "test");
        }


        // methode de benchmark pour tester les performances de GetValue
        [TestMethod(), TestCategory("Benchmark")]
        public void GetValueBenchmark()
        {
            TESTS.MODELS.DemoClass demoobj = new TESTS.MODELS.DemoClass() { Name = "testb" };
            object value = null;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < 1000000; i++)
            {
                value = Nglib.APP.CODE.PropertiesTools.GetValue(demoobj, "Name");
            }
            sw.Stop();
            Assert.IsTrue(sw.ElapsedMilliseconds < 1000);
            System.Console.WriteLine("GetValueBenchmark: " + sw.ElapsedMilliseconds + "ms");
        }

        [TestMethod(), TestCategory("Unit")]
        public void GetSetValuesTest()
        {
            // Obtenir les valeurs
            TESTS.MODELS.DemoClass demoobj = new TESTS.MODELS.DemoClass() { Name = "testb" };
            var values = Nglib.APP.CODE.PropertiesTools.GetValues(demoobj);
            Assert.IsTrue(values.Count > 0);
            Assert.AreEqual(values["Name"], "testb");

            // Modifier les valeurs
            values["Name"] = "testc";
            Nglib.APP.CODE.PropertiesTools.SetValues(demoobj, values);
            Assert.AreEqual(demoobj.Name, "testc");
        }


        [TestMethod(), TestCategory("Unit")]
        public void SetValuesWithConvertTest()
        {
            // tester la conversion de valeurs du SetValue
            TESTS.MODELS.DemoClass demoobj = new TESTS.MODELS.DemoClass();
            var values = Nglib.APP.CODE.PropertiesTools.GetValues(demoobj);
            values["LongValue"] = "123";
            values["Name"] = 123;
            values["Date"] = "23/01/2024";
            values["Enabled"] = "1";

            Nglib.APP.CODE.PropertiesTools.SetValues(demoobj, values);

            Assert.AreEqual(demoobj.LongValue, 123);
            Assert.AreEqual(demoobj.Name, "123");
            Assert.AreEqual(demoobj.Date, new System.DateTime(2024, 1, 23));
            Assert.AreEqual(demoobj.Enabled, true);

        }

        [TestMethod()]
        public void GetPropertiesTest()
        {
            TESTS.MODELS.DemoClass demoobj = new TESTS.MODELS.DemoClass();
            var props = Nglib.APP.CODE.PropertiesTools.GetProperties(demoobj?.GetType());
            Assert.IsNotNull(props);

        }
    }
}
