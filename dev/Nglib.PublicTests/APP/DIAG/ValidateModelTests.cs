using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.APP.DIAG;

namespace Nglib.PublicTests.APP.DIAG
{
    /// <summary>
    /// Tests for ValidateModel improvements
    /// </summary>
    [TestClass]
    public class ValidateModelTests
    {
        /// <summary>
        /// Test implicit conversion to bool
        /// </summary>
        [TestMethod]
        public void ValidateModel_ImplicitBoolConversion_Test()
        {
            // === Valid model ===
            var validModel = ValidateModel.Success;
            
            // Implicit conversion should work
            if (validModel)
            {
                Assert.IsTrue(true, "Valid model should convert to true");
            }
            else
            {
                Assert.Fail("Valid model should be true");
            }

            // === Invalid model ===
            var invalidModel = ValidateModel.Fail;
            
            if (!invalidModel)
            {
                Assert.IsTrue(true, "Invalid model should convert to false");
            }
            else
            {
                Assert.Fail("Invalid model should be false");
            }

            // === Null model ===
            ValidateModel nullModel = null;
            
            if (!nullModel)
            {
                Assert.IsTrue(true, "Null model should convert to false");
            }
            else
            {
                Assert.Fail("Null model should be false");
            }

            // === Usage in conditions ===
            var validation = new ValidateModel(true);
            Assert.IsTrue(validation, "Should implicitly convert to true");
            
            validation = new ValidateModel(false);
            Assert.IsFalse(validation, "Should implicitly convert to false");

            // === Usage with && operator ===
            var validation1 = ValidateModel.Success;
            var validation2 = ValidateModel.Success;
            
            if (validation1 && validation2)
            {
                Assert.IsTrue(true, "Both validations should be true");
            }

            validation2 = ValidateModel.Fail;
            Assert.IsFalse(validation1 && validation2, "One failed validation should make expression false");
        }
    }
}
