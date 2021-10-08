using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.DATA.BASICS
{

    interface IPostalAddress
    {
        string AddressLines { get; set; }
        string AddressPostcode { get; set; }
        string AddressCity { get; set; }
        string AddressCountryCode { get; set; }
    }

}
