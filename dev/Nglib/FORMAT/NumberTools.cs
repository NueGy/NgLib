﻿// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Nglib.FORMAT
{
    /// <summary>
    ///     Outils pour manipulation des nombres
    /// </summary>
    public static class NumberTools
    {
        /// <summary>
        ///     Il s'agit d'une nombre dans une chaine de caractère ?
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsNumeric(string input, bool AllowDecimal = false)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            double result;
            input = input.Replace(" ", "");
            if (AllowDecimal) input = input.Replace(".", "").Replace(",", "");
            return double.TryParse(input, out result);
        }

        /// <summary>
        ///     la chaine contient au moins un nombre
        /// </summary>
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
        ///     Montant , Math.Round(number, 2);
        /// </summary>
        public static double RoundAmount(double number)
        {
            return Math.Round(number, 2);
        }


        /// <summary>
        ///     Calculate Percentage from Integer Values
        /// </summary>
        /// <param name="expression1">Numerator value</param>
        /// <param name="expression2">Divisor value</param>
        /// <returns>Calculated Percentage</returns>
        public static int CalcPercent(int expression1, int expression2)
        {
            if (expression2 == 0) return 0;
            return (int)(100 * (long)expression1) / expression2;
        }

        public static int CalcPercent(long expression1, long expression2)
        {
            if (expression2 == 0) return 0;
            return (int)(100 * expression1 / expression2);
        }

        public static int CalcPercent(double expression1, double expression2)
        {
            if (expression2 == 0) return 0;
            return (int)(100 * (long)expression1 / expression2);
        }


 
    }
}