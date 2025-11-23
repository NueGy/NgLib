using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.DATA.BASICS.Enums
{
    /// <summary>
    /// Status d'une ressource
    /// 
    /// </summary>
    [Obsolete]
    public enum BasicRessStatusEnum
    {
        /// <summary>
        /// Pas de status
        /// </summary>
        NA = 0,

        /// <summary>
        /// non disponible
        /// </summary>
        NOTREADY = 1,

        /// <summary>
        /// En préparation
        /// </summary>
        INIT = 2,

        /// <summary>
        /// Invalide
        /// </summary>
        ERROR = 3,

        /// <summary>
        /// Ressource prete
        /// </summary>
        READY = 4,

        //CLOSED = 5

    }
}
