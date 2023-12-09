using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nglib.SECURITY.CRYPTO
{
    /// <summary>
    /// Outils de cryptages et hachages
    /// </summary>
    public static class CryptoTools
    {






        //http://csharpexamples.com/c-stream-encryption-and-decryption-with-multi-algorithm-support/


        public static async Task<Stream> EncryptAsync(Stream streamToBeEncrypted, ICryptoOption cryptoInformation, Stream encryptedstream = null)
        {
            try
            {
                if (cryptoInformation == null) throw new ArgumentNullException("cryptoInformation");
                if (streamToBeEncrypted == null) return null;

                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                if (encryptedstream == null) encryptedstream = new MemoryStream();

                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    //AES.KeySize = 256;
                    //AES.BlockSize = 128;
                    //AES.Padding= PaddingMode. = null;
                    AES.Key = cryptoInformation.GetCryptoKeyBytes(); //passwordDerived.GetBytes(AES.KeySize / 8);
                    AES.IV = cryptoInformation.GetCryptoIVBytes();
                    //AES.IV = GenerateRandomBytes(16); //passwordDerived.GetBytes(AES.BlockSize / 8); //InitializedVector
                    AES.Mode = CipherMode.CBC;
                    encryptedstream.Seek(0, SeekOrigin.Begin);
                    streamToBeEncrypted.Seek(0, SeekOrigin.Begin);

                    using (var cs = new CryptoStream(streamToBeEncrypted, AES.CreateEncryptor(), CryptoStreamMode.Read))
                    {
                        BufferedStream bufferedStream = new BufferedStream(cs);
                        await cs.CopyToAsync(encryptedstream);
                        encryptedstream.Seek(0, SeekOrigin.Begin);
                        //cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        ///cs.Close();

                    }


                    //var cs = new CryptoStream(streamToBeEncrypted, AES.CreateEncryptor() CryptoStreamMode.Write);
                    //await streamToBeEncrypted.CopyToAsync(cs);
                    //cs.Close();
                    //cs.Close();
                }
                return encryptedstream;
            }
            catch (Exception ex)
            {
                throw new Exception("EncryptAsync " + ex.Message, ex);
            }
            //return Encrypt(bytesToBeEncrypted, GetDerived(passwordBytes), WithAdditionalSalt);
        }

        public static async Task<Stream> DecryptAsync(Stream streamToBeDecrypted, ICryptoOption cryptoInformation, Stream decryptedstream = null)
        {
            if (cryptoInformation == null) throw new ArgumentNullException("cryptoInformation");
            if (streamToBeDecrypted == null) return null;
            using (RijndaelManaged AES = new RijndaelManaged())
            {
                AES.KeySize = 256;
                AES.BlockSize = 128;
                AES.Key = cryptoInformation.GetCryptoKeyBytes();
                AES.IV = cryptoInformation.GetCryptoIVBytes();
                AES.Mode = CipherMode.CBC;

                using (var cs = new CryptoStream(streamToBeDecrypted, AES.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    await cs.CopyToAsync(decryptedstream);
                    decryptedstream.Seek(0, SeekOrigin.Begin);
                }
            }
            return decryptedstream;
        }



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
            if (bytesToBeEncrypted == null) return null;
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

       

        /// <summary>
        /// Décryptage Binaire AES
        /// </summary>
        public static byte[] Decrypt(byte[] bytesToBeDecrypted, ICryptoOption cryptoInformation)
        {
            if (cryptoInformation == null) throw new ArgumentNullException("cryptoInformation");
            if (bytesToBeDecrypted == null) return null;
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
















    }
}
