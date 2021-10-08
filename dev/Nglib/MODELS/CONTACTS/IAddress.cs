using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.MODELS.CONTACTS
{
    /// <summary>
    /// Représente une adresse postale
    /// </summary>
    public interface IAddress
    {

        string IdentityName { get; set; }


        string AddressLines { get; set; }


        string AddressPostCode { get; set; }


        string AddressCity { get; set; }

        string AddressCountryCode { get; set; }




    }
}
