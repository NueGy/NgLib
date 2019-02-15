using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nglib.FORMAT.MORE
{
    public static class PhoneNumberUtility
    {
        public class PhoneNumberResult
        {

            public int CountryCode { get; set; }

            public string NationalNumber { get; set; }

            public bool IsValid { get; set; }

        }
        // REGEX STANDARD : \+[0-9]{1,3}-[0-9()+\-]{1,30}   

        public static PhoneNumberResult Parse(string number, string DefaultCountry=null)
        {
            try
            {
                PhoneNumberResult num = new PhoneNumberResult();
                string countryString = FindCountrystring(number);
                if (!string.IsNullOrEmpty(countryString))
                    num.CountryCode = int.Parse(countryString.Replace("+", ""));
                number.Replace(countryString, "");
                number = number.Replace("(0)", ""); // zero inutile
                number = number.Replace(")", "").Replace("(", "");
                number = number.Replace(".", "").Replace("-", "").Replace(" ", "").Replace("_", "");
                num.NationalNumber = number;
                return num;
            }
            catch (Exception)
            {
                return null;
               // throw;
            }
        }


        private static string FindCountrystring(string number)
        {
            if (string.IsNullOrWhiteSpace(number)) return "";
            if (number.StartsWith("+"))
            {
                if (number.StartsWith("+33"))
                    return "+33";
                // if (!DATA.FORMAT.StringUtilities.HasNumeric(number[3])) // +33
                //     int.TryParse(number.Substring(1, 2), out countryCodeNumber);
                // rechercher parmis une liste
            }
            return "";
        }




        /// <summary>
        /// Obtient le numéro le plus simple possible en international
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string FormatPhoneNumber(string number)
        {
            PhoneNumberResult pnumber = PhoneNumberUtility.Parse(number,null);
            if (pnumber == null) return number;
            if (pnumber.CountryCode > 0)
                return string.Format("+{0}-", pnumber.CountryCode) + pnumber.NationalNumber;
            else return pnumber.NationalNumber;
        }






    }
}
