using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.CONNECTOR
{

    /// <summary>
    /// Type de commande SQL
    /// </summary>
    public enum SqlCommandTypeEnum
    {
        /// <summary>SELECT</summary>
        Select,
        /// <summary>UPDATE</summary>
        Update,
        /// <summary>DELETE</summary>
        Delete,
        /// <summary>INSERT</summary>
        Insert
    }


}
