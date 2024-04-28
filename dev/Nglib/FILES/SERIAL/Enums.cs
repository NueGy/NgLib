using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.FILES.SERIAL
{
    /// <summary>
    /// Principaux types Serialisable
    /// </summary>
    public enum SerialFileTypeEnum
    {
        /// <summary>
        /// application/csv
        /// </summary>
        CSV,
        /// <summary>
        /// application/txt
        /// </summary>
        TXT,
        /// <summary>
        /// application/json
        /// </summary>
        JSON,
        /// <summary>
        /// application/xml
        /// </summary>
        XML,
        /// <summary>
        /// application/dat application/octet-stream 
        /// Données Brutes binaires (Fileset)
        /// </summary>
        DAT
    }
}
