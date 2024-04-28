using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.SOLUTIONS.RESS.RESSOURCES
{
    /// <summary>
    /// Interface Principale d'une ressource
    /// </summary>
    public interface IRessourceItem
    {
        /// <summary>
        /// Identifiant unique de la ressource
        /// </summary>
        int RessId { get; }

        /// <summary>
        /// Cloisonnement de la ressource
        /// </summary>
        int TenantId { get; }

        /// <summary>
        /// Données supplémentaires
        /// </summary>
        DATA.ACCESSORS.IDataAccessor DataFlow { get; }

    }
}
