using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Nglib.APP.CODE
{
 
    [TestClass]
    public class ReflectionToolsTests
    {
        [TestMethod(), TestCategory("Unit")]
        public void ReflectionTools_AllMethods_FullyTest()
        {
            // Test GetType
            var stringType = ReflectionTools.GetType("System.String");
            Assert.IsNotNull(stringType, "Should find System.String type");
            Assert.AreEqual(typeof(string), stringType, "Should return correct string type");
            
            var intType = ReflectionTools.GetType("System.Int32");
            Assert.IsNotNull(intType, "Should find System.Int32 type");
            Assert.AreEqual(typeof(int), intType, "Should return correct int type");
            
            // Test case insensitive
            var stringTypeLower = ReflectionTools.GetType("system.string");
            Assert.IsNotNull(stringTypeLower, "Should find type with case insensitive search");
            
            // Test non-existing type
            var nullType = ReflectionTools.GetType("NonExisting.Type");
            Assert.IsNull(nullType, "Should return null for non-existing type");
            
            // Test CreateInstance<T> without parameters - use type with default constructor
            var dateTimeInstance1 = ReflectionTools.CreateInstance<DateTime>();
            Assert.IsNotNull(dateTimeInstance1, "Should create DateTime instance");
            Assert.AreEqual(default(DateTime), dateTimeInstance1, "Should create default DateTime");
            
            var demoInstance = ReflectionTools.CreateInstance<TESTS.MODELS.DemoClass>();
            Assert.IsNotNull(demoInstance, "Should create DemoClass instance");
            Assert.IsInstanceOfType(demoInstance, typeof(TESTS.MODELS.DemoClass), "Should be correct type");
            
            // Test CreateInstance<T> with exactType parameter
            var demoFromType = ReflectionTools.CreateInstance<TESTS.MODELS.DemoClass>(typeof(TESTS.MODELS.DemoClass));
            Assert.IsNotNull(demoFromType, "Should create instance from exact type");
            Assert.IsInstanceOfType(demoFromType, typeof(TESTS.MODELS.DemoClass), "Should be correct type");
            
            // Test CreateInstance<T> with constructor args
            var dateTimeInstance = ReflectionTools.CreateInstance<DateTime>(typeof(DateTime), 2024, 10, 3);
            Assert.IsNotNull(dateTimeInstance, "Should create DateTime with constructor args");
            Assert.AreEqual(new DateTime(2024, 10, 3), dateTimeInstance, "Should have correct date value");
            
            // Test CreateInstance(Type) without parameters
            var objectFromType = ReflectionTools.CreateInstance(typeof(TESTS.MODELS.DemoClass));
            Assert.IsNotNull(objectFromType, "Should create instance from Type");
            Assert.IsInstanceOfType(objectFromType, typeof(TESTS.MODELS.DemoClass), "Should be correct type");
            
            // Test CreateInstance(Type) with constructor args
            var dateFromType = ReflectionTools.CreateInstance(typeof(DateTime), 2025, 1, 1);
            Assert.IsNotNull(dateFromType, "Should create instance with constructor args");
            Assert.IsInstanceOfType(dateFromType, typeof(DateTime), "Should be DateTime type");
            Assert.AreEqual(new DateTime(2025, 1, 1), dateFromType, "Should have correct date value");
            
            // Test error handling
            try
            {
                // Try to create instance of abstract type
                ReflectionTools.CreateInstance<System.IO.Stream>();
                Assert.Fail("Should throw exception for abstract type");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("CreateInstance"), "Exception should mention CreateInstance");
            }
            
            try
            {
                // Try to create instance with wrong constructor args
                ReflectionTools.CreateInstance(typeof(DateTime), "invalid");
                Assert.Fail("Should throw exception for invalid constructor args");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("CreateInstance"), "Exception should mention CreateInstance");
            }
            
            // Test with null type (should throw)
            try
            {
                ReflectionTools.CreateInstance(null);
                Assert.Fail("Should throw exception for null type");
            }
            catch (Exception)
            {
                // Expected
            }
            
            // Test edge cases
            var emptyArgsInstance = ReflectionTools.CreateInstance<TESTS.MODELS.DemoClass>(null, new object[0]);
            Assert.IsNotNull(emptyArgsInstance, "Should handle empty args array");
            
            var nullArgsInstance = ReflectionTools.CreateInstance(typeof(TESTS.MODELS.DemoClass), null);
            Assert.IsNotNull(nullArgsInstance, "Should handle null args");
        }
    }
}
