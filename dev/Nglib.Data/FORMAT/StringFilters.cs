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

namespace Nglib.FORMAT.MORE
{
    public class StringFilters
    {

        #region " Filter constants       "
        /// <summary>
        /// Filter Address Line
        /// </summary>
        private const string FilterListAddressLine = FilterListAlphanumeric + " .,:/\\()-=+";

        /// <summary>
        /// Alpha (upper and lower) characters
        /// </summary>
        private const string FilterListAlpha = FilterListLowercase + FilterListUppercase;

        /// <summary>
        /// Alpha-numeric characters
        /// </summary>
        private const string FilterListAlphanumeric = FilterListAlpha + FilterListUnsignedInteger;

        /// <summary>
        /// Numeric plus negative and decimal point
        /// </summary>
        private const string FilterListDecimal = FilterListUnsignedInteger + "-.";

        /// <summary>
        /// Lowercase + numeric + period + @
        /// </summary>
        private const string FilterListEmailAddress = FilterListLowercase + ".@-" + FilterListUnsignedInteger;

        /// <summary>
        /// General text characters
        /// </summary>
        private const string FilterListGeneralText = FilterListAlphanumeric + " .,:/\\()-=+\n";

        /// <summary>
        /// Guid Input
        /// </summary>
        private const string FilterListGuidString = FilterListUnsignedInteger + "-ABCDEF";

        /// <summary>
        /// Integer characters
        /// </summary>
        private const string FilterListInteger = FilterListUnsignedInteger + "-";

        /// <summary>
        /// IP Address characters
        /// </summary>
        private const string FilterListIPAddress = FilterListUnsignedInteger + ".";

        /// <summary>
        /// Lower case characters
        /// </summary>
        private const string FilterListLowercase = "abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// Characters for name (including space)
        /// </summary>
        private const string FilterListName = FilterListLowercase + FilterListInteger + FilterListUppercase + " -.";

        /// <summary>
        /// Password characters
        /// </summary>
        private const string FilterListPassword = FilterListAlpha + FilterListUnsignedInteger + FilterListSpecial;

        /// <summary>
        /// Characters in post codes
        /// </summary>
        private const string FilterListPostCode = "ABCDEFGHIJKLMNOPQRSTUVWXYZ 0123456789";

        /// <summary>
        /// Special characters
        /// </summary>
        /// <remarks>This list is subject to change owing to 
        /// what might or might not be acceptable in a password.</remarks>
        private const string FilterListSpecial = "#~!£$*@=:";

        /// <summary>
        /// Telephone characters
        /// </summary>
        private const string FilterListTelephone = FilterListLowercase + FilterListInteger + " +()";

        /// <summary>
        /// Time characters
        /// </summary>
        private const string FilterListTime = FilterListUnsignedInteger + ":";

        /// <summary>
        /// Numeric characters
        /// </summary>
        private const string FilterListUnsignedInteger = "0123456789";

        /// <summary>
        /// Upper Case Characters
        /// </summary>
        private const string FilterListUppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// URL characters
        /// </summary>
        private const string FilterListUrl =
          FilterListUppercase + FilterListLowercase + FilterListUnsignedInteger + " /:.?&+=-";

        /// <summary>
        /// User name characters
        /// </summary>
        private const string FilterListUserName = FilterListUppercase + FilterListUnsignedInteger;
        #endregion


        #region " Filter Functions       "
        #region " FilterAddressLine      "
        /// <summary>
        /// Filter address line
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <returns>Filtered string</returns>
        public static string FilterAddressLine(string input)
        {
            return FilterWorker(input, FilterListAddressLine);
        }

        /// <summary>
        /// Filter address line
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="output">Filtered string</param>
        /// <returns>True if input is valid</returns>
        public static bool FilterAddressLineTryParse(string input, out string output)
        {
            return FilterWorkerTry(input, FilterListAddressLine, out output);
        }

