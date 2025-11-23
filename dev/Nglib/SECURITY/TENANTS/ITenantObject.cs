using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.SECURITY.TENANTS
{
    /// <summary>
    /// Représente un objet lié à un tenant pour gérer le cloisonnement des données
    /// </summary>
    public interface ITenantObject
    {

        int TenantId { get; }

    }
}
