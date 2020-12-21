using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Linq;

namespace Nglib.SECURITY.CRYPTO
{
    /// <summary>
    /// Outils de cryptages et hachages
    /// </summary>
    public static class CryptoTools
    {


        /// <summary>
        /// Utilisé pour le Rfc2898DeriveBytes GetDerived
        /// </summary>
        public static byte[] DefaultDerivedSaltBytes = new byte[] { 7, 1, 8, 2, 2, 2, 6, 9 };


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



        //public static byte[] EncryptWithDerivedPassword(byte[] bytesToBeEncrypted, string password)
        //{
        //    CRYPTO.CryptoInformation cryptoInformation = new CryptoInformation();
        //    cryptoInformation.SetCryptoPassword(password);
        //    var derivekey = GetDerived(passwordBytes);

        //    return EncryptWithDerivedKey(bytesToBeEncrypted, derivekey);
        //}

        //public static byte[] EncryptWithDerivedKey(byte[] bytesToBeEncrypted, Rfc2898DeriveBytes passwordDerived)
        //{
        //    const int keysize = 256;
        //    const int ivsize = 128;
        //    byte[] deriverdkey = passwordDerived.GetBytes(keysize / 8);
        //    byte[] deriverdIV = passwordDerived.GetBytes(ivsize / 8);
        //    CRYPTO.CryptoInformation()
        //    return Encrypt(bytesToBeEncrypted, deriverdkey, deriverdIV);
        //}




        /// <summary>
        ///  Encryptage binaire AES
        /// </summary>
        /// <param name="bytesToBeEncrypted">data</param>
        /// <param name="passwordBytes">password</param>
        /// <param name="WithBeforeSalt">Ajoutera un bloc 16byte aléatoire au début</param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] bytesToBeEncrypted, ICryptoOption cryptoInformation)
        {
            if (cryptoInformation == null) throw new ArgumentNullException("cryptoInformation");
            byte[] encryptedBytes = null;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    //AES.KeySize = 256;
                    //AES.BlockSize = 128;
                    //AES.Padding= PaddingMode. = null;
                    AES.Key = cryptoInformation.GetCryptoKeyBytes(); //passwordDerived.GetBytes(AES.KeySize / 8);
                    AES.IV = cryptoInformation.GetCryptoIVBytes();
                    //AES.IV = GenerateRandomBytes(16); //passwordDerived.GetBytes(AES.BlockSize / 8); //InitializedVector
                    AES.Mode = CipherMode.CBC;
                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }
            return encryptedBytes;
            //return Encrypt(bytesToBeEncrypted, GetDerived(passwordBytes), WithAdditionalSalt);
        }








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
        /// Décryptage Binaire AES
        /// </summary>
        public static byte[] Decrypt(byte[] bytesToBeDecrypted, ICryptoOption cryptoInformation)
        {
            if (cryptoInformation == null) throw new ArgumentNullException("cryptoInformation");
            byte[] decryptedBytes = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    AES.Key = cryptoInformation.GetCryptoKeyBytes();
                    AES.IV = cryptoInformation.GetCryptoIVBytes();
                    AES.Mode = CipherMode.CBC;
                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }
            return decryptedBytes;
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
