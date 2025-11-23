using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Text.RegularExpressions;

namespace Nglib.FORMAT
{
    /// <summary>
    /// Utility class for string manipulation and formatting.
    /// Documentation: <see href="https://github.com/NueGy/NgLibComponents/wiki/wiki_components_format"/>
    /// </summary>
    public static class StringTools
    {
        internal const string AlphaNumCharsConst = "azertyuiopqsdfghjklmwxcvbnAZERTYUIOPQSDFGHJKLMWXCVBN0123456789";
        internal static readonly HashSet<char> removeCharsSet = new HashSet<char>
        {
            '?', '&', '^', '$', '#', '@', '!', '<', '>', '\'', '"', '*',
            // Control and problematic characters
            '\uFFFD', // Unicode Replacement Character 
            '\u00A0', // Non-breaking space
            '\u200B', // Zero-width space
            '\u200C', // Zero-width non-joiner
            '\u200D', // Zero-width joiner
            '\uFEFF'  // Byte order mark
        };

        internal static readonly Dictionary<char, char> DiacriticsMap = new()
        {
            {'à', 'a'}, {'á', 'a'}, {'ä', 'a'}, {'â', 'a'}, {'ã', 'a'}, {'å', 'a'},
            {'é', 'e'}, {'è', 'e'}, {'ê', 'e'}, {'ë', 'e'},
            {'ì', 'i'}, {'í', 'i'}, {'ï', 'i'}, {'î', 'i'},
            {'ò', 'o'}, {'ó', 'o'}, {'ô', 'o'}, {'ö', 'o'},
            {'û', 'u'}, {'ü', 'u'}, {'ù', 'u'}, {'ú', 'u'},
            {'ý', 'y'}, {'ÿ', 'y'}, {'ç', 'c'}, {'ñ', 'n'}
        };


        private static readonly System.Threading.ThreadLocal<Random> threadSafeRandom 
            =  new(() => new Random(Guid.NewGuid().GetHashCode()));


