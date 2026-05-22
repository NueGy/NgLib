using System;
using System.Collections.Generic;
using System.Linq;
using static Nglib.DATA.CONNECTOR.QUERYBUILDER.QueryBuilderModels;

namespace Nglib.DATA.CONNECTOR.QUERYBUILDER
{
    /// <summary>
    /// Contexte contenant l'état et les données d'une requête SQL en construction
    /// </summary>
    public class QueryBuilderContext : IDisposable
    {
        /// <summary>
        /// Nom de la table principale
        /// </summary>
        public string TableName { get; set; } = "";

        /// <summary>
        /// Alias de la table principale (optionnel)
        /// </summary>
        public string TableAlias { get; set; } = "";

        /// <summary>
        /// Paramètres de la requête
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new();

        /// <summary>
        /// Colonnes à sélectionner
        /// </summary>
        public List<string> SelectColumns { get; set; } = new();

        /// <summary>
        /// Clauses WHERE
        /// </summary>
        public List<string> WhereClauses { get; set; } = new();

        /// <summary>
        /// Clauses JOIN
        /// </summary>
        public List<string> JoinClauses { get; set; } = new();

        /// <summary>
        /// Clauses ORDER BY
        /// </summary>
        public List<string> OrderByClauses { get; set; } = new();

        /// <summary>
        /// Clauses GROUP BY
        /// </summary>
        public List<string> GroupByClauses { get; set; } = new();

        /// <summary>
        /// Valeurs pour UPDATE/INSERT
        /// </summary>
        public Dictionary<string, object> UpdateValues { get; set; } = new();

        /// <summary>
        /// Type de commande SQL
        /// </summary>
        public SqlCommandTypeEnum CommandType { get; set; } = SqlCommandTypeEnum.Select;

        /// <summary>
        /// Table INTO (SELECT INTO)
        /// </summary>
        public string IntoTable { get; set; } = "";

        /// <summary>
        /// Clause HAVING
        /// </summary>
        public string HavingClause { get; set; } = "";

        /// <summary>
        /// Nombre limite de résultats
        /// </summary>
        public int? LimitCount { get; set; }

        /// <summary>
        /// Décalage pour la pagination
        /// </summary>
        public int? OffsetCount { get; set; }

        /// <summary>
        /// Flag DISTINCT pour SELECT
        /// </summary>
        public bool IsDistinct { get; set; } = false;

        /// <summary>
        /// Flag pour multi-INSERT
        /// </summary>
        public bool IsMultiInsert { get; set; } = false;

        /// <summary>
        /// Colonnes pour multi-INSERT
        /// </summary>
        public List<string> MultiInsertColumns { get; set; } = new();

        /// <summary>
        /// Lignes de données pour multi-INSERT
        /// </summary>
        public List<Dictionary<string, string>> MultiInsertRows { get; set; } = new();

        /// <summary>
        /// CTEs (Common Table Expressions)
        /// </summary>
        public List<(string alias, IQueryBuilder query)> CteClauses { get; set; } = new();

        /// <summary>
        /// Options de configuration du QueryBuilder
        /// </summary>
        public QueryBuilderOptionModel Options { get; set; } = QueryBuilderOptionModel.Default;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public QueryBuilderContext()
        {
        }

        /// <summary>
        /// Clone le contexte actuel
        /// </summary>
        /// <returns>Nouvelle instance avec les mêmes données</returns>
        public QueryBuilderContext Clone()
        {
            var clone = new QueryBuilderContext
            {
                TableName = TableName,
                CommandType = CommandType,
                IntoTable = IntoTable,
                HavingClause = HavingClause,
                LimitCount = LimitCount,
                OffsetCount = OffsetCount,
                IsDistinct = IsDistinct,
                IsMultiInsert = IsMultiInsert,

                // Clone des collections
                SelectColumns = new List<string>(SelectColumns),
                WhereClauses = new List<string>(WhereClauses),
                JoinClauses = new List<string>(JoinClauses),
                OrderByClauses = new List<string>(OrderByClauses),
                GroupByClauses = new List<string>(GroupByClauses),
                Parameters = new Dictionary<string, object>(Parameters),
                UpdateValues = new Dictionary<string, object>(UpdateValues),
                MultiInsertColumns = new List<string>(MultiInsertColumns),
                MultiInsertRows = new List<Dictionary<string, string>>()
            };

            // Clone des lignes multi-insert
            foreach (var row in MultiInsertRows)
                clone.MultiInsertRows.Add(new Dictionary<string, string>(row));

            // Clone des CTEs
            foreach (var cte in CteClauses)
                clone.CteClauses.Add((cte.alias, cte.query.Clone()));

            // Clone des options
            clone.Options = Options?.Clone() ?? QueryBuilderOptionModel.Default;

            return clone;
        }

        /// <summary>
        /// Réinitialise le contexte à son état initial
        /// </summary>
        public void Reset()
        {
            TableName = "";
            CommandType = SqlCommandTypeEnum.Select;
            IntoTable = "";
            HavingClause = "";
            LimitCount = null;
            OffsetCount = null;
            IsDistinct = false;
            IsMultiInsert = false;

            SelectColumns.Clear();
            WhereClauses.Clear();
            JoinClauses.Clear();
            OrderByClauses.Clear();
            GroupByClauses.Clear();
            Parameters.Clear();
            UpdateValues.Clear();
            MultiInsertColumns.Clear();
            MultiInsertRows.Clear();
            CteClauses.Clear();
        }



        /// <summary>
        /// Alias court pour Validate()
        /// </summary>
        /// <returns>True si valide</returns>
        public bool IsValid()
        {
            try
            {
                return QueryBuilderBaseTools.ValidateQueryContext(this);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Dispose les ressources
        /// </summary>
        public void Dispose()
        {
            // Libération des références pour le GC
            SelectColumns?.Clear();
            WhereClauses?.Clear();
            JoinClauses?.Clear();
            OrderByClauses?.Clear();
            GroupByClauses?.Clear();
            Parameters?.Clear();
            UpdateValues?.Clear();
            MultiInsertColumns?.Clear();
            MultiInsertRows?.Clear();
            CteClauses?.Clear();

            GC.SuppressFinalize(this);
        }
    }
}