        /// <summary>
        /// Filter address line and reject box numbers
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="output">Filtered string</param>
        /// <returns>True if input is valid </returns>
        public static bool FilterAddressLineNoBoxTryParse(string input, out string output)
        {
            var valid = FilterWorkerTry(input, FilterListAddressLine, out output);
            var upperInput = output.ToUpperInvariant();
            if (upperInput.Contains("BOX NO"))
            {
                valid = false;
            }

            if (upperInput.Contains("BOX NUMBER"))
            {
                valid = false;
            }

            if (upperInput.Contains("POBOX"))
            {
                valid = false;
            }

            if (upperInput.Contains("PO BOX"))
            {
                valid = false;
            }

            if (upperInput.Contains("P O BOX"))
            {
                valid = false;
            }

            if (upperInput.Contains("P  O  BOX"))
            {
                valid = false;
            }

            if (upperInput.Contains("P.O. BOX"))
            {
                valid = false;
            }

            if (upperInput.Contains("P.O BOX"))
            {
                valid = false;
            }

            if (upperInput.Contains("PO. BOX"))
            {
                valid = false;
            }

            if (upperInput.Contains("BOX NO"))
            {
                valid = false;
            }

            if (upperInput.Contains("POST OFFICE BOX"))
            {
                valid = false;
            }

            if (upperInput.Contains("POST BOX"))
            {
                valid = false;
            }

            if (upperInput.Contains("POST OFFICE"))
            {
                valid = false;
            }

            if (upperInput.Contains("P BOX"))
            {
                valid = false;
            }

            return valid;
        }
        #endregion
        #region " FilterAlpha            "
        /// <summary>
        /// Filter Alpha (upper + lower case) Values
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <returns>Filtered string</returns>
        public static string FilterAlpha(string input)
        {
            return FilterWorker(input, FilterListAlpha);
        }

        /// <summary>
        /// Try Filter Alpha (upper + lower case) Values
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="output">Filtered string</param>
        /// <returns>True if input is valid</returns>
        public static bool FilterAlphaTryParse(string input, out string output)
        {
            return FilterWorkerTry(input, FilterListAlpha, out output);
        }
        #endregion
        #region " FilterAlphanumeric     "
        /// <summary>
        /// Filter AlphaNumeric (upper + lower + integer) Values
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <returns>Filtered string</returns>
        public static string FilterAlphanumeric(string input)
        {
            return FilterWorker(input, FilterListAlphanumeric);
        }

        /// <summary>
        /// Try Filter AlphaNumeric (upper + lower + integer) Values
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="output">Filtered string</param>
        /// <returns>True if input is valid</returns>
        public static bool FilterAlphanumericTryParse(string input, out string output)
        {
            return FilterWorkerTry(input, FilterListAlphanumeric, out output);
        }
        #endregion
        #region " FilterDecimal          "
        /// <summary>
        /// Filter Decimal Values
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <returns>Filtered string</returns>
        public static string FilterDecimal(string input)
        {
            return FilterWorker(input, FilterListDecimal);
        }

        /// <summary>
        /// Filter Decimal Values
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="output">Filtered string</param>
        /// <returns>True if input is valid</returns>
        public static bool FilterDecimalTryParse(string input, out string output)
        {
            return FilterWorkerTry(input, FilterListDecimal, out output);
        }
        #endregion
        #region " FilterEmailAddress     "
        /// <summary>
        /// Filter EmailAddress case
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <returns>Filtered string</returns>
        public static string FilterEmailAddress(string input)
        {
            return FilterWorker(input.ToLowerInvariant(), FilterListEmailAddress);
        }

