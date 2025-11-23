using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.FORMAT;

namespace Nglib.PublicTests.FORMAT
{
    /// <summary>
    /// Tests unitaires pour NumberTools
    /// </summary>
    [TestClass]
    public class NumberToolsTests
    {
        /// <summary>
        /// Test complet de toutes les méthodes NumberTools
        /// </summary>
        [TestMethod]
        public void NumberTools_AllMethods_WorkCorrectly()
        {
            // IsNumeric Tests
            Assert.IsTrue(NumberTools.IsNumeric("123"));
            Assert.IsTrue(NumberTools.IsNumeric("0"));
            Assert.IsTrue(NumberTools.IsNumeric("-456"));
            Assert.IsTrue(NumberTools.IsNumeric("123.45", true));
            Assert.IsTrue(NumberTools.IsNumeric("0.0", true));
            Assert.IsTrue(NumberTools.IsNumeric("-456.789", true));
            Assert.IsFalse(NumberTools.IsNumeric("123.45", false));
            Assert.IsFalse(NumberTools.IsNumeric("0.0", false));
            Assert.IsFalse(NumberTools.IsNumeric("abc"));
            Assert.IsFalse(NumberTools.IsNumeric("12a3"));
            Assert.IsFalse(NumberTools.IsNumeric(""));
            Assert.IsFalse(NumberTools.IsNumeric(null));
            Assert.IsFalse(NumberTools.IsNumeric("   "));
            Assert.IsTrue(NumberTools.IsNumeric("1 2 3"));
            Assert.IsTrue(NumberTools.IsNumeric(" 123 "));

            // HasNumeric Tests
            Assert.IsTrue(NumberTools.HasNumeric("abc123"));
            Assert.IsTrue(NumberTools.HasNumeric("123abc"));
            Assert.IsTrue(NumberTools.HasNumeric("ab1c"));
            Assert.IsTrue(NumberTools.HasNumeric("0"));
            Assert.IsFalse(NumberTools.HasNumeric("abc"));
            Assert.IsFalse(NumberTools.HasNumeric(""));
            Assert.IsFalse(NumberTools.HasNumeric(null));
            Assert.IsFalse(NumberTools.HasNumeric("!@#$%"));

            // RoundAmount Tests
            Assert.AreEqual(123.46, NumberTools.RoundAmount(123.456));
            Assert.AreEqual(123.45, NumberTools.RoundAmount(123.454));
            Assert.AreEqual(0.00, NumberTools.RoundAmount(0.001));
            Assert.AreEqual(100.00, NumberTools.RoundAmount(100));

            // CalcPercent Tests - Int
            Assert.AreEqual(50, NumberTools.CalcPercent(50, 100));
            Assert.AreEqual(25, NumberTools.CalcPercent(1, 4));
            Assert.AreEqual(200, NumberTools.CalcPercent(200, 100));
            Assert.AreEqual(0, NumberTools.CalcPercent(0, 100));

            // CalcPercent Tests - Long
            Assert.AreEqual(50, NumberTools.CalcPercent(50L, 100L));
            Assert.AreEqual(25, NumberTools.CalcPercent(1L, 4L));
            Assert.AreEqual(200, NumberTools.CalcPercent(200L, 100L));
            Assert.AreEqual(0, NumberTools.CalcPercent(0L, 100L));

            // CalcPercent Tests - Double
            Assert.AreEqual(50, NumberTools.CalcPercent(50.0, 100.0));
            Assert.AreEqual(25, NumberTools.CalcPercent(1.0, 4.0));
            Assert.AreEqual(33, NumberTools.CalcPercent(1.0, 3.0)); // 33.33... truncated to 33
            Assert.AreEqual(0, NumberTools.CalcPercent(0.0, 100.0));

            // CalcPercent Tests - Division par zéro
            Assert.AreEqual(0, NumberTools.CalcPercent(50, 0));
            Assert.AreEqual(0, NumberTools.CalcPercent(50L, 0L));
            Assert.AreEqual(0, NumberTools.CalcPercent(50.0, 0.0));

            // PadNumeric Tests
            Assert.AreEqual("00123", NumberTools.PadNumeric("123", 5));
            Assert.AreEqual("123", NumberTools.PadNumeric("123", 3));
            Assert.AreEqual("001", NumberTools.PadNumeric("1", 3));
            Assert.AreEqual("00123", NumberTools.PadNumeric("1 2 3", 5));
            Assert.AreEqual("00123", NumberTools.PadNumeric(" 123 ", 5));
            Assert.AreEqual("123", NumberTools.PadNumeric("12345", 3));
            Assert.AreEqual("000", NumberTools.PadNumeric(null, 3));
            Assert.AreEqual("0123.45", NumberTools.PadNumeric("123.45", 7));
            Assert.AreEqual("01,23", NumberTools.PadNumeric("1,23", 5));
        }
    }
}