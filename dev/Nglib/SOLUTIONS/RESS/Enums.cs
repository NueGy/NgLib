using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.SOLUTIONS.RESS.RESSOURCES
{
    /// <summary>
    /// Status d'une ressource
    /// 
    /// </summary>
    public enum RessStateEnum
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
