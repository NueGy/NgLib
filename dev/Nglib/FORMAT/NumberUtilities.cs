// ----------------------------------------------------------------
// Open Source Code on the MIT License (MIT)
// Copyright (c) 2015 NUEGY SARL
// https://github.com/NueGy/NgLib
// ----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.FORMAT
{
    /// <summary>
    /// Outils pour manipulation des nombres
    /// </summary>
    public static class NumberUtilities
    {











        public static bool IsNumericTry(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            double result;
            return double.TryParse(input, out result);
        }





        /// <summary>
        /// Calculate Percentage from Decimal Values
        /// </summary>
        /// <param name="decExpression1">Numerator value</param>
        /// <param name="decExpression2">Divisor value</param>
        /// <returns>Calculated percentage</returns>
        public static decimal CalcPercent(decimal decExpression1, decimal decExpression2)
        {
            if (decExpression2 == 0)
            {
                return 0;
            }

            return 100 * (decExpression1 / decExpression2);
        }


        /// <summary>
        /// Calculate Percentage from Integer Values
        /// </summary>
        /// <param name="expression1">Numerator value</param>
        /// <param name="expression2">Divisor value</param>
        /// <returns>Calculated Percentage</returns>
        public static int CalcPercent(int expression1, int expression2)
        {
            if (expression2 == 0)
            {
                return 0;
            }

            return (int)(100 * (long)expression1) / expression2;
        }



        /// <summary>
        /// Adjust decimal value to a (sanitized) number of decimal places
        /// </summary>
        /// <param name="valueIn">Decimal value</param>
        /// <param name="places">Number of decimal places</param>
        /// <returns>Adjusted value</returns>
        public static decimal DecimalAdjust(decimal valueIn, int places)
        {
            if (places < 0)
            {
                places = 0;
            }

            if (places > 28)
            {
                places = 28;
            }

            return decimal.Round(valueIn, places);
        }


    }
}