        /// <summary>
        /// Filter EmailAddress case
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="output">Filtered string</param>
        /// <returns>True if input is valid</returns>
        public static bool FilterEmailAddressTryParse(string input, out string output)
        {
            return FilterWorkerTry(input.ToLowerInvariant(), FilterListEmailAddress, out output);
        }
        #endregion
        #region " FilterGeneralText (plus notes on usage) "
        /// <summary>
        /// Filter general text
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <returns>Filtered string</returns>
        /// <remarks>These filter functions are built upon the premise: "All input is evil".
        /// These functions filter input to a range of permitted values controlled by   
        /// appropriate constants. By defining the allowed values within string constants
        /// the allowed values can readily be changed.
        /// Input is usually from a text box but could also be an argument from the query string.
        /// When working with non-English languages the General Text will need to be rewritten
        /// on an exclude basis.
        /// The filter functions filter on a permitted values basis - Thus <see cref="FilterDecimal"/>
        /// would pass the second decimal point in "1.22.2".
        /// </remarks>
        public static string FilterGeneralText(string input)
        {
            return FilterWorker(input, FilterListGeneralText);
        }

        /// <summary>
        /// Filter general text
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="output">Filtered string</param>
        /// <returns>True if input is valid</returns>
        public static bool FilterGeneralTextTryParse(string input, out string output)
        {
            return FilterWorkerTry(input, FilterListGeneralText, out output);
        }
        #endregion
        #region " FilterGuidString       "
        /// <summary>
        /// Filter Guid String
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <returns>Filtered string</returns>
        public static string FilterGuidString(string input)
        {
            return FilterWorker(input.ToUpperInvariant(), FilterListGuidString);
        }

        /// <summary>
        /// Filter Guid String
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="output">Filtered string</param>
        /// <returns>True if input is valid</returns>
        public static bool FilterGuidStringTryParse(string input, out string output)
        {
            return FilterWorkerTry(input.ToUpperInvariant(), FilterListGuidString, out output);
        }
        #endregion
        #region " FilterHtmlText         "
        /// <summary>
        /// Filters the HTML markup text.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>Text between the markup</returns>
        public static string FilterHtmlText(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, @"<(.|\n)*?>", " ").Replace("  ", " ").Replace("  ", " ").Trim();
        }
        #endregion
        #region " FilterInteger          "
        /// <summary>
        /// Filter Integer Values
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <returns>Filtered string</returns>
        public static string FilterInteger(string input)
        {
            return FilterWorker(input, FilterListInteger);
        }

        /// <summary>
        /// Filter Integer Values
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="result">String Filtered and convert</param>
        /// <returns>True if input is valid</returns>
        public static bool FilterIntegerTryParse(string input, out int result)
        {
            string output;
            result = 0;
            if (FilterWorkerTry(input, FilterListInteger, out output))
            {
                return int.TryParse("0" + output, out result);
            }

            return false;
        }

        /// <summary>
        /// Filter Integer Values
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="output">Filtered string</param>
        /// <returns>True if input is valid</returns>
        public static bool FilterIntegerTryParse(string input, out string output)
        {
            return FilterWorkerTry(input, FilterListInteger, out output);
        }
        #endregion
        #region " FilterIPAddress        "
        /// <summary>
        /// Filter IP Address Values
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <returns>Filtered string</returns>
        public static string FilterIPAddress(string input)
        {
            if (FilterWorker(input, ".").Length != 3)
            {
                return string.Empty;
            }

            var segment = input.Split('.');
            for (var i = 0; i < 4; i++)
            {
                int result;
                if (!FilterUnsignedIntegerTryParse(segment[i], out result))
                {
                    return string.Empty;
                }

                if (result > 255)
                {
                    return string.Empty;
                }
            }

            return FilterWorker(input, FilterListIPAddress);
        }

        /// <summary>
        /// Filter IP Address Values
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="output">Filtered string</param>
        /// <returns>True if input is valid</returns>
        public static bool FilterIPAddressTryParse(string input, out string output)
        {
            output = string.Empty;
            if (FilterWorker(input, ".").Length != 3)
            {
                return false;
            }

            var segment = input.Split('.');
            for (var i = 0; i < 4; i++)
            {
                int result;
                if (!FilterUnsignedIntegerTryParse(segment[i], out result))
                {
                    return false;
                }

                if (result > 255)
                {
                    return false;
                }
            }

            return FilterWorkerTry(input, FilterListIPAddress, out output);
        }
        #endregion
        #region " FilterLowercase        "
        /// <summary>
        /// Filter lower case
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <returns>Filtered string</returns>
        public static string FilterLowercase(string input)
        {
            return FilterWorker(input, FilterListLowercase);
        }

