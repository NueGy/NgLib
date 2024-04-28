using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.FORMAT
{
    [Obsolete("BETA")]
    public static class ObjectBaseTools
    {

        /// <summary>
        /// Convert an array of bytes to a string of hex digits
        /// </summary>
        /// <param name="bytes">array of bytes</param>
        /// <returns>String of hex digits</returns>
        public static string HexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }


        public static string BaseConvert(string number, int fromBase, int toBase)
        {
            return BaseConvertGeneric(number, fromBase, toBase);
        }


        /// <summary>
        /// Conversion en base 10,16,...
        /// </summary>
        private static string BaseConvertGeneric(string number, int fromBase, int toBase)
        {
            var digits = "0123456789abcdefghijklmnopqrstuvwxyz";
            var length = number.Length;
            var result = string.Empty;

            var nibbles = number.Select(c => digits.IndexOf(c)).ToList();
            int newlen;
            do
            {
                var value = 0;
                newlen = 0;

                for (var i = 0; i < length; ++i)
                {
                    value = value * fromBase + nibbles[i];
                    if (value >= toBase)
                    {
                        if (newlen == nibbles.Count)
                        {
                            nibbles.Add(0);
                        }
                        nibbles[newlen++] = value / toBase;
                        value %= toBase;
                    }
                    else if (newlen > 0)
                    {
                        if (newlen == nibbles.Count)
                        {
                            nibbles.Add(0);
                        }
                        nibbles[newlen++] = 0;
                    }
                }
                length = newlen;
                result = digits[value] + result; //
            }
            while (newlen != 0);

            return result;
        }


        /// <summary>
        /// Convert string to base64 string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RebaseString(this string input, int fromBase, int toBase)
        {
            try
            {
                if (string.IsNullOrEmpty(input)) return string.Empty;
                byte[] bytes = null;
                if (fromBase > 255) fromBase = 0;
                if (toBase > 255) toBase = 0;

                // Convert to bytes
                if (fromBase == 16)
                {
                    bytes = Convert.FromHexString(input);
                }
                else if (fromBase == 64)
                {
                    bytes = Convert.FromBase64String(input);
                }
                else if (fromBase == 0)
                {
                    bytes = Encoding.UTF8.GetBytes(input);
                }
                else throw new Exception($"fromBase{fromBase} not supported");

                // Convert to string
                if (toBase == 16)
                {
                    return Convert.ToHexString(bytes);
                }
                else if (toBase == 64)
                {
                    return Convert.ToBase64String(bytes);
                }
                else if (toBase == 0)
                {
                    return Encoding.UTF8.GetString(bytes);
                }
                else throw new Exception($"toBase{toBase} not supported");
            }
            catch (Exception ex)
            {
                throw new Nglib.APP.DIAG.CascadeException("RebaseString", ex);
            }

        }






        /// <summary>
        /// Base64 String to string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>String equivalent of </returns>
        public static string FromBase64(this string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            var work = Convert.FromBase64String(string.Empty + input);
            return Encoding.UTF8.GetString(work);
        }


        /// <summary>
        /// Strings to base64 string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>Base 64 string</returns>
        public static string ToBase64(this string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            var work = Encoding.UTF8.GetBytes(string.Empty + input);
            return Convert.ToBase64String(work);
        }


        public static string ToBase64(byte[] val)
        {
            if (val == null) return null;
            return Convert.ToBase64String(val);
        }










        /// <summary>
        /// Convert Integer to hex string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Hexadecimal equivalent of input</returns>
        public static string ToHexString(int input)
        {
            return Convert.ToString(input, 16);
        }

        public static string ToHexString(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return ToHexString(bytes);
        }

        public static string ToHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower(); // retour hexa
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
