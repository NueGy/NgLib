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
    public static class ValidateUtilities
    {


        public static bool ValidateMail(string pEmailAddress)
        {
            bool addressIsValid = true;
            try
            {
                System.Net.Mail.MailAddress mailAddress = new System.Net.Mail.MailAddress(pEmailAddress);
            }
            catch
            {
                addressIsValid = false;
            }

            return addressIsValid;
        }


        public static bool IsValidIPAddress(string input)
        {
            System.Net.IPAddress address;
            return System.Net.IPAddress.TryParse(input, out address);
        }

    }
}
