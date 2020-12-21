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
            var encstr = Nglib.FORMAT.CryptHashTools.Encrypt(orgn, "monpass");
            string dest = Nglib.FORMAT.CryptHashTools.Decrypt(encstr, "monpass");
            Assert.AreEqual(orgn, dest);

        }
    }
}
