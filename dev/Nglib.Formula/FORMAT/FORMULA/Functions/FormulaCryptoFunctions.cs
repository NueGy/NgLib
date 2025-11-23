using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;


namespace Nglib.FORMAT.FORMULA
{
    /// <summary>
    /// Calculateurs de formule pour des données
    /// </summary>
    public static class FormulaCryptoFunctions
    {

        /*

        [Formula("md5", 1, "hash md5")]
        public static object Md5(string[] args)
         => Nglib.FORMAT.CryptHashTools.Hash(args[0], Nglib.SECURITY.CRYPTO.HashModeEnum.MD5);


        [Formula("sha", 1, "hash sha256")]
        public static object Sha(string[] args)
            => Nglib.FORMAT.CryptHashTools.Hash(args[0], Nglib.SECURITY.CRYPTO.HashModeEnum.SHA256);



        [Formula("Encrypt", 2, "AES 256 encryption (Without Vector)", UseExample = "Encrypt('valeur','key')")]
        public static object Encrypt(string[] args)
            => Nglib.FORMAT.CryptHashTools.Encrypt(args[0], args[1]);

        [Formula("Decrypt", 2, "AES 256 decryption (Without Vector)", UseExample = "Decrypt('xxxxcc','key')")]
        public static object Decrypt(string[] args)
            => Nglib.FORMAT.CryptHashTools.Decrypt(args[0], args[1]);
        */

        [Formula("ToBase64", 1, "ToBase64")]
        public static object ToBase36(string[] args)
                => Nglib.FORMAT.ObjectBaseTools.ToBase64(args[0]);

        [Formula("FromBase64", 1, "FromBase64")]
        public static object FromBase36(string[] args)
            => Nglib.FORMAT.ObjectBaseTools.FromBase64(args[0]);



         






    }
}
