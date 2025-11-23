using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nglib.DATA.CONNECTOR.QUERYBUILDER
{
    /// <summary>
    /// Modèles et énumérations pour QueryBuilder
    /// </summary>
    public static class QueryBuilderModels
    {
        /// <summary>
        /// Type de jointure SQL
        /// </summary>
        public enum JoinTypeEnum
        {
            /// <summary>INNER JOIN</summary>
            Inner,
            /// <summary>LEFT JOIN</summary>
            Left,
            /// <summary>RIGHT JOIN</summary>
            Right,
            /// <summary>FULL JOIN</summary>
            Full,
            /// <summary>CROSS JOIN</summary>
            Cross
        }

        /// <summary>
        /// Comportement en cas de valeur null ou vide
        /// </summary>
        public enum IfNullEmptyEnum
        {
            /// <summary>Accepte les valeurs nullables</summary>
            Nullable,
            /// <summary>Ignore la condition</summary>
            Ignore,
            /// <summary>Génère une erreur</summary>
            Error
        }

        /// <summary>
        /// Type de commande SQL
        /// </summary>
        public enum SqlCommandTypeEnum
        {
            /// <summary>SELECT</summary>
            Select,
            /// <summary>INSERT</summary>
            Insert,
            /// <summary>UPDATE</summary>
            Update,
            /// <summary>DELETE</summary>
            Delete
        }
    }
}