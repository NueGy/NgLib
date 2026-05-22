using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.DATA.COLLECTIONS;
using System.Collections.Generic;

namespace Nglib.PublicTests.DATA.COLLECTIONS
{
    /// <summary>
    /// Tests for ListResult improvements
    /// </summary>
    [TestClass]
    public class ListResultTests
    {
        /// <summary>
        /// Test that Info property uses correct Pascal case convention
        /// </summary>
        [TestMethod]
        public void ListResult_InfoPropertyPascalCase_Test()
        {
            // === Create ListResult ===
            var result = new ListResult<string>();
            
            // === Verify Info property exists and is accessible ===
            Assert.IsNotNull(result.Info, "Info property should exist");
            
            // === Set properties ===
            result.Info.TotalCount = 100;
            result.Info.Error = "Test error";
            result.Info.ExecutionTimeMs = 250;
            result.Info.RequestId = "REQ123";
            
            // === Verify values ===
            Assert.AreEqual(100, result.Info.TotalCount);
            Assert.AreEqual("Test error", result.Info.Error);
            Assert.AreEqual(250, result.Info.ExecutionTimeMs);
            Assert.AreEqual("REQ123", result.Info.RequestId);
            
            // === Test PrepareForError static method ===
            var errorResult = ListResult<int>.PrepareForError("Database connection failed");
            Assert.IsNotNull(errorResult.Info);
            Assert.AreEqual("Database connection failed", errorResult.Info.Error);
        }

        /// <summary>
        /// Test ListResult with data
        /// </summary>
        [TestMethod]
        public void ListResult_WithData_Test()
        {
            // === Create with data ===
            var items = new List<string> { "Item1", "Item2", "Item3" };
            var result = new ListResult<string>(items);
            
            // === Verify data ===
            Assert.AreEqual(3, result.data.Count);
            
            // === Set metadata ===
            result.Info.TotalCount = 10;
            result.Info.ExecutionTimeMs = 50;
            
            Assert.AreEqual(10, result.Info.TotalCount);
        }
    }
}
