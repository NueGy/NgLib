using System;
using System.Collections.Generic;
using System.Text;
using Nglib.SECURITY.IDENTITY;

namespace Nglib.SOLUTIONS.IDENTITY.TENANTS
{
    public interface ITenant2 : ITenantObject, DATA.ACCESSORS.IDataAccessor
    {

        /// <summary>
        /// Identifiant unique du tenant
        /// </summary>
        int TenantId { get; }


        /// <summary>
        /// Données NoSql supplémentaires
        /// </summary>
        DATA.ACCESSORS.IDataAccessor Flux { get; } // !!! utiliser un Idataaccesssor


        //https://github.com/Finbuckle/Finbuckle.MultiTenant
        string[] Hostnames { get; }

        /// <summary>
        /// Permet de cloner le crypto context (si cloisonement des tenants par cryptage)
        /// </summary>
        /// <param name="datapoType"></param>
        /// <returns></returns>
        DATA.ACCESSORS.IDataAccessorCryptoContext CloneCryptoContext(Type datapoType = null);
    }
}
