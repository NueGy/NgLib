using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.SECURITY.CRYPTO;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.SECURITY.CRYPTO
{
    /// <summary>
    /// Test exhaustif pour CryptoTools
    /// </summary>
    [TestClass]
    public class CryptoToolsFullyTest
    {
        [TestMethod]
        public void CryptoToolsFullyTestMethod()
        {
            try
            {
                // Préparation des données de test
                string testData = "Données de test pour cryptage/décryptage avec des caractères spéciaux: àéèç!@#$%^&*()";
                byte[] originalBytes = Encoding.UTF8.GetBytes(testData);
                
                CryptoOption cryptoOption = new CryptoOption();
                cryptoOption.SetCryptoPassword("MotDePasseSecurise123!");
                cryptoOption.SetInitializationVector("IV1234567890ABCD");

                // Test Encrypt/Decrypt binaire
                byte[] encryptedBytes = CryptoTools.Encrypt(originalBytes, cryptoOption);
                Assert.IsNotNull(encryptedBytes);
                Assert.IsTrue(encryptedBytes.Length > 0);
                Assert.IsFalse(ArraysEqual(originalBytes, encryptedBytes)); // Données différentes après cryptage

                byte[] decryptedBytes = CryptoTools.Decrypt(encryptedBytes, cryptoOption);
                Assert.IsNotNull(decryptedBytes);
                Assert.IsTrue(ArraysEqual(originalBytes, decryptedBytes)); // Round-trip réussi

                string decryptedString = Encoding.UTF8.GetString(decryptedBytes);
                Assert.AreEqual(testData, decryptedString);

                // Test avec données nulles
                Assert.IsNull(CryptoTools.Encrypt(null, cryptoOption));
                Assert.IsNull(CryptoTools.Decrypt(null, cryptoOption));

                // Test exceptions avec cryptoOption null
                Assert.ThrowsException<ArgumentNullException>(() => CryptoTools.Encrypt(originalBytes, null));
                Assert.ThrowsException<ArgumentNullException>(() => CryptoTools.Decrypt(encryptedBytes, null));

                // Test EncryptAsync/DecryptAsync avec Stream
                TestStreamEncryptionAsync(testData, cryptoOption).Wait();

                // Test avec différentes clés (doit échouer ou donner du garbage)
                CryptoOption wrongOption = new CryptoOption();
                wrongOption.SetCryptoPassword("MauvaiseClé123!");
                wrongOption.SetInitializationVector("IV1234567890ABCD");

                try
                {
                    byte[] wrongDecrypt = CryptoTools.Decrypt(encryptedBytes, wrongOption);
                    string wrongString = Encoding.UTF8.GetString(wrongDecrypt);
                    Assert.AreNotEqual(testData, wrongString); // Doit être différent avec mauvaise clé
                }
                catch (Exception)
                {
                    // Exception attendue avec mauvaise clé - OK
                }

                Console.WriteLine("✅ CryptoToolsFullyTest : Tous les tests réussis");
            }
            catch (Exception ex)
            {
                Assert.Fail($"CryptoToolsFullyTest échoué : {ex.Message}");
            }
        }

        private async Task TestStreamEncryptionAsync(string testData, ICryptoOption cryptoOption)
        {
            byte[] originalBytes = Encoding.UTF8.GetBytes(testData);
            
            using (MemoryStream originalStream = new MemoryStream(originalBytes))
            using (MemoryStream encryptedStream = new MemoryStream())
            using (MemoryStream decryptedStream = new MemoryStream())
            {
                // Test EncryptAsync
                Stream resultEncrypt = await CryptoTools.EncryptAsync(originalStream, cryptoOption, encryptedStream);
                Assert.IsNotNull(resultEncrypt);
                Assert.IsTrue(encryptedStream.Length > 0);

                // Test DecryptAsync
                encryptedStream.Seek(0, SeekOrigin.Begin);
                Stream resultDecrypt = await CryptoTools.DecryptAsync(encryptedStream, cryptoOption, decryptedStream);
                Assert.IsNotNull(resultDecrypt);
                
                string decryptedString = Encoding.UTF8.GetString(decryptedStream.ToArray());
                Assert.AreEqual(testData, decryptedString);
            }

            // Test avec streams null
            Assert.IsNull(await CryptoTools.EncryptAsync(null, cryptoOption));
            Assert.IsNull(await CryptoTools.DecryptAsync(null, cryptoOption));

            // Test exception avec cryptoOption null
            using (MemoryStream testStream = new MemoryStream(originalBytes))
            {
                await Assert.ThrowsExceptionAsync<Exception>(() => CryptoTools.EncryptAsync(testStream, null));
                await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => CryptoTools.DecryptAsync(testStream, null));
            }
        }

        private bool ArraysEqual(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length) return false;
            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i]) return false;
            }
            return true;
        }
    }
}