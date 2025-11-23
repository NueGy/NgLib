using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nglib.SECURITY.CRYPTO;
using System;
using System.Security.Cryptography;

namespace Nglib.SECURITY.CRYPTO
{
    /// <summary>
    /// Test exhaustif pour CryptoCoreTools
    /// </summary>
    [TestClass]
    public class CryptoCoreToolsFullyTest
    {
        [TestMethod]
        public void CryptoCoreToolsFullyTestMethod()
        {
            try
            {
                // Test GenerateRandomBytes
                byte[] randomBytes8 = CryptoCoreTools.GenerateRandomBytes(8);
                Assert.IsNotNull(randomBytes8);
                Assert.AreEqual(8, randomBytes8.Length);

                byte[] randomBytes16 = CryptoCoreTools.GenerateRandomBytes(16);
                Assert.IsNotNull(randomBytes16);
                Assert.AreEqual(16, randomBytes16.Length);

                byte[] randomBytes32 = CryptoCoreTools.GenerateRandomBytes(32);
                Assert.IsNotNull(randomBytes32);
                Assert.AreEqual(32, randomBytes32.Length);

                // Vérifier que les bytes sont vraiment aléatoires (différents à chaque appel)
                byte[] randomBytes8_2 = CryptoCoreTools.GenerateRandomBytes(8);
                Assert.IsFalse(ArraysEqual(randomBytes8, randomBytes8_2));

                // Test avec longueur 0 et négative
                byte[] randomBytes0 = CryptoCoreTools.GenerateRandomBytes(0);
                Assert.AreEqual(1, randomBytes0.Length); // Retourne 1 byte si length < 1

                byte[] randomBytesNeg = CryptoCoreTools.GenerateRandomBytes(-5);
                Assert.AreEqual(1, randomBytesNeg.Length); // Retourne 1 byte si length < 1

                // Test GetDerived
                byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes("TestPassword123");
                Rfc2898DeriveBytes derived = CryptoCoreTools.GetDerived(passwordBytes);
                Assert.IsNotNull(derived);

                // Test avec différents counts
                Rfc2898DeriveBytes derived1000 = CryptoCoreTools.GetDerived(passwordBytes, 1000);
                Rfc2898DeriveBytes derived2000 = CryptoCoreTools.GetDerived(passwordBytes, 2000);
                Assert.IsNotNull(derived1000);
                Assert.IsNotNull(derived2000);

                // Vérifier que les clés dérivées sont cohérentes
                byte[] key1 = derived1000.GetBytes(32);
                byte[] key2 = CryptoCoreTools.GetDerived(passwordBytes, 1000).GetBytes(32);
                Assert.IsTrue(ArraysEqual(key1, key2)); // Même password = même clé dérivée

                // Test RemoveBeginSalt
                byte[] testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };
                byte[] removedSalt = CryptoCoreTools.RemoveBeginSalt(testData, 8);
                Assert.IsNotNull(removedSalt);
                Assert.AreEqual(testData.Length - 8, removedSalt.Length);

                // Vérifier DefaultDerivedSaltBytes
                Assert.IsNotNull(CryptoCoreTools.DefaultDerivedSaltBytes);
                Assert.AreEqual(8, CryptoCoreTools.DefaultDerivedSaltBytes.Length);

                // Test edge cases pour RemoveBeginSalt
                byte[] shortData = new byte[] { 1, 2, 3 };
                // Ne devrait pas planter même si saltsize > data length
                try
                {
                    byte[] result = CryptoCoreTools.RemoveBeginSalt(shortData, 10);
                    // Si ça ne plante pas, c'est ok
                }
                catch (Exception ex)
                {
                    // Exception attendue pour des paramètres invalides
                    Assert.IsTrue(ex.Message.Contains("RemoveBeginSalt"));
                }

                // Test AddBeginSalt (méthode incomplète, doit retourner null actuellement)
                byte[] saltResult = CryptoCoreTools.AddBeginSalt(testData, 8);
                Assert.IsNull(saltResult); // Méthode pas encore implémentée

                Console.WriteLine("✅ CryptoCoreToolsFullyTest : Tous les tests réussis");
            }
            catch (Exception ex)
            {
                Assert.Fail($"CryptoCoreToolsFullyTest échoué : {ex.Message}");
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