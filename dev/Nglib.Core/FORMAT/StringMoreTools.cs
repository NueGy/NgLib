using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nglib.FORMAT
{
    public static class StringMoreTools
    {




        /// <summary>
        ///     Permet de supprimer les balises d'une chaine de caratères
        /// </summary>
        /// <param name="orgnStr">Chaine d'origine</param>
        /// <param name="pTagsToStrip">tag spécifique seulements</param>
        /// <returns></returns>
        public static string StripTags(string orgnStr, string[] pTagsToStrip = null)
        {
            if (pTagsToStrip == null || pTagsToStrip.Length == 0) //strip all tags
            {
                var rx = new Regex("<[^>]+>");
                var resultText = rx.Replace(orgnStr, "");

                return resultText;
            }
            else //strip only specified tags
            {
                var tagsToStrip = "";
                for (var s = 0; s < pTagsToStrip.Length; s++)
                {
                    if (s > 0) tagsToStrip += "|";
                    tagsToStrip += pTagsToStrip[s];
                }

                var rx = new Regex("</?(?i:" + tagsToStrip + ")([^>]*>");
                var resultText = rx.Replace(orgnStr, "");

                return resultText;
            }
        }



        /// <summary>
        /// La distance Levenshtein est définie comme le nombre minimal de caractères qu'il faut remplacer, insérer ou supprimer pour transformer la chaîne str1 en str2.
        /// </summary>
        /// <param name="S1"></param>
        /// <param name="S2"></param>
        /// <returns></returns>
        public static double LevenshteinCompare(string S1, string S2)
        {

            try
            {
                if (string.IsNullOrWhiteSpace(S1)) S1 = "";
                if (string.IsNullOrWhiteSpace(S2)) S2 = "";

                string SC1 = S1.ToUpper();
                string SC2 = S2.ToUpper();

                int n = SC1.Length;
                int m = SC2.Length;

                if (n + m == 0) return 100;
                else if (n == 0) return 0;
                else if (m == 0) return 0;


                int[,] d = new int[n + 1, m + 1];
                int cost = 0;

                for (int i = 0; i <= n; i++) d[i, 0] = i;
                for (int j = 0; j <= m; j++) d[0, j] = j;

                for (int i = 1; i <= n; i++)
                {
                    for (int j = 1; j <= m; j++)
                    {
                        if (SC1[i - 1] == SC2[j - 1]) cost = 0;
                        else cost = 1;

                        d[i, j] = System.Math.Min(System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                    }
                }

                return System.Math.Round((1.0 - ((double)d[n, m] / (double)System.Math.Max(n, m))) * 100.0, 2);
            }
            catch (Exception)
            {
                return -1;
            }

        }



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
