using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Nglib.DATA.CONNECTOR
{
    /// <summary>
    /// Outils statiques pour les connecteurs
    /// </summary>
    public static class ConnectorTools
    {






        /// <summary>
        /// Savoir Si il s'agit d'une requette
        /// </summary>
        public static bool IsSQLQuery(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) return false;
            if (sql.Trim().Contains(" ")) return true;
            else return false;
        }



        


    }
}
