using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.SECURITY.CRYPTO
{
    public static class CryptoCoreTools
    {

        /// <summary>
        /// Utilisé pour le Rfc2898DeriveBytes GetDerived
        /// </summary>
        public static byte[] DefaultDerivedSaltBytes = new byte[] { 7, 1, 8, 2, 2, 2, 6, 9 };







        ///// <summary>
        ///// Décryptage Binaire AES
        ///// </summary>
        //public static byte[] Decrypt(byte[] bytesToBeDecrypted, string password, string initializationVector = null)
        //{
        //    // Hash the password with SHA256
        //    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        //    passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
        //    // Vecteur d'initialisation
        //    byte[] bytesInitializationVector = (!string.IsNullOrEmpty(initializationVector)) ? Encoding.UTF8.GetBytes(initializationVector) : new byte[16];
        //    return Decrypt(bytesToBeDecrypted, passwordBytes, bytesInitializationVector);
        //}

        ///// <summary>
        ///// Décryptage Binaire AES
        ///// </summary>
        //public static byte[] DecryptWithDerivedKey(byte[] bytesToBeDecrypted, Rfc2898DeriveBytes passwordDerived)
        //{
        //    const int keysize = 256;
        //    const int BlockSize = 128;
        //    byte[] deriverdkey = passwordDerived.GetBytes(keysize / 8);
        //    byte[] deriverdIV = passwordDerived.GetBytes(BlockSize / 8);
        //    return Decrypt(bytesToBeDecrypted, deriverdkey, deriverdIV);
        //}


        ///// <summary>
        ///// Décryptage Binaire AES
        ///// </summary>
        //public static byte[] DecryptWithDerivedKey(byte[] bytesToBeDecrypted, string password)
        //{
        //    // Hash the password with SHA256
        //    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        //    passwordBytes = SHA256.Create().ComputeHash(passwordBytes); // permet d'avoir la clef sur 32 byte
        //    var derivekey = GetDerived(passwordBytes);
        //    return DecryptWithDerivedKey(bytesToBeDecrypted, derivekey);
        //}









        /// <summary>
        /// Dérivation - salt
        /// </summary>
        /// <param name="passwordBytes"></param>
        /// <returns></returns>
        public static Rfc2898DeriveBytes GetDerived(byte[] passwordBytes, int countderivation = 1000)
        {
            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = DefaultDerivedSaltBytes;// new byte[] { 7, 1, 8, 2, 2, 2, 6, 9 };
            //saltBytes = GenerateRandomBytes(8);  Comprendre pourquoi on ne peus pas mettre un salt aléatoire ???
            var derivKey = new Rfc2898DeriveBytes(passwordBytes, saltBytes, countderivation);
            return derivKey;
        }







        public static byte[] RemoveBeginSalt(byte[] orgnValue, int saltsize = 8)
        {
            try
            {
                byte[] RealdecryptedBytes = new byte[orgnValue.Length - saltsize];
                Buffer.BlockCopy(orgnValue, 32, RealdecryptedBytes, 0, orgnValue.Length - saltsize);
                return RealdecryptedBytes;
            }
            catch (Exception ex)
            {
                throw new Exception("RemoveBeginSalt " + ex.Message);
            }
        }
        public static byte[] AddBeginSalt(byte[] orgnValue, int saltsize = 8)
        {
            try
            {
                byte[] random = GenerateRandomBytes(16);
                // !!!
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        /// <summary>
        /// Génération de bytes aléatoires avec RNGCryptoServiceProvider
        /// </summary>
        public static byte[] GenerateRandomBytes(int length)
        {
            // Create a buffer
            byte[] randBytes;

            if (length >= 1)
            {
                randBytes = new byte[length];
            }
            else
            {
                randBytes = new byte[1];
            }

            // Create a new RNGCryptoServiceProvider.
            System.Security.Cryptography.RNGCryptoServiceProvider rand =
                 new System.Security.Cryptography.RNGCryptoServiceProvider();

            // Fill the buffer with random bytes.
            rand.GetBytes(randBytes);

            // return the bytes.
            return randBytes;
        }


    }
}