        /// <summary>
        /// Filter lower case
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="output">Filtered string</param>
        /// <returns>True if input is valid</returns>
        public static bool FilterLowercaseTryParse(string input, out string output)
        {
            return FilterWorkerTry(input, FilterListLowercase, out output);
        }
        #endregion
        #region " FilterName             "
        /// <summary>
        /// Filter Name (with space or hyphen)
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <returns>Filtered string</returns>
        public static string FilterName(string input)
        {
            return FilterWorker(input, FilterListName);
        }

        /// <summary>
        /// Filter Name (with space or hyphen)
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="output">Filtered string</param>
        /// <returns>True if input is valid</returns>
        public static bool FilterNameTryParse(string input, out string output)
        {
            return FilterWorkerTry(input, FilterListName, out output);
        }
        #endregion
        #region " FilterPassword         "
        /// <summary>
        /// Filter Password (upper + lower case + special) Values
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <returns>Filtered string</returns>
        public static string FilterPassword(string input)
        {
            return FilterWorker(input, FilterListPassword);
        }

        /// <summary>
        /// Try Filter Password (upper + lower case + special) Values
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="output">Filtered string</param>
        /// <returns>True if input is valid</returns>
        public static bool FilterPasswordTryParse(string input, out string output)
        {
            return FilterWorkerTry(input, FilterListPassword, out output);
        }
        #endregion
        #region " FilterPostcode         "
        /// <summary>
        /// Filter Postcode (text forced to upper case)
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <returns>Filtered string</returns>
        public static string FilterPostcode(string input)
        {
            return FilterWorker(input.ToUpperInvariant(), FilterListPostCode);
        }

        /// <summary>
        /// Filter Postcode (text forced to upper case)
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="output">Filtered string</param>
        /// <returns>True if input is valid</returns>
        public static bool FilterPostcodeTryParse(string input, out string output)
        {
            return FilterWorkerTry(input.ToUpperInvariant(), FilterListPostCode, out output);
        }
        #endregion
        #region " FilterSpecial          "
        /// <summary>
        /// Filter Special characters (text forced to lower case)
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <returns>Filtered string</returns>
        public static string FilterSpecial(string input)
        {
            return FilterWorker(input.ToLowerInvariant(), FilterListSpecial);
        }

        /// <summary>
        /// Filter Special characters (text forced to lower case)
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="output">Filtered string</param>
        /// <returns>True if input is valid</returns>
        public static bool FilterSpecialTryParse(string input, out string output)
        {
            return FilterWorkerTry(input.ToLowerInvariant(), FilterListSpecial, out output);
        }
        #endregion
        #region " FilterTelephone        "
        /// <summary>
        /// Filter Telephone (text forced to lower case)
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <returns>Filtered string</returns>
        public static string FilterTelephone(string input)
        {
            return FilterWorker(input.ToLowerInvariant(), FilterListTelephone);
        }

        /// <summary>
        /// Filter Telephone (text forced to lower case)
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="output">Filtered string</param>
        /// <returns>True if input is valid</returns>
        public static bool FilterTelephoneTryParse(string input, out string output)
        {
            return FilterWorkerTry(input.ToLowerInvariant(), FilterListTelephone, out output);
        }
        #endregion
        #region " FilterTime             "
        /// <summary>
        /// FilterTime (00:00 or 00:00:00 format)
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <returns>Filtered string</returns>
        public static string FilterTime(string input)
        {
            return FilterWorker(input, FilterListTime);
        }

