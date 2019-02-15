using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Nglib.SECURITY.CRYPTO
{
    /// <summary>
    /// Outils de cryptages et hachages
    /// </summary>
    public static class CryptoTools
    {
        /// <summary>
        /// Liste des hash disponibles
        /// </summary>
        public enum HashModeEnum { SHA256, SHA1, SHA512, MD5 }

        /// <summary>
        /// Utilisé pour le Rfc2898DeriveBytes GetDerived
        /// </summary>
        public static byte[] DefaultDerivedSaltBytes = new byte[] { 7, 1, 8, 2, 2, 2, 6, 9 };


        /// <summary>
        /// Dérivation - salt
        /// </summary>
        /// <param name="passwordBytes"></param>
        /// <returns></returns>
        public static Rfc2898DeriveBytes GetDerived(byte[] passwordBytes)
        {
            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            // GenerateRandomBytes(8) !!!  Comprendre pourquoi on ne peus pas mettre un salt aléatoire ???
            //byte[] saltBytes = new byte[] { 7, 1, 8, 2, 2, 2, 6, 9 };
            var derivKey = new Rfc2898DeriveBytes(passwordBytes, DefaultDerivedSaltBytes, 1000);
            return derivKey;
        }



        /// <summary>
        /// Encryptage binaire AES
        /// </summary>
        /// <param name="bytesToBeEncrypted">Données originales</param>
        /// <param name="password">mot de passe</param>
        /// <param name="WithBeforeSalt">Ajoutera un bloc 8byte aléatoire au début</param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] bytesToBeEncrypted, string password, bool WithAdditionalSalt = false)
        {
            // Hash the password with SHA256
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
            return Encrypt(bytesToBeEncrypted, passwordBytes, WithAdditionalSalt);
        }




        /// <summary>
        ///  Encryptage binaire AES
        /// </summary>
        /// <param name="bytesToBeEncrypted">data</param>
        /// <param name="passwordBytes">password</param>
        /// <param name="WithBeforeSalt">Ajoutera un bloc 16byte aléatoire au début</param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes, bool WithAdditionalSalt = false)
        {
            return Encrypt(bytesToBeEncrypted, GetDerived(passwordBytes), WithAdditionalSalt);
        }

        /// <summary>
        ///   Encryptage binaire  AES
        /// </summary>
        /// <param name="bytesToBeEncrypted"></param>
        /// <param name="passwordDerived"></param>
        /// <param name="WithAdditionalSalt">Ajoutera un bloc 16byte aléatoire au début</param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] bytesToBeEncrypted, Rfc2898DeriveBytes passwordDerived, bool WithAdditionalSalt = false)
        {
            byte[] encryptedBytes = null;
            byte[] InitializedVector = null;

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    AES.Key = passwordDerived.GetBytes(AES.KeySize / 8);
                    AES.IV = passwordDerived.GetBytes(AES.BlockSize / 8); //InitializedVector
                    InitializedVector = AES.IV;
                    AES.Mode = CipherMode.CBC;


                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        if (WithAdditionalSalt)
                        {
                            byte[] random = GenerateRandomBytes(16);
                            cs.Write(random, 0, 16); // on ajoute un salt, un random ce qui va rendre aléatoire la valeur de sortie (ce random de 16 bytes sera supprimé au decrypt)
                            cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        }
                        else
                        {
                            cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        }
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;

        }


        /// <summary>
        /// Décryptage Binaire AES
        /// </summary>
        public static byte[] Decrypt(byte[] bytesToBeDecrypted, string password, bool WithAdditionalSalt = false)
        {
            // Hash the password with SHA256
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
            return Decrypt(bytesToBeDecrypted, passwordBytes, WithAdditionalSalt);
        }


        /// <summary>
        /// Décryptage Binaire AES
        /// </summary>
        public static byte[] Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes, bool WithAdditionalSalt = false)
        {
            return Decrypt(bytesToBeDecrypted, GetDerived(passwordBytes), WithAdditionalSalt);
        }


        /// <summary>
        /// Décryptage Binaire AES
        /// </summary>
        public static byte[] Decrypt(byte[] bytesToBeDecrypted, Rfc2898DeriveBytes passwordDerived, bool WithBeforeSalt = false)
        {
            byte[] decryptedBytes = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    AES.Key = passwordDerived.GetBytes(AES.KeySize / 8);
                    AES.IV = passwordDerived.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }


            if (WithBeforeSalt)
            {
                byte[] RealdecryptedBytes = new byte[decryptedBytes.Length - 16];
                Buffer.BlockCopy(decryptedBytes, 32, RealdecryptedBytes, 0, decryptedBytes.Length - 16);
                return RealdecryptedBytes;
            }

            return decryptedBytes;
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
