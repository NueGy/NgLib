using Nglib.APP.CODE;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.TESTS.MODELS;
using System;
using System.Linq;
using System.Reflection;

namespace Nglib.APP.CODE
{

    [TestClass]
    public class AttributesToolsTests
    {

        [TestMethod(), TestCategory("Unit")]
        public void AttributesTools_AllMethods_FullyTest()
        {
            // Test GetAttribute depuis type et objet
            var demoType = typeof(TESTS.MODELS.DemoClass);
            var demoObj = new TESTS.MODELS.DemoClass();
            
            // GetAttribute<T>(Type)
            var demoAttr = AttributesTools.GetAttribute<DemoAttribute>(demoType);
            Assert.IsNotNull(demoAttr, "GetAttribute<T>(Type) should return attribute");
            Assert.AreEqual("test", demoAttr.Name, "Attribute name should match");
            
            // GetAttribute<T>(object)
            demoAttr = AttributesTools.GetAttribute<DemoAttribute>(demoObj);
            Assert.IsNotNull(demoAttr, "GetAttribute<T>(object) should return attribute");
            Assert.AreEqual("test", demoAttr.Name, "Attribute name should match");
            
            // GetAttribute(Type, Type)
            var attr = AttributesTools.GetAttribute(demoType, typeof(DemoAttribute));
            Assert.IsNotNull(attr, "GetAttribute(Type, Type) should return attribute");
            
            // Test null handling
            Assert.IsNull(AttributesTools.GetAttribute<DemoAttribute>((Type)null), "Should handle null type");
            Assert.IsNull(AttributesTools.GetAttribute<DemoAttribute>((object)null), "Should handle null object");
            
            // Test GetMembersWithAttribute
            var membersWithAttr = AttributesTools.GetMembersWithAttribute<DemoAttribute>(demoType);
            Assert.IsNotNull(membersWithAttr, "GetMembersWithAttribute should not be null");
            Assert.AreEqual(3, membersWithAttr.Count, "Should find 3 members with attribute");
            
            // Test GetMembersWithAttribute with filter
            var methodsOnly = AttributesTools.GetMembersWithAttribute<DemoAttribute>(demoType, MemberTypes.Method);
            Assert.AreEqual(1, methodsOnly.Count, "Should find 1 method with attribute");
            
            // Test GetMethodsWithAttribute
            var methodsWithAttr = AttributesTools.GetMethodsWithAttribute<DemoAttribute>(demoType);
            Assert.IsNotNull(methodsWithAttr, "GetMethodsWithAttribute should not be null");
            Assert.AreEqual(1, methodsWithAttr.Count, "Should find 1 method with attribute");
            Assert.AreEqual("Calculate", methodsWithAttr.FirstOrDefault().Key.Name, "Method name should match");
            Assert.AreEqual("calculatemethod", methodsWithAttr.FirstOrDefault().Value.Name, "Attribute name should match");
            
            // Test GetPropertiesWithAttribute
            var propertiesWithAttr = AttributesTools.GetPropertiesWithAttribute<DemoAttribute>(demoType);
            Assert.IsNotNull(propertiesWithAttr, "GetPropertiesWithAttribute should not be null");
            Assert.IsTrue(propertiesWithAttr.Count >= 1, "Should find properties with attribute");
            
            // Test GetValuesWithAttribute
            demoObj.Name = "TestValue";
            var valuesWithAttr = AttributesTools.GetValuesWithAttribute<DemoAttribute>(demoObj);
            Assert.IsNotNull(valuesWithAttr, "GetValuesWithAttribute should not be null");
            Assert.IsTrue(valuesWithAttr.Count >= 1, "Should find property values with attribute");
            
            // Test GetTypesWithAttribute
            var typesWithAttr = AttributesTools.GetTypesWithAttribute<DemoAttribute>();
            Assert.IsNotNull(typesWithAttr, "GetTypesWithAttribute should not be null");
            Assert.IsTrue(typesWithAttr.Count > 0, "Should find types with attribute");
            
            var obsoleteTypes = AttributesTools.GetTypesWithAttribute<ObsoleteAttribute>();
            Assert.IsNotNull(obsoleteTypes, "Should find obsolete types");
            Assert.IsTrue(obsoleteTypes.Count > 0, "Should find obsolete types");
            
            // Test GetTypesWithAttribute with assembly
            var currentAssembly = Assembly.GetExecutingAssembly();
            var typesInAssembly = AttributesTools.GetTypesWithAttribute<DemoAttribute>(currentAssembly);
            Assert.IsNotNull(typesInAssembly, "GetTypesWithAttribute(Assembly) should not be null");
            
            // Test null handling for methods
            Assert.IsNull(AttributesTools.GetMembersWithAttribute<DemoAttribute>(null), "Should handle null type");
            Assert.IsNull(AttributesTools.GetMethodsWithAttribute<DemoAttribute>(null), "Should handle null type");
            Assert.IsNull(AttributesTools.GetPropertiesWithAttribute<DemoAttribute>(null), "Should handle null type");
            Assert.IsNull(AttributesTools.GetValuesWithAttribute<DemoAttribute>(null), "Should handle null object");
            
        }


    }
}
