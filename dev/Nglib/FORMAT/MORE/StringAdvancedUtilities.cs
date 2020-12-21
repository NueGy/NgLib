using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.FORMAT.MORE
{
    public static class StringAdvancedUtilities
    {





        /// <summary>
        /// Returns the initials of each word in a string. Words must be separated with spaces.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="capitalizeInitials">True to capitalize each initial in the output string.</param>
        /// <param name="preserveSpaces">True to preserver the spaces between initials in the output string.</param>
        /// <param name="includePeriod">True to include a '.' after each intiali</param>
        public static string GetInitials(string input, bool capitalizeInitials, bool preserveSpaces, bool includePeriod)
        {
            //Verify input
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            string[] words = input.Split(' ');

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    if (capitalizeInitials)
                        words[i] = char.ToUpper(words[i][0]).ToString(); //only keep the first letter
                    else
                        words[i] = words[i][0].ToString(); //only keep the first letter

                    if (includePeriod)
                        words[i] += ".";
                }
            }

            if (preserveSpaces)
                return string.Join(" ", words);
            else
                return string.Join("", words);
        }




        /// <summary>
        /// Capitalizes the first character in a string.
        /// </summary>
        public static string Capitalize(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            if (input.Length == 1) return input.ToUpper();

            return input[0].ToString().ToUpper() + input.Substring(1);
        }

        /// <summary>
        /// Returns a string with each word's first character capitalized. Words are separated according to the sepecified string sequence.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="separator">The string sequence that separates words.</param>
        public static string GetTitle(string input, string separator)
        {
            //Verify input
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            string[] words = input.Split(separator.ToCharArray());

            for (int i = 0; i < words.Length; i++)
            {
                //Capitalize each word
                if (words[i].Length > 0)
                    words[i] = char.ToUpper(words[i][0]).ToString() + words[i].Substring(1);
            }

            return string.Join(separator, words);
        }


        /// <summary>
        /// Returns a string with characters in reverse order.
        /// </summary>
        public static string Reverse(string input)
        {
            //Validate input
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            char[] outputChars = input.ToCharArray();

            //Reverse
            Array.Reverse(outputChars);

            //build a string from the processed characters and return it
            return new string(outputChars);
        }


    }
}
