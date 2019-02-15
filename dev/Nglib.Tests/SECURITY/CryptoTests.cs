using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nglib.SECURITY
{
    [TestClass]
    public class CryptoTests
    {
        [TestMethod]
        public void EncryptDecryptTest()
        {

            string orgn = "mavaleur1238";
            var encstr = Nglib.FORMAT.CryptHash.Encrypt(orgn, "monpass", false);
            string dest = Nglib.FORMAT.CryptHash.Decrypt(encstr, "monpass", false);
            Assert.AreEqual(orgn, dest);

        }
    }
}
