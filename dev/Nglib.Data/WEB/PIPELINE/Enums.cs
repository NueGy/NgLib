using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.WEB.PIPELINE
{
    public enum RequestStateEnum
    {
        CANCEL=0,
        INIT=1,
        WAIT=2,
        RUN=3,
        ERROR=4,
        OK=5
    }

    public enum RequestPersistantModeEnum
    {

        /// <summary>
        /// Ne jamais tracer ce endoint
        /// </summary>
        NO,

        /// <summary>
        /// Tracer que si erreur
        /// </summary>
        ERROR,

        /// <summary>
        /// Toujours tracer
        /// </summary>
        NORMAL,

        /// <summary>
        /// Toujours tracer avec un maximum d'information
        /// </summary>
        FULL
    }

}