        /// <summary>
        /// Generates a random string of specified length
        /// </summary>
        /// <param name="length">Length of the string</param>
        /// <param name="chars">Character set to use</param>
        /// <returns>Random string</returns>
        public static string RandomString(int length, string chars = "abcdefghijklmnopqrstuvwxyz123456789")
        {
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[threadSafeRandom.Value.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Generates a time-based GUID as a 32-character string (no hyphens)
        /// </summary>
        /// <returns>32-character GUID string</returns>
        public static string RandomGuid32()
        {
            try
            {
                var tempGuid = Guid.NewGuid();
                var bytes = tempGuid.ToByteArray();
                var time = DateTime.Now;

                bytes[3] = (byte)time.Year;
                bytes[2] = (byte)time.Month;
                bytes[1] = (byte)time.Day;
                bytes[0] = (byte)time.Hour;
                bytes[5] = (byte)time.Minute;
                bytes[4] = (byte)time.Second;

                var CurrentGuid = new Guid(bytes);

                return CurrentGuid.ToString("N");
            }
            catch (Exception)
            {
                return null;
            }
        }



        /// <summary>
        /// Checks if string contains only letters and numbers
        /// </summary>
        /// <param name="input">String to check</param>
        /// <returns>True if alphanumeric only</returns>
        public static bool IsAlphaNumeric(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            ReadOnlySpan<char> span = input.AsSpan();
            for (int i = 0; i < span.Length; i++)
                if (!char.IsLetterOrDigit(span[i])) return false;
            return true;
        }






        /// <summary>
        /// Filters a string to keep only specified characters
        /// </summary>
        /// <param name="original">Original string</param>
        /// <param name="characters">Allowed characters</param>
        /// <returns>Filtered string</returns>
        public static string FilterCharacters(string original, string characters = AlphaNumCharsConst)
        {
            if (original == null) return null;
            var retour = new StringBuilder();
            foreach (var item in original)
                if (characters.Contains(item))
                    retour.Append(item);
            return retour.ToString();
        }


        /// <summary>
        /// Limits string length and removes carriage returns. Handles cases where string is shorter than limit.
        /// </summary>
        /// <param name="original">Original string</param>
        /// <param name="num">Maximum length</param>
        /// <returns>Limited string</returns>
        public static string Limit(string original, int num)
        {
            if (original == null) return null;
            var nb = original.Length;
            if (nb > num) nb = num;
            original = original.Substring(0, nb);
            return original;
        }


        /// <summary>
        /// Safely extracts substring from a position with out-of-range handling
        /// </summary>
        /// <param name="original">Original string</param>
        /// <param name="Position">Start position</param>
        /// <returns>Substring or empty string</returns>
        public static string SubstringSafe(string original, int Position)
        {
            if (original == null) return string.Empty; 
            var originalLength = original.Length;
            if (originalLength < Position) return string.Empty; // too far
            original = original.Substring(Position);
            return original;
        }

        /// <summary>
        /// Safely extracts substring with position and length, handles out-of-range cases
        /// </summary>
        /// <param name="original">Original string</param>
        /// <param name="Position">Start position</param>
        /// <param name="lenght">Length to extract</param>
        /// <returns>Substring or empty string</returns>
        public static string SubstringSafe(string original, int Position, int lenght)
        {
            if (original == null) return string.Empty; 
            var originalLength = original.Length;
            if (originalLength < Position) return string.Empty; // too far
            if (originalLength < lenght + Position) lenght = originalLength - Position; // not enough characters
            original = original.Substring(Position, lenght);
            return original;
        }


        /// <summary>
        /// Replaces diacritics (accented characters) with their base equivalents (à=>a, é=>e, etc.)
        /// </summary>
        /// <param name="inputString">Input string with diacritics</param>
        /// <returns>String without diacritics</returns>
        public static string ReplaceDiacritics(string inputString)
        {
            if (string.IsNullOrEmpty(inputString)) return inputString;
            
            var sb = new StringBuilder(inputString.Length);
            foreach (char c in inputString)
            {
                sb.Append(DiacriticsMap.TryGetValue(c, out char replacement) ? replacement : c);
            }
            return sb.ToString();
        }




        /// <summary>
        /// Sanitizes a string by removing problematic characters (XML, line breaks, etc.) and normalizing whitespace
        /// </summary>
        /// <param name="orgnStr">Original string</param>
        /// <returns>Cleaned string</returns>
        public static string CleanString(string orgnStr)
        {
            if (orgnStr == null) return null;
            if (string.IsNullOrWhiteSpace(orgnStr)) return string.Empty;
            var sb = new StringBuilder(orgnStr.Length);
            bool lastWasSpace = false;
            
            foreach (char c in orgnStr)
            {
                if (c == '\r' || c == '\n' || c == '\t' || removeCharsSet.Contains(c))
                {
                    // Replace with space, but avoid consecutive spaces
                    if (!lastWasSpace)
                    {
                        sb.Append(' ');
                        lastWasSpace = true;
                    }
                }
                else if (char.IsWhiteSpace(c))
                {
                    // Keep normal spaces but avoid consecutive spaces
                    if (!lastWasSpace)
                    {
                        sb.Append(' ');
                        lastWasSpace = true;
                    }
                }
                else
                {
                    sb.Append(c);
                    lastWasSpace = false;
                }
            }
            
            return sb.ToString().Trim();
        }





        /// <summary>
        /// Replaces a character at a specific position in the string, pads with spaces if position exceeds length
        /// </summary>
        /// <param name="orgn">Original string</param>
        /// <param name="pos">Position to replace</param>
        /// <param name="c">Character to insert</param>
        /// <returns>Modified string</returns>
        public static string ReplaceChar(this string orgn, int pos, char c)
        {
            var sb = new StringBuilder(orgn);
            while (sb.Length < pos + 1) sb.Append(' ');
            sb[pos] = c;
            return sb.ToString();
        }








        /// <summary>
        /// Splits tag values with CSV separator ';'. Filters empty values and sanitizes keys. Used for tag management.
        /// </summary>
        /// <param name="valuesstr">Tag string separated by ;</param>
        /// <param name="toUpper">Convert to uppercase</param>
        /// <param name="neverNull">Return empty array instead of null</param>
        /// <returns>Array of sanitized tags</returns>
        public static string[] SplitTag(string valuesstr, bool toUpper = true, bool neverNull=false)
        {
            if (valuesstr == null) return (neverNull) ? new string[0]:null;
            if (string.IsNullOrWhiteSpace(valuesstr)) return new string[0];
            string[] retour = valuesstr.Split(';', StringSplitOptions.None).ToArray();
            retour = retour.Select(x => KeyTools.SanitizeKey(x,toUpper)).ToArray();
            return retour;
        }







        /// <summary>
        /// Extracts encapsulated substrings between start and end separators. Example: "aaaa{bbbbb}ccccc" => "{bbbbb}"
        /// </summary>
        /// <param name="chaine">Source string</param>
        /// <param name="startSeparator">Start delimiter</param>
        /// <param name="endSeparator">End delimiter</param>
        /// <returns>Array of encapsulated strings</returns>
        public static string[] SplitEncapsuled(string chaine, string startSeparator, string endSeparator)
        {
            var retour = new List<string>();
            if (string.IsNullOrEmpty(chaine) || string.IsNullOrEmpty(startSeparator) ||
                string.IsNullOrEmpty(endSeparator))
                return retour.ToArray();


            var positionchaine = 0;
            for (var iteration = 0;
                 iteration < 999;
                 iteration++) // limit to 999 dynamic elements to avoid infinite loops
            {
                var positiondynstart = chaine.IndexOf(startSeparator, positionchaine, StringComparison.Ordinal);
                if (positiondynstart < 0) break; // nothing found
                var positiondynstop = chaine.IndexOf(endSeparator, positiondynstart, StringComparison.Ordinal) +
                    endSeparator.Length - 1;
                positionchaine = positiondynstop; // move to next
                if (positiondynstop < positiondynstart) continue; // throw new Exception("erreur dans les découpes");
                var positiondyncount = positiondynstop - positiondynstart;
                if (positiondyncount < 2 || positiondyncount > 99)
                    continue; //throw new Exception("erreur dans les découpes (chaine dynamique trop grande ou trop petite)");

                var subchainedyn = chaine.Substring(positiondynstart, positiondyncount + 1);
                if (subchainedyn.IndexOf(startSeparator, startSeparator.Length, StringComparison.Ordinal) > 1)
                    continue; // throw new Exception("pas fermé corectement");

                retour.Add(subchainedyn);
            }

            return retour.ToArray();
        }

    }
}