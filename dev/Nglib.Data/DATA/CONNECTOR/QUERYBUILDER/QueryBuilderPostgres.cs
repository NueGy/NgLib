using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Nglib.DATA.CONNECTOR.QUERYBUILDER.QueryBuilderModels;

namespace Nglib.DATA.CONNECTOR.QUERYBUILDER
{
    /// <summary>
    /// QueryBuilder spécifique pour PostgreSQL
    /// </summary>
    public class QueryBuilderPostgres : QueryBuilderBase, IQueryBuilder
    {
        public override string EngineName => "PostgreSQL";

        /// <summary>
        /// Override pour ajouter le cast ::jsonb pour les colonnes JSON/JSONB PostgreSQL
        /// </summary>
        protected override string FormatPlaceholder(string columnName, string paramName)
        {
            // Vérifier si la colonne nécessite un cast JSONB
            // Utilise context.UpdateValues car Parameters n'est pas encore rempli lors de l'UPDATE
            object value = null;
            if (this.Parameters.ContainsKey(paramName))
                value = this.Parameters[paramName];
            else if (this.context.UpdateValues.ContainsKey(columnName))
                value = this.context.UpdateValues[columnName];
            
            if (value != null && QueryBuilderBaseTools.IsJsonColumn(columnName, value))
                return $"@{paramName}::jsonb";
            
            return $"@{paramName}";
        }

    }
}
