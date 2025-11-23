using System;

namespace Nglib.DATA.CONNECTOR.QUERYBUILDER
{
    /// <summary>
    /// Modèle pour gérer les options particulières du QueryBuilder.
    /// Permet de personnaliser le comportement de génération des requêtes SQL.
    /// </summary>
    public class QueryBuilderOptionModel
    {
        /// <summary>
        /// Options par défaut du QueryBuilder
        /// </summary>
        public static QueryBuilderOptionModel Default => new QueryBuilderOptionModel();

        // TODO: Ajouter les options ici après validation
        
        /// <summary>
        /// Crée une copie profonde des options
        /// </summary>
        public QueryBuilderOptionModel Clone()
        {
            return new QueryBuilderOptionModel
            {
                // TODO: Copier les propriétés d'options
            };
        }
    }
}
