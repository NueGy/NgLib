using Nglib.APP.CODE;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace Nglib.APP.CODE
{

    [TestClass]
    public class PropertiesToolsTests
    {
        [TestMethod(), TestCategory("Unit")]
        public void PropertiesTools_AllMethods_FullyTest()
        {
            var demoObj = new TESTS.MODELS.DemoClass();
            var demoType = typeof(TESTS.MODELS.DemoClass);
            
            // Test GetProperties
            var props = PropertiesTools.GetProperties(demoType);
            Assert.IsNotNull(props, "GetProperties should not be null");
            Assert.IsTrue(props.Length > 0, "Should find properties");
            
            var propsAll = PropertiesTools.GetProperties(demoType, false);
            Assert.IsNotNull(propsAll, "GetProperties(false) should not be null");
            Assert.IsTrue(propsAll.Length >= props.Length, "Should find more or equal properties when including non-public");
            
            // Test null handling for GetProperties
            Assert.IsNull(PropertiesTools.GetProperties(null), "Should handle null type");
            
            // Test GetProperty and GetField
            demoObj.Name = "testValue";
            var nameProp = PropertiesTools.GetProperty(demoObj, "Name");
            Assert.IsNotNull(nameProp, "Should find Name property");
            Assert.AreEqual("Name", nameProp.Name, "Property name should match");
            
            // Test case insensitive search
            var namePropIgnoreCase = PropertiesTools.GetProperty(demoObj, "name");
            Assert.IsNotNull(namePropIgnoreCase, "Should find property with case insensitive search");
            
            // Test GetField (if any fields exist)
            var field = PropertiesTools.GetField(demoObj, "_info");
            if (field != null)
            {
                Assert.AreEqual("_info", field.Name, "Field name should match");
            }
            
            // Test null handling
            Assert.IsNull(PropertiesTools.GetProperty(null, "Name"), "Should handle null object");
            Assert.IsNull(PropertiesTools.GetProperty(demoObj, null), "Should handle null property name");
            Assert.IsNull(PropertiesTools.GetProperty(demoObj, ""), "Should handle empty property name");
            Assert.IsNull(PropertiesTools.GetField(null, "field"), "Should handle null object for field");
            
            // Test GetValue safe and unsafe
            var value = PropertiesTools.GetValue(demoObj, "Name");
            Assert.AreEqual("testValue", value, "GetValue should return correct value");
            
            var safeValue = PropertiesTools.GetValue(demoObj, "Name", true);
            Assert.AreEqual("testValue", safeValue, "GetValue safe should return correct value");
            
            // Test safe mode with non-existing property
            var nullValue = PropertiesTools.GetValue(demoObj, "NonExistingProperty", true);
            Assert.IsNull(nullValue, "Safe mode should return null for non-existing property");
            
            // Test exception in unsafe mode
            try
            {
                PropertiesTools.GetValue(demoObj, "NonExistingProperty", false);
                Assert.Fail("Should throw exception for non-existing property in unsafe mode");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("not found"), "Exception should mention property not found");
            }
            
            // Test null handling for GetValue
            try
            {
                PropertiesTools.GetValue(null, "Name", false);
                Assert.Fail("Should throw ArgumentNullException for null object");
            }
            catch (ArgumentNullException) { /* Expected */ }
            
            Assert.IsNull(PropertiesTools.GetValue(null, "Name", true), "Safe mode should handle null object");
            
            // Test GetString
            var stringValue = PropertiesTools.GetString(demoObj, "Name");
            Assert.AreEqual("testValue", stringValue, "GetString should return string value");
            
            var nullStringValue = PropertiesTools.GetString(demoObj, "NonExistingProperty");
            Assert.IsNull(nullStringValue, "GetString should return null for non-existing property");
            
            // Test GetValues
            demoObj.LongValue = 123;
            demoObj.Enabled = true;
            demoObj.Date = new DateTime(2024, 1, 15);
            
            var values = PropertiesTools.GetValues(demoObj);
            Assert.IsNotNull(values, "GetValues should not be null");
            Assert.IsTrue(values.Count > 0, "Should have values");
            Assert.AreEqual("testValue", values["Name"], "Values should contain correct Name");
            Assert.AreEqual(123L, values["LongValue"], "Values should contain correct LongValue");
            
            // Test SetValue with PropertyInfo
            var newNameProp = PropertiesTools.GetProperty(demoObj, "Name");
            PropertiesTools.SetValue(demoObj, newNameProp, "newValue");
            Assert.AreEqual("newValue", demoObj.Name, "SetValue should update property");
            
            // Test SetValue with property name
            PropertiesTools.SetValue(demoObj, "Name", "anotherValue");
            Assert.AreEqual("anotherValue", demoObj.Name, "SetValue by name should update property");
            
            // Test type conversion in SetValue
            PropertiesTools.SetValue(demoObj, "LongValue", "456");
            Assert.AreEqual(456L, demoObj.LongValue, "SetValue should convert string to long");
            
            PropertiesTools.SetValue(demoObj, "Enabled", "0");
            Assert.AreEqual(false, demoObj.Enabled, "SetValue should convert string to bool");
            
            PropertiesTools.SetValue(demoObj, "Date", "01/02/2025");
            Assert.AreEqual(new DateTime(2025, 2, 1), demoObj.Date, "SetValue should convert string to DateTime");
            
            // Test SetValues from dictionary
            var newValues = new System.Collections.Generic.Dictionary<string, object>
            {
                ["Name"] = "dictValue",
                ["LongValue"] = "789",
                ["Enabled"] = "1",
                ["Date"] = "15/03/2025"
            };
            
            PropertiesTools.SetValues(demoObj, newValues);
            Assert.AreEqual("dictValue", demoObj.Name, "SetValues should update Name");
            Assert.AreEqual(789L, demoObj.LongValue, "SetValues should convert and update LongValue");
            Assert.AreEqual(true, demoObj.Enabled, "SetValues should convert and update Enabled");
            Assert.AreEqual(new DateTime(2025, 3, 15), demoObj.Date, "SetValues should convert and update Date");
            
            // Test error handling
            try
            {
                PropertiesTools.SetValue(null, "Name", "value");
                Assert.Fail("Should throw ArgumentNullException for null object");
            }
            catch (ArgumentNullException) { /* Expected */ }
            
            try
            {
                PropertiesTools.SetValue(demoObj, "NonExistingProperty", "value");
                Assert.Fail("Should throw exception for non-existing property");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("not found"), "Exception should mention property not found");
            }
            
            // Test SetValues with null values
            PropertiesTools.SetValues(demoObj, null);
            // Should not throw exception
            
            // Benchmark test (simplified)
            var startTime = DateTime.Now;
            for (int i = 0; i < 1000; i++)
            {
                PropertiesTools.GetValue(demoObj, "Name");
            }
            var elapsed = DateTime.Now - startTime;
            Assert.IsTrue(elapsed.TotalMilliseconds < 1000, "Performance test: 1000 GetValue calls should be fast");
        }


    }
}