        /// <summary>
        /// <see cref="FilterTime"/> (00:00 or 00:00:00 format)
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="output">Filtered string</param>
        /// <returns>True if input is valid</returns>
        public static bool FilterTimeTryParse(string input, out string output)
        {
            return FilterWorkerTry(input, FilterListTime, out output);
        }
        #endregion
        #region " FilterUnsignedInteger  "
        /// <summary>
        /// Filter UnsignedInteger Values
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <returns>Filtered string</returns>
        public static string FilterUnsignedInteger(string input)
        {
            return FilterWorker(input, FilterListUnsignedInteger);
        }

        /// <summary>
        /// Filter Unsigned Integer Values
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="result">String Filtered and convert</param>
        /// <returns>True if input is valid</returns>
        public static bool FilterUnsignedIntegerTryParse(string input, out int result)
        {
            string output;
            result = 0;
            if (FilterWorkerTry(input, FilterListUnsignedInteger, out output))
            {
                return int.TryParse("0" + output, out result);
            }

            return false;
        }

        /// <summary>
        /// Filter Unsigned Integer Values
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="output">Filtered string</param>
        /// <returns>True if input is valid</returns>
        public static bool FilterUnsignedIntegerTryParse(string input, out string output)
        {
            return FilterWorkerTry(input, FilterListUnsignedInteger, out output);
        }
        #endregion
        #region " FilterUppercase        "
        /// <summary>
        /// Filter upper case
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <returns>Filtered string</returns>
        public static string FilterUppercase(string input)
        {
            return FilterWorker(input, FilterListUppercase);
        }

        /// <summary>
        /// Filter upper case
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="output">Filtered string</param>
        /// <returns>True if input is valid</returns>
        public static bool FilterUppercaseTryParse(string input, out string output)
        {
            return FilterWorkerTry(input, FilterListUppercase, out output);
        }
        #endregion
        #region " FilterUrl              "
        /// <summary>
        /// Filter URL 
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <returns>Filtered string</returns>
        /// <remarks>
        /// The absence of an initial http:// or https:// does not constitute an
        /// error, but an http:// is silently prepended if one is absent.
        /// Positional validity is not currently included.
        /// </remarks>
        public static string FilterUrl(string input)
        {
            string output;
            var valid = FilterWorkerTry(input, FilterListUrl, out output);
            if (valid)
            {
                if (!(output.StartsWith("http://", StringComparison.Ordinal)
                      || output.StartsWith("http://", StringComparison.Ordinal)))
                {
                    output = "http://" + output;
                }
            }

            return output;
        }

        /// <summary>
        /// Filter URL 
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="output">Filtered string</param>
        /// <returns>True if input is valid</returns>
        /// <remarks>
        /// The absence of an initial http:// or https:// does not constitute an
        /// error, but an http:// is silently prepended if one is absent.
        /// Positional validity is not currently included.
        /// </remarks>
        public static bool FilterUrlTryParse(string input, out string output)
        {
            var valid = FilterWorkerTry(input, FilterListUrl, out output);
            if (valid)
            {
                if (!(output.StartsWith("http://", StringComparison.Ordinal)
                      || output.StartsWith("http://", StringComparison.Ordinal)))
                {
                    output = "http://" + output;
                }
            }

            return valid;
        }
        #endregion
        #region " FilterUserName         "
        /// <summary>
        /// Filter user name (and force to upper case)
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <returns>Filtered string</returns>
        public static string FilterUserName(string input)
        {
            return FilterWorker(input.ToUpperInvariant(), FilterListUserName);
        }

        /// <summary>
        /// Filter user name (and force to upper case)
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="output">Filtered string</param>
        /// <returns>True if input is valid</returns>
        public static bool FilterUserNameTryParse(string input, out string output)
        {
            return FilterWorkerTry(input.ToUpperInvariant(), FilterListUserName, out output);
        }
        #endregion
        #region " FilterWorker           "
        /// <summary>
        /// Filter Worker
        /// </summary>
        /// <param name="input">Input to filter</param>
        /// <param name="validChars">White list of allowed characters</param>
        /// <returns>Filtered string</returns>
        public static string FilterWorker(string input, string validChars)
        {
            input = string.Empty + input;
            if (input.Length == 0)
            {
                return input;
            }

            var output = new StringBuilder(input.Length);
            for (var loop = 0; loop < input.Length; loop++)
            {
                var value = input.Substring(loop, 1);
                if (validChars.IndexOf(value, StringComparison.Ordinal) >= 0)
                {
                    output.Append(value);
                }
            }

            return output.ToString();
        }

