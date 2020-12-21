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
    public static class ValidateTools
    {
        public const string PhoneNumberRegex = @"\+[0-9]{1,3}-[0-9()+\-]{1,30} ";// REGEX STANDARD :   

        /// <summary>
        /// Simple validation regex
        /// </summary>
        public static bool IsValid(string value, string regexPattern)
            => System.Text.RegularExpressions.Regex.IsMatch(value, regexPattern);
        



        /// <summary>
        /// Validation d'un mail
        /// </summary>
        public static bool MailIsValid(string pEmailAddress)
        {
            bool addressIsValid = true;
            try
            {
                System.Net.Mail.MailAddress mailAddress = new System.Net.Mail.MailAddress(pEmailAddress);
            }
            catch { addressIsValid = false; }
            return addressIsValid;
        }

        /// <summary>
        /// Validation d'une adresse IP
        /// </summary>
        public static bool IPAddressIsValid(string input)
        {
            System.Net.IPAddress address;
            return System.Net.IPAddress.TryParse(input, out address);
        }


        /// <summary>
        /// Permet de valider et corriger un numero en numéro international  (+33)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Obsolete("soon")]
        public static string ValidatePhoneNumber(string input, int defaultCountryPrefix = 33)
        {
            return null;
        }
       
    }
}
