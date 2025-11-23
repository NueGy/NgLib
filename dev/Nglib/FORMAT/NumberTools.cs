using System;
using System.Collections.Generic;

namespace Nglib.FORMAT
{
    /// <summary>
    /// Utility class for number manipulation and validation.
    /// Documentation: <see href="https://github.com/NueGy/NgLibComponents/wiki/wiki_components_format"/>
    /// </summary>
    public static class NumberTools
    {
        /// <summary>
        /// Checks if string represents a numeric value
        /// </summary>
        /// <param name="input">String to check</param>
        /// <param name="allowDecimal">Allow decimal separators</param>
        /// <returns>True if numeric</returns>
        public static bool IsNumeric(string input, bool allowDecimal = false)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            long result;
            input = input.Replace(" ", "");
            if (allowDecimal) input = input.Replace(".", "").Replace(",", "");
            return long.TryParse(input, out result);
        }

        /// <summary>
        /// Checks if string contains at least one numeric character
        /// </summary>
        /// <param name="input">String to check</param>
        /// <returns>True if contains numbers</returns>
        public static bool HasNumeric(string input)
        {
            //Verify input
            if (string.IsNullOrEmpty(input))
                return false;

            for (var i = 0; i < input.Length; i++)
                if (char.IsNumber(input[i]))
                    return true; //single numeric integer makes function true
            return false;
        }

        /// <summary>
        /// Rounds amount to 2 decimals, equivalent to Math.Round(number, 2)
        /// </summary>
        /// <param name="number">Number to round</param>
        /// <returns>Rounded amount</returns>
        public static double RoundAmount(double number)
        {
            return Math.Round(number, 2);
        }


        /// <summary>
        /// Calculate Percentage from Integer Values
        /// </summary>
        /// <param name="expression1">Numerator value</param>
        /// <param name="expression2">Divisor value</param>
        /// <returns>Calculated Percentage</returns>
        public static int CalcPercent(int expression1, int expression2)
        {
            if (expression2 == 0) return 0;
            return (int)(100.0 * expression1 / expression2);
        }

        /// <summary>
        /// Calculate Percentage from Long Values
        /// </summary>
        /// <param name="expression1">Numerator value</param>
        /// <param name="expression2">Divisor value</param>
        /// <returns>Calculated Percentage</returns>
        public static int CalcPercent(long expression1, long expression2)
        {
            if (expression2 == 0) return 0;
            return (int)(100.0 * expression1 / expression2);
        }

        /// <summary>
        /// Calculate Percentage from Double Values
        /// </summary>
        /// <param name="expression1">Numerator value</param>
        /// <param name="expression2">Divisor value</param>
        /// <returns>Calculated Percentage</returns>
        public static int CalcPercent(double expression1, double expression2)
        {
            if (expression2 == 0) return 0;
            return (int)(100.0 * expression1 / expression2);
        }


        /// <summary>
        /// PadLeft pour les nombres
        /// </summary>
        /// <param name="value">valeur</param>
        /// <param name="totalWidth">nombre de caracteres sur le champs</param>
        public static string PadNumeric(string value, int totalWidth)
        {
            if (value == null) value = ""; // jamais null
            value = value.Replace(" ", ""); // on supprime aussi les espaces (gardera les , et .)
            if (value.Length > totalWidth)
                return StringTools.Limit(value, totalWidth);

            value = value.PadLeft(totalWidth, '0'); // on ajoute les zero sur la gauche
            return value;
        }



        /// <summary>
        /// Permet de savoir si c'est un type Numerique. Ne prend pas en compte les string meme si ils représente un nombre
        /// "45"=False  45=true  (STRICT)
        /// </summary>
        public static bool IsTypeNumeric(object obj)
        {
            if (obj is string) return false; // string interdit, car on concatenera et pas d'addition
            if (obj is int?) return true;
            if (obj is int) return true;
            if (obj is long?) return true;
            if (obj is long) return true;
            if (obj is double?) return true;
            if (obj is double) return true;
            return false;
        }
    }
}