        /// <summary>
        /// Filter worker try action
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="validChars">The string of valid characters</param>
        /// <param name="output">The output string.</param>
        /// <returns>True if data passed</returns>
        public static bool FilterWorkerTry(string input, string validChars, out string output)
        {
            output = string.Empty;
            var dataGood = true;
            input = string.Empty + input;
            if (input.Length != 0)
            {
                var work = new StringBuilder(input.Length);
                for (var loop = 0; loop < input.Length; loop++)
                {
                    var value = input.Substring(loop, 1);
                    if (validChars.IndexOf(value, StringComparison.Ordinal) >= 0)
                    {
                        work.Append(value);
                    }
                    else
                    {
                        dataGood = false;
                    }
                }

                output = work.ToString();
            }

            return dataGood;
        }
        #endregion
        #endregion


        public static bool HasAdjacentCharacters(string input)
        {
            return HasAdjacentCharacters(input, 1);
        }

        /// <summary>
        /// Determines whether the input has adjacent characters the same.
        /// </summary>
        /// <param name="input">The string to test (<see langword="null"/> treated as empty string).</param>
        /// <param name="maximumAllowed">The number of repeats allowable (negative values set to 0).</param>
        /// <returns>
        /// <c>true</c> if [is repeated character] [the specified input]; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasAdjacentCharacters(string input, int maximumAllowed)
        {
            if (input == null)
            {
                return false;
            }

            if (maximumAllowed < 0)
            {
                maximumAllowed = 0;
            }

            var last = string.Empty;
            var repeats = 0;
            for (var i = 0; i < input.Length; i++)
            {
                var c = input.Substring(i, 1);
                if (c == last)
                {
                    repeats++;
                    if (repeats > maximumAllowed)
                    {
                        return true;
                    }
                }
                else
                {
                    repeats = 0;
                }

                last = c;
            }

            return false;
        }




        public static bool HasRepeatedCharacters(string input)
        {
            return HasRepeatedCharacters(input, 1, FilterListPassword);
        }

        /// <summary>
        /// Determines whether the input has repeated characters.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="maximumAllowed">The number of repeats allowable (minimum value 1).</param>
        /// <returns>
        ///     <c>true</c> if [has repeated characters] [the specified input]; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasRepeatedCharacters(string input, int maximumAllowed)
        {
            return HasRepeatedCharacters(input, maximumAllowed, FilterListPassword);
        }

        /// <summary>
        /// Determines whether the input has repeated characters.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="maximumAllowed">The number of repeats allowable (minimum value 1).</param>
        /// <param name="lookup">Characters to lookup</param>
        /// <returns>
        ///    <c>true</c> if [has repeated characters] [the specified input]; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasRepeatedCharacters(string input, int maximumAllowed, string lookup)
        {
            if (input == null)
            {
                return false;
            }

            if (maximumAllowed < 1)
            {
                maximumAllowed = 1;
            }

            var length = lookup.Length;
            var counts = new int[length];
            for (var i = 1; i < length; i++)
            {
                counts[i] = 0;
            }

            for (var i = 0; i < input.Length; i++)
            {
                var c = input.Substring(i, 1);
                var find = lookup.IndexOf(c, StringComparison.Ordinal);
                if (find >= 0)
                {
                    counts[find]++;
                }
            }

            for (var i = 0; i < length; i++)
            {
                if (counts[i] > maximumAllowed)
                {
                    return true;
                }
            }

            return false;
        }





    }
}
