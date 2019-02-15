using System;
using System.Collections.Generic;
using System.Text;

namespace Nglib.DATA.CONNECTOR
{
    public static class ConnectorConstants
    {

        /// <summary>
        /// SGBD COMPATIBLES NGLIB
        /// </summary>
        public enum ConnectorEngineEnum
        {
            /// <summary>
            /// Moteur inconnu
            /// </summary>
            NA,

            /// <summary>
            /// Serveur postgresql
            /// </summary>
            POSTGRESQL,

            /// <summary>
            /// MySQL
            /// </summary>
            MYSQL,

            ///// <summary>
            ///// MariaDB, NOte: Sera identifié comme MySQL dans la plus pars des cas
            ///// </summary>
            //MARIADB,

            /// <summary>
            /// SQLITE
            /// </summary>
            SQLITE,

            /// <summary>
            /// Microsoft SQL Server
            /// </summary>
            MSSQL,

            /// <summary>
            /// Microsoft ACCESS
            /// </summary>
            ACCESS,

            /// <summary>
            /// Oracle
            /// </summary>
            [Obsolete("non géré")]
            ORACLE

        }

    }
}
