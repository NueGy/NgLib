// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Nglib.SECURITY.CRYPTO;

namespace Nglib.FORMAT
{
    /// <summary>
    /// Outils de cryptages string
    /// </summary>
    public static class CryptHashTools
    {


      

        /// <summary>
        /// Hash string (password, ...)  UTF8 => HASH
        /// </summary>
        /// <param name="origine">chaine origine</param>
        /// <param name="mode">SHA256 ou UTF8</param>
        /// <returns></returns>
        public static string Hash(this string origine, HashModeEnum mode = HashModeEnum.SHA256)
        {
            if (string.IsNullOrEmpty(origine)) return string.Empty;
            HashAlgorithm hasher = null;
            try
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(origine);
                byte[] hashedBytes = null;
                if(mode== HashModeEnum.SHA256)
                    hasher = (new System.Security.Cryptography.SHA256CryptoServiceProvider());
                else if (mode == HashModeEnum.SHA1)
                    hasher = (new System.Security.Cryptography.SHA1CryptoServiceProvider());
                else if (mode == HashModeEnum.SHA512)
                    hasher = (new System.Security.Cryptography.SHA512CryptoServiceProvider());
                else if (mode == HashModeEnum.MD5)
                    hasher = (new System.Security.Cryptography.MD5CryptoServiceProvider());

                hashedBytes= hasher.ComputeHash(inputBytes);
                return BitConverter.ToString(hashedBytes).Replace("-", string.Empty).ToLower();
            }
            catch (Exception e)
            {
                throw new Exception("error hash " + e.Message);
            }
            finally
            {
                if (hasher != null) hasher.Dispose();
            }
        }


        /// <summary>
        /// Encrypter une chaine de caratère
        /// </summary>
        /// <param name="input">text value</param>
        /// <param name="cryptoInformation">informations de cryptage</param>
        /// <returns></returns>
        public static string Encrypt(this string input, SECURITY.CRYPTO.ICryptoOption cryptoInformation)
        {
            //https://webman.developpez.com/articles/dotnet/aes-rijndael/
            //https://stackoverflow.com/questions/32972126/creating-decrypt-passwords-with-salt-iv
            if (string.IsNullOrEmpty(input)) return string.Empty;
            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
            byte[] bytesEncrypted = SECURITY.CRYPTO.CryptoTools.Encrypt(bytesToBeEncrypted, cryptoInformation);
            string result = Convert.ToBase64String(bytesEncrypted);
            return result;
        }




        /// <summary>
        /// Encrypter une chaine de caratère
        /// </summary>
        /// <param name="input">Chaine à crypter</param>
        /// <param name="passwordtext">mot de passe (ajoutera un hashage)</param>
        /// <param name="initializationVector">IV pour rendre unique le cryptage</param>
        /// <returns></returns>
        public static string Encrypt(this string input, string passwordtext, string initializationVector=null)
        {
            SECURITY.CRYPTO.CryptoOption cryptoInformation = new CryptoOption();
            cryptoInformation.SetCryptoPassword(passwordtext);
            cryptoInformation.SetInitializationVector(initializationVector);
            return Encrypt(input, cryptoInformation);
        }



        /// <summary>
        /// Décrypter une chaine de caractère
        /// </summary>
        /// <param name="input">text value</param>
        /// <param name="cryptoInformation">informations de cryptage</param>
        /// <returns></returns>
        public static string Decrypt(this string input, SECURITY.CRYPTO.ICryptoOption cryptoInformation)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            byte[] bytesToBeDecrypted = Convert.FromBase64String(input);
            byte[] bytesDecrypted = SECURITY.CRYPTO.CryptoTools.Decrypt(bytesToBeDecrypted, cryptoInformation);
            string result = Encoding.UTF8.GetString(bytesDecrypted);
            return result;
        }




        /// <summary>
        /// Décrypter une chaine de caractère
        /// </summary>
        /// <param name="input">text value</param>
        /// <param name="passwordtext">Mode de passe (ajoutera un hashage)</param>
        /// <param name="initializationVector">Vecteur d'initialisation</param>
        /// <returns></returns>
        public static string Decrypt(this string input, string passwordtext, string initializationVector = null)
        {
            SECURITY.CRYPTO.CryptoOption cryptoInformation = new CryptoOption();
            cryptoInformation.SetCryptoPassword(passwordtext);
            cryptoInformation.SetInitializationVector(initializationVector);
            return Decrypt(input, cryptoInformation);
        }









        /// <summary>
        /// Convert Integer to hex string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Hexadecimal equivalent of input</returns>
        public static string IntegerToHexString(int input)
        {
            return Convert.ToString(input, 16);
        }







        public static string EncryptCeasar(string str, int cryptoNumber)
        {
            return string.Join("", str.Select(chr => {
                int x = chr - 65;
                return (char)((65) + ((x + cryptoNumber) % 26));
            }));
        }

        public static string DecryptCeasar(string str, int cryptoNumber)
        {
            return string.Join("", str.Select(chr => {
                int x = chr - 65;
                return (char)((65) + ((x - cryptoNumber) % 26));
            }));
        }








    }
}
