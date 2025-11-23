using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.SECURITY.CRYPTO;
using System;
using System.Text;

namespace Nglib.SECURITY.CRYPTO
{
    /// <summary>
    /// Test exhaustif pour CryptoOption
    /// </summary>
    [TestClass]
    public class CryptoOptionFullyTest
    {
        [TestMethod]
        public void CryptoOptionFullyTestMethod()
        {
            try
            {
                // Test création et configuration basique
                CryptoOption option = new CryptoOption();
                Assert.IsNotNull(option);
                Assert.AreEqual(CryptoModeEnum.AES256, option.CryptoMode);
                Assert.AreEqual(0, option.RandomStartSalt);

                // Test SetCryptoPassword
                string testPassword = "TestPassword123";
                option.SetCryptoPassword(testPassword);
                Assert.IsNotNull(option.GetCryptoKeyBytes());
                Assert.AreEqual(32, option.GetCryptoKeyBytes().Length); // SHA256 = 32 bytes

                // Test SetCryptoKey avec string
                string testKey = "TestKey123456789";
                option.SetCryptoKey(testKey);
                Assert.IsNotNull(option.GetCryptoKeyBytes());
                
                // Test SetCryptoKey avec bytes
                byte[] keyBytes = Encoding.UTF8.GetBytes("TestKeyBytes1234");
                option.SetCryptoKey(keyBytes);
                Assert.AreEqual(keyBytes, option.GetCryptoKeyBytes());

                // Test InitializationVector
                Assert.IsTrue(option.IsEmptyIV());
                string ivValue = "TestIV1234567890";
                option.SetInitializationVector(ivValue);
                Assert.IsFalse(option.IsEmptyIV());
                Assert.IsNotNull(option.GetCryptoIVBytes());
                Assert.AreEqual(16, option.GetCryptoIVBytes().Length); // Tronqué à 16 bytes

                // Test IV vide
                option.SetInitializationVector(null);
                Assert.IsTrue(option.IsEmptyIV());
                option.SetInitializationVector("");
                Assert.IsTrue(option.IsEmptyIV());

                // Test GetCryptoIVBytes quand vide retourne 16 bytes
                byte[] emptyIV = option.GetCryptoIVBytes();
                Assert.AreEqual(16, emptyIV.Length);

                // Test CloneOption
                option.SetCryptoPassword("CloneTest");
                option.SetInitializationVector("CloneIV12345678");
                option.RandomStartSalt = 8;
                
                ICryptoOption cloned = option.CloneOption();
                Assert.IsNotNull(cloned);
                Assert.AreEqual(option.GetCryptoKeyBytes().Length, cloned.GetCryptoKeyBytes().Length);
                Assert.AreEqual(option.GetCryptoIVBytes().Length, cloned.GetCryptoIVBytes().Length);
                Assert.AreEqual(option.IsEmptyIV(), cloned.IsEmptyIV());

                // Test exceptions
                Assert.ThrowsException<ArgumentNullException>(() => option.SetCryptoPassword(null));
                Assert.ThrowsException<ArgumentNullException>(() => option.SetCryptoPassword(""));
                Assert.ThrowsException<ArgumentNullException>(() => option.SetCryptoKey((string)null));
                Assert.ThrowsException<ArgumentNullException>(() => option.SetCryptoKey((byte[])null));

                Console.WriteLine("✅ CryptoOptionFullyTest : Tous les tests réussis");
            }
            catch (Exception ex)
            {
                Assert.Fail($"CryptoOptionFullyTest échoué : {ex.Message}");
            }
        }
    }
}