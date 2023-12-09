using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.FORMAT
{
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



        /// <summary>
        /// Conversion en base 10,16,...
        /// </summary>
        public static string BaseConvert(string number, int fromBase, int toBase)
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

    }
}